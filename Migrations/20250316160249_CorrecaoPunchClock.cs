using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaDePontosAPI.Migrations
{
    /// <inheritdoc />
    public partial class CorrecaoPunchClock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PunchClocks_Users_UserId",
                table: "PunchClocks");

            migrationBuilder.DropIndex(
                name: "IX_PunchClocks_UserId",
                table: "PunchClocks");

            migrationBuilder.AddColumn<int>(
                name: "PunchClockType",
                table: "PunchClocks",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PunchClockType",
                table: "PunchClocks");

            migrationBuilder.CreateIndex(
                name: "IX_PunchClocks_UserId",
                table: "PunchClocks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PunchClocks_Users_UserId",
                table: "PunchClocks",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
