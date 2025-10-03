using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CampusEventHub.Migrations
{
    /// <inheritdoc />
    public partial class dotnetefdatabaseupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "QRCode",
                table: "Checkins",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QRCode",
                table: "Checkins");
        }
    }
}
