using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class IngestionRuns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ingestion_runs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    source = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    finished_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    outcome = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    content_hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    rows_read = table.Column<int>(type: "integer", nullable: false),
                    accepted = table.Column<int>(type: "integer", nullable: false),
                    rejected = table.Column<int>(type: "integer", nullable: false),
                    duplicates = table.Column<int>(type: "integer", nullable: false),
                    detail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingestion_runs", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ingestion_runs_source_outcome_started_at",
                table: "ingestion_runs",
                columns: new[] { "source", "outcome", "started_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ingestion_runs");
        }
    }
}
