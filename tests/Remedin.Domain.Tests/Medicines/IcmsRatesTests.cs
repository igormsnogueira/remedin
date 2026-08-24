using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class IcmsRatesTests
{
    [Theory]
    [InlineData("RJ", 20)]
    [InlineData("RO", 17.5)]
    [InlineData("SP", 18)]
    [InlineData("MG", 18)]
    [InlineData("AM", 18)]
    [InlineData("SC", 17)]
    [InlineData("DF", 17)]
    [InlineData("AC", 17)]
    public void Devolve_a_aliquota_do_estado(string state, decimal expected)
    {
        Assert.Equal(expected, IcmsRates.For(state));
    }

    [Fact]
    public void Aceita_a_sigla_em_minuscula()
    {
        Assert.Equal(20m, IcmsRates.For("rj"));
    }

    [Fact]
    public void Cobre_as_vinte_e_sete_unidades_da_federacao()
    {
        // Sem isso, um estado esquecido cairia numa alíquota padrão silenciosa
        // e o site publicaria teto errado para quem mora lá.
        Assert.Equal(27, IcmsRates.States.Count);
    }

    [Theory]
    [InlineData("XX")]
    [InlineData("")]
    [InlineData(null)]
    public void Estado_desconhecido_nao_devolve_padrao(string? state)
    {
        Assert.False(IcmsRates.TryGet(state, out _));
    }

    [Fact]
    public void Estado_desconhecido_falha_alto_no_acesso_direto()
    {
        Assert.Throws<ArgumentException>(() => IcmsRates.For("XX"));
    }
}
