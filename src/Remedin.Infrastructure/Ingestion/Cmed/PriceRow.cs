using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Cmed;

public sealed record PriceQuote(PriceKind Kind, decimal? Aliquot, bool FreeTradeZone, decimal Value);

public sealed record PriceRow(
    RegistrationNumber Registration,
    string GgremCode,
    string Product,
    string? Substance,
    string? Laboratory,
    string? TherapeuticClassCode,
    string? TherapeuticClassName,
    string Presentation,
    string? PrescriptionBand,
    bool HospitalOnly,
    bool SoldRecently,
    IReadOnlyList<PriceQuote> Prices);

public sealed record PriceReadResult(
    IReadOnlyList<PriceRow> Rows,
    int RowsRead,
    IReadOnlyList<RejectedPriceRow> Rejected,
    int PreambleLines)
{
    public override string ToString() =>
        $"{RowsRead} linhas lidas, {Rows.Count} aceitas, {Rejected.Count} recusadas, " +
        $"{PreambleLines} linhas de texto antes do cabeçalho";
}

public sealed record RejectedPriceRow(int LineNumber, string Reason);
