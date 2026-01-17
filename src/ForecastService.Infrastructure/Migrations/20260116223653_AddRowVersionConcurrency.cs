using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ForecastService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRowVersionConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PowerPlants",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Forecasts",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Companies",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PowerPlants");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Forecasts");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Companies");
        }
    }
}
