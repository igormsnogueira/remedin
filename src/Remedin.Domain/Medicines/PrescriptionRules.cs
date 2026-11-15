namespace Remedin.Domain.Medicines;

// RequiresPrescription nulo: a fonte não informa a tarja, o que é diferente de não exigir receita.
public sealed record PrescriptionRule(string Label, bool? RequiresPrescription);

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
