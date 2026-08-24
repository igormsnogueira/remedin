using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Remedin.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class PricesByIcmsRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "consumer_price",
                table: "presentations");

            migrationBuilder.DropColumn(
                name: "factory_price",
                table: "presentations");

            migrationBuilder.CreateTable(
                name: "prices",
                columns: table => new
                {
                    registration_number = table.Column<string>(type: "character(9)", nullable: false),
                    ggrem_code = table.Column<string>(type: "character varying(20)", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    kind = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    icms_rate = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: true),
                    free_trade_zone = table.Column<bool>(type: "boolean", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prices", x => new { x.registration_number, x.ggrem_code, x.Id });
                    table.ForeignKey(
                        name: "FK_prices_presentations_registration_number_ggrem_code",
                        columns: x => new { x.registration_number, x.ggrem_code },
                        principalTable: "presentations",
                        principalColumns: new[] { "registration_number", "ggrem_code" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_prices_presentation_kind_rate",
                table: "prices",
                columns: new[] { "registration_number", "ggrem_code", "kind", "icms_rate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "prices");

            migrationBuilder.AddColumn<decimal>(
                name: "consumer_price",
                table: "presentations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "factory_price",
                table: "presentations",
                type: "numeric(12,2)",
                precision: 12,
                scale: 2,
                nullable: true);
        }
    }
}
