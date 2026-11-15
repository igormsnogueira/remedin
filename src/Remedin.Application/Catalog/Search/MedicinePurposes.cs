namespace Remedin.Application.Catalog.Search;

public sealed record PurposeSummary(string Code, string Label, int MedicineCount);

public sealed record PurposeMedicines(
    string Code,
    string Label,
    string State,
    decimal IcmsRate,
    PagedResults<MedicineSummary> Medicines);

public interface IMedicinePurposes
{
    Task<IReadOnlyList<PurposeSummary>> ListAsync(CancellationToken cancellationToken);

    Task<PurposeMedicines?> FindAsync(
        string code,
        string state,
        Page page,
        CancellationToken cancellationToken);
}
