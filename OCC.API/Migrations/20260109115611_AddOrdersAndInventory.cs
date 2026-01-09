using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdersAndInventory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("01d93513-d8d5-42a8-8a08-23156ba00cc0"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("05f2bc8d-b7cc-4ef2-87f5-3aa5f77b37c1"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("06893e26-ad85-49d1-a59e-866d18cfd6d5"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("24f09bd7-1b14-4691-b264-deb6d0a39ddd"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("41e36b1a-e0f8-464c-b197-b1f7f860fcc7"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5059ae2b-8bb2-4d1e-853a-1ee4d24cd62d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("89915147-4a7c-478f-9ee8-9552715bba1b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("95a00626-7ad5-40d1-a194-09685fd2e5c3"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c4ac56bf-aac7-465a-bb39-c9cfa48b09b0"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("c64ce1ad-716c-4a0a-acce-cc9d1356fa28"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("dfc76ca7-4b41-475d-a689-9c8d5472d3a5"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("e2856ba5-b66c-45cb-a99f-159260c6e0c2"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("eba7c4c2-620d-4179-b071-21837ecfae5e"));

            migrationBuilder.CreateTable(
                name: "InventoryItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityOnHand = table.Column<double>(type: "float", nullable: false),
                    ReorderPoint = table.Column<double>(type: "float", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SupplierName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpectedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DestinationType = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ProjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderLines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuantityOrdered = table.Column<double>(type: "float", nullable: false),
                    QuantityReceived = table.Column<double>(type: "float", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLines_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("143d8b0c-cd0e-42de-9880-b4b8237e672a"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("2e78b119-f31c-421c-a9e5-b420be8d1cbb"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("31be3e2c-bf88-4306-a8cb-09f062ebc40b"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("404571ee-46aa-440e-aeff-d71a2cd69b8f"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("4adccde9-4f7d-4ee8-9d81-5001caf95476"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("584822c3-1a4d-451c-968e-dad89e96dc6c"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("679ff7b9-6665-45cb-b2bc-5d719fe13528"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("70c061c3-df30-4174-8e8a-0aea850487e2"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("7b90f15c-245c-4b97-a36d-9f8142da5158"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("96e79440-f4a8-453f-9a18-ae114d0f5d10"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("ce05d56f-9972-4479-a5a7-0ade5f00664b"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("d9b909ce-4224-4ac4-ae74-a8dd05713453"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("ee7e6d38-b89c-4f72-bacd-0e4a6f0d81d2"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLines_OrderId",
                table: "OrderLines",
                column: "OrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryItems");

            migrationBuilder.DropTable(
                name: "OrderLines");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("143d8b0c-cd0e-42de-9880-b4b8237e672a"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2e78b119-f31c-421c-a9e5-b420be8d1cbb"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("31be3e2c-bf88-4306-a8cb-09f062ebc40b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("404571ee-46aa-440e-aeff-d71a2cd69b8f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("4adccde9-4f7d-4ee8-9d81-5001caf95476"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("584822c3-1a4d-451c-968e-dad89e96dc6c"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("679ff7b9-6665-45cb-b2bc-5d719fe13528"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("70c061c3-df30-4174-8e8a-0aea850487e2"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7b90f15c-245c-4b97-a36d-9f8142da5158"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("96e79440-f4a8-453f-9a18-ae114d0f5d10"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ce05d56f-9972-4479-a5a7-0ade5f00664b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d9b909ce-4224-4ac4-ae74-a8dd05713453"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ee7e6d38-b89c-4f72-bacd-0e4a6f0d81d2"));

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("01d93513-d8d5-42a8-8a08-23156ba00cc0"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("05f2bc8d-b7cc-4ef2-87f5-3aa5f77b37c1"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("06893e26-ad85-49d1-a59e-866d18cfd6d5"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("24f09bd7-1b14-4691-b264-deb6d0a39ddd"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("41e36b1a-e0f8-464c-b197-b1f7f860fcc7"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("5059ae2b-8bb2-4d1e-853a-1ee4d24cd62d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" },
                    { new Guid("89915147-4a7c-478f-9ee8-9552715bba1b"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("95a00626-7ad5-40d1-a194-09685fd2e5c3"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("c4ac56bf-aac7-465a-bb39-c9cfa48b09b0"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("c64ce1ad-716c-4a0a-acce-cc9d1356fa28"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("dfc76ca7-4b41-475d-a689-9c8d5472d3a5"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("e2856ba5-b66c-45cb-a99f-159260c6e0c2"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("eba7c4c2-620d-4179-b071-21837ecfae5e"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" }
                });
        }
    }
}
