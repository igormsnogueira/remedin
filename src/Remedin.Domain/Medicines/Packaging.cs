using System.Globalization;
using System.Text.RegularExpressions;

namespace Remedin.Domain.Medicines;

/// <summary>
/// Dosagem e quantidade extraídas do texto da apresentação.
///
/// A CMED descreve as duas dentro de uma frase só, sem campo próprio:
/// "50 MG COM REV CT BL AL AL X 30" é um comprimido de 50 mg numa caixa com 30.
/// Sem isso, comparar preços entre embalagens de tamanhos diferentes produz
/// número errado (ADR 0010).
///
/// A leitura é deliberadamente conservadora. Das 25.691 apresentações, 14.548
/// têm os dois valores legíveis com segurança; nas demais, o resultado é
/// ausência, e não palpite.
/// </summary>
public sealed partial record Packaging(decimal? DosageInMilligrams, int? UnitCount)
{
    private static readonly Packaging Unknown = new(null, null);

    private static readonly CultureInfo BrazilianNumbers = CultureInfo.GetCultureInfo("pt-BR");

    public static Packaging Read(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return Unknown;
        }

        return new Packaging(ReadDosage(description), ReadUnitCount(description));
    }

    private static decimal? ReadDosage(string description)
    {
        var match = Dosage().Match(description);

        return match.Success
            && decimal.TryParse(match.Groups[1].Value, NumberStyles.Number, BrazilianNumbers, out var dosage)
                ? dosage
                : null;
    }

    private static int? ReadUnitCount(string description)
    {
        var match = UnitsInPack().Match(description);

        return match.Success && int.TryParse(match.Groups[1].Value, out var count) ? count : null;
    }

    /// <summary>
    /// Dosagem única em miligrama, no começo do texto.
    ///
    /// Exige espaço ou fim depois de MG, o que exclui "50 MG/ML" e "10 MG/G":
    /// esses são concentração de líquido ou pomada, e não dose por unidade.
    /// Também não casa com dose combinada entre parênteses, "(0,5 + 0,1) MG",
    /// onde não existe um valor único para comparar.
    /// </summary>
    [GeneratedRegex(@"^(\d[\d.,]*)\s*MG(?=\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex Dosage();

    /// <summary>
    /// Quantidade no fim do texto, sem unidade depois.
    ///
    /// O ancoramento no fim é o que evita o erro grave: em "CT 25 AMP VD AMB
    /// X 1ML" o número após o X é o volume de cada ampola, e em "CT BG AL X
    /// 40G" é o peso do tubo. Ler qualquer um deles como quantidade daria um
    /// preço por unidade absurdo, e em silêncio.
    /// </summary>
    [GeneratedRegex(@"X\s*(\d+)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex UnitsInPack();
}
