using System.Collections.Generic;
using JobBoard.ApiService.Features.Resumes.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateResumeDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "ContactPhone",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.AddColumn<List<ContactMethodDto>>(
                name: "ContactMethods",
                schema: "resumes",
                table: "resumes",
                type: "jsonb",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactMethods",
                schema: "resumes",
                table: "resumes");

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
        }
    }
}
