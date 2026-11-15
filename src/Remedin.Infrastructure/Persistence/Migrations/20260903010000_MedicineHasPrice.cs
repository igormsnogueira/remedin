using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    public partial class MedicineHasPrice : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE medicines
                ADD COLUMN has_price boolean NOT NULL DEFAULT false;
                """);

            migrationBuilder.Sql("""
                UPDATE medicines m
                SET has_price = EXISTS (
                    SELECT 1 FROM presentations p
                    WHERE p.registration_number = m.registration_number
                );
                """);

            migrationBuilder.Sql("""
                CREATE INDEX ix_medicines_has_price ON medicines (has_price);
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP INDEX IF EXISTS ix_medicines_has_price;");
            migrationBuilder.Sql("ALTER TABLE medicines DROP COLUMN IF EXISTS has_price;");
        }
    }
}
