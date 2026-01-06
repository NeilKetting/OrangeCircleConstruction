using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OCC.Client.Migrations
{
    /// <inheritdoc />
    public partial class AddContractDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContractDuration",
                table: "StaffMembers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContractDuration",
                table: "StaffMembers");
        }
    }
}
