using Microsoft.EntityFrameworkCore;
using Remedin.Application.Catalog.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

public sealed class MedicineCatalog(RemedinDbContext context) : IMedicineCatalog
{
    public async Task ReplaceAllAsync(
        IReadOnlyList<Medicine> medicines,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(medicines);

        // Apagar e reinserir dentro de uma transação já dá a troca atômica:
        // quem consulta continua vendo o catálogo anterior até o commit. Uma
        // tabela de staging só se pagaria se a carga demorasse a ponto de o
        // bloqueio incomodar, o que não é o caso com 32 mil linhas.
        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // As apresentações saem junto pela cascata declarada na migration.
        await context.Medicines.ExecuteDeleteAsync(cancellationToken);

        context.Medicines.AddRange(medicines);
        await context.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<IReadOnlySet<string>> RegistrationNumbersAsync(CancellationToken cancellationToken)
    {
        var numbers = await context.Medicines
            .AsNoTracking()
            .Select(medicine => medicine.RegistrationNumber.Value)
            .ToListAsync(cancellationToken);

        return numbers.ToHashSet(StringComparer.Ordinal);
    }
}
