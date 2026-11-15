namespace Remedin.Domain.Medicines;

public sealed record TherapeuticCategory(string Code, string Label, string GroupCode, string GroupLabel);

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
    };

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
        ["A4A1"] = "Enjoo, náusea e vômito",
        ["A4A9"] = "Enjoo, náusea e vômito",
        ["B1F"] = "Anticoagulante",
        ["N1A2"] = "Anestesia",
        ["N7D1"] = "Alzheimer",
    };

    public static IReadOnlyList<TherapeuticCategory> Groups =>
    [
        .. GroupLabels
            .Select(group => new TherapeuticCategory(group.Key, group.Value, group.Key, group.Value))
            .OrderBy(category => category.Label, StringComparer.CurrentCulture)
    ];

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
