using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Data.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureUserIdIdentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK_UserRoles_Users_UserId]; ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [PK_UserRoles]; ALTER TABLE [dbo].[Users] DROP CONSTRAINT [PK_Users];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] ADD [UserId_Original] int NULL;");
            migrationBuilder.Sql("UPDATE [dbo].[Users] SET [UserId_Original] = [UserId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] ADD [UserId_New] int IDENTITY(1,1) NOT NULL;");
            migrationBuilder.Sql("UPDATE ur SET ur.[UserId] = u.[UserId_New] FROM [dbo].[UserRoles] AS ur INNER JOIN [dbo].[Users] AS u ON ur.[UserId] = u.[UserId_Original];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] DROP COLUMN [UserId];");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.Users.UserId_New', N'UserId', N'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] DROP COLUMN [UserId_Original];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]); ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]); ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [FK_UserRoles_Users_UserId]; ALTER TABLE [dbo].[UserRoles] DROP CONSTRAINT [PK_UserRoles]; ALTER TABLE [dbo].[Users] DROP CONSTRAINT [PK_Users];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] ADD [UserId_Original] int NULL;");
            migrationBuilder.Sql("UPDATE [dbo].[Users] SET [UserId_Original] = [UserId];");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] DROP COLUMN [UserId];");
            migrationBuilder.Sql("EXEC sp_rename N'dbo.Users.UserId_Original', N'UserId', N'COLUMN';");
            migrationBuilder.Sql("ALTER TABLE [dbo].[Users] ADD CONSTRAINT [PK_Users] PRIMARY KEY ([UserId]); ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId]); ALTER TABLE [dbo].[UserRoles] ADD CONSTRAINT [FK_UserRoles_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId]) ON DELETE CASCADE;");
        }
    }
}
