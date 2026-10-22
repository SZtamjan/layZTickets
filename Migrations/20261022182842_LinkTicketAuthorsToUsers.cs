using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace layZTickets.Migrations
{
    /// <inheritdoc />
    public partial class LinkTicketAuthorsToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Legacy authors were free text. Discard orphaned sample data rather than
            // incorrectly attributing it to an existing account. Accounts are preserved.
            migrationBuilder.Sql("""
                DELETE comment
                FROM [TicketComments] AS comment
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUsers] AS account WHERE account.[Id] = comment.[AuthorUserId]);

                DELETE ticket
                FROM [Tickets] AS ticket
                WHERE NOT EXISTS (
                    SELECT 1 FROM [AspNetUsers] AS account WHERE account.[Id] = ticket.[CreatedByUserId]);

                UPDATE ticket SET [AssignedAgentId] = NULL
                FROM [Tickets] AS ticket
                WHERE [AssignedAgentId] IS NOT NULL AND NOT EXISTS (
                    SELECT 1 FROM [AspNetUsers] AS account WHERE account.[Id] = ticket.[AssignedAgentId]);
                """);

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedAgentId",
                table: "Tickets",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "TicketComments",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_AssignedAgentId",
                table: "Tickets",
                column: "AssignedAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_Tickets_CreatedByUserId",
                table: "Tickets",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_AuthorUserId",
                table: "TicketComments",
                column: "AuthorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_AspNetUsers_AuthorUserId",
                table: "TicketComments",
                column: "AuthorUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedAgentId",
                table: "Tickets",
                column: "AssignedAgentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tickets_AspNetUsers_CreatedByUserId",
                table: "Tickets",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_AspNetUsers_AuthorUserId",
                table: "TicketComments");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_AssignedAgentId",
                table: "Tickets");

            migrationBuilder.DropForeignKey(
                name: "FK_Tickets_AspNetUsers_CreatedByUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_AssignedAgentId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_Tickets_CreatedByUserId",
                table: "Tickets");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_AuthorUserId",
                table: "TicketComments");

            migrationBuilder.AlterColumn<string>(
                name: "CreatedByUserId",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "AssignedAgentId",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AuthorUserId",
                table: "TicketComments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
