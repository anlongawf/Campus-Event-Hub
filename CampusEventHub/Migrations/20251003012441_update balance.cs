using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEventHub.Migrations
{
    /// <inheritdoc />
    public partial class updatebalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Balance",
                table: "Users",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Seats",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Seats_UserId",
                table: "Seats",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Seats_Users_UserId",
                table: "Seats",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Seats_Users_UserId",
                table: "Seats");

            migrationBuilder.DropIndex(
                name: "IX_Seats_UserId",
                table: "Seats");

            migrationBuilder.DropColumn(
                name: "Balance",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Seats");
        }
    }
}
