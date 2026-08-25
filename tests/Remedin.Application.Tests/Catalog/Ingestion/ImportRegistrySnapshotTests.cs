using Microsoft.Extensions.Logging.Abstractions;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Application.Tests.Catalog.Ingestion;

public class ImportRegistrySnapshotTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 10, 3, 0, 0, TimeSpan.Zero);

    private static Medicine AnyMedicine(string registrationNumber = "102980592") =>
        Medicine.Register(RegistrationNumber.Parse(registrationNumber), "DIPIRONA", RegistrationStatus.Active);

    private static RegistrySnapshot AnySnapshot(string hash = "abc123", int rowsRead = 3) =>
        new([AnyMedicine()], rowsRead, Rejected: 1, Duplicates: 1, ContentHash: hash);

    private sealed class FakeSource(RegistrySnapshot? snapshot = null, Exception? failure = null)
        : IRegistrySnapshotSource
    {
        public Task<RegistrySnapshot> FetchAsync(CancellationToken cancellationToken) =>
            failure is not null
                ? Task.FromException<RegistrySnapshot>(failure)
                : Task.FromResult(snapshot!);
    }

    private sealed class FakeCatalog : IMedicineCatalog
    {
        public IReadOnlyList<Medicine>? Saved { get; private set; }

        public Task ReplaceAllAsync(IReadOnlyList<Medicine> medicines, CancellationToken cancellationToken)
        {
            Saved = medicines;
            return Task.CompletedTask;
        }

        public Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<string>>(
                (Saved ?? []).Select(medicine => medicine.RegistrationNumber.Value).ToHashSet());
    }

    private sealed class FakeJournal(string? lastHash = null) : IIngestionJournal
    {
        public List<IngestionRun> Recorded { get; } = [];

        public Task<string?> LastSuccessfulContentHashAsync(string source, CancellationToken cancellationToken) =>
            Task.FromResult(lastHash);

        public Task RecordAsync(IngestionRun run, CancellationToken cancellationToken)
        {
            Recorded.Add(run);
            return Task.CompletedTask;
        }
    }

    private static ImportRegistrySnapshot Build(
        IRegistrySnapshotSource source,
        IMedicineCatalog catalog,
        IIngestionJournal journal) =>
        new(source, catalog, journal, new FakeTimeProvider(Now), NullLogger<ImportRegistrySnapshot>.Instance);

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    [Fact]
    public async Task Carrega_o_catalogo_e_registra_a_execucao()
    {
        var catalog = new FakeCatalog();
        var journal = new FakeJournal();
        var import = Build(new FakeSource(AnySnapshot()), catalog, journal);

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Succeeded, run.Outcome);
        Assert.Single(catalog.Saved!);
        Assert.Equal(3, run.RowsRead);
        Assert.Equal(1, run.Accepted);
        Assert.Equal(1, run.Rejected);
        Assert.Equal(1, run.Duplicates);
        Assert.Single(journal.Recorded);
    }

    [Fact]
    public async Task Nao_reprocessa_a_mesma_publicacao()
    {
        var catalog = new FakeCatalog();
        var journal = new FakeJournal(lastHash: "abc123");
        var import = Build(new FakeSource(AnySnapshot(hash: "abc123")), catalog, journal);

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Skipped, run.Outcome);
        Assert.Null(catalog.Saved);
        Assert.Single(journal.Recorded);
    }

    [Fact]
    public async Task Publicacao_nova_substitui_o_catalogo()
    {
        var catalog = new FakeCatalog();
        var journal = new FakeJournal(lastHash: "hash-antigo");
        var import = Build(new FakeSource(AnySnapshot(hash: "hash-novo")), catalog, journal);

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Succeeded, run.Outcome);
        Assert.NotNull(catalog.Saved);
    }

    [Fact]
    public async Task Falha_e_registrada_antes_de_propagar()
    {
        var catalog = new FakeCatalog();
        var journal = new FakeJournal();
        var import = Build(new FakeSource(failure: new HttpRequestException("origem fora do ar")), catalog, journal);

        await Assert.ThrowsAsync<HttpRequestException>(() => import.ExecuteAsync());

        var run = Assert.Single(journal.Recorded);
        Assert.Equal(IngestionOutcome.Failed, run.Outcome);
        Assert.Contains("fora do ar", run.Detail);
        Assert.Null(catalog.Saved);
    }

    [Fact]
    public async Task Catalogo_anterior_permanece_se_a_gravacao_falhar()
    {
        var journal = new FakeJournal();
        var failing = new ThrowingCatalog();
        var import = Build(new FakeSource(AnySnapshot()), failing, journal);

        await Assert.ThrowsAsync<InvalidOperationException>(() => import.ExecuteAsync());

        Assert.Equal(IngestionOutcome.Failed, journal.Recorded[0].Outcome);
    }

    private sealed class ThrowingCatalog : IMedicineCatalog
    {
        public Task ReplaceAllAsync(IReadOnlyList<Medicine> medicines, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("banco indisponível");

        public Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<string>>(new HashSet<string>());
    }
}
