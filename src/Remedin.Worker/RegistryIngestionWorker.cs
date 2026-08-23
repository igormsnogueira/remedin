using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Worker;

/// <summary>
/// Executa a carga da base de registro uma vez e encerra o processo.
///
/// Agendamento ainda não entrou: rodar sob demanda é o que permite disparar
/// a carga manualmente e reprocessar. A periodicidade vem depois, quando
/// houver onde publicar.
/// </summary>
public sealed class RegistryIngestionWorker(
    IServiceProvider services,
    IHostApplicationLifetime lifetime,
    ILogger<RegistryIngestionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // O comando é scoped porque depende do DbContext; um serviço de
            // fundo é singleton, então o escopo é criado aqui.
            using var scope = services.CreateScope();
            var import = scope.ServiceProvider.GetRequiredService<ImportRegistrySnapshot>();

            var run = await import.ExecuteAsync(stoppingToken);

            logger.LogInformation(
                "Execução {Id} terminou como {Outcome}.", run.Id, run.Outcome);
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
