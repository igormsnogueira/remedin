using System.Globalization;
using System.Text;

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

    /// <summary>
    /// Nomes como aparecem em fontes externas que não usam a grafia oficial:
    /// a base de geolocalização por IP escreve em inglês o Distrito Federal.
    /// Os demais nomes batem com os de cima depois de tirar acento e caixa.
    /// </summary>
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.Ordinal)
    {
        ["federal district"] = "DF",
        ["brasilia"] = "DF",
    };

    private static readonly Dictionary<string, string> CodesByName = NamesByCode
        .ToDictionary(pair => Normalize(pair.Value), pair => pair.Key, StringComparer.Ordinal);

    public static IReadOnlyCollection<string> Codes => NamesByCode.Keys;

    /// <summary>
    /// A sigla a partir do nome do estado, com ou sem acento, em qualquer
    /// caixa: "São Paulo", "Sao Paulo" e "SAO PAULO" dão "SP".
    /// </summary>
    public static bool TryFindByName(string? name, out string code)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            var key = Normalize(name);

            if (CodesByName.TryGetValue(key, out var found) || Aliases.TryGetValue(key, out found))
            {
                code = found;
                return true;
            }
        }

        code = string.Empty;
        return false;
    }

    private static string Normalize(string name)
    {
        // FormD separa a letra do acento ("ã" vira "a" + til), e o acento sai
        // por ser da categoria de marca sem espaço.
        var decomposed = name.Trim().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(decomposed.Length);

        foreach (var character in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(char.ToLowerInvariant(character));
            }
        }

        return builder.ToString();
    }

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
