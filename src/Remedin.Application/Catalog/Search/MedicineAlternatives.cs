namespace Remedin.Application.Catalog.Search;

public sealed record MedicineAlternative(
    string RegistrationNumber,
    string Name,
    string? Manufacturer,
    string Presentation,
    decimal ConsumerPrice,
    decimal? DosageInMilligrams,
    int? UnitCount,
    bool IsCurrent)
{
    public decimal? PricePerUnit =>
        UnitCount is > 0 ? Math.Round(ConsumerPrice / UnitCount.Value, 2) : null;
}

public sealed record AlternativesResult(
    string RegistrationNumber,
    string? ActiveIngredient,
    string State,
    decimal IcmsRate,
    IReadOnlyList<MedicineAlternative> Alternatives)
{
    private MedicineAlternative? Current =>
        Alternatives.FirstOrDefault(alternative => alternative.IsCurrent);

    public MedicineAlternative? CheapestComparable
    {
        get
        {
            if (Current?.PricePerUnit is not { } current)
            {
                return null;
            }

            var cheapest = Alternatives
                .Where(alternative =>
                    !alternative.IsCurrent
                    && alternative.PricePerUnit is not null
                    && alternative.DosageInMilligrams == Current.DosageInMilligrams)
                .MinBy(alternative => alternative.PricePerUnit);

            return cheapest?.PricePerUnit < current ? cheapest : null;
        }
    }

    public decimal? SavingsPerUnit =>
        Current?.PricePerUnit is { } current && CheapestComparable?.PricePerUnit is { } cheapest
            ? current - cheapest
            : null;

    public string Notice =>
        "Medicamentos com o mesmo princípio ativo. Confira a dosagem e a quantidade " +
        "de cada embalagem, e confirme a troca com o farmacêutico.";
}

public interface IMedicineAlternatives
{
    Task<AlternativesResult?> FindAsync(
        string registrationNumber,
        string state,
        CancellationToken cancellationToken);
}
