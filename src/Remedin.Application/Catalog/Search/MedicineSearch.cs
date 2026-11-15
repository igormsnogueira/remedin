namespace Remedin.Application.Catalog.Search;

public sealed record MedicineSummary(
    string RegistrationNumber,
    string Name,
    string? ActiveIngredient,
    string? Manufacturer,
    string? TherapeuticClassCode,
    string? TherapeuticClass,
    bool IsActive,
    decimal? CheapestConsumerPrice)
{
    public string? Purpose => Domain.Medicines.TherapeuticCategories.Describe(TherapeuticClassCode)?.Label;
}

public sealed record SearchResults(
    string Term,
    string State,
    decimal IcmsRate,
    PagedResults<MedicineSummary> Medicines);

public interface IMedicineSearch
{
    Task<PagedResults<MedicineSummary>> SearchAsync(
        string term,
        string state,
        Page page,
        CancellationToken cancellationToken);
}
