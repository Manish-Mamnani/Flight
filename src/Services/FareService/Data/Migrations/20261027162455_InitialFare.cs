using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FareService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialFare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Fares",
                columns: table => new
                {
                    FareId = table.Column<int>(type: "int", nullable: false),
                    FlightInstanceId = table.Column<int>(type: "int", nullable: false),
                    FareClass = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    BaseFare = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fares", x => x.FareId);
                    table.CheckConstraint("CK_Fares_BaseFare_NonNegative", "[BaseFare] >= 0");
                    table.CheckConstraint("CK_Fares_TaxAmount_NonNegative", "[TaxAmount] >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Fares_FlightInstanceId_FareClass_Status",
                table: "Fares",
                columns: new[] { "FlightInstanceId", "FareClass", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Fares");
        }
    }
}
