using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace VechileCleaningAPI.Migrations
{
    /// <inheritdoc />
    public partial class DateOverrideRefactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HourOverrides");

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "WeeklySchedules",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DropColumn(
                name: "IsClosed",
                table: "WeeklySchedules");

            migrationBuilder.AddColumn<int>(
                name: "Capacity",
                table: "WeeklySchedules",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DateOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartHour = table.Column<int>(type: "int", nullable: true),
                    EndHour = table.Column<int>(type: "int", nullable: true),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    IsFullyClosed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DateOverrides", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DateOverrides");

            migrationBuilder.DropColumn(
                name: "Capacity",
                table: "WeeklySchedules");

            migrationBuilder.AddColumn<bool>(
                name: "IsClosed",
                table: "WeeklySchedules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HourOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Capacity = table.Column<int>(type: "int", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hour = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourOverrides", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "WeeklySchedules",
                columns: new[] { "Id", "DayOfWeek", "EndHour", "IsClosed", "StartHour" },
                values: new object[,]
                {
                    { 1, 0, 17, true, 8 },
                    { 2, 1, 17, false, 8 },
                    { 3, 2, 17, false, 8 },
                    { 4, 3, 17, false, 8 },
                    { 5, 4, 17, false, 8 },
                    { 6, 5, 17, false, 8 },
                    { 7, 6, 13, false, 9 }
                });
        }
    }
}
