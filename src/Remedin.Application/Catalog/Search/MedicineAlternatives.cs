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
    bool IsCurrent);

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
    /// <summary>
    /// Aviso que acompanha a lista. A comparação é por princípio ativo, e as
    /// embalagens têm dosagem e quantidade diferentes — 20 MG com 7 cápsulas
    /// e 20 MG com 28 aparecem lado a lado.
    ///
    /// Não existe campo de economia aqui de propósito: subtrair dois preços de
    /// embalagens de tamanhos diferentes produz um número errado, e número que
    /// o site afirma a pessoa acredita. Comparação por unidade depende de
    /// extrair dosagem e quantidade do texto da apresentação, o que ainda não
    /// foi medido.
    /// </summary>
    public string Notice =>
        "Medicamentos com o mesmo princípio ativo. Compare a dosagem e a quantidade " +
        "de cada embalagem antes de decidir, e confirme a troca com o farmacêutico.";
}

public interface IMedicineAlternatives
{
    Task<AlternativesResult?> FindAsync(
        string registrationNumber,
        string state,
        CancellationToken cancellationToken);
}
