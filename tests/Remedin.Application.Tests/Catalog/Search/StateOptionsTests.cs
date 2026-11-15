using Remedin.Application.Catalog.Search;

namespace Remedin.Application.Tests.Catalog.Search;

public class StateOptionsTests
{
    [Fact]
    public void Traz_as_vinte_e_sete_unidades_da_federacao()
    {
        Assert.Equal(27, StateOptions.All.Count);
    }

    [Fact]
    public void Ordena_por_nome_respeitando_acento()
    {
        var names = StateOptions.All.Select(option => option.Name).ToArray();

        Assert.Equal("Acre", names[0]);
        Assert.Equal("Espírito Santo", names[Array.IndexOf(names, "Goiás") - 1]);
        Assert.Equal("Tocantins", names[^1]);
    }

    [Fact]
    public void Cada_estado_vem_com_a_aliquota_que_define_o_preco_teto()
    {
        var rioDeJaneiro = StateOptions.All.Single(option => option.Code == "RJ");

        Assert.Equal("Rio de Janeiro", rioDeJaneiro.Name);
        Assert.Equal(20m, rioDeJaneiro.IcmsRate);
    }
}
