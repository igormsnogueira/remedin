using System.Net;
using System.Text;
using Remedin.Infrastructure.Ingestion.DbIp;

namespace Remedin.Infrastructure.Tests.Ingestion.DbIp;

public class DbIpStateSourceTests
{
    private static MemoryStream Csv(params string[] lines) =>
        new(Encoding.UTF8.GetBytes(string.Join('\n', lines)));

    [Fact]
    public async Task Fica_so_com_as_faixas_do_brasil_e_le_a_sigla_do_estado()
    {
        using var csv = Csv(
            "8.8.8.0,8.8.8.255,NA,US,California,\"Mountain View\",37.4229,-122.085",
            "177.0.0.0,177.0.0.255,SA,BR,\"Santa Catarina\",Florianópolis,-27.59,-48.54",
            "189.0.0.0,189.0.0.255,SA,BR,\"Sao Paulo\",\"São Paulo\",-23.55,-46.63");

        var snapshot = await DbIpStateSource.ReadAsync("2026-11", csv, minimumRows: 1, CancellationToken.None);

        Assert.Equal(2, snapshot.Ranges.Count);
        Assert.Equal("SC", snapshot.Ranges[0].State);
        Assert.Equal("SP", snapshot.Ranges[1].State);
        Assert.Equal(IPAddress.Parse("177.0.0.0"), snapshot.Ranges[0].Start);
    }

    [Fact]
    public async Task Junta_faixas_vizinhas_do_mesmo_estado()
    {
        // Cidades diferentes do mesmo estado vêm em linhas seguidas. Para
        // descobrir o estado, são uma faixa só.
        using var csv = Csv(
            "177.0.0.0,177.0.0.255,SA,BR,\"Santa Catarina\",Florianópolis,0,0",
            "177.0.1.0,177.0.1.255,SA,BR,\"Santa Catarina\",Joinville,0,0",
            "177.0.2.0,177.0.2.255,SA,BR,Paraná,Curitiba,0,0");

        var snapshot = await DbIpStateSource.ReadAsync("2026-11", csv, minimumRows: 1, CancellationToken.None);

        Assert.Equal(2, snapshot.Ranges.Count);
        Assert.Equal(IPAddress.Parse("177.0.1.255"), snapshot.Ranges[0].End);
        Assert.Equal(3, snapshot.RowsRead);
    }

    [Fact]
    public async Task Nao_junta_faixas_do_mesmo_estado_com_buraco_no_meio()
    {
        using var csv = Csv(
            "177.0.0.0,177.0.0.255,SA,BR,\"Santa Catarina\",Florianópolis,0,0",
            "177.0.5.0,177.0.5.255,SA,BR,\"Santa Catarina\",Joinville,0,0");

        var snapshot = await DbIpStateSource.ReadAsync("2026-11", csv, minimumRows: 1, CancellationToken.None);

        Assert.Equal(2, snapshot.Ranges.Count);
    }

    [Fact]
    public async Task Conta_estado_nao_reconhecido_sem_derrubar_a_carga()
    {
        using var csv = Csv(
            "177.0.0.0,177.0.0.255,SA,BR,\"Santa Catarina\",Florianópolis,0,0",
            "177.0.9.0,177.0.9.255,SA,BR,,,0,0");

        var snapshot = await DbIpStateSource.ReadAsync("2026-11", csv, minimumRows: 1, CancellationToken.None);

        Assert.Single(snapshot.Ranges);
        Assert.Equal(1, snapshot.Unmapped);
    }

    [Fact]
    public async Task Recusa_arquivo_com_poucas_faixas_do_brasil()
    {
        // Arquivo truncado trocaria a tabela boa por uma quase vazia.
        using var csv = Csv("177.0.0.0,177.0.0.255,SA,BR,\"Santa Catarina\",Florianópolis,0,0");

        await Assert.ThrowsAsync<InvalidDataException>(
            () => DbIpStateSource.ReadAsync("2026-11", csv, minimumRows: 100, CancellationToken.None));
    }
}
