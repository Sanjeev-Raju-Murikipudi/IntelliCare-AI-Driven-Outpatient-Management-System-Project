using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IntelliCare.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addeddoctorimagefield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "Doctor",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Education",
                table: "Doctor",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExperienceYears",
                table: "Doctor",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhotoUrl",
                table: "Doctor",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "Education",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "ExperienceYears",
                table: "Doctor");

            migrationBuilder.DropColumn(
                name: "PhotoUrl",
                table: "Doctor");
        }
    }
}
