using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Remedin.Domain.Ingestion;

namespace Remedin.Infrastructure.Persistence.Configurations;

public sealed class IngestionRunConfiguration : IEntityTypeConfiguration<IngestionRun>
{
    public void Configure(EntityTypeBuilder<IngestionRun> builder)
    {
        builder.ToTable("ingestion_runs");

        builder.HasKey(run => run.Id);

        builder.Property(run => run.Id).HasColumnName("id");

        builder.Property(run => run.Source)
            .HasColumnName("source")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(run => run.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(run => run.FinishedAt).HasColumnName("finished_at");

        builder.Property(run => run.Outcome)
            .HasColumnName("outcome")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(run => run.ContentHash)
            .HasColumnName("content_hash")
            .HasMaxLength(64);

        builder.Property(run => run.RowsRead).HasColumnName("rows_read");
        builder.Property(run => run.Accepted).HasColumnName("accepted");
        builder.Property(run => run.Rejected).HasColumnName("rejected");
        builder.Property(run => run.Duplicates).HasColumnName("duplicates");

        builder.Property(run => run.Detail).HasColumnName("detail").HasMaxLength(2000);

        // A consulta de idempotência é sempre "última execução bem-sucedida
        // desta origem".
        builder.HasIndex(run => new { run.Source, run.Outcome, run.StartedAt })
            .HasDatabaseName("ix_ingestion_runs_source_outcome_started_at");
    }
}
