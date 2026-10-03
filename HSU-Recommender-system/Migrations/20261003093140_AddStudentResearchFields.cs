using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSU_Recommender_system.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentResearchFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GREQuantScore",
                table: "StudentProfiles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ResearchInterests",
                table: "StudentProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GREQuantScore",
                table: "StudentProfiles");

            migrationBuilder.DropColumn(
                name: "ResearchInterests",
                table: "StudentProfiles");
        }
    }
}
