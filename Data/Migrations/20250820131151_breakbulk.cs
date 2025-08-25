using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Break_Bulk_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class breakbulk : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingLines",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingLines", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "VesselTypes",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselTypes", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "VesselMasters",
                columns: table => new
                {
                    VesselCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    VesselName = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: false),
                    LoadingBerth = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    LoadingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoadingTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    VesselTypeCode = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    ImpExp = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ArrivalTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    SailDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SailTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    BillNo = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true),
                    StockCompleted = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    VoyageNumber = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    CallSign = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    Charterer = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    VPM = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    ShippingLineCode = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    StorageDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselMasters", x => x.VesselCode);
                    table.ForeignKey(
                        name: "FK_VesselMasters_ShippingLines_ShippingLineCode",
                        column: x => x.ShippingLineCode,
                        principalTable: "ShippingLines",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VesselMasters_VesselTypes_VesselTypeCode",
                        column: x => x.VesselTypeCode,
                        principalTable: "VesselTypes",
                        principalColumn: "Code",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Manifests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BillNo = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: false),
                    Mark = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PackKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Mark2 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Mark3 = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LdPort = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    ExpectedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    NetWeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GrossUnitKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    LOrderComp = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    CargoType = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Commodity = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    SubCommodity = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    Customer = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    ImpExp = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    Transhipment = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    HandlingAccount = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true),
                    StorageAccount = table.Column<string>(type: "nvarchar(26)", maxLength: 26, nullable: true),
                    ExclWEnd = table.Column<string>(type: "nvarchar(1)", maxLength: 1, nullable: true),
                    ExPRBC = table.Column<string>(type: "nvarchar(6)", maxLength: 6, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Manifests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Manifests_VesselMasters_VesselCode",
                        column: x => x.VesselCode,
                        principalTable: "VesselMasters",
                        principalColumn: "VesselCode",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ShippingLines",
                columns: new[] { "Code", "Name" },
                values: new object[,]
                {
                    { "CMDU", "CMA CGM" },
                    { "COSU", "COSCO Shipping" },
                    { "MAEU", "Maersk Line" },
                    { "MSC", "Mediterranean Shipping Company" }
                });

            migrationBuilder.InsertData(
                table: "VesselTypes",
                columns: new[] { "Code", "Description" },
                values: new object[,]
                {
                    { "01", "Container Ship" },
                    { "02", "Bulk Carrier" },
                    { "03", "Tanker" },
                    { "04", "General Cargo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Manifests_VesselCode",
                table: "Manifests",
                column: "VesselCode");

            migrationBuilder.CreateIndex(
                name: "IX_VesselMasters_ShippingLineCode",
                table: "VesselMasters",
                column: "ShippingLineCode");

            migrationBuilder.CreateIndex(
                name: "IX_VesselMasters_VesselTypeCode",
                table: "VesselMasters",
                column: "VesselTypeCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Manifests");

            migrationBuilder.DropTable(
                name: "VesselMasters");

            migrationBuilder.DropTable(
                name: "ShippingLines");

            migrationBuilder.DropTable(
                name: "VesselTypes");
        }
    }
}
