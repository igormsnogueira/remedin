using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class IcmsRatesTests
{
    [Fact]
    public void As_aliquotas_servidas_sao_as_da_tabela_de_estados()
    {
        // Quatro valores para 27 estados. É o que a carga usa para descartar
        // as outras alíquotas publicadas pela CMED.
        Assert.Equal([17m, 17.5m, 18m, 20m], IcmsRates.Served.Order());
    }

    [Theory]
    [InlineData(18)]
    [InlineData(20)]
    public void Reconhece_aliquota_de_algum_estado(decimal rate)
    {
        Assert.True(IcmsRates.Serves(rate));
    }

    [Theory]
    [InlineData(12)]
    [InlineData(23)]
    [InlineData(0)]
    public void Recusa_aliquota_que_nenhum_estado_usa(decimal rate)
    {
        Assert.False(IcmsRates.Serves(rate));
    }

    [Fact]
    public void Recusa_preco_sem_imposto()
    {
        // A coluna "sem impostos" da CMED chega com alíquota nula, e nenhuma
        // consulta a alcança: em SQL, NULL = 18 não é verdadeiro.
        Assert.False(IcmsRates.Serves(null));
    }

    [Fact]
    public void Toda_aliquota_servida_pertence_a_algum_estado()
    {
        var fromStates = IcmsRates.States.Select(IcmsRates.For).Distinct();

        Assert.Equal(IcmsRates.Served.Order(), fromStates.Order());
    }
}
