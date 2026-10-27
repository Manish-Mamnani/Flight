using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SearchService.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenerateIntegerKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([FlightId]) + 1 FROM [dbo].[Flights]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[FlightIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([FlightInstanceId]) + 1 FROM [dbo].[FlightInstances]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[FlightInstanceIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Flights] ADD CONSTRAINT [DF_Flights_FlightId] DEFAULT (NEXT VALUE FOR [dbo].[FlightIdSequence]) FOR [FlightId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[FlightInstances] ADD CONSTRAINT [DF_FlightInstances_FlightInstanceId] DEFAULT (NEXT VALUE FOR [dbo].[FlightInstanceIdSequence]) FOR [FlightInstanceId];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[FlightInstances] DROP CONSTRAINT [DF_FlightInstances_FlightInstanceId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Flights] DROP CONSTRAINT [DF_Flights_FlightId];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[FlightInstanceIdSequence];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[FlightIdSequence];");
        }
    }
}
