using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddResumeDetailsAndTechnologies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Technologies",
                schema: "resumes",
                table: "work_experiences",
                type: "jsonb",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                schema: "resumes",
                table: "resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPhone",
                schema: "resumes",
                table: "resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "resumes",
                table: "resumes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "resumes",
                table: "resumes",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Technologies",
                schema: "resumes",
                table: "work_experiences");

            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "resumes",
                table: "resumes");
        }
    }
}
