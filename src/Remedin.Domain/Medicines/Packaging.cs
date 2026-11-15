using System.Globalization;
using System.Text.RegularExpressions;

namespace Remedin.Domain.Medicines;

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

    // Até 200 unidades ainda é balcão: "X 120" é comum em tratamento contínuo.
    public const int RetailUnitLimit = 200;

    public const int RetailContainerLimit = 20;

    public static bool IsInstitutional(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            return false;
        }

        if (InstitutionalMark().IsMatch(description))
        {
            return true;
        }

        var containers = MultipleContainers().Match(description);

        if (containers.Success
            && int.TryParse(containers.Groups[1].Value, out var count)
            && count >= RetailContainerLimit)
        {
            return true;
        }

        return ReadUnitCount(description) > RetailUnitLimit;
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

    // Exige espaço depois de MG: "50 MG/ML" é concentração, não dose por unidade.
    [GeneratedRegex(@"^(\d[\d.,]*)\s*MG(?=\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex Dosage();

    // Ancorado no fim: em "X 1ML" ou "X 40G" o número é volume ou peso, não quantidade.
    [GeneratedRegex(@"X\s*(\d+)\s*$", RegexOptions.IgnoreCase)]
    private static partial Regex UnitsInPack();

    [GeneratedRegex(@"\bEMB\s*(FRAC|HOSP)", RegexOptions.IgnoreCase)]
    private static partial Regex InstitutionalMark();

    [GeneratedRegex(@"\bCX\s+(\d+)\s+(FR|FA|AMP|BG|SER|TB|ENV|STR)\b", RegexOptions.IgnoreCase)]
    private static partial Regex MultipleContainers();
}
