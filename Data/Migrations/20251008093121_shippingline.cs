using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Break_Bulk_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class shippingline : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                table: "ShippingLines",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                table: "ShippingLines",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "CMDU",
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8386), null });

            migrationBuilder.UpdateData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "COSU",
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8388), null });

            migrationBuilder.UpdateData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "MAEU",
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8359), null });

            migrationBuilder.UpdateData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "MSC",
                columns: new[] { "CreatedDate", "ModifiedDate" },
                values: new object[] { new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8385), null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedDate",
                table: "ShippingLines");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                table: "ShippingLines");
        }
    }
}
