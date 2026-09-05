namespace Remedin.Domain.Medicines;

/// <summary>
/// A finalidade do medicamento em linguagem comum.
/// </summary>
/// <param name="Label">
/// Descreve para que serve a categoria, sem recomendar uso. A ficha exibe isto
/// junto do aviso de que o site não substitui médico ou farmacêutico.
/// </param>
public sealed record TherapeuticCategory(string Code, string Label, string GroupCode, string GroupLabel);

/// <summary>
/// Traduz o código de classe terapêutica da CMED para linguagem que o cidadão
/// entende. "ANTIRREUMÁTICOS NÃO ESTEROIDAIS PUROS" não comunica nada para
/// quem só quer saber se aquilo serve para dor.
///
/// São 528 classes distintas no catálogo, e as 36 mais comuns cobrem apenas
/// 42% dele. Por isso o mapeamento tem dois níveis: a letra inicial do código
/// é o grupo anatômico, e os 17 grupos cobrem 100% — nenhum medicamento fica
/// sem descrição. A classe específica refina onde há volume que justifique.
///
/// Dado curado do projeto, e não das fontes. Cresce conforme a interface
/// mostrar onde a descrição genérica não basta.
/// </summary>
public static class TherapeuticCategories
{
    private static readonly Dictionary<string, string> GroupLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["A"] = "Estômago, intestino e diabetes",
        ["B"] = "Sangue e circulação",
        ["C"] = "Coração e pressão",
        ["D"] = "Pele",
        ["G"] = "Saúde íntima e hormônios sexuais",
        ["H"] = "Hormônios",
        ["J"] = "Infecções",
        ["K"] = "Soluções e suplementos hospitalares",
        ["L"] = "Câncer e imunidade",
        ["M"] = "Músculos, ossos e articulações",
        ["N"] = "Sistema nervoso e dor",
        ["P"] = "Vermes e parasitas",
        ["R"] = "Respiração, gripe e alergia",
        ["S"] = "Olhos e ouvidos",
        ["T"] = "Exames e diagnóstico",
        ["V"] = "Outros",

        // O grupo F não consta na classificação de referência e tem 4
        // medicamentos no catálogo. Fica sem tradução até alguém conferir na
        // fonte, e o nome técnico aparece no lugar.
    };

    /// <summary>
    /// Classes específicas, escolhidas por volume no catálogo. Quem não está
    /// aqui cai na descrição do grupo.
    /// </summary>
    private static readonly Dictionary<string, string> ClassLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["M1A1"] = "Anti-inflamatório para dor e inflamação",
        ["N2B2"] = "Analgésico e antitérmico de venda livre",
        ["N3A"] = "Controle de convulsões e epilepsia",
        ["R6A"] = "Alergia",
        ["N6A4"] = "Antidepressivo",
        ["N6A5"] = "Antidepressivo",
        ["N6A9"] = "Antidepressivo",
        ["N5A1"] = "Antipsicótico",
        ["R5C"] = "Tosse com catarro",
        ["A2B2"] = "Azia, refluxo e gastrite",
        ["C7A0"] = "Pressão alta e problemas do coração",
        ["G3A1"] = "Anticoncepcional",
        ["S1E2"] = "Glaucoma, em colírio",
        ["J1C1"] = "Antibiótico",
        ["J1D2"] = "Antibiótico injetável",
        ["J1F"] = "Antibiótico",
        ["J1G1"] = "Antibiótico",
        ["N2A"] = "Analgésico forte, de receita controlada",
        ["N2D"] = "Dor de origem nervosa",
        ["C10A1"] = "Colesterol alto",
        ["C8A"] = "Pressão alta",
        ["C9A"] = "Pressão alta",
        ["C9C"] = "Pressão alta",
        ["J2A"] = "Infecção por fungos",
        ["D1A1"] = "Micose de pele, em pomada",
        ["D7A"] = "Pomada anti-inflamatória para a pele",
        ["H2A2"] = "Anti-inflamatório em comprimido",
        ["N5B1"] = "Insônia e sedação",
        ["L1B"] = "Quimioterapia",
        ["M3B"] = "Relaxante muscular",
        ["M5B3"] = "Osteoporose",
        ["A3F"] = "Náusea e digestão lenta",
        ["B1F"] = "Anticoagulante",
        ["N1A2"] = "Anestesia",
        ["N7D1"] = "Alzheimer",
    };

    /// <summary>Grupos disponíveis para navegação por finalidade.</summary>
    public static IReadOnlyList<TherapeuticCategory> Groups =>
    [
        .. GroupLabels
            .Select(group => new TherapeuticCategory(group.Key, group.Value, group.Key, group.Value))
            .OrderBy(category => category.Label, StringComparer.CurrentCulture)
    ];

    /// <summary>
    /// Devolve a finalidade em linguagem comum, ou nulo quando o código não
    /// tem tradução. Nulo é resposta honesta: a interface mostra o nome
    /// técnico em vez de inventar.
    /// </summary>
    public static TherapeuticCategory? Describe(string? code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        var trimmed = code.Trim();
        var groupCode = trimmed[..1];

        if (!GroupLabels.TryGetValue(groupCode, out var groupLabel))
        {
            return null;
        }

        var label = ClassLabels.TryGetValue(trimmed, out var specific) ? specific : groupLabel;

        return new TherapeuticCategory(trimmed, label, groupCode.ToUpperInvariant(), groupLabel);
    }
}
