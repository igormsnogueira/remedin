using Npgsql;

namespace Remedin.Infrastructure.Persistence;

/// <summary>
/// Confere o formato da string de conexão antes de qualquer tentativa de
/// conectar, com erro que nunca repete o conteúdo dela.
///
/// Existe porque o Npgsql, ao receber um parâmetro inválido, coloca o valor
/// recebido na mensagem da exceção — e o valor pode conter a senha. Na
/// configuração do banco de produção isso escreveu a senha em texto puro no log
/// duas vezes, e o log do GitHub Actions de um repositório público é público.
/// </summary>
public static class DatabaseConnectionString
{
    public static void EnsureValid(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        // O erro mais provável, porque é o formato que os provedores mostram
        // primeiro no painel.
        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A string de conexão está em formato de URL (postgresql://...). O Npgsql precisa do " +
                "formato 'Host=...;Database=...;Username=...;Password=...;SSL Mode=Require'.");
        }

        try
        {
            _ = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            // A exceção original não é anexada de propósito: a mensagem dela é
            // justamente o que carrega o valor do parâmetro.
            throw new InvalidOperationException(
                "A string de conexão tem um parâmetro com nome ou valor inválido. O conteúdo não é " +
                "exibido porque pode conter a senha.");
        }
    }
}
