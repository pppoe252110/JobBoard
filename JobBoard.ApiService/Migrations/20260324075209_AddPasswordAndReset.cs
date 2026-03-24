using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordAndReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "VerificationCode",
                schema: "identity",
                table: "users",
                newName: "PasswordResetToken");

            migrationBuilder.RenameColumn(
                name: "CodeExpiresAt",
                schema: "identity",
                table: "users",
                newName: "ResetTokenExpiresAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ResetTokenExpiresAt",
                schema: "identity",
                table: "users",
                newName: "CodeExpiresAt");

            migrationBuilder.RenameColumn(
                name: "PasswordResetToken",
                schema: "identity",
                table: "users",
                newName: "VerificationCode");
        }
    }
}
