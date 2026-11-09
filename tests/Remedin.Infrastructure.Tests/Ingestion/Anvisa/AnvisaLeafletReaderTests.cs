using System.Text;
using Remedin.Infrastructure.Ingestion.Anvisa;

namespace Remedin.Infrastructure.Tests.Ingestion.Anvisa;

public class AnvisaLeafletReaderTests
{
    private static readonly AnvisaLeafletReader Reader = new();

    // Linhas copiadas da publicação de outubro de 2026.
    private const string Meclin =
        "172780;\"MECLIN\";\"101180165\";6;\"25351162067200258\";\"62462015000129\";" +
        "\"APSEN FARMACEUTICA S/A\";34546761;\"1578799252\";\"12/09/2025 06:48:52\";" +
        "\"17727362025\";\"10/12/2026 00:00:00\"";

    private const string Omeprazol =
        "400212;\"OMEPRAZOL\";\"102980354\";5;\"25351277933200476\";\"44734671000151\";" +
        "\"CRISTÁLIA PRODUTOS QUÍMICOS FARMACÊUTICOS LTDA.\";35343023;\"0739575261\";" +
        "\"07/24/2026 09:29:29\";\"9770202026\";\"10/12/2026 00:00:00\"";

    private static MemoryStream Csv(params string[] lines) =>
        new(Encoding.Latin1.GetBytes(string.Join("\r\n", lines)));

    [Fact]
    public void Le_registro_e_data_da_bula()
    {
        using var csv = Csv(Meclin, Omeprazol);

        var snapshot = Reader.ReadRows(csv, "hash");

        var meclin = Assert.Single(snapshot.Leaflets, leaflet => leaflet.RegistrationNumber == "101180165");
        Assert.Equal(new DateOnly(2025, 12, 9), meclin.PublishedOn);
        Assert.Equal(2, snapshot.Leaflets.Count);
    }

    [Fact]
    public void Data_vem_com_o_mes_antes_do_dia()
    {
        // "07/24/2026" só existe como 24 de julho. Lida como dia/mês, seria
        // recusada; em datas como 12/09 sairia trocada sem aviso.
        using var csv = Csv(Omeprazol);

        var snapshot = Reader.ReadRows(csv, "hash");

        Assert.Equal(new DateOnly(2026, 7, 24), Assert.Single(snapshot.Leaflets).PublishedOn);
    }

    [Fact]
    public void Registro_repetido_fica_com_a_bula_mais_recente()
    {
        var older = Meclin.Replace("12/09/2025", "01/15/2024", StringComparison.Ordinal);
        using var csv = Csv(older, Meclin);

        var snapshot = Reader.ReadRows(csv, "hash");

        Assert.Equal(new DateOnly(2025, 12, 9), Assert.Single(snapshot.Leaflets).PublishedOn);
        Assert.Equal(1, snapshot.Duplicates);
    }

    [Fact]
    public void Linha_sem_registro_valido_e_recusada()
    {
        var broken = Meclin.Replace("\"101180165\"", "\"\"", StringComparison.Ordinal);
        using var csv = Csv(broken, Omeprazol);

        var snapshot = Reader.ReadRows(csv, "hash");

        Assert.Single(snapshot.Leaflets);
        Assert.Equal(1, snapshot.Rejected);
    }
}
