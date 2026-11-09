using System.Security.Cryptography;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

/// <summary>Baixa e interpreta a lista de bulas dos dados abertos da ANVISA.</summary>
public sealed class AnvisaLeafletSource(HttpClient http, AnvisaLeafletReader reader) : ILeafletSource
{
    public static readonly Uri FileUrl =
        new("https://dados.anvisa.gov.br/dados/CONSULTAS/DOCUMENTOS/TA_CONSULTA_BULA_PRODUTO.CSV");

    public async Task<LeafletSnapshot> FetchAsync(CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(FileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        // 1,6 MB: cabe em memória, e o conteúdo posicionável permite calcular
        // o hash antes de ler.
        using var content = new MemoryStream();
        await response.Content.CopyToAsync(content, cancellationToken);

        content.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(content, cancellationToken));

        content.Position = 0;
        return reader.Read(content, hash);
    }
}
