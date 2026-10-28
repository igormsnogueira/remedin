using Microsoft.Extensions.Options;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Worker;

/// <summary>
/// Executa as cargas em sequência e encerra o processo.
///
/// A ordem importa: o preço se liga ao medicamento pelo número de registro,
/// então carregar o registro primeiro é o que evita a lista inteira cair como
/// órfã na primeira execução.
///
/// O agendamento fica fora daqui, no workflow do GitHub Actions. O processo só
/// sabe rodar uma vez e sair, com código diferente de zero quando falha — é o
/// que o agendador lê para avisar.
/// </summary>
public sealed class CatalogIngestionWorker(
    IServiceProvider services,
    IOptions<IngestionSettings> settings,
    IHostApplicationLifetime lifetime,
    ILogger<CatalogIngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var whenRepeated = settings.Value.WhenRepeated;

        try
        {
            // Os comandos dependem do DbContext, que é scoped; um serviço de
            // fundo é singleton, então o escopo é criado aqui.
            using var scope = services.CreateScope();

            var registry = scope.ServiceProvider.GetRequiredService<ImportRegistrySnapshot>();
            var registryRun = await registry.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Registro: execução {Id} terminou como {Outcome}.",
                registryRun.Id, registryRun.Outcome);

            var prices = scope.ServiceProvider.GetRequiredService<ImportPriceList>();
            var priceRun = await prices.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Preço: execução {Id} terminou como {Outcome}.",
                priceRun.Id, priceRun.Outcome);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Carga cancelada.");
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Carga terminou com erro.");
            Environment.ExitCode = 1;
        }
        finally
        {
            lifetime.StopApplication();
        }
    }
}
