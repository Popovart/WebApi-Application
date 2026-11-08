using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedAtIndexAndDefaultValueOnCreateAndFillOldCreateAtValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')",
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CreatedAt",
                schema: "dbo",
                table: "Products",
                column: "CreatedAt");

            migrationBuilder.Sql(
            """
                UPDATE dbo.Products
                SET CreatedAt = TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')
                WHERE CreatedAt = '0001-01-01T00:00:00+00:00';
            """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Products_CreatedAt",
                schema: "dbo",
                table: "Products");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreatedAt",
                schema: "dbo",
                table: "Products",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: DateTimeOffset.MinValue,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset",
                oldDefaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')");
        }
    }
}
