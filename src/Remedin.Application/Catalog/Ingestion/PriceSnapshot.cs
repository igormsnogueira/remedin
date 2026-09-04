using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Ingestion;

/// <summary>
/// Informação clínica que a lista de preço traz e a base de registro não tem,
/// ou tem pior. Quem vence cada campo está na ADR 0009.
/// </summary>
public sealed record ClinicalInformation(
    string? ActiveIngredient,
    string? TherapeuticClassCode,
    string? TherapeuticClassName,
    string? PrescriptionBand);

/// <summary>As apresentações de um medicamento, vindas da lista de preço.</summary>
public sealed record PricedMedicine(
    RegistrationNumber Registration,
    ClinicalInformation Clinical,
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
