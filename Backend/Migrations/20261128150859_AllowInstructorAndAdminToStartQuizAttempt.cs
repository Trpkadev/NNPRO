using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AllowInstructorAndAdminToStartQuizAttempt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_StudentId",
                table: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "StudentAnswers");

            migrationBuilder.RenameColumn(
                name: "StudentId",
                table: "QuizAttempts",
                newName: "AttempterId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_StudentId_QuizId",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_AttempterId_QuizId");

            migrationBuilder.CreateTable(
                name: "AttempterAnswers",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuizAttemptId = table.Column<uint>(type: "int unsigned", nullable: false),
                    QuestionId = table.Column<uint>(type: "int unsigned", nullable: false),
                    SelectedOptionIds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsFullyCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    AwardedPoints = table.Column<ushort>(type: "smallint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttempterAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttempterAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AttempterAnswers_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_AttempterAnswers_QuestionId",
                table: "AttempterAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AttempterAnswers_QuizAttemptId_QuestionId",
                table: "AttempterAnswers",
                columns: new[] { "QuizAttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_AttempterId",
                table: "QuizAttempts",
                column: "AttempterId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_QuizAttempts_Users_AttempterId",
                table: "QuizAttempts");

            migrationBuilder.DropTable(
                name: "AttempterAnswers");

            migrationBuilder.RenameColumn(
                name: "AttempterId",
                table: "QuizAttempts",
                newName: "StudentId");

            migrationBuilder.RenameIndex(
                name: "IX_QuizAttempts_AttempterId_QuizId",
                table: "QuizAttempts",
                newName: "IX_QuizAttempts_StudentId_QuizId");

            migrationBuilder.CreateTable(
                name: "StudentAnswers",
                columns: table => new
                {
                    Id = table.Column<uint>(type: "int unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    QuestionId = table.Column<uint>(type: "int unsigned", nullable: false),
                    QuizAttemptId = table.Column<uint>(type: "int unsigned", nullable: false),
                    AwardedPoints = table.Column<ushort>(type: "smallint unsigned", nullable: false),
                    IsFullyCorrect = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SelectedOptionIds = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAnswers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_Questions_QuestionId",
                        column: x => x.QuestionId,
                        principalTable: "Questions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentAnswers_QuizAttempts_QuizAttemptId",
                        column: x => x.QuizAttemptId,
                        principalTable: "QuizAttempts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_QuestionId",
                table: "StudentAnswers",
                column: "QuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAnswers_QuizAttemptId_QuestionId",
                table: "StudentAnswers",
                columns: new[] { "QuizAttemptId", "QuestionId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_QuizAttempts_Users_StudentId",
                table: "QuizAttempts",
                column: "StudentId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
