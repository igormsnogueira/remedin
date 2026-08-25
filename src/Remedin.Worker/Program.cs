using Remedin.Application;
using Remedin.Infrastructure;
using Remedin.Worker;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Remedin")
    ?? throw new InvalidOperationException("Connection string 'Remedin' não configurada.");

builder.Services.AddInfrastructure(connectionString);
builder.Services.AddApplication();
builder.Services.AddHostedService<CatalogIngestionWorker>();

var host = builder.Build();
host.Run();
