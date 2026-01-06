using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCC.Client.Migrations
{
    /// <inheritdoc />
    public partial class RenameStaffToEmployees : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_StaffMembers_SiteManagerId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StaffMembers",
                table: "StaffMembers");

            migrationBuilder.RenameTable(
                name: "StaffMembers",
                newName: "Employees");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Employees",
                table: "Employees",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Employees_SiteManagerId",
                table: "Projects",
                column: "SiteManagerId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Employees_SiteManagerId",
                table: "Projects");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Employees",
                table: "Employees");

            migrationBuilder.RenameTable(
                name: "Employees",
                newName: "StaffMembers");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StaffMembers",
                table: "StaffMembers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_StaffMembers_SiteManagerId",
                table: "Projects",
                column: "SiteManagerId",
                principalTable: "StaffMembers",
                principalColumn: "Id");
        }
    }
}
