using Microsoft.EntityFrameworkCore;
using Remedin.Domain.Ingestion;
using Remedin.Domain.Medicines;

namespace Remedin.Infrastructure.Persistence;

public sealed class RemedinDbContext(DbContextOptions<RemedinDbContext> options) : DbContext(options)
{
    public DbSet<Medicine> Medicines => Set<Medicine>();

    public DbSet<IngestionRun> IngestionRuns => Set<IngestionRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Declaradas aqui para que a migration as crie. Se ficassem só no
        // script de inicialização do container, existiriam em desenvolvimento
        // e faltariam em produção.
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RemedinDbContext).Assembly);
    }
}
