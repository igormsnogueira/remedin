namespace Remedin.Application.Catalog.Search;

/// <summary>
/// Uma finalidade de uso, com quantos medicamentos do catálogo se encaixam nela.
/// </summary>
public sealed record PurposeSummary(string Code, string Label, int MedicineCount);

/// <summary>
/// Os medicamentos de uma finalidade.
/// </summary>
public sealed record PurposeMedicines(
    string Code,
    string Label,
    string State,
    decimal IcmsRate,
    IReadOnlyList<MedicineSummary> Medicines);

public interface IMedicinePurposes
{
    /// <summary>
    /// As finalidades que têm ao menos um medicamento. Uma categoria vazia na
    /// tela é um caminho que não leva a lugar nenhum.
    /// </summary>
    Task<IReadOnlyList<PurposeSummary>> ListAsync(CancellationToken cancellationToken);

    /// <summary>Nulo quando o código não corresponde a nenhuma finalidade.</summary>
    Task<PurposeMedicines?> FindAsync(
        string code,
        string state,
        int limit,
        CancellationToken cancellationToken);
}
