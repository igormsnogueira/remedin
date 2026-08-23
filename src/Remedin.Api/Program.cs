using Remedin.Application.Catalog.Search;
using Remedin.Infrastructure;
using Remedin.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Remedin")
    ?? throw new InvalidOperationException("Connection string 'Remedin' não configurada.");

builder.Services.AddInfrastructure(connectionString);

// Sem AddDbContextCheck a rota responderia 200 com o banco fora do ar, que é
// o oposto do que um health check serve para dizer.
builder.Services.AddHealthChecks().AddDbContextCheck<RemedinDbContext>("postgres");

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapGet("/medicamentos", async (
    string? q,
    IMedicineSearch search,
    CancellationToken cancellationToken,
    int limite = 20) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { erro = "Informe o termo de busca em 'q'." });
    }

    var results = await search.SearchAsync(q, Math.Clamp(limite, 1, 50), cancellationToken);

    return Results.Ok(new { termo = q, total = results.Count, resultados = results });
});

app.Run();
