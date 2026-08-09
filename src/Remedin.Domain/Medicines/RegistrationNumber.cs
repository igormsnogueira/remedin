using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Remedin.Domain.Medicines;

/// <summary>
/// Número de registro do medicamento na ANVISA, com nove dígitos.
///
/// Centraliza a normalização da chave porque ela aparece em dois formatos:
/// nove dígitos na base de registro e treze na lista de preços da CMED, onde
/// os nove primeiros são este mesmo número. Ter a regra num lugar só é o que
/// impede as duas implementações de divergirem.
/// </summary>
public sealed record RegistrationNumber
{
    /// <summary>Dígitos do número de registro na base da ANVISA.</summary>
    public const int Length = 9;

    /// <summary>Dígitos do campo REGISTRO na lista de preços da CMED.</summary>
    private const int PriceListLength = 13;

    private RegistrationNumber(string value) => Value = value;

    public string Value { get; }

    /// <exception cref="ArgumentException">Quando o valor não é um registro válido.</exception>
    public static RegistrationNumber Parse(string? raw) =>
        TryParse(raw, out var number)
            ? number
            : throw new ArgumentException($"Número de registro inválido: '{raw}'.", nameof(raw));

    public static bool TryParse(string? raw, [NotNullWhen(true)] out RegistrationNumber? number)
    {
        number = null;

        var digits = KeepDigits(raw);

        // Os quatro dígitos finais do formato da CMED não foram decodificados
        // e não são usados: eles não identificam a apresentação, que é o que o
        // código GGREM faz.
        if (digits.Length == PriceListLength)
        {
            digits = digits[..Length];
        }

        if (digits.Length != Length)
        {
            return false;
        }

        number = new RegistrationNumber(digits);
        return true;
    }

    private static string KeepDigits(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var digits = new StringBuilder(raw.Length);

        foreach (var character in raw)
        {
            if (char.IsAsciiDigit(character))
            {
                digits.Append(character);
            }
        }

        return digits.ToString();
    }

    public override string ToString() => Value;
}
