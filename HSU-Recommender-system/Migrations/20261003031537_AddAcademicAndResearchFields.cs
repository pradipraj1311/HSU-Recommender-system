using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSU_Recommender_system.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicAndResearchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CarnegieClassification",
                table: "Universities",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "StudentFacultyRatio",
                table: "Universities",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGRERequired",
                table: "AcademicPrograms",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "OpenAlexInstitutionId",
                table: "AcademicPrograms",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetGREQuant",
                table: "AcademicPrograms",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarnegieClassification",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "StudentFacultyRatio",
                table: "Universities");

            migrationBuilder.DropColumn(
                name: "IsGRERequired",
                table: "AcademicPrograms");

            migrationBuilder.DropColumn(
                name: "OpenAlexInstitutionId",
                table: "AcademicPrograms");

            migrationBuilder.DropColumn(
                name: "TargetGREQuant",
                table: "AcademicPrograms");
        }
    }
}
