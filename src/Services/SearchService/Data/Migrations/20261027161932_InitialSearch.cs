using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SearchService.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Airlines",
                columns: table => new
                {
                    AirlineId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirlineCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    AirlineName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airlines", x => x.AirlineId);
                });

            migrationBuilder.CreateTable(
                name: "Airports",
                columns: table => new
                {
                    AirportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirportCode = table.Column<string>(type: "varchar(3)", unicode: false, maxLength: 3, nullable: false),
                    AirportName = table.Column<string>(type: "varchar(150)", unicode: false, maxLength: 150, nullable: false),
                    City = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Airports", x => x.AirportId);
                });

            migrationBuilder.CreateTable(
                name: "Flights",
                columns: table => new
                {
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    FlightNumber = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    AirlineId = table.Column<int>(type: "int", nullable: false),
                    OriginAirportId = table.Column<int>(type: "int", nullable: false),
                    DestinationAirportId = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "varchar(30)", unicode: false, maxLength: 30, nullable: false, defaultValue: "Active")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Flights", x => x.FlightId);
                    table.CheckConstraint("CK_Flight_Origin_Destination", "[OriginAirportId] <> [DestinationAirportId]");
                    table.ForeignKey(
                        name: "FK_Flights_Airlines_AirlineId",
                        column: x => x.AirlineId,
                        principalTable: "Airlines",
                        principalColumn: "AirlineId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_DestinationAirportId",
                        column: x => x.DestinationAirportId,
                        principalTable: "Airports",
                        principalColumn: "AirportId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Flights_Airports_OriginAirportId",
                        column: x => x.OriginAirportId,
                        principalTable: "Airports",
                        principalColumn: "AirportId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FlightInstances",
                columns: table => new
                {
                    FlightInstanceId = table.Column<int>(type: "int", nullable: false),
                    FlightId = table.Column<int>(type: "int", nullable: false),
                    DepartureAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalSeats = table.Column<int>(type: "int", nullable: false),
                    FlightStatus = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightInstances", x => x.FlightInstanceId);
                    table.CheckConstraint("CK_FlightInstance_Arrival_Departure", "[ArrivalAtUtc] > [DepartureAtUtc]");
                    table.CheckConstraint("CK_FlightInstance_TotalSeats_Positive", "[TotalSeats] > 0");
                    table.ForeignKey(
                        name: "FK_FlightInstances_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "FlightId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Airlines",
                columns: new[] { "AirlineId", "AirlineCode", "AirlineName", "Status" },
                values: new object[,]
                {
                    { 1, "AI", "Air India", "Active" },
                    { 2, "6E", "IndiGo", "Active" }
                });

            migrationBuilder.InsertData(
                table: "Airports",
                columns: new[] { "AirportId", "AirportCode", "AirportName", "City", "Country", "Status" },
                values: new object[,]
                {
                    { 1, "DEL", "Indira Gandhi International Airport", "New Delhi", "India", "Active" },
                    { 2, "BOM", "Chhatrapati Shivaji Maharaj International Airport", "Mumbai", "India", "Active" },
                    { 3, "BLR", "Kempegowda International Airport", "Bengaluru", "India", "Active" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Airlines_AirlineCode",
                table: "Airlines",
                column: "AirlineCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Airports_AirportCode",
                table: "Airports",
                column: "AirportCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightInstances_FlightId",
                table: "FlightInstances",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_AirlineId",
                table: "Flights",
                column: "AirlineId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_DestinationAirportId",
                table: "Flights",
                column: "DestinationAirportId");

            migrationBuilder.CreateIndex(
                name: "IX_Flights_OriginAirportId",
                table: "Flights",
                column: "OriginAirportId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FlightInstances");

            migrationBuilder.DropTable(
                name: "Flights");

            migrationBuilder.DropTable(
                name: "Airlines");

            migrationBuilder.DropTable(
                name: "Airports");
        }
    }
}
