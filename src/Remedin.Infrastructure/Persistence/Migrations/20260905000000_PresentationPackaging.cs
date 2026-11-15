using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    public partial class PresentationPackaging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE presentations
                ADD COLUMN dosage_mg numeric(12,4),
                ADD COLUMN unit_count integer;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE presentations
                DROP COLUMN IF EXISTS dosage_mg,
                DROP COLUMN IF EXISTS unit_count;
                """);
        }
    }
}
