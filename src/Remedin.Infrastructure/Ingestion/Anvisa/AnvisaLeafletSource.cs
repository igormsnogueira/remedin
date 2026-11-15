using System.Security.Cryptography;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

public sealed class AnvisaLeafletSource(HttpClient http, AnvisaLeafletReader reader) : ILeafletSource
{
    public static readonly Uri FileUrl =
        new("https://dados.anvisa.gov.br/dados/CONSULTAS/DOCUMENTOS/TA_CONSULTA_BULA_PRODUTO.CSV");

    public async Task<LeafletSnapshot> FetchAsync(CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(FileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var content = new MemoryStream();
        await response.Content.CopyToAsync(content, cancellationToken);

        content.Position = 0;
        var hash = Convert.ToHexStringLower(await SHA256.HashDataAsync(content, cancellationToken));

        content.Position = 0;
        return reader.Read(content, hash);
    }
}
