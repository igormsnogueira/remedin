using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class SubstanceKeyTests
{
    [Fact]
    public void A_mesma_associacao_em_ordem_diferente_gera_a_mesma_chave()
    {
        // Os dois existem no catálogo, escritos assim.
        var first = SubstanceKey.From("CITRATO DE ORFENADRINA;CAFEÍNA ANIDRA;DIPIRONA MONOIDRATADA");
        var second = SubstanceKey.From("DIPIRONA MONOIDRATADA;CITRATO DE ORFENADRINA;CAFEÍNA ANIDRA");

        Assert.Equal(first, second);
    }

    [Fact]
    public void Acentuacao_nao_separa_equivalentes()
    {
        Assert.Equal(SubstanceKey.From("CAFEÍNA ANIDRA"), SubstanceKey.From("CAFEINA ANIDRA"));
    }

    [Fact]
    public void Espaco_duplicado_nao_separa_equivalentes()
    {
        // "CLORIDRATO  DE ONDANSETRONA DI-HIDRATADO" aparece assim na origem.
        Assert.Equal(
            SubstanceKey.From("CLORIDRATO DE ONDANSETRONA"),
            SubstanceKey.From("CLORIDRATO  DE ONDANSETRONA"));
    }

    [Fact]
    public void Caixa_nao_separa_equivalentes()
    {
        Assert.Equal(SubstanceKey.From("ibuprofeno"), SubstanceKey.From("IBUPROFENO"));
    }

    [Fact]
    public void Componente_repetido_na_origem_conta_uma_vez()
    {
        // "DIPIRONA;DIPIRONA" é erro da fonte.
        Assert.Equal(SubstanceKey.From("DIPIRONA"), SubstanceKey.From("DIPIRONA;DIPIRONA"));
    }

    [Fact]
    public void Sal_e_hidratacao_continuam_separados()
    {
        // Decisão da ADR 0010: agrupar de mais é pior que agrupar de menos,
        // porque alguém pode comprar a coisa errada.
        Assert.NotEqual(SubstanceKey.From("DIPIRONA"), SubstanceKey.From("DIPIRONA MONOIDRATADA"));
    }

    [Fact]
    public void Sinonimo_continua_separado()
    {
        Assert.NotEqual(
            SubstanceKey.From("MALEATO DE CLORFENAMINA"),
            SubstanceKey.From("MALEATO DE CLORFENIRAMINA"));
    }

    [Fact]
    public void Substancias_diferentes_nao_se_confundem()
    {
        Assert.NotEqual(SubstanceKey.From("IBUPROFENO"), SubstanceKey.From("PARACETAMOL"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(";;;")]
    public void Sem_principio_ativo_nao_ha_chave(string? activeIngredient)
    {
        Assert.Null(SubstanceKey.From(activeIngredient));
    }
}
