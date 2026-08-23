using System.Text;
using Remedin.Infrastructure.Ingestion;

namespace Remedin.Infrastructure.Tests.Ingestion;

public class SourceFileTests
{
    private static MemoryStream Content(string text, int padToBytes = 0)
    {
        var bytes = Encoding.Latin1.GetBytes(text.PadRight(Math.Max(text.Length, padToBytes), ' '));
        return new MemoryStream(bytes);
    }

    [Fact]
    public void Aceita_csv_com_tamanho_esperado()
    {
        using var content = Content("COLUNA_A;COLUNA_B\r\nvalor;valor", padToBytes: 2_000);

        SourceFile.EnsureLooksLikeCsv(content, minimumBytes: 1_000);
    }

    [Fact]
    public void Recusa_arquivo_menor_que_o_minimo()
    {
        using var content = Content("COLUNA_A;COLUNA_B");

        var exception = Assert.Throws<InvalidDataException>(
            () => SourceFile.EnsureLooksLikeCsv(content, minimumBytes: 1_000_000));

        Assert.Contains("truncado", exception.Message);
    }

    [Theory]
    [InlineData("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML 2.0//EN\">")]
    [InlineData("<html><head><title>404 Not Found</title></head>")]
    [InlineData("\r\n  <HTML>")]
    public void Recusa_pagina_html_disfarcada_de_csv(string html)
    {
        // A origem responde com página de erro quando a URL muda, e o corpo
        // seria gravado como uma linha de lixo sem esta checagem.
        using var content = Content(html, padToBytes: 2_000);

        var exception = Assert.Throws<InvalidDataException>(
            () => SourceFile.EnsureLooksLikeCsv(content, minimumBytes: 1_000));

        Assert.Contains("HTML", exception.Message);
    }

    [Fact]
    public void Nao_consome_o_conteudo_ao_validar()
    {
        using var content = Content("COLUNA_A;COLUNA_B\r\nvalor;valor", padToBytes: 2_000);

        SourceFile.EnsureLooksLikeCsv(content, minimumBytes: 1_000);

        Assert.Equal(0, content.Position);
    }
}
