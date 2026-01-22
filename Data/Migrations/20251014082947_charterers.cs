using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Break_Bulk_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class charterers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VesselMasters_ShippingLines_ShippingLineCode",
                table: "VesselMasters");

            migrationBuilder.DeleteData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "CMDU");

            migrationBuilder.DeleteData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "COSU");

            migrationBuilder.DeleteData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "MAEU");

            migrationBuilder.DeleteData(
                table: "ShippingLines",
                keyColumn: "Code",
                keyValue: "MSC");

            migrationBuilder.CreateTable(
                name: "Charterers",
                columns: table => new
                {
                    KeyCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LongDescription = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Charterers", x => x.KeyCode);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_VesselMasters_ShippingLines_ShippingLineCode",
                table: "VesselMasters",
                column: "ShippingLineCode",
                principalTable: "ShippingLines",
                principalColumn: "Code",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VesselMasters_ShippingLines_ShippingLineCode",
                table: "VesselMasters");

            migrationBuilder.DropTable(
                name: "Charterers");

            migrationBuilder.InsertData(
                table: "ShippingLines",
                columns: new[] { "Code", "CreatedDate", "ModifiedDate", "Name" },
                values: new object[,]
                {
                    { "CMDU", new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8386), null, "CMA CGM" },
                    { "COSU", new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8388), null, "COSCO Shipping" },
                    { "MAEU", new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8359), null, "Maersk Line" },
                    { "MSC", new DateTime(2025, 10, 8, 11, 31, 17, 600, DateTimeKind.Local).AddTicks(8385), null, "Mediterranean Shipping Company" }
                });

            migrationBuilder.AddForeignKey(
                name: "FK_VesselMasters_ShippingLines_ShippingLineCode",
                table: "VesselMasters",
                column: "ShippingLineCode",
                principalTable: "ShippingLines",
                principalColumn: "Code",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
