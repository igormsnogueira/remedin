using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Remedin.Infrastructure.Persistence;

namespace Remedin.Api;

/// <summary>
/// Verifica se a API consegue consultar o banco.
///
/// Substitui o AddDbContextCheck do EF, que usa CanConnectAsync: ele engole a
/// exceção e devolve só "não conectou", e o log registrava "(null)" como
/// motivo. No primeiro deploy isso impediu saber se era o banco acordando ou a
/// senha errada. Aqui a exceção segue junto, e o log mostra a causa.
///
/// A mensagem da exceção de conexão do Npgsql não carrega a senha. O caso em
/// que carregaria, string de conexão malformada, é barrado antes, na
/// inicialização, por DatabaseConnectionString.
/// </summary>
public sealed class DatabaseHealthCheck(RemedinDbContext database) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await database.Database.ExecuteSqlRawAsync("SELECT 1", cancellationToken);
            return HealthCheckResult.Healthy();
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy("O banco não respondeu à consulta de verificação.", exception);
        }
    }
}
