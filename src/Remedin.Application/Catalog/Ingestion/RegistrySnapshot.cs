using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Ingestion;

public sealed record RegistrySnapshot(
    IReadOnlyList<Medicine> Medicines,
    int RowsRead,
    int Rejected,
    int Duplicates,
    string ContentHash);

public interface IRegistrySnapshotSource
{
    Task<RegistrySnapshot> FetchAsync(CancellationToken cancellationToken);
}

public interface IMedicineCatalog
{
    Task ReplaceAllAsync(IReadOnlyList<Medicine> medicines, CancellationToken cancellationToken);

    Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken);
}

public interface IIngestionJournal
{
    Task<string?> LastSuccessfulContentHashAsync(string source, CancellationToken cancellationToken);

    Task RecordAsync(Domain.Ingestion.IngestionRun run, CancellationToken cancellationToken);
}
