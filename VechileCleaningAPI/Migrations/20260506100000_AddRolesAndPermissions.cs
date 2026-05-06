using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace VechileCleaningAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesAndPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Role",
                table: "Users",
                newName: "RoleName");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsProtected = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    PermissionKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedAt", "IsProtected", "Name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Owner" },
                    { 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Dispatcher" },
                    { 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), false, "Cleaner" }
                });

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "PermissionKey", "RoleId" },
                values: new object[,]
                {
                    { 1,  "pages.daily",                    1 },
                    { 2,  "pages.bookings",                 1 },
                    { 3,  "pages.schedule",                 1 },
                    { 4,  "pages.users",                    1 },
                    { 5,  "pages.roles",                    1 },
                    { 6,  "actions.booking.updateStatus",   1 },
                    { 7,  "actions.schedule.edit",          1 },
                    { 8,  "actions.override.manage",        1 },
                    { 9,  "actions.user.create",            1 },
                    { 10, "actions.user.toggleActive",      1 },
                    { 11, "actions.role.manage",            1 },
                    { 12, "pages.daily",                    2 },
                    { 13, "pages.bookings",                 2 },
                    { 14, "pages.schedule",                 2 },
                    { 15, "actions.booking.updateStatus",   2 },
                    { 16, "actions.schedule.edit",          2 },
                    { 17, "actions.override.manage",        2 },
                    { 18, "pages.daily",                    3 },
                    { 19, "pages.bookings",                 3 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId",
                table: "RolePermissions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Name",
                table: "Roles",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RolePermissions");
            migrationBuilder.DropTable(name: "Roles");

            migrationBuilder.RenameColumn(
                name: "RoleName",
                table: "Users",
                newName: "Role");
        }
    }
}
