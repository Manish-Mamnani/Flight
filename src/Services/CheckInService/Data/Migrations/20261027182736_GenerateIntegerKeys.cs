using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CheckInService.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenerateIntegerKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([SeatId]) + 1 FROM [dbo].[FlightSeatInventory]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[SeatIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("ALTER TABLE [dbo].[FlightSeatInventory] ADD CONSTRAINT [DF_FlightSeatInventory_SeatId] DEFAULT (NEXT VALUE FOR [dbo].[SeatIdSequence]) FOR [SeatId];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[FlightSeatInventory] DROP CONSTRAINT [DF_FlightSeatInventory_SeatId];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[SeatIdSequence];");
        }
    }
}
