namespace Remedin.Domain.Medicines;

public static class IcmsRates
{
    public static readonly DateOnly SourcedOn = new(2026, 7, 21);

    public const string DefaultState = "SP";

    private const decimal Standard = 17m;

    private static readonly Dictionary<string, decimal> ByState = new(StringComparer.OrdinalIgnoreCase)
    {
        ["RJ"] = 20m,
        ["RO"] = 17.5m,

        ["AM"] = 18m, ["AP"] = 18m, ["BA"] = 18m, ["CE"] = 18m, ["MA"] = 18m,
        ["MG"] = 18m, ["PB"] = 18m, ["PE"] = 18m, ["PI"] = 18m, ["PR"] = 18m,
        ["RN"] = 18m, ["RS"] = 18m, ["SE"] = 18m, ["SP"] = 18m, ["TO"] = 18m,

        ["AC"] = Standard, ["AL"] = Standard, ["DF"] = Standard, ["ES"] = Standard,
        ["GO"] = Standard, ["MS"] = Standard, ["MT"] = Standard, ["PA"] = Standard,
        ["RR"] = Standard, ["SC"] = Standard,
    };

    public static IReadOnlyCollection<string> States => ByState.Keys;

    public static IReadOnlySet<decimal> Served { get; } = ByState.Values.ToHashSet();

    public static bool Serves(decimal? rate) => rate is { } value && Served.Contains(value);

    public static decimal For(string state)
    {
        if (!TryGet(state, out var rate))
        {
            throw new ArgumentException($"Estado desconhecido: '{state}'.", nameof(state));
        }

        return rate;
    }

    public static bool TryGet(string? state, out decimal rate)
    {
        if (!string.IsNullOrWhiteSpace(state) && ByState.TryGetValue(state.Trim(), out rate))
        {
            return true;
        }

        rate = default;
        return false;
    }
}
