namespace Remedin.Domain.Medicines;

/// <summary>
/// Alíquota de ICMS aplicável a medicamento, por estado.
///
/// Derivada do dicionário oficial da lista de preços da CMED, cópia em
/// docs/fontes/. É dado que envelhece: mudança de alíquota estadual exige
/// atualizar esta tabela e a data abaixo.
///
/// Exibir a alíquota errada faz o site publicar um teto abaixo do legal, e o
/// cidadão reclamar na farmácia de um preço que é legítimo.
/// </summary>
public static class IcmsRates
{
    /// <summary>Data da publicação de onde a tabela foi extraída.</summary>
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

        // "Demais estados" no dicionário, listados aqui para que consultar um
        // estado inexistente falhe em vez de devolver a alíquota padrão.
        ["AC"] = Standard, ["AL"] = Standard, ["DF"] = Standard, ["ES"] = Standard,
        ["GO"] = Standard, ["MS"] = Standard, ["MT"] = Standard, ["PA"] = Standard,
        ["RR"] = Standard, ["SC"] = Standard,
    };

    public static IReadOnlyCollection<string> States => ByState.Keys;

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
