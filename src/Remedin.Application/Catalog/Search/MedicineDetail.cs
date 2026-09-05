namespace Remedin.Application.Catalog.Search;

/// <summary>
/// Uma embalagem na ficha, com o teto legal no estado consultado.
/// </summary>
/// <param name="ConsumerPrice">
/// Ausente em produto de uso hospitalar, que não vai ao balcão.
/// </param>
public sealed record PresentationDetail(
    string GgremCode,
    string Description,
    bool HospitalOnly,
    bool SoldRecently,
    decimal? ConsumerPrice,
    decimal? FactoryPrice);

/// <summary>
/// A ficha do medicamento.
///
/// Os campos ausentes são declarados em vez de escondidos: o catálogo tem
/// medicamento sem preço publicado, sem tarja informada e sem registro de
/// comercialização recente, e omitir isso faria o produto parecer incompleto
/// sem explicar por quê (ADR 0007).
/// </summary>
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
    IReadOnlyList<PresentationDetail> Presentations)
{
    /// <summary>
    /// Para que serve, em linguagem comum. Nulo quando não há tradução, e
    /// nesse caso a interface mostra o nome técnico da fonte.
    /// </summary>
    public string? Purpose => Domain.Medicines.TherapeuticCategories.Describe(TherapeuticClassCode)?.Label;

    /// <summary>Exigência de receita em linguagem comum.</summary>
    public string? PrescriptionRule =>
        Domain.Medicines.PrescriptionRules.Describe(PrescriptionBand)?.Label;

    /// <summary>Nulo quando a fonte não informa, que é diferente de não exigir.</summary>
    public bool? RequiresPrescription =>
        Domain.Medicines.PrescriptionRules.Describe(PrescriptionBand)?.RequiresPrescription;

    public bool HasPrice => Presentations.Any(p => p.ConsumerPrice is not null || p.FactoryPrice is not null);

    public bool SoldInPharmacy => Presentations.Any(p => !p.HospitalOnly);

    public bool SoldRecently => Presentations.Any(p => p.SoldRecently);

    public decimal? CheapestConsumerPrice => Presentations
        .Where(p => !p.HospitalOnly)
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
