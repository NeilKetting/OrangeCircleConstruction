using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OCC.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveBalances : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("010c68ed-310d-4005-9cc3-d72a8b58d410"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("1fde9626-576a-48cf-8aa0-d99ed6fcf30f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2bdeb9f0-1396-4b21-9305-3e58e94d53b5"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("3d0bf74e-20b7-4220-acd2-596babd2ee94"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("72eed25a-ae35-4dc4-9eea-04b97c5dd8d8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7b9bee09-1aa7-408d-a02c-0e51af42492d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("7c1864de-980f-4cbf-a3b5-70a7550193c8"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("88268e90-6f0b-4b5c-abb6-67b672ecdb04"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8c2a17da-a22b-4d25-991e-9e7a86860d81"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("a6c57454-0d93-4c23-aba9-42c0546b17f4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("d80da547-6df8-43d1-a13b-fc7c1dff0f09"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("f86d7965-9f31-4ee6-8ec1-39c1eca2ff0e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("fdff892e-d223-46f4-be82-32f88c0d025e"));

            migrationBuilder.AddColumn<double>(
                name: "AnnualLeaveBalance",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LeaveCycleStartDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SickLeaveBalance",
                table: "Employees",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "CachedHourlyRate",
                table: "AttendanceRecords",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("04b38421-94e1-4705-b978-6684fad2dad9"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("0ae94b89-d103-4c2a-9122-83a8564c2b9b"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("0b062823-6c9e-4d3b-804d-fbc525ee4737"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("1c55afc9-9895-4444-b266-4df7e34c5d30"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("21686a30-6880-4ad1-93e9-377c8c641933"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("2482f84d-8f4b-4536-b952-b163083f281e"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("57e330c3-ab8f-4c66-b79b-8d489069f45f"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("5b179f2b-69dd-49c6-a818-c89dc3b15992"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("65958e98-139e-439e-89de-0c916d1f5b4d"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("72da373a-a92b-4ab5-97d9-fab0ddc01756"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("8e91e88d-ecd0-4abd-8a2e-0de57341e8c4"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("cd0f9be5-2509-4524-b08f-dca2212b6eb7"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("ddac9c5f-d069-473b-821c-bc240bd5122d"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("04b38421-94e1-4705-b978-6684fad2dad9"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0ae94b89-d103-4c2a-9122-83a8564c2b9b"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("0b062823-6c9e-4d3b-804d-fbc525ee4737"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("1c55afc9-9895-4444-b266-4df7e34c5d30"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("21686a30-6880-4ad1-93e9-377c8c641933"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("2482f84d-8f4b-4536-b952-b163083f281e"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("57e330c3-ab8f-4c66-b79b-8d489069f45f"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("5b179f2b-69dd-49c6-a818-c89dc3b15992"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("65958e98-139e-439e-89de-0c916d1f5b4d"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("72da373a-a92b-4ab5-97d9-fab0ddc01756"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("8e91e88d-ecd0-4abd-8a2e-0de57341e8c4"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("cd0f9be5-2509-4524-b08f-dca2212b6eb7"));

            migrationBuilder.DeleteData(
                table: "PublicHolidays",
                keyColumn: "Id",
                keyValue: new Guid("ddac9c5f-d069-473b-821c-bc240bd5122d"));

            migrationBuilder.DropColumn(
                name: "AnnualLeaveBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LeaveCycleStartDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "SickLeaveBalance",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "CachedHourlyRate",
                table: "AttendanceRecords");

            migrationBuilder.InsertData(
                table: "PublicHolidays",
                columns: new[] { "Id", "Date", "Name" },
                values: new object[,]
                {
                    { new Guid("010c68ed-310d-4005-9cc3-d72a8b58d410"), new DateTime(2026, 4, 6, 0, 0, 0, 0, DateTimeKind.Unspecified), "Family Day" },
                    { new Guid("1fde9626-576a-48cf-8aa0-d99ed6fcf30f"), new DateTime(2026, 12, 26, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Goodwill" },
                    { new Guid("2bdeb9f0-1396-4b21-9305-3e58e94d53b5"), new DateTime(2026, 8, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "National Women's Day" },
                    { new Guid("3d0bf74e-20b7-4220-acd2-596babd2ee94"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), "Heritage Day" },
                    { new Guid("72eed25a-ae35-4dc4-9eea-04b97c5dd8d8"), new DateTime(2026, 8, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Public Holiday" },
                    { new Guid("7b9bee09-1aa7-408d-a02c-0e51af42492d"), new DateTime(2026, 6, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Youth Day" },
                    { new Guid("7c1864de-980f-4cbf-a3b5-70a7550193c8"), new DateTime(2026, 12, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Christmas Day" },
                    { new Guid("88268e90-6f0b-4b5c-abb6-67b672ecdb04"), new DateTime(2026, 3, 21, 0, 0, 0, 0, DateTimeKind.Unspecified), "Human Rights Day" },
                    { new Guid("8c2a17da-a22b-4d25-991e-9e7a86860d81"), new DateTime(2026, 4, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Good Friday" },
                    { new Guid("a6c57454-0d93-4c23-aba9-42c0546b17f4"), new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Workers' Day" },
                    { new Guid("d80da547-6df8-43d1-a13b-fc7c1dff0f09"), new DateTime(2026, 4, 27, 0, 0, 0, 0, DateTimeKind.Unspecified), "Freedom Day" },
                    { new Guid("f86d7965-9f31-4ee6-8ec1-39c1eca2ff0e"), new DateTime(2026, 12, 16, 0, 0, 0, 0, DateTimeKind.Unspecified), "Day of Reconciliation" },
                    { new Guid("fdff892e-d223-46f4-be82-32f88c0d025e"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "New Year's Day" }
                });
        }
    }
}
