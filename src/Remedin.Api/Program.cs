using Remedin.Application.Catalog.Search;
using Remedin.Domain.Medicines;
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
    string uf = IcmsRates.DefaultState,
    int limite = 20) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { erro = "Informe o termo de busca em 'q'." });
    }

    if (!IcmsRates.TryGet(uf, out var rate))
    {
        return InvalidState(uf);
    }

    var results = await search.SearchAsync(q, uf, Math.Clamp(limite, 1, 50), cancellationToken);

    return Results.Ok(new SearchResults(q, uf.ToUpperInvariant(), rate, results));
});

app.MapGet("/medicamentos/{registro}", async (
    string registro,
    IMedicineDetails details,
    CancellationToken cancellationToken,
    string uf = IcmsRates.DefaultState) =>
{
    if (!IcmsRates.TryGet(uf, out _))
    {
        return InvalidState(uf);
    }

    var medicine = await details.FindAsync(registro, uf, cancellationToken);

    return medicine is null ? Results.NotFound() : Results.Ok(medicine);
});

app.Run();

static IResult InvalidState(string uf) =>
    Results.BadRequest(new
    {
        erro = $"Unidade da federação desconhecida: '{uf}'.",
        // O preço-teto depende do ICMS estadual, então chutar um padrão
        // publicaria valor errado para quem digitou a sigla errada.
        aceitos = IcmsRates.States.Order(),
    });
