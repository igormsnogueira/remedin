using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RemedinDbContext))]
[Migration("20261110000000_InstitutionalPackaging")]
public partial class InstitutionalPackaging : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            "ALTER TABLE presentations ADD COLUMN institutional boolean NOT NULL DEFAULT false;");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("ALTER TABLE presentations DROP COLUMN IF EXISTS institutional;");
    }
}
