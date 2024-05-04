using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketAPIServer.Migrations
{
    /// <inheritdoc />
    public partial class third : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_PackageDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 5, 1, 6, 56, 22, 97, DateTimeKind.Utc).AddTicks(5846),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 821, DateTimeKind.Utc).AddTicks(654));

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_AppDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 5, 1, 6, 56, 22, 94, DateTimeKind.Utc).AddTicks(1097),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 818, DateTimeKind.Utc).AddTicks(1001));

            migrationBuilder.AddColumn<string>(
                name: "imageURL",
                table: "Steam_AppDetails",
                type: "varchar(600)",
                maxLength: 600,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "imageURL",
                table: "Steam_AppDetails");

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_PackageDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 821, DateTimeKind.Utc).AddTicks(654),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 5, 1, 6, 56, 22, 97, DateTimeKind.Utc).AddTicks(5846));

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_AppDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 818, DateTimeKind.Utc).AddTicks(1001),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 5, 1, 6, 56, 22, 94, DateTimeKind.Utc).AddTicks(1097));
        }
    }
}
