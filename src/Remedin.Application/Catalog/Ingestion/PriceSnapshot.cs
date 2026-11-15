using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Ingestion;

public sealed record ClinicalInformation(
    string? ActiveIngredient,
    string? TherapeuticClassCode,
    string? TherapeuticClassName,
    string? PrescriptionBand);

public sealed record PricedMedicine(
    RegistrationNumber Registration,
    ClinicalInformation Clinical,
    IReadOnlyList<Presentation> Presentations);

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
    Task ReplaceAllAsync(
        IReadOnlyList<PricedMedicine> medicines,
        CancellationToken cancellationToken);
}
