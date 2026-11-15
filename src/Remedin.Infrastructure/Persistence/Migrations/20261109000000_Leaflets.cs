using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations;

[DbContext(typeof(RemedinDbContext))]
[Migration("20261109000000_Leaflets")]
public partial class Leaflets : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            CREATE TABLE leaflets (
                registration_number char(9) PRIMARY KEY,
                published_on        date    NOT NULL
            );
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("DROP TABLE IF EXISTS leaflets;");
    }
}
