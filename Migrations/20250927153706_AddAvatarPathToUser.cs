using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PricePulse.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarPathToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarPath",
                table: "users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "prices",
                type: "varchar(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarPath",
                table: "users");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "prices");
        }
    }
}
