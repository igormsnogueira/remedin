using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Remedin.Domain.Medicines;

public sealed record RegistrationNumber
{
    public const int Length = 9;

    private const int PriceListLength = 13;

    private RegistrationNumber(string value) => Value = value;

    public string Value { get; }

    public static RegistrationNumber Parse(string? raw) =>
        TryParse(raw, out var number)
            ? number
            : throw new ArgumentException($"Número de registro inválido: '{raw}'.", nameof(raw));

    public static bool TryParse(string? raw, [NotNullWhen(true)] out RegistrationNumber? number)
    {
        number = null;

        var digits = KeepDigits(raw);

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
