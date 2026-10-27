using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingService.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenerateIntegerKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([BookingId]) + 1 FROM [dbo].[Bookings]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[BookingIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([PassengerId]) + 1 FROM [dbo].[Passengers]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[PassengerIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Bookings] ADD CONSTRAINT [DF_Bookings_BookingId] DEFAULT (NEXT VALUE FOR [dbo].[BookingIdSequence]) FOR [BookingId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Passengers] ADD CONSTRAINT [DF_Passengers_PassengerId] DEFAULT (NEXT VALUE FOR [dbo].[PassengerIdSequence]) FOR [PassengerId];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[Passengers] DROP CONSTRAINT [DF_Passengers_PassengerId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Bookings] DROP CONSTRAINT [DF_Bookings_BookingId];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[PassengerIdSequence];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[BookingIdSequence];");
        }
    }
}
