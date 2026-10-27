using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckInService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCheckIn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FlightSeatInventory",
                columns: table => new
                {
                    SeatId = table.Column<int>(type: "int", nullable: false),
                    FlightInstanceId = table.Column<int>(type: "int", nullable: false),
                    SeatNumber = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: false),
                    SeatClass = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    SeatStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    BookingId = table.Column<int>(type: "int", nullable: true),
                    PassengerId = table.Column<int>(type: "int", nullable: true),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSeatInventory", x => x.SeatId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FlightSeatInventory_BookingId_PassengerId",
                table: "FlightSeatInventory",
                columns: new[] { "BookingId", "PassengerId" });

            migrationBuilder.CreateIndex(
                name: "IX_FlightSeatInventory_FlightInstanceId_SeatNumber",
                table: "FlightSeatInventory",
                columns: new[] { "FlightInstanceId", "SeatNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightSeatInventory");
        }
    }
}
