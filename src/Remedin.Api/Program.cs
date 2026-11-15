using Microsoft.AspNetCore.HttpLogging;
using Remedin.Api;
using Remedin.Application.Catalog.Search;
using Remedin.Application.Location;
using Remedin.Domain.Medicines;
using Remedin.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Remedin")
    ?? throw new InvalidOperationException("Connection string 'Remedin' não configurada.");

builder.Services.AddInfrastructure(connectionString);

builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("postgres");

// Sem cabeçalhos no log: eles podem carregar dado pessoal.
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod
        | HttpLoggingFields.RequestPath
        | HttpLoggingFields.RequestQuery
        | HttpLoggingFields.ResponseStatusCode
        | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

const string FrontendCors = "frontend";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

builder.Services.AddCors(options => options.AddPolicy(
    FrontendCors,
    policy => policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

app.UseWhen(
    context => !context.Request.Path.StartsWithSegments("/health"),
    branch => branch.UseHttpLogging());

app.UseCors(FrontendCors);

app.MapHealthChecks("/health");

app.MapGet("/estados", () => StateOptions.All);

// O IP serve só para esta consulta: não vai para o log nem para o banco.
app.MapGet("/localizacao", async (
    HttpContext http,
    IStateLocator locator,
    CancellationToken cancellationToken) =>
{
    http.Response.Headers.CacheControl = "private, no-store";

    var address = ClientAddress.From(http);

    if (address is null || ClientAddress.IsPrivate(address))
    {
        return Results.Ok(new { state = (string?)null });
    }

    var state = await locator.FindStateAsync(address, cancellationToken);
    return Results.Ok(new { state });
});

app.MapGet("/medicamentos", async (
    string? q,
    IMedicineSearch search,
    CancellationToken cancellationToken,
    string uf = IcmsRates.DefaultState,
    int pagina = 1,
    int limite = Page.DefaultSize) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { erro = "Informe o termo de busca em 'q'." });
    }

    if (!IcmsRates.TryGet(uf, out var rate))
    {
        return InvalidState(uf);
    }

    var results = await search.SearchAsync(q, uf, Page.Of(pagina, limite), cancellationToken);

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

app.MapGet("/finalidades", async (
    IMedicinePurposes purposes,
    CancellationToken cancellationToken) => await purposes.ListAsync(cancellationToken));

app.MapGet("/finalidades/{codigo}", async (
    string codigo,
    IMedicinePurposes purposes,
    CancellationToken cancellationToken,
    string uf = IcmsRates.DefaultState,
    int pagina = 1,
    int limite = Page.DefaultSize) =>
{
    if (!IcmsRates.TryGet(uf, out _))
    {
        return InvalidState(uf);
    }

    var result = await purposes.FindAsync(codigo, uf, Page.Of(pagina, limite), cancellationToken);

    return result is null ? Results.NotFound() : Results.Ok(result);
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
        aceitos = IcmsRates.States.Order(),
    });
