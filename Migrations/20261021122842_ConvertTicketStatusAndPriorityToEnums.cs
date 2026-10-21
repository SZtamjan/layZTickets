using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace layZTickets.Migrations
{
    /// <inheritdoc />
    public partial class ConvertTicketStatusAndPriorityToEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert names before SQL Server changes the columns to int.
            // Unknown values stop the migration instead of silently changing their meaning.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Tickets]
                    WHERE [Status] NOT IN (N'New', N'InProgress', N'Pending', N'Closed'))
                    THROW 50001, 'Cannot convert an unknown ticket status.', 1;

                IF EXISTS (SELECT 1 FROM [Tickets]
                    WHERE [Priority] NOT IN (N'Low', N'Normal', N'High', N'Critical', N'Highest'))
                    THROW 50002, 'Cannot convert an unknown ticket priority.', 1;

                UPDATE [Tickets]
                SET [Status] = CASE [Status]
                        WHEN N'New' THEN N'0'
                        WHEN N'InProgress' THEN N'1'
                        WHEN N'Pending' THEN N'2'
                        WHEN N'Closed' THEN N'3'
                    END,
                    [Priority] = CASE [Priority]
                        WHEN N'Low' THEN N'0'
                        WHEN N'Normal' THEN N'1'
                        WHEN N'High' THEN N'2'
                        WHEN N'Critical' THEN N'3'
                        WHEN N'Highest' THEN N'3'
                    END;
                """);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Tickets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<int>(
                name: "Priority",
                table: "Tickets",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Tickets]
                    WHERE [Status] NOT IN (0, 1, 2, 3) OR [Priority] NOT IN (0, 1, 2, 3))
                    THROW 50003, 'Cannot restore an unknown ticket status or priority.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Priority",
                table: "Tickets",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // The previous application used Critical for the highest priority.
            migrationBuilder.Sql("""
                UPDATE [Tickets]
                SET [Status] = CASE [Status]
                        WHEN N'0' THEN N'New'
                        WHEN N'1' THEN N'InProgress'
                        WHEN N'2' THEN N'Pending'
                        WHEN N'3' THEN N'Closed'
                    END,
                    [Priority] = CASE [Priority]
                        WHEN N'0' THEN N'Low'
                        WHEN N'1' THEN N'Normal'
                        WHEN N'2' THEN N'High'
                        WHEN N'3' THEN N'Critical'
                    END;
                """);
        }
    }
}
