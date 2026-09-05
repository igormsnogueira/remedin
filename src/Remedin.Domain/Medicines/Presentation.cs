namespace Remedin.Domain.Medicines;

/// <summary>
/// Uma embalagem ou dosagem específica de um medicamento, com os preços dela.
///
/// Vive dentro do agregado <see cref="Medicine"/>: não se busca uma embalagem
/// sem o nome do produto, e o ciclo de vida dela é o do registro.
/// </summary>
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

    /// <summary>Código GGREM, que identifica a apresentação na lista da CMED.</summary>
    public string GgremCode { get; }

    public string Description { get; }

    /// <summary>Uso restrito a hospital, o que a mantém fora do balcão.</summary>
    public bool HospitalOnly { get; }

    /// <summary>Houve registro de comercialização no último ano informado pela CMED.</summary>
    public bool SoldRecently { get; }

    public IReadOnlyList<Price> Prices => _prices;

    public bool HasPrice => _prices.Count > 0;

    /// <summary>
    /// Dosagem e quantidade lidas da descrição, quando legíveis. Sem elas não
    /// há preço por unidade, e a comparação entre embalagens de tamanhos
    /// diferentes não é feita (ADR 0010).
    /// </summary>
    public Packaging Packaging => Packaging.Read(Description);

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

    /// <summary>
    /// Preço para a alíquota pedida, ou nulo se a CMED não publicou.
    /// Produto de uso hospitalar não tem preço ao consumidor.
    /// </summary>
    public decimal? PriceFor(PriceKind kind, decimal icmsRate, bool freeTradeZone = false) =>
        _prices.FirstOrDefault(price => price.Matches(kind, icmsRate, freeTradeZone))?.Amount;

    public decimal? ConsumerPriceIn(string state) =>
        IcmsRates.TryGet(state, out var rate) ? PriceFor(PriceKind.Consumer, rate) : null;
}
