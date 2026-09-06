namespace Remedin.Domain.Medicines;

/// <summary>
/// As 27 unidades da federação, por sigla.
///
/// Existe porque a consulta de preço é por estado e a pessoa escolhe o dela numa
/// lista: "Acre" é uma escolha, "AC" é uma adivinhação.
///
/// Separada de <see cref="IcmsRates"/> de propósito. A alíquota muda quando o
/// estado altera a lei; o nome não muda nunca. Guardar as duas no mesmo
/// dicionário faria a tabela de imposto carregar dado que não é dela.
/// </summary>
public static class BrazilianStates
{
    private static readonly Dictionary<string, string> NamesByCode =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["AC"] = "Acre",
            ["AL"] = "Alagoas",
            ["AM"] = "Amazonas",
            ["AP"] = "Amapá",
            ["BA"] = "Bahia",
            ["CE"] = "Ceará",
            ["DF"] = "Distrito Federal",
            ["ES"] = "Espírito Santo",
            ["GO"] = "Goiás",
            ["MA"] = "Maranhão",
            ["MG"] = "Minas Gerais",
            ["MS"] = "Mato Grosso do Sul",
            ["MT"] = "Mato Grosso",
            ["PA"] = "Pará",
            ["PB"] = "Paraíba",
            ["PE"] = "Pernambuco",
            ["PI"] = "Piauí",
            ["PR"] = "Paraná",
            ["RJ"] = "Rio de Janeiro",
            ["RN"] = "Rio Grande do Norte",
            ["RO"] = "Rondônia",
            ["RR"] = "Roraima",
            ["RS"] = "Rio Grande do Sul",
            ["SC"] = "Santa Catarina",
            ["SE"] = "Sergipe",
            ["SP"] = "São Paulo",
            ["TO"] = "Tocantins",
        };

    public static IReadOnlyCollection<string> Codes => NamesByCode.Keys;

    public static string NameOf(string code)
    {
        if (!TryGetName(code, out var name))
        {
            throw new ArgumentException($"Estado desconhecido: '{code}'.", nameof(code));
        }

        return name;
    }

    public static bool TryGetName(string? code, out string name)
    {
        if (!string.IsNullOrWhiteSpace(code) && NamesByCode.TryGetValue(code.Trim(), out var found))
        {
            name = found;
            return true;
        }

        name = string.Empty;
        return false;
    }
}
