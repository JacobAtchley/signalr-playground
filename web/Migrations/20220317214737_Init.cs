using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace web.Migrations
{
    public partial class Init : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    ConnectionId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastConnectedDate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.ConnectionId);
                });

            migrationBuilder.CreateTable(
                name: "PeopleEventSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ConnectionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Trigger = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Filter = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeopleEventSubscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PeopleEventSubscriptions_UserSessions_ConnectionId",
                        column: x => x.ConnectionId,
                        principalTable: "UserSessions",
                        principalColumn: "ConnectionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PeopleEventSubscriptions_ConnectionId",
                table: "PeopleEventSubscriptions",
                column: "ConnectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PeopleEventSubscriptions");

            migrationBuilder.DropTable(
                name: "UserSessions");
        }
    }
}
