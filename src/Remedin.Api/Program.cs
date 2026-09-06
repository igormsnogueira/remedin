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

// O front-end roda em outra porta, e o navegador bloqueia a chamada sem isso.
// A lista vem de configuração porque o endereço muda entre a máquina de
// desenvolvimento e o ambiente publicado.
const string FrontendCors = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy(
    FrontendCors,
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseCors(FrontendCors);

app.MapHealthChecks("/health");

app.MapGet("/estados", () => StateOptions.All);

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

app.MapGet("/medicamentos/{registro}/alternativas", async (
    string registro,
    IMedicineAlternatives alternatives,
    CancellationToken cancellationToken,
    string uf = IcmsRates.DefaultState) =>
{
    if (!IcmsRates.TryGet(uf, out _))
    {
        return InvalidState(uf);
    }

    var result = await alternatives.FindAsync(registro, uf, cancellationToken);

    return result is null ? Results.NotFound() : Results.Ok(result);
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
