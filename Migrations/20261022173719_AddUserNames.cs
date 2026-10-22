using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace layZTickets.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                table: "AspNetUsers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Uzgodnione dane demonstracyjne dla kont istniejących przed tą migracją.
            migrationBuilder.Sql("""
                UPDATE [users]
                SET [FirstName] = CASE WHEN [admins].[UserId] IS NOT NULL THEN N'Admin' ELSE N'Jan' END,
                    [LastName] = CASE WHEN [admins].[UserId] IS NOT NULL THEN N'Admin' ELSE N'Kowalski' END
                FROM [AspNetUsers] AS [users]
                LEFT JOIN (
                    SELECT DISTINCT [userRoles].[UserId]
                    FROM [AspNetUserRoles] AS [userRoles]
                    INNER JOIN [AspNetRoles] AS [roles] ON [roles].[Id] = [userRoles].[RoleId]
                    WHERE [roles].[NormalizedName] = N'ADMIN'
                ) AS [admins] ON [admins].[UserId] = [users].[Id];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LastName",
                table: "AspNetUsers");
        }
    }
}
