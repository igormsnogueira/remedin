using System.Globalization;
using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Search;

public sealed record StateOption(string Code, string Name, decimal IcmsRate);

public static class StateOptions
{
    private static readonly StringComparer ByPortugueseAlphabet =
        StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: false);

    private static readonly IReadOnlyList<StateOption> Options = BrazilianStates.Codes
        .Select(code => new StateOption(code, BrazilianStates.NameOf(code), IcmsRates.For(code)))
        .OrderBy(option => option.Name, ByPortugueseAlphabet)
        .ToArray();

    public static IReadOnlyList<StateOption> All => Options;
}
