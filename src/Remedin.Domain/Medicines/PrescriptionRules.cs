namespace Remedin.Domain.Medicines;

/// <summary>
/// O que a tarja significa para quem vai à farmácia.
/// </summary>
/// <param name="RequiresPrescription">
/// Nulo quando a fonte não informa, o que acontece em parte do catálogo. Nulo
/// e "não exige" são coisas diferentes, e a interface precisa distinguir.
/// </param>
public sealed record PrescriptionRule(string Label, bool? RequiresPrescription);

/// <summary>
/// Traduz a tarja publicada pela CMED.
///
/// Os valores vêm como a fonte escreve, inclusive "Tarja Sem Tarja", que é
/// redundante e ruim de ler. A tradução é da interface; o valor original
/// continua guardado, porque é o que a fonte oficial diz.
/// </summary>
public static class PrescriptionRules
{
    private static readonly Dictionary<string, PrescriptionRule> ByBand =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["Tarja Sem Tarja"] = new("Venda livre, sem receita", false),
            ["Tarja Vermelha"] = new("Exige receita", true),
            ["Tarja Vermelha sob restrição"] = new("Exige receita, retida na farmácia", true),
            ["Tarja Preta"] = new("Exige receita de controle especial", true),
        };

    public static PrescriptionRule? Describe(string? band) =>
        band is not null && ByBand.TryGetValue(band.Trim(), out var rule) ? rule : null;
}
