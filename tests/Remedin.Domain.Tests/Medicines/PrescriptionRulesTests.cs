using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class PrescriptionRulesTests
{
    [Theory]
    [InlineData("Tarja Sem Tarja", "Venda livre, sem receita", false)]
    [InlineData("Tarja Vermelha", "Exige receita", true)]
    [InlineData("Tarja Vermelha sob restrição", "Exige receita, retida na farmácia", true)]
    [InlineData("Tarja Preta", "Exige receita de controle especial", true)]
    public void Traduz_a_tarja_publicada(string band, string label, bool requiresPrescription)
    {
        var rule = PrescriptionRules.Describe(band)!;

        Assert.Equal(label, rule.Label);
        Assert.Equal(requiresPrescription, rule.RequiresPrescription);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Tarja Roxa")]
    public void Tarja_ausente_ou_desconhecida_nao_vira_venda_livre(string? band)
    {
        // Dizer "não exige receita" sem a fonte confirmar seria pior que não
        // dizer nada: alguém pode ir à farmácia contando com isso.
        Assert.Null(PrescriptionRules.Describe(band));
    }
}
