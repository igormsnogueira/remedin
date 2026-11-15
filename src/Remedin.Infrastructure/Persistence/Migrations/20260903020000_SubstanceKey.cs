using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    public partial class SubstanceKey : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE medicines ADD COLUMN substance_key text;");

            migrationBuilder.Sql("""
                CREATE INDEX ix_medicines_substance_key
                ON medicines (substance_key)
                WHERE substance_key IS NOT NULL AND has_price;
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_substance_key;");
            migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS substance_key;");
        }
    }
}
