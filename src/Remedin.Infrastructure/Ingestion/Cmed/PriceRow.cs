using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Ingestion.Cmed;

public enum PriceKind
{
    /// <summary>Preço Fábrica: teto que a indústria cobra da farmácia.</summary>
    Factory = 0,

    /// <summary>Preço Máximo ao Consumidor: teto que a farmácia cobra do cliente.</summary>
    Consumer = 1,
}

/// <summary>
/// Um dos 52 preços publicados por apresentação.
///
/// O teto legal depende da alíquota de ICMS do estado onde o medicamento é
/// vendido, então guardar um só faria o site exibir valor abaixo do legal em
/// boa parte do país (ADR 0006).
/// </summary>
/// <param name="Aliquot">Nulo na coluna "Sem Impostos".</param>
/// <param name="FreeTradeZone">Coluna ALC, das áreas de livre comércio.</param>
public sealed record PriceQuote(PriceKind Kind, decimal? Aliquot, bool FreeTradeZone, decimal Value);

/// <summary>Uma linha da lista de preços, já interpretada.</summary>
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
