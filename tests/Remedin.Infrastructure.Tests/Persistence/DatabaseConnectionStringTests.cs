using Remedin.Infrastructure.Persistence;

namespace Remedin.Infrastructure.Tests.Persistence;

public class DatabaseConnectionStringTests
{
    private const string Password = "senha-que-nao-pode-vazar";

    [Fact]
    public void Formato_do_Npgsql_passa()
    {
        DatabaseConnectionString.EnsureValid(
            $"Host=db.exemplo;Database=remedin;Username=remedin;Password={Password};SSL Mode=Require");
    }

    [Theory]
    [InlineData($"postgresql://remedin:{Password}@db.exemplo/remedin?sslmode=require")]
    [InlineData($"postgres://remedin:{Password}@db.exemplo/remedin")]
    public void Url_e_recusada_com_explicacao_e_sem_a_senha(string url)
    {
        var error = Assert.Throws<InvalidOperationException>(() => DatabaseConnectionString.EnsureValid(url));

        Assert.Contains("formato de URL", error.Message);
        Assert.DoesNotContain(Password, error.Message);
    }

    [Fact]
    public void Parametro_invalido_nao_repete_o_valor_recebido()
    {
        // O caso real: a URL colada no lugar de "Require". O Npgsql poria a
        // string inteira, senha incluída, na mensagem do erro.
        var malformed =
            $"Host=db.exemplo;Database=remedin;Username=remedin;Password={Password};" +
            $"SSL Mode=postgresql://remedin:{Password}@db.exemplo/remedin";

        var error = Assert.Throws<InvalidOperationException>(
            () => DatabaseConnectionString.EnsureValid(malformed));

        Assert.DoesNotContain(Password, error.Message);
        Assert.Null(error.InnerException);
    }

    [Fact]
    public void Nome_de_parametro_desconhecido_tambem_nao_vaza()
    {
        var error = Assert.Throws<InvalidOperationException>(
            () => DatabaseConnectionString.EnsureValid($"Host=db.exemplo;Senha={Password}"));

        Assert.DoesNotContain(Password, error.Message);
    }
}
