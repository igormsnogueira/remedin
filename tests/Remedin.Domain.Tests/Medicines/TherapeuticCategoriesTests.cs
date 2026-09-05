using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class TherapeuticCategoriesTests
{
    [Theory]
    [InlineData("M1A1", "Anti-inflamatório para dor e inflamação")]
    [InlineData("A2B2", "Azia, refluxo e gastrite")]
    [InlineData("R6A", "Alergia")]
    public void Classe_conhecida_ganha_descricao_especifica(string code, string expected)
    {
        Assert.Equal(expected, TherapeuticCategories.Describe(code)!.Label);
    }

    [Theory]
    [InlineData("M9Z9", "Músculos, ossos e articulações")]
    [InlineData("N7X", "Sistema nervoso e dor")]
    [InlineData("A10L", "Estômago, intestino e diabetes")]
    public void Classe_sem_traducao_cai_na_descricao_do_grupo(string code, string expected)
    {
        // São 528 classes e as mais comuns cobrem 42% do catálogo. O grupo
        // garante que nenhum medicamento fique sem descrição.
        Assert.Equal(expected, TherapeuticCategories.Describe(code)!.Label);
    }

    [Fact]
    public void A_descricao_carrega_o_grupo_junto()
    {
        var category = TherapeuticCategories.Describe("C10A1")!;

        Assert.Equal("Colesterol alto", category.Label);
        Assert.Equal("C", category.GroupCode);
        Assert.Equal("Coração e pressão", category.GroupLabel);
    }

    [Theory]
    [InlineData("m1a1")]
    [InlineData(" M1A1 ")]
    public void Aceita_variacao_de_caixa_e_espaco(string code)
    {
        Assert.NotNull(TherapeuticCategories.Describe(code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("F1A")]
    [InlineData("Z9")]
    public void Codigo_sem_traducao_devolve_ausencia(string? code)
    {
        // Nulo em vez de chute: a interface mostra o nome técnico da fonte,
        // que é impreciso mas verdadeiro.
        Assert.Null(TherapeuticCategories.Describe(code));
    }

    [Fact]
    public void Os_grupos_cobrem_a_navegacao_por_finalidade()
    {
        var groups = TherapeuticCategories.Groups;

        Assert.Equal(16, groups.Count);
        Assert.All(groups, group => Assert.False(string.IsNullOrWhiteSpace(group.Label)));
        Assert.Equal(groups.Select(g => g.Code).Distinct().Count(), groups.Count);
    }
}
