using Npgsql;

namespace Remedin.Infrastructure.Persistence;

public static class DatabaseConnectionString
{
    public static void EnsureValid(string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
            || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A string de conexão está em formato de URL (postgresql://...). O Npgsql precisa do " +
                "formato 'Host=...;Database=...;Username=...;Password=...;SSL Mode=Require'.");
        }

        try
        {
            // A exceção do Npgsql repete a string de conexão, com a senha.
            _ = new NpgsqlConnectionStringBuilder(connectionString);
        }
        catch (ArgumentException)
        {
            throw new InvalidOperationException(
                "A string de conexão tem um parâmetro com nome ou valor inválido. O conteúdo não é " +
                "exibido porque pode conter a senha.");
        }
    }
}
