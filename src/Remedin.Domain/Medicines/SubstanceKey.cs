using System.Globalization;
using System.Text;

namespace Remedin.Domain.Medicines;

public static class SubstanceKey
{
    private const char ComponentSeparator = ';';

    public static string? From(string? activeIngredient)
    {
        if (string.IsNullOrWhiteSpace(activeIngredient))
        {
            return null;
        }

        var components = activeIngredient
            .Split(ComponentSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(Normalize)
            .Where(component => component.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal);

        var key = string.Join(ComponentSeparator, components);

        return key.Length == 0 ? null : key;
    }

    private static string Normalize(string component)
    {
        var withoutAccents = RemoveAccents(component).ToUpperInvariant();
        var collapsed = new StringBuilder(withoutAccents.Length);
        var previousWasSpace = false;

        foreach (var character in withoutAccents)
        {
            var isSpace = char.IsWhiteSpace(character);

            if (isSpace && previousWasSpace)
            {
                continue;
            }

            collapsed.Append(isSpace ? ' ' : character);
            previousWasSpace = isSpace;
        }

        return collapsed.ToString().Trim();
    }

    private static string RemoveAccents(string value)
    {
        var decomposed = value.Normalize(NormalizationForm.FormD);
        var withoutMarks = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                withoutMarks.Append(character);
            }
        }

        return withoutMarks.ToString().Normalize(NormalizationForm.FormC);
    }
}
