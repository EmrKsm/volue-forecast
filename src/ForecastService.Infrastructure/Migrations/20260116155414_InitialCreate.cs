using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ForecastService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Companies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Companies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PowerPlants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompanyId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PowerPlants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PowerPlants_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Forecasts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PowerPlantId = table.Column<Guid>(type: "uuid", nullable: false),
                    ForecastDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProductionMWh = table.Column<decimal>(type: "numeric(18,6)", precision: 18, scale: 6, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Forecasts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Forecasts_PowerPlants_PowerPlantId",
                        column: x => x.PowerPlantId,
                        principalTable: "PowerPlants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Companies",
                columns: new[] { "Id", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788), "Energy Trading Corp", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788) });

            migrationBuilder.InsertData(
                table: "PowerPlants",
                columns: new[] { "Id", "CompanyId", "Country", "CreatedAt", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("11111111-1111-1111-1111-111111111111"), "Turkey", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788), "Turkey Power Plant", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788) },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("11111111-1111-1111-1111-111111111111"), "Bulgaria", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788), "Bulgaria Power Plant", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788) },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("11111111-1111-1111-1111-111111111111"), "Spain", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788), "Spain Power Plant", new DateTime(2026, 1, 16, 15, 54, 13, 574, DateTimeKind.Utc).AddTicks(2788) }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Forecasts_PowerPlantId_ForecastDateTime_IsActive",
                table: "Forecasts",
                columns: new[] { "PowerPlantId", "ForecastDateTime", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PowerPlants_CompanyId_Country",
                table: "PowerPlants",
                columns: new[] { "CompanyId", "Country" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Forecasts");

            migrationBuilder.DropTable(
                name: "PowerPlants");

            migrationBuilder.DropTable(
                name: "Companies");
        }
    }
}
