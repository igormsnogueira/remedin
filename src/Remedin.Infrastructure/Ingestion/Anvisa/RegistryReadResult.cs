using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

public sealed record RegistryReadResult(
    IReadOnlyList<Medicine> Medicines,
    int RowsRead,
    IReadOnlyList<RejectedRow> Rejected,
    int Duplicates)
{
    public int Accepted => Medicines.Count;

    public override string ToString() =>
        $"{RowsRead} linhas lidas, {Accepted} aceitas, " +
        $"{Rejected.Count} recusadas, {Duplicates} duplicadas";
}

public sealed record RejectedRow(int LineNumber, string Reason);
