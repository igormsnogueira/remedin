using Remedin.Infrastructure;
using Remedin.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Remedin")
    ?? throw new InvalidOperationException(
        "Connection string 'Remedin' não configurada. Ver docs/RUNBOOK.md.");

builder.Services.AddInfrastructure(connectionString);

// Sem AddDbContextCheck a rota responderia 200 com o banco fora do ar, que é
// o oposto do que um health check serve para dizer.
builder.Services.AddHealthChecks().AddDbContextCheck<RemedinDbContext>("postgres");

var app = builder.Build();

app.MapHealthChecks("/health");

app.Run();
