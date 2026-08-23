using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Ingestion;

/// <summary>
/// Uma publicação da base de registro, já lida e convertida.
///
/// Do ponto de vista do caso de uso, baixar e interpretar o arquivo são um
/// passo só; o que muda entre origens é detalhe de infraestrutura.
/// </summary>
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
    /// <summary>
    /// Troca o catálogo inteiro numa transação. A origem publica a lista
    /// completa, e falha no meio não pode deixar metade dos medicamentos.
    /// </summary>
    Task ReplaceAllAsync(IReadOnlyList<Medicine> medicines, CancellationToken cancellationToken);
}

public interface IIngestionJournal
{
    Task<string?> LastSuccessfulContentHashAsync(string source, CancellationToken cancellationToken);

    Task RecordAsync(Domain.Ingestion.IngestionRun run, CancellationToken cancellationToken);
}
