using System.Globalization;
using Remedin.Domain.Medicines;

namespace Remedin.Application.Catalog.Search;

/// <summary>
/// Um estado no seletor da interface, com a alíquota que define o preço-teto ali.
/// </summary>
public sealed record StateOption(string Code, string Name, decimal IcmsRate);

/// <summary>
/// A lista de estados que a interface oferece.
///
/// Não passa pelo banco: a tabela de alíquotas e a de nomes vivem no domínio, e
/// nenhuma das duas muda entre uma requisição e outra. Criar repositório e
/// consulta para servir dado constante seria cerimônia sem ganho.
/// </summary>
public static class StateOptions
{
    /// <summary>
    /// Ordenar com a cultura do servidor deixaria "Espírito Santo" em posição
    /// diferente conforme onde a aplicação roda.
    /// </summary>
    private static readonly StringComparer ByPortugueseAlphabet =
        StringComparer.Create(CultureInfo.GetCultureInfo("pt-BR"), ignoreCase: false);

    private static readonly IReadOnlyList<StateOption> Options = BrazilianStates.Codes
        .Select(code => new StateOption(code, BrazilianStates.NameOf(code), IcmsRates.For(code)))
        .OrderBy(option => option.Name, ByPortugueseAlphabet)
        .ToArray();

    /// <summary>Ordenada por nome, que é como a pessoa procura o estado dela.</summary>
    public static IReadOnlyList<StateOption> All => Options;
}
