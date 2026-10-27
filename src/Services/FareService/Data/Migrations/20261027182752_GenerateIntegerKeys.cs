using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FareService.Data.Migrations
{
    /// <inheritdoc />
    public partial class GenerateIntegerKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DECLARE @start int = ISNULL((SELECT MAX([FareId]) + 1 FROM [dbo].[Fares]), 1); DECLARE @sql nvarchar(max) = N'CREATE SEQUENCE [dbo].[FareIdSequence] AS int START WITH ' + CONVERT(nvarchar(20), @start) + N' INCREMENT BY 1;'; EXEC sys.sp_executesql @sql;");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Fares] ADD CONSTRAINT [DF_Fares_FareId] DEFAULT (NEXT VALUE FOR [dbo].[FareIdSequence]) FOR [FareId];");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[Fares] DROP CONSTRAINT [DF_Fares_FareId];");
            migrationBuilder.Sql("DROP SEQUENCE [dbo].[FareIdSequence];");
        }
    }
}
