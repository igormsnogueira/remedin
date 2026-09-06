using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class BrazilianStatesTests
{
    [Fact]
    public void Toda_sigla_com_nome_tem_aliquota_e_o_contrario_tambem()
    {
        // As duas tabelas são listas de 27 escritas à mão, em arquivos
        // diferentes. Acrescentar um estado numa e esquecer da outra derrubaria
        // o seletor da interface, ou publicaria preço sem alíquota.
        Assert.Equal(
            BrazilianStates.Codes.Order(),
            IcmsRates.States.Order());
    }

    [Fact]
    public void Sao_vinte_e_sete_unidades_da_federacao()
    {
        Assert.Equal(27, BrazilianStates.Codes.Count);
    }

    [Theory]
    [InlineData("SP", "São Paulo")]
    [InlineData("sp", "São Paulo")]
    [InlineData(" RJ ", "Rio de Janeiro")]
    public void Sigla_e_lida_sem_depender_de_caixa_nem_de_espaco(string code, string expected)
    {
        Assert.Equal(expected, BrazilianStates.NameOf(code));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("XX")]
    public void Sigla_desconhecida_nao_devolve_nome(string? code)
    {
        Assert.False(BrazilianStates.TryGetName(code, out _));
    }
}
