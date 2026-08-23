using System.Security.Cryptography;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Infrastructure.Ingestion.Anvisa;

/// <summary>
/// Baixa e interpreta a publicação corrente da base de registro da ANVISA.
/// </summary>
public sealed class AnvisaRegistrySnapshotSource(HttpClient http, AnvisaRegistryReader reader)
    : IRegistrySnapshotSource
{
    public static readonly Uri FileUrl =
        new("https://dados.anvisa.gov.br/dados/DADOS_ABERTOS_MEDICAMENTOS.csv");

    public async Task<RegistrySnapshot> FetchAsync(CancellationToken cancellationToken)
    {
        using var response = await http.GetAsync(FileUrl, cancellationToken);
        response.EnsureSuccessStatusCode();

        // O arquivo tem 7,9 MB: cabe em memória, e ter o conteúdo posicionável
        // é o que permite validar antes de ler e calcular o hash depois.
        using var content = new MemoryStream();
        await response.Content.CopyToAsync(content, cancellationToken);
        content.Position = 0;

        var result = reader.Read(content);

        content.Position = 0;
        var hash = await ComputeHashAsync(content, cancellationToken);

        return new RegistrySnapshot(
            result.Medicines,
            result.RowsRead,
            result.Rejected.Count,
            result.Duplicates,
            hash);
    }

    private static async Task<string> ComputeHashAsync(Stream content, CancellationToken cancellationToken)
    {
        var hash = await SHA256.HashDataAsync(content, cancellationToken);
        return Convert.ToHexStringLower(hash);
    }
}
