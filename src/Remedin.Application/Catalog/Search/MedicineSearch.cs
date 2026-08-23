namespace Remedin.Application.Catalog.Search;

/// <summary>
/// O que a lista de resultados precisa mostrar.
///
/// Consulta não passa pelo agregado: carregar o domínio inteiro para exibir
/// cinco campos traria junções e validações que não servem para leitura.
/// </summary>
public sealed record MedicineSummary(
    string RegistrationNumber,
    string Name,
    string? ActiveIngredient,
    string? Manufacturer,
    string? TherapeuticClass,
    bool IsActive);

public interface IMedicineSearch
{
    Task<IReadOnlyList<MedicineSummary>> SearchAsync(
        string term,
        int limit,
        CancellationToken cancellationToken);
}
