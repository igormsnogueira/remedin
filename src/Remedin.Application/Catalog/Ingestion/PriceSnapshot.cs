using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Ingestion;

/// <summary>As apresentações de um medicamento, vindas da lista de preço.</summary>
public sealed record PricedMedicine(
    RegistrationNumber Registration,
    IReadOnlyList<Presentation> Presentations);

/// <summary>Uma publicação da lista de preços, já lida e agrupada por registro.</summary>
public sealed record PriceSnapshot(
    IReadOnlyList<PricedMedicine> Medicines,
    int RowsRead,
    int Rejected,
    string ContentHash);

public interface IPriceSnapshotSource
{
    Task<PriceSnapshot> FetchAsync(CancellationToken cancellationToken);
}

public interface IMedicinePriceStore
{
    /// <summary>
    /// Troca as apresentações de todo o catálogo numa transação. A CMED
    /// publica a lista completa, não um incremento.
    /// </summary>
    Task ReplaceAllAsync(
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken);
}
