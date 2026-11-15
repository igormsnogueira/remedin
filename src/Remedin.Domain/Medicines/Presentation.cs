namespace Remedin.Domain.Medicines;

public sealed class Presentation
{
    private readonly List<Price> _prices = [];

    private Presentation(string ggremCode, string description, bool hospitalOnly, bool soldRecently)
    {
        GgremCode = ggremCode;
        Description = description;
        HospitalOnly = hospitalOnly;
        SoldRecently = soldRecently;
    }

    public string GgremCode { get; }

    public string Description { get; }

    public bool HospitalOnly { get; }

    public bool SoldRecently { get; }

    public IReadOnlyList<Price> Prices => _prices;

    public bool HasPrice => _prices.Count > 0;

    public Packaging Packaging => Packaging.Read(Description);

    public bool Institutional => Packaging.IsInstitutional(Description);

    public static Presentation Create(
        string ggremCode,
        string description,
        bool hospitalOnly = false,
        bool soldRecently = false,
        IEnumerable<Price>? prices = null)
    {
        if (string.IsNullOrWhiteSpace(ggremCode))
        {
            throw new ArgumentException("Apresentação exige código GGREM.", nameof(ggremCode));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Apresentação exige descrição.", nameof(description));
        }

        var presentation = new Presentation(
            ggremCode.Trim(), description.Trim(), hospitalOnly, soldRecently);

        if (prices is not null)
        {
            presentation._prices.AddRange(prices);
        }

        return presentation;
    }

    public decimal? PriceFor(PriceKind kind, decimal icmsRate, bool freeTradeZone = false) =>
        _prices.FirstOrDefault(price => price.Matches(kind, icmsRate, freeTradeZone))?.Amount;

    public decimal? ConsumerPriceIn(string state) =>
        IcmsRates.TryGet(state, out var rate) ? PriceFor(PriceKind.Consumer, rate) : null;
}
