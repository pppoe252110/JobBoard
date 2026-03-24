using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddNicknameAndPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Nickname",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                schema: "identity",
                table: "users",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Nickname",
                schema: "identity",
                table: "users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                schema: "identity",
                table: "users");
        }
    }
}
