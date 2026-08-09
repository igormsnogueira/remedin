using Remedin.Domain.Medicines;

namespace Remedin.Domain.Tests.Medicines;

public class RegistrationNumberTests
{
    [Theory]
    [InlineData("102980592", "102980592")]      // formato da base de registro
    [InlineData("1705600230032", "170560023")]  // formato da lista de preços
    [InlineData("1.0298.0592", "102980592")]    // com pontuação
    [InlineData("  102980592  ", "102980592")]  // com espaço em volta
    public void Parse_normaliza_os_formatos_das_duas_fontes(string raw, string expected)
    {
        var number = RegistrationNumber.Parse(raw);

        Assert.Equal(expected, number.Value);
    }

    [Fact]
    public void O_registro_da_cmed_e_o_da_anvisa_apontam_para_o_mesmo_medicamento()
    {
        var fromPriceList = RegistrationNumber.Parse("1705600230032");
        var fromRegistry = RegistrationNumber.Parse("170560023");

        Assert.Equal(fromRegistry, fromPriceList);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("12345678")]     // oito dígitos: a análise confirmou que é lixo,
    [InlineData("1234567890")]   // e não zero à esquerda perdido
    [InlineData("12345678901")]
    public void TryParse_recusa_valor_que_nao_e_registro(string? raw)
    {
        var parsed = RegistrationNumber.TryParse(raw, out var number);

        Assert.False(parsed);
        Assert.Null(number);
    }

    [Fact]
    public void Parse_falha_alto_em_valor_invalido()
    {
        var exception = Assert.Throws<ArgumentException>(() => RegistrationNumber.Parse("12345678"));

        Assert.Contains("12345678", exception.Message);
    }

    [Fact]
    public void Numeros_iguais_sao_o_mesmo_valor()
    {
        var one = RegistrationNumber.Parse("102980592");
        var other = RegistrationNumber.Parse("102980592");

        Assert.Equal(one, other);
        Assert.Equal(one.GetHashCode(), other.GetHashCode());
    }

    [Fact]
    public void Numeros_diferentes_nao_se_confundem()
    {
        var one = RegistrationNumber.Parse("102980592");
        var other = RegistrationNumber.Parse("112130229");

        Assert.NotEqual(one, other);
    }

    [Fact]
    public void ToString_devolve_so_os_digitos()
    {
        var number = RegistrationNumber.Parse("1.0298.0592");

        Assert.Equal("102980592", number.ToString());
    }
}
