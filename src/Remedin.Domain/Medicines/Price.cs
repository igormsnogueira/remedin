namespace Remedin.Domain.Medicines;

public enum PriceKind
{
    Factory = 0,

    Consumer = 1,
}

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

    public decimal? IcmsRate { get; }

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
