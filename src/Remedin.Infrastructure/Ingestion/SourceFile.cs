using System.Text;

namespace Remedin.Infrastructure.Ingestion;

/// <summary>
/// Checagens no arquivo inteiro, antes de olhar qualquer linha.
///
/// Existem porque os dois defeitos mais prováveis não geram exceção sozinhos:
/// a origem devolve uma página de erro em HTML quando a URL muda, e um
/// download interrompido produz um CSV válido, só que pela metade.
/// </summary>
public static class SourceFile
{
    private static readonly string[] HtmlMarkers = ["<!doctype", "<html"];

    private const int MarkerSampleBytes = 512;

    public static void EnsureLooksLikeCsv(Stream content, long minimumBytes)
    {
        ArgumentNullException.ThrowIfNull(content);

        if (!content.CanSeek)
        {
            throw new ArgumentException(
                "A validação precisa reposicionar o conteúdo; use um stream com seek.",
                nameof(content));
        }

        if (content.Length < minimumBytes)
        {
            throw new InvalidDataException(
                $"Arquivo tem {content.Length} bytes, abaixo do mínimo de {minimumBytes}. " +
                "Download provavelmente truncado.");
        }

        var marker = ReadMarker(content);

        if (HtmlMarkers.Any(html => marker.StartsWith(html, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidDataException(
                "O conteúdo é HTML, não CSV. A origem costuma responder com página " +
                "de erro quando a URL muda.");
        }
    }

    private static string ReadMarker(Stream content)
    {
        var position = content.Position;
        content.Position = 0;

        var buffer = new byte[Math.Min(MarkerSampleBytes, content.Length)];
        content.ReadExactly(buffer);
        content.Position = position;

        return Encoding.Latin1.GetString(buffer).TrimStart('﻿', ' ', '\r', '\n', '\t');
    }
}
