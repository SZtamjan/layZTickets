using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace layZTickets.Migrations
{
    /// <inheritdoc />
    public partial class AddAgentCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AgentCategory",
                table: "AspNetUsers",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AgentCategory",
                table: "AspNetUsers");
        }
    }
}
