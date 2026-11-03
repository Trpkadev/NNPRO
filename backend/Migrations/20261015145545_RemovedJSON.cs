using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovedJSON : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SelectedOptionIdsJson",
                table: "StudentAnswers");

            migrationBuilder.CreateTable(
                name: "StudentAnswerSelectedOptions",
                columns: table => new
                {
                    AnswerOptionId = table.Column<int>(type: "int", nullable: false),
                    StudentAnswerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswerSelectedOptions", x => new { x.AnswerOptionId, x.StudentAnswerId });
                    table.ForeignKey(
                        name: "FK_StudentAnswerSelectedOptions_AnswerOptions_AnswerOptionId",
                        column: x => x.AnswerOptionId,
                        principalTable: "AnswerOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAnswerSelectedOptions_StudentAnswers_StudentAnswerId",
                        column: x => x.StudentAnswerId,
                        principalTable: "StudentAnswers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswerSelectedOptions_StudentAnswerId",
                table: "StudentAnswerSelectedOptions",
                column: "StudentAnswerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StudentAnswerSelectedOptions");

            migrationBuilder.AddColumn<string>(
                name: "SelectedOptionIdsJson",
                table: "StudentAnswers",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
