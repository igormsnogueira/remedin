using System.Reflection;
using NetArchTest.Rules;
using Remedin.Application;
using Remedin.Domain.Medicines;

namespace Remedin.Architecture.Tests;

/// <summary>
/// A regra de dependência do Clean Architecture já é imposta pelas referências
/// de projeto: o compilador não deixa o domínio enxergar infraestrutura.
///
/// Estes testes cobrem o que a referência de projeto não cobre — dependência
/// que entra por transitividade, ou por alguém adicionar a referência errada
/// sem perceber o que ela significa.
/// </summary>
public class DependencyRuleTests
{
    private static readonly Assembly Domain = typeof(RegistrationNumber).Assembly;
    private static readonly Assembly Application = typeof(ApplicationAssembly).Assembly;

    private static readonly string[] InfrastructureConcerns =
    [
        "Remedin.Infrastructure",
        "Microsoft.EntityFrameworkCore",
        "Npgsql",
        "System.Net.Http",
        "System.Data.Common",
    ];

    [Fact]
    public void Dominio_nao_depende_de_nenhuma_outra_camada()
    {
        AssertDoesNotDependOn(
            Domain,
            [.. InfrastructureConcerns, "Remedin.Application"],
            "O domínio não pode conhecer aplicação nem infraestrutura.");
    }

    [Fact]
    public void Aplicacao_nao_depende_de_infraestrutura()
    {
        AssertDoesNotDependOn(
            Application,
            InfrastructureConcerns,
            "A aplicação declara interfaces; quem implementa é a infraestrutura.");
    }

    private static void AssertDoesNotDependOn(Assembly assembly, string[] forbidden, string rule)
    {
        // Sem esta guarda, um assembly vazio faria a regra passar sem ter sido
        // verificada — o teste viraria enfeite conforme o projeto crescesse.
        var types = Types.InAssembly(assembly).GetTypes().ToArray();
        Assert.True(
            types.Length > 0,
            $"{assembly.GetName().Name} não tem tipos: a regra não foi verificada.");

        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        if (result.IsSuccessful)
        {
            return;
        }

        var offenders = string.Join(", ", result.FailingTypeNames ?? []);
        Assert.Fail($"{rule}{Environment.NewLine}Tipos que violam a regra: {offenders}");
    }
}
