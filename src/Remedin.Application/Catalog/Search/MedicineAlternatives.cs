namespace Remedin.Application.Catalog.Search;

/// <summary>
/// Um medicamento com o mesmo princípio ativo, para comparação de preço.
/// </summary>
/// <param name="Presentation">
/// A embalagem do preço citado. Vem junto porque a comparação de dosagem
/// depende dela: a fonte descreve a dosagem dentro deste texto, sem campo
/// próprio (ADR 0010).
/// </param>
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
    /// <summary>
    /// Preço por comprimido. Nulo quando a descrição não permite ler a
    /// quantidade com segurança, o que acontece em pouco mais de 40% das
    /// apresentações — líquidos, pomadas e frascos injetáveis.
    /// </summary>
    public decimal? PricePerUnit =>
        UnitCount is > 0 ? Math.Round(ConsumerPrice / UnitCount.Value, 2) : null;
}

/// <summary>
/// Medicamentos com o mesmo princípio ativo, do mais barato ao mais caro.
///
/// A lista é de equivalentes por princípio ativo, e não de substitutos: a
/// troca é decisão do farmacêutico ou do médico, e a interface precisa dizer
/// isso junto do resultado.
/// </summary>
public sealed record AlternativesResult(
    string RegistrationNumber,
    string? ActiveIngredient,
    string State,
    decimal IcmsRate,
    IReadOnlyList<MedicineAlternative> Alternatives)
{
    private MedicineAlternative? Current =>
        Alternatives.FirstOrDefault(alternative => alternative.IsCurrent);

    /// <summary>
    /// A alternativa mais barata por comprimido, entre as de mesma dosagem do
    /// medicamento consultado.
    ///
    /// A restrição de dosagem é o que torna a comparação honesta: 10 MG e
    /// 40 MG do mesmo princípio ativo não são a mesma coisa, e apareciam lado
    /// a lado na lista.
    ///
    /// Nula quando o consultado já é o mais barato: não há o que oferecer, e
    /// apontar o próprio medicamento como alternativa confunde.
    /// </summary>
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

    /// <summary>
    /// Economia por comprimido em relação ao medicamento consultado. Só existe
    /// quando a comparação é entre a mesma dosagem e as duas quantidades são
    /// conhecidas — fora disso, subtrair preços daria um número errado que o
    /// site afirmaria como verdade.
    /// </summary>
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
