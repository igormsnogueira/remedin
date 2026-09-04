using Microsoft.Extensions.Logging.Abstractions;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Application.Tests.Catalog.Ingestion;

public class ImportPriceListTests
{
    private static readonly DateTimeOffset Now = new(2026, 8, 24, 3, 0, 0, TimeSpan.Zero);

    private static PricedMedicine Priced(string registrationNumber, int presentations = 1) =>
        new(
            RegistrationNumber.Parse(registrationNumber),
            new ClinicalInformation("dipirona", "N2B", "ANALGÉSICOS NÃO NARCÓTICOS", "Tarja Vermelha"),
            [.. Enumerable.Range(0, presentations).Select(index =>
                Presentation.Create(
                    $"{registrationNumber}{index:D6}",
                    "500 MG COM CT BL AL PLAS INC X 10",
                    prices: [Price.Create(PriceKind.Consumer, 18m, freeTradeZone: false, 27.44m)]))]);

    private sealed class FakeSource(PriceSnapshot snapshot) : IPriceSnapshotSource
    {
        public Task<PriceSnapshot> FetchAsync(CancellationToken cancellationToken) =>
            Task.FromResult(snapshot);
    }

    private sealed class FakeCatalog(params string[] registered) : IMedicineCatalog
    {
        public Task ReplaceAllAsync(IReadOnlyList<Medicine> medicines, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlySet<string>>(registered.ToHashSet(StringComparer.Ordinal));
    }

    private sealed class FakePriceStore : IMedicinePriceStore
    {
        public IReadOnlyList<PricedMedicine>? Saved { get; private set; }

        public Task ReplaceAllAsync(IReadOnlyList<PricedMedicine> medicines, CancellationToken cancellationToken)
        {
            Saved = medicines;
            return Task.CompletedTask;
        }
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

    private sealed class FakeTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private static ImportPriceList Build(
        PriceSnapshot snapshot,
        IMedicineCatalog catalog,
        IMedicinePriceStore store,
        IIngestionJournal journal) =>
        new(new FakeSource(snapshot), catalog, store, journal,
            new FakeTimeProvider(Now), NullLogger<ImportPriceList>.Instance);

    /// <summary>
    /// Registros conhecidos, mais alguns órfãos. A proporção importa: a regra
    /// de cobertura rejeita a carga inteira quando muita linha fica sem
    /// medicamento, então cenário com metade de órfão testa outra coisa.
    /// </summary>
    private static (PriceSnapshot Snapshot, string[] Registered) Catalog(int registered, int orphans)
    {
        var known = Enumerable.Range(0, registered).Select(index => $"1{index:D8}").ToArray();
        var unknown = Enumerable.Range(0, orphans).Select(index => $"9{index:D8}");

        var medicines = known.Concat(unknown).Select(number => Priced(number)).ToList();

        return (new PriceSnapshot(medicines, medicines.Count, Rejected: 0, ContentHash: "abc"), known);
    }

    [Fact]
    public async Task Grava_apenas_o_preco_de_medicamento_ja_registrado()
    {
        var (snapshot, registered) = Catalog(registered: 99, orphans: 1);

        var store = new FakePriceStore();
        var import = Build(snapshot, new FakeCatalog(registered), store, new FakeJournal());

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Succeeded, run.Outcome);
        Assert.Equal(99, store.Saved!.Count);
        Assert.DoesNotContain(store.Saved, medicine => medicine.Registration.Value.StartsWith('9'));
    }

    [Fact]
    public async Task Linha_orfa_conta_como_recusada()
    {
        var (snapshot, registered) = Catalog(registered: 99, orphans: 1);

        var import = Build(snapshot, new FakeCatalog(registered), new FakePriceStore(), new FakeJournal());

        var run = await import.ExecuteAsync();

        Assert.Equal(99, run.Accepted);
        Assert.Equal(1, run.Rejected);
    }

    [Fact]
    public async Task Cobertura_abaixo_do_minimo_rejeita_a_carga_inteira()
    {
        // Metade das linhas sem medicamento correspondente indica que o
        // formato da origem mudou. Gravar assim esvaziaria o preço do
        // catálogo; manter o anterior é velho mas correto.
        var (snapshot, registered) = Catalog(registered: 50, orphans: 50);

        var store = new FakePriceStore();
        var journal = new FakeJournal();
        var import = Build(snapshot, new FakeCatalog(registered), store, journal);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => import.ExecuteAsync());

        Assert.Contains("50", exception.Message);
        Assert.Null(store.Saved);
        Assert.Equal(IngestionOutcome.Failed, journal.Recorded[0].Outcome);
    }

    [Fact]
    public async Task Cobertura_entre_o_minimo_e_o_esperado_grava_mesmo_assim()
    {
        // 97% é abaixo do esperado e acima do mínimo: registra alerta e segue,
        // porque rejeitar deixaria o catálogo sem preço por causa de ruído.
        var (snapshot, registered) = Catalog(registered: 97, orphans: 3);

        var store = new FakePriceStore();
        var import = Build(snapshot, new FakeCatalog(registered), store, new FakeJournal());

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Succeeded, run.Outcome);
        Assert.Equal(97, store.Saved!.Count);
    }

    [Fact]
    public async Task Cobertura_total_grava_normalmente()
    {
        var (snapshot, registered) = Catalog(registered: 100, orphans: 0);

        var store = new FakePriceStore();
        var import = Build(snapshot, new FakeCatalog(registered), store, new FakeJournal());

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Succeeded, run.Outcome);
        Assert.Equal(100, store.Saved!.Count);
    }

    [Fact]
    public async Task Nao_reprocessa_a_mesma_publicacao()
    {
        var snapshot = new PriceSnapshot([Priced("102980592")], 1, 0, "mesmo-hash");
        var store = new FakePriceStore();

        var import = Build(
            snapshot, new FakeCatalog("102980592"), store, new FakeJournal(lastHash: "mesmo-hash"));

        var run = await import.ExecuteAsync();

        Assert.Equal(IngestionOutcome.Skipped, run.Outcome);
        Assert.Null(store.Saved);
    }
}
