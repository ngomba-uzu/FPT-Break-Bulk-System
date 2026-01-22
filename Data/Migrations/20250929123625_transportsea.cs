using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Break_Bulk_System.Data.Migrations
{
    /// <inheritdoc />
    public partial class transportsea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TransportSeas",
                columns: table => new
                {
                    TransportID = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CarrierCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    CarrierName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransportSeas", x => x.TransportID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TransportSeas");
        }
    }
}
