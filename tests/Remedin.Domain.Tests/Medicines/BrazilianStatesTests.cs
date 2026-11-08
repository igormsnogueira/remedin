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

    [Theory]
    [InlineData("São Paulo", "SP")]
    [InlineData("Sao Paulo", "SP")]
    [InlineData("SANTA CATARINA", "SC")]
    [InlineData(" Espirito Santo ", "ES")]
    [InlineData("Federal District", "DF")]
    [InlineData("Distrito Federal", "DF")]
    public void Nome_do_estado_vira_sigla_com_ou_sem_acento(string name, string expected)
    {
        // A base de geolocalização por IP escreve os nomes sem acento, e o
        // Distrito Federal em inglês.
        Assert.True(BrazilianStates.TryFindByName(name, out var code));
        Assert.Equal(expected, code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("California")]
    public void Nome_desconhecido_nao_vira_sigla(string? name)
    {
        Assert.False(BrazilianStates.TryFindByName(name, out _));
    }

    [Fact]
    public void Todo_nome_oficial_volta_para_a_propria_sigla()
    {
        foreach (var code in BrazilianStates.Codes)
        {
            Assert.True(BrazilianStates.TryFindByName(BrazilianStates.NameOf(code), out var found));
            Assert.Equal(code, found);
        }
    }
}
