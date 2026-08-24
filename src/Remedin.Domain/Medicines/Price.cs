namespace Remedin.Domain.Medicines;

public enum PriceKind
{
    /// <summary>Preço Fábrica: teto que a indústria pode cobrar da farmácia.</summary>
    Factory = 0,

    /// <summary>Preço Máximo ao Consumidor: teto que a farmácia pode cobrar do cliente.</summary>
    Consumer = 1,
}

/// <summary>
/// Um teto de preço publicado pela CMED, válido para uma alíquota de ICMS.
///
/// O mesmo medicamento tem preço legal diferente conforme o estado onde é
/// vendido, então cada apresentação carrega dezenas destes.
/// </summary>
public sealed class Price
{
    private Price(PriceKind kind, decimal? icmsRate, bool freeTradeZone, decimal amount)
    {
        Kind = kind;
        IcmsRate = icmsRate;
        FreeTradeZone = freeTradeZone;
        Amount = amount;
    }

    public PriceKind Kind { get; }

    /// <summary>Alíquota de ICMS. Nulo na coluna sem impostos.</summary>
    public decimal? IcmsRate { get; }

    /// <summary>Preço das áreas de livre comércio, publicado à parte.</summary>
    public bool FreeTradeZone { get; }

    public decimal Amount { get; }

    public static Price Create(PriceKind kind, decimal? icmsRate, bool freeTradeZone, decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), amount, "Preço não pode ser negativo.");
        }

        if (icmsRate is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(icmsRate), icmsRate, "Alíquota fora da faixa possível.");
        }

        return new Price(kind, icmsRate, freeTradeZone, amount);
    }

    public bool Matches(PriceKind kind, decimal icmsRate, bool freeTradeZone) =>
        Kind == kind && IcmsRate == icmsRate && FreeTradeZone == freeTradeZone;
}
