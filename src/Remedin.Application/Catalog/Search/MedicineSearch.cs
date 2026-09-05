namespace Remedin.Application.Catalog.Search;

/// <summary>
/// O que a lista de resultados precisa mostrar.
///
/// Consulta não passa pelo agregado: carregar o domínio inteiro para exibir
/// meia dúzia de campos traria junções e validações que não servem para
/// leitura.
///
/// Nome e fabricante aparecem juntos na interface, porque a busca por um
/// princípio ativo comum devolve vários produtos com o mesmo nome e é o
/// fabricante que os diferencia — e que muda o preço (ADR 0008).
/// </summary>
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
    /// <summary>Para que serve, em linguagem comum.</summary>
    public string? Purpose => Domain.Medicines.TherapeuticCategories.Describe(TherapeuticClassCode)?.Label;
}

public sealed record SearchResults(
    string Term,
    string State,
    decimal IcmsRate,
    IReadOnlyList<MedicineSummary> Medicines);

public interface IMedicineSearch
{
    /// <param name="state">
    /// Sigla da unidade da federação. O teto legal muda conforme o ICMS
    /// estadual, então não existe preço sem estado (ADR 0006).
    /// </param>
    Task<IReadOnlyList<MedicineSummary>> SearchAsync(
        string term,
        string state,
        int limit,
        CancellationToken cancellationToken);
}
