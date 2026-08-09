namespace Remedin.Domain.Medicines;

/// <summary>
/// Uma embalagem ou dosagem específica de um medicamento, com o preço dela.
///
/// Vive dentro do agregado <see cref="Medicine"/>: não se busca uma embalagem
/// sem o nome do produto, e o ciclo de vida dela é o do registro.
/// </summary>
public sealed class Presentation
{
    private Presentation(
        string ggremCode,
        string description,
        decimal? consumerPrice,
        decimal? factoryPrice,
        bool hospitalOnly,
        bool soldRecently)
    {
        GgremCode = ggremCode;
        Description = description;
        ConsumerPrice = consumerPrice;
        FactoryPrice = factoryPrice;
        HospitalOnly = hospitalOnly;
        SoldRecently = soldRecently;
    }

    /// <summary>Código GGREM, que identifica a apresentação na lista da CMED.</summary>
    public string GgremCode { get; }

    public string Description { get; }

    /// <summary>
    /// Preço Máximo ao Consumidor: o teto que a farmácia pode cobrar.
    /// Ausente em produto de uso restrito a hospital, que não vai ao balcão.
    /// </summary>
    public decimal? ConsumerPrice { get; }

    /// <summary>Preço Fábrica: o teto que a indústria pode cobrar da farmácia.</summary>
    public decimal? FactoryPrice { get; }

    public bool HospitalOnly { get; }

    /// <summary>Houve registro de comercialização no último ano informado pela CMED.</summary>
    public bool SoldRecently { get; }

    public static Presentation Create(
        string ggremCode,
        string description,
        decimal? consumerPrice = null,
        decimal? factoryPrice = null,
        bool hospitalOnly = false,
        bool soldRecently = false)
    {
        if (string.IsNullOrWhiteSpace(ggremCode))
        {
            throw new ArgumentException("Apresentação exige código GGREM.", nameof(ggremCode));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Apresentação exige descrição.", nameof(description));
        }

        EnsureNotNegative(consumerPrice, nameof(consumerPrice));
        EnsureNotNegative(factoryPrice, nameof(factoryPrice));

        return new Presentation(
            ggremCode.Trim(),
            description.Trim(),
            consumerPrice,
            factoryPrice,
            hospitalOnly,
            soldRecently);
    }

    private static void EnsureNotNegative(decimal? price, string parameter)
    {
        if (price is < 0)
        {
            throw new ArgumentOutOfRangeException(parameter, price, "Preço não pode ser negativo.");
        }
    }
}
