using Microsoft.Extensions.Options;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Application.Location;

namespace Remedin.Worker;

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
            using var scope = services.CreateScope();

            // Registro antes do preço: o preço se liga ao medicamento pelo número de registro.
            var registry = scope.ServiceProvider.GetRequiredService<ImportRegistrySnapshot>();
            var registryRun = await registry.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Registro: execução {Id} terminou como {Outcome}.",
                registryRun.Id, registryRun.Outcome);

            var prices = scope.ServiceProvider.GetRequiredService<ImportPriceList>();
            var priceRun = await prices.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Preço: execução {Id} terminou como {Outcome}.",
                priceRun.Id, priceRun.Outcome);

            var leaflets = scope.ServiceProvider.GetRequiredService<ImportLeaflets>();
            var leafletRun = await leaflets.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Bulas: execução {Id} terminou como {Outcome}.",
                leafletRun.Id, leafletRun.Outcome);

            var location = scope.ServiceProvider.GetRequiredService<ImportIpStateRanges>();
            var locationRun = await location.ExecuteAsync(whenRepeated, stoppingToken);
            logger.LogInformation("Geolocalização: execução {Id} terminou como {Outcome}.",
                locationRun.Id, locationRun.Outcome);
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
