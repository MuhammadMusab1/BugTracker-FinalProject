using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.Data.Migrations
{
    public partial class AddTicketLogItemClass : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewValue",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "OldValue",
                table: "TicketHistory");

            migrationBuilder.DropColumn(
                name: "Property",
                table: "TicketHistory");

            migrationBuilder.AddColumn<int>(
                name: "TicketLogItemId",
                table: "TicketHistory",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TicketLogItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OldTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldPriority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TicketHistoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketLogItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketLogItem_TicketHistory_TicketHistoryId",
                        column: x => x.TicketHistoryId,
                        principalTable: "TicketHistory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketLogItem_TicketHistoryId",
                table: "TicketLogItem",
                column: "TicketHistoryId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketLogItem");

            migrationBuilder.DropColumn(
                name: "TicketLogItemId",
                table: "TicketHistory");

            migrationBuilder.AddColumn<string>(
                name: "NewValue",
                table: "TicketHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "OldValue",
                table: "TicketHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Property",
                table: "TicketHistory",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
