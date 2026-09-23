using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HSU_Recommender_system.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentProfileSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "StudentProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CGPA = table.Column<double>(type: "float", nullable: false),
                    EnglishProficiencyScore = table.Column<double>(type: "float", nullable: false),
                    MaximumBudget = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NumberOfProjects = table.Column<int>(type: "int", nullable: false),
                    NumberOfResearchPapers = table.Column<int>(type: "int", nullable: false),
                    WorkExperienceMonths = table.Column<int>(type: "int", nullable: false),
                    TargetDegree = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreferredCountry = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentProfiles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StudentProfiles_UserId",
                table: "StudentProfiles",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentProfiles");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
