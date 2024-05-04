using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GameMarketAPIServer.Migrations
{
    /// <inheritdoc />
    public partial class sec : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "publisherName",
                table: "Xbox_MarketDetail",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "developerName",
                table: "Xbox_MarketDetail",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_PackageDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 821, DateTimeKind.Utc).AddTicks(654),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 214, DateTimeKind.Utc).AddTicks(8425));

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_AppDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 818, DateTimeKind.Utc).AddTicks(1001),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 211, DateTimeKind.Utc).AddTicks(6219));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Xbox_MarketDetail",
                keyColumn: "publisherName",
                keyValue: null,
                column: "publisherName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "publisherName",
                table: "Xbox_MarketDetail",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Xbox_MarketDetail",
                keyColumn: "developerName",
                keyValue: null,
                column: "developerName",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "developerName",
                table: "Xbox_MarketDetail",
                type: "varchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(150)",
                oldMaxLength: 150,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_PackageDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 214, DateTimeKind.Utc).AddTicks(8425),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 821, DateTimeKind.Utc).AddTicks(654));

            migrationBuilder.AlterColumn<DateTime>(
                name: "lastScanned",
                table: "Steam_AppDetails",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(2024, 4, 29, 9, 22, 6, 211, DateTimeKind.Utc).AddTicks(6219),
                oldClrType: typeof(DateTime),
                oldType: "datetime(6)",
                oldDefaultValue: new DateTime(2024, 4, 30, 4, 45, 3, 818, DateTimeKind.Utc).AddTicks(1001));
        }
    }
}
