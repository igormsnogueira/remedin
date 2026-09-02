using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Application.Catalog.Search;
using Remedin.Infrastructure.Ingestion.Anvisa;
using Remedin.Infrastructure.Ingestion.Cmed;
using Remedin.Infrastructure.Persistence;

namespace Remedin.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// A ANVISA responde 403 a requisição sem User-Agent de navegador. Não é
    /// proteção contra automação: é checagem de cabeçalho, e declarar um
    /// agente honesto basta.
    /// </summary>
    private const string UserAgent =
        "Remedin/1.0 (+https://github.com/igormsnogueira/remedin) Mozilla/5.0 compatible";

    private static readonly TimeSpan DownloadTimeout = TimeSpan.FromMinutes(5);

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

        services.AddScoped<IMedicineCatalog, MedicineCatalog>();
        services.AddScoped<IIngestionJournal, IngestionJournal>();
        services.AddScoped<IMedicineSearch, PostgresMedicineSearch>();
        services.AddScoped<IMedicineDetails, PostgresMedicineDetails>();

        services.AddScoped<IMedicinePriceStore, MedicinePriceStore>();

        services.AddSingleton<AnvisaRegistryReader>();
        services.AddSingleton<CmedPriceReader>();

        services.AddHttpClient<IRegistrySnapshotSource, AnvisaRegistrySnapshotSource>(ConfigureDownload);
        services.AddHttpClient<IPriceSnapshotSource, CmedPriceSnapshotSource>(ConfigureDownload);

        return services;
    }

    private static void ConfigureDownload(HttpClient client)
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
        client.Timeout = DownloadTimeout;
    }
}
