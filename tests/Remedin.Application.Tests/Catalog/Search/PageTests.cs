using Remedin.Application.Catalog.Search;

namespace Remedin.Application.Tests.Catalog.Search;

public class PageTests
{
    [Theory]
    [InlineData(1, 0)]
    [InlineData(2, 20)]
    [InlineData(11, 200)]
    public void Pagina_vira_deslocamento(int number, int expected)
    {
        Assert.Equal(expected, Page.Of(number).Offset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Pagina_abaixo_de_um_vira_a_primeira(int number)
    {
        // O número vem da barra de endereço, onde qualquer um digita qualquer
        // coisa. Corrigir serve melhor que recusar.
        Assert.Equal(1, Page.Of(number).Number);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(500, Page.MaximumSize)]
    public void Tamanho_fica_dentro_da_faixa(int size, int expected)
    {
        Assert.Equal(expected, Page.Of(1, size).Size);
    }

    [Fact]
    public void Pagina_do_meio_sabe_que_tem_vizinhas()
    {
        var results = PagedResults<string>.Of(Items(20), total: 206, Page.Of(2));

        Assert.True(results.HasPrevious);
        Assert.True(results.HasNext);
        Assert.Equal(21, results.FirstPosition);
        Assert.Equal(40, results.LastPosition);
    }

    [Fact]
    public void Ultima_pagina_nao_oferece_proxima()
    {
        var results = PagedResults<string>.Of(Items(6), total: 206, Page.Of(11));

        Assert.True(results.HasPrevious);
        Assert.False(results.HasNext);
        Assert.Equal(201, results.FirstPosition);
        Assert.Equal(206, results.LastPosition);
    }

    [Fact]
    public void Pagina_vazia_nao_tem_intervalo()
    {
        // Pedir a página 99 de uma lista de 206: o deslocamento sozinho diria
        // "0 a 1960", um intervalo que não existe.
        var results = PagedResults<string>.Of([], total: 0, Page.Of(99));

        Assert.Equal(0, results.FirstPosition);
        Assert.Equal(0, results.LastPosition);
        Assert.False(results.HasNext);
    }

    private static string[] Items(int count) => [.. Enumerable.Range(1, count).Select(n => $"item {n}")];
}
