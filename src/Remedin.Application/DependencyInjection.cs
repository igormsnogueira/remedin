using Microsoft.Extensions.DependencyInjection;
using Remedin.Application.Catalog.Ingestion;

namespace Remedin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.TryAddTimeProvider();
        services.AddScoped<ImportRegistrySnapshot>();
        services.AddScoped<ImportPriceList>();

        return services;
    }

    private static void TryAddTimeProvider(this IServiceCollection services)
    {
        if (services.All(service => service.ServiceType != typeof(TimeProvider)))
        {
            services.AddSingleton(TimeProvider.System);
        }
    }
}
