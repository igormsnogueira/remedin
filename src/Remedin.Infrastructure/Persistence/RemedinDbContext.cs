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
        modelBuilder.HasPostgresExtension("unaccent");
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RemedinDbContext).Assembly);
    }
}
