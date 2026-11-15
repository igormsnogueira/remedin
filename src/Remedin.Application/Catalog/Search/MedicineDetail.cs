namespace Remedin.Application.Catalog.Search;

public sealed record PresentationDetail(
    string GgremCode,
    string Description,
    bool HospitalOnly,
    bool SoldRecently,
    decimal? ConsumerPrice,
    decimal? FactoryPrice,
    bool Institutional = false);

public sealed record MedicineDetail(
    string RegistrationNumber,
    string Name,
    string? ActiveIngredient,
    string? Manufacturer,
    string? TherapeuticClassCode,
    string? TherapeuticClassName,
    string? PrescriptionBand,
    bool IsActive,
    string State,
    decimal IcmsRate,
    IReadOnlyList<PresentationDetail> Presentations,
    DateOnly? LeafletPublishedOn = null)
{
    public string? Purpose => Domain.Medicines.TherapeuticCategories.Describe(TherapeuticClassCode)?.Label;

    public string? PrescriptionRule =>
        Domain.Medicines.PrescriptionRules.Describe(PrescriptionBand)?.Label;

    public bool? RequiresPrescription =>
        Domain.Medicines.PrescriptionRules.Describe(PrescriptionBand)?.RequiresPrescription;

    public bool HasPrice => Presentations.Any(p => p.ConsumerPrice is not null || p.FactoryPrice is not null);

    public bool SoldInPharmacy => Presentations.Any(p => !p.HospitalOnly);

    public bool SoldRecently => Presentations.Any(p => p.SoldRecently);

    public decimal? CheapestConsumerPrice => Presentations
        .Where(p => !p.HospitalOnly && !p.Institutional)
        .Select(p => p.ConsumerPrice)
        .Where(price => price is not null)
        .Min();
}

public interface IMedicineDetails
{
    Task<MedicineDetail?> FindAsync(
        string registrationNumber,
        string state,
        CancellationToken cancellationToken);
}
