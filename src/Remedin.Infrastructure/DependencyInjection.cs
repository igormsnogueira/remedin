using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remedin.Infrastructure.Persistence;

namespace Remedin.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Recebe a string de conexão em vez de <c>IConfiguration</c>: a
    /// infraestrutura não precisa saber de onde a configuração veio, e isso
    /// mantém quem chama no controle.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<RemedinDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
