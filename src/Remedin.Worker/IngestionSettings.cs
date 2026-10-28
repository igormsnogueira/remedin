using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Worker;

/// <summary>
/// Configuração de uma execução do worker, lida da seção "Ingestion".
///
/// Em produção vem de variável de ambiente: <c>Ingestion__ReloadRepeated=true</c>
/// faz a carga reprocessar arquivos que já foram carregados.
/// </summary>
public sealed class IngestionSettings
{
    public const string Section = "Ingestion";

    public bool ReloadRepeated { get; init; }

    public RepeatedPublication WhenRepeated =>
        ReloadRepeated ? RepeatedPublication.Reload : RepeatedPublication.Skip;
}
