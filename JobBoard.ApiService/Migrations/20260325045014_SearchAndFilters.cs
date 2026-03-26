using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobBoard.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class SearchAndFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRemote",
                schema: "vacancies",
                table: "vacancies",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                schema: "vacancies",
                table: "vacancies",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ExpectedSalary",
                schema: "resumes",
                table: "resumes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                schema: "resumes",
                table: "resumes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "UpdatedAt",
                schema: "resumes",
                table: "resumes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRemote",
                schema: "vacancies",
                table: "vacancies");

            migrationBuilder.DropColumn(
                name: "Location",
                schema: "vacancies",
                table: "vacancies");

            migrationBuilder.DropColumn(
                name: "ExpectedSalary",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                schema: "resumes",
                table: "resumes");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                schema: "resumes",
                table: "resumes");
        }
    }
}
