using System.Text;
using Remedin.Infrastructure.Ingestion.Cmed;

namespace Remedin.Infrastructure.Tests.Ingestion.Cmed;

public class CmedPriceReaderTests
{
    private const string Header =
        "SUBSTÂNCIA;CNPJ;LABORATÓRIO;CÓDIGO GGREM;REGISTRO;EAN 1;PRODUTO;APRESENTAÇÃO;" +
        "CLASSE TERAPÊUTICA;PF Sem Impostos;PF 0%;PF 18 %;PF 18 %  ALC;PMC 18 %;PMC 22,5 %;" +
        "RESTRIÇÃO HOSPITALAR;COMERCIALIZAÇÃO 2025;TARJA";

    /// <summary>
    /// A lista real traz 41 linhas de texto jurídico antes do cabeçalho.
    /// Reproduzir isso é o que faz o teste cobrir a detecção.
    /// </summary>
    private static readonly string[] Preamble =
    [
        "Secretaria Executiva - CMED;;;;;;;;;;;;;;;;;",
        "LISTA DE PREÇOS DE MEDICAMENTOS;;;;;;;;;;;;;;;;;",
        "Publicada em 21/07/2026 17h30min.;;;;;;;;;;;;;;;;;",
        ";;;;;;;;;;;;;;;;;",
        "\"O campo \"\"PF sem impostos\"\" corresponde à ausência de PIS/Cofins.\";;;;;;;;;;;;;;;;;",
    ];

    private static Stream Csv(params string[] rows) =>
        new MemoryStream(Encoding.UTF8.GetBytes(
            string.Join("\r\n", [.. Preamble, Header, .. rows])));

    private static string Row(
        string registration = "1705600230032",
        string ggrem = "538912020009303",
        string product = "DIPIRONA",
        string substance = "dipirona monoidratada",
        string therapeuticClass = "N2B - ANALGÉSICOS NÃO NARCÓTICOS",
        string consumerPrice = "27,44",
        string hospitalOnly = "Não",
        string soldRecently = "Sim",
        string band = "Tarja Vermelha") =>
        $"{substance};18.459.628/0001-15;BAYER S.A.;{ggrem};{registration};7891106000956;" +
        $"{product};500 MG COM CT BL AL PLAS INC X 10;{therapeuticClass};" +
        $"18,10;19,00;22,08;21,00;{consumerPrice};29,15;" +
        $"{hospitalOnly};{soldRecently};{band}";

    private static readonly CmedPriceReader Reader = new();

    [Fact]
    public void Encontra_o_cabecalho_depois_do_texto_juridico()
    {
        var result = Reader.ReadRows(Csv(Row()));

        Assert.Equal(Preamble.Length, result.PreambleLines);
        Assert.Single(result.Rows);
        Assert.Equal(1, result.RowsRead);
    }

    [Fact]
    public void Usa_os_nove_primeiros_digitos_do_registro()
    {
        var result = Reader.ReadRows(Csv(Row(registration: "1705600230032")));

        Assert.Equal("170560023", result.Rows[0].Registration.Value);
    }

    [Fact]
    public void Interpreta_preco_com_virgula_decimal()
    {
        // Com cultura invariável, "27,44" viraria 2744 e o site mostraria
        // preço cem vezes maior.
        var result = Reader.ReadRows(Csv(Row(consumerPrice: "1.234,56")));

        var consumer = result.Rows[0].Prices.First(p => p.Kind == PriceKind.Consumer && p.Aliquot == 18m);
        Assert.Equal(1234.56m, consumer.Value);
    }

    [Fact]
    public void Separa_os_precos_por_tipo_aliquota_e_zona_franca()
    {
        var prices = Reader.ReadRows(Csv(Row())).Rows[0].Prices;

        Assert.Contains(prices, p => p.Kind == PriceKind.Factory && p.Aliquot is null);
        Assert.Contains(prices, p => p.Kind == PriceKind.Factory && p.Aliquot == 0m);
        Assert.Contains(prices, p => p.Kind == PriceKind.Factory && p.Aliquot == 18m && !p.FreeTradeZone);
        Assert.Contains(prices, p => p.Kind == PriceKind.Factory && p.Aliquot == 18m && p.FreeTradeZone);
        Assert.Contains(prices, p => p.Kind == PriceKind.Consumer && p.Aliquot == 22.5m);
        Assert.Equal(6, prices.Count);
    }

    [Fact]
    public void Separa_codigo_e_descricao_da_classe_terapeutica()
    {
        var row = Reader.ReadRows(Csv(Row())).Rows[0];

        Assert.Equal("N2B", row.TherapeuticClassCode);
        Assert.Equal("ANALGÉSICOS NÃO NARCÓTICOS", row.TherapeuticClassName);
    }

    [Theory]
    [InlineData("-")]
    [InlineData("- (*)")]
    public void Preenchimento_vira_ausencia_de_informacao(string placeholder)
    {
        var row = Reader.ReadRows(Csv(Row(band: placeholder))).Rows[0];

        Assert.Null(row.PrescriptionBand);
    }

    [Fact]
    public void Le_os_indicadores_de_balcao_e_comercializacao()
    {
        var hospital = Reader.ReadRows(Csv(Row(hospitalOnly: "Sim", soldRecently: "Não"))).Rows[0];

        Assert.True(hospital.HospitalOnly);
        Assert.False(hospital.SoldRecently);
    }

    [Fact]
    public void Preco_vazio_nao_vira_zero()
    {
        // Produto hospitalar não tem preço ao consumidor. Gravar zero diria
        // que é de graça.
        var result = Reader.ReadRows(Csv(Row(consumerPrice: "")));

        Assert.DoesNotContain(
            result.Rows[0].Prices,
            p => p.Kind == PriceKind.Consumer && p.Aliquot == 18m);
    }

    [Fact]
    public void Substancia_com_ponto_e_virgula_dentro_nao_desalinha_a_linha()
    {
        // A CMED escreve associação como "A;B;C" entre aspas. Quebrar a linha
        // por ";" na mão jogaria todos os campos seguintes para o lugar errado.
        var result = Reader.ReadRows(Csv(Row(substance: "\"NICOTINAMIDA;DEXPANTENOL\"")));

        var row = result.Rows[0];
        Assert.Equal("NICOTINAMIDA;DEXPANTENOL", row.Substance);
        Assert.Equal("DIPIRONA", row.Product);
        Assert.Equal("170560023", row.Registration.Value);
    }

    [Fact]
    public void Recusa_linha_sem_registro_sem_parar_a_leitura()
    {
        var result = Reader.ReadRows(Csv(
            Row(registration: ""),
            Row(registration: "1018003900019")));

        Assert.Single(result.Rows);
        Assert.Single(result.Rejected);
        Assert.Equal(2, result.RowsRead);
    }

    [Fact]
    public void Falha_alto_se_o_layout_da_origem_mudar()
    {
        var content = new MemoryStream(Encoding.UTF8.GetBytes(
            "SUBSTÂNCIA;LABORATÓRIO\r\ndipirona;BAYER"));

        var exception = Assert.Throws<InvalidDataException>(() => Reader.ReadRows(content));

        Assert.Contains("REGISTRO", exception.Message);
    }
}
