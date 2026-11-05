using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class DatatypeChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_CourseInstructors_Users_InstructorsId", table: "CourseInstructors");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_Users_StudentId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_QuizAttempts_Users_StudentId", table: "QuizAttempts");
            migrationBuilder.DropForeignKey(name: "FK_StudentLessonProgresses_Users_StudentId", table: "StudentLessonProgresses");

            migrationBuilder.DropForeignKey(name: "FK_CourseInstructors_Courses_CourseId", table: "CourseInstructors");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_Courses_CourseId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_Sections_Courses_CourseId", table: "Sections");

            migrationBuilder.DropForeignKey(name: "FK_Lessons_Sections_SectionId", table: "Lessons");
            migrationBuilder.DropForeignKey(name: "FK_Quizzes_Sections_SectionId", table: "Quizzes");

            migrationBuilder.DropForeignKey(name: "FK_LessonAttachments_Lessons_LessonId", table: "LessonAttachments");
            migrationBuilder.DropForeignKey(name: "FK_StudentLessonProgresses_Lessons_LessonId", table: "StudentLessonProgresses");

            migrationBuilder.DropForeignKey(name: "FK_Questions_Quizzes_QuizId", table: "Questions");
            migrationBuilder.DropForeignKey(name: "FK_QuizAttempts_Quizzes_QuizId", table: "QuizAttempts");

            migrationBuilder.DropForeignKey(name: "FK_AnswerOptions_Questions_QuestionId", table: "AnswerOptions");
            migrationBuilder.DropForeignKey(name: "FK_StudentAnswers_Questions_QuestionId", table: "StudentAnswers");

            migrationBuilder.DropForeignKey(name: "FK_StudentAnswers_QuizAttempts_QuizAttemptId", table: "StudentAnswers");
            
            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Users",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "StudentId",
                table: "StudentLessonProgresses",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "LessonId",
                table: "StudentLessonProgresses",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "StudentLessonProgresses",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "QuizAttemptId",
                table: "StudentAnswers",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "QuestionId",
                table: "StudentAnswers",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<ushort>(
                name: "AwardedPoints",
                table: "StudentAnswers",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "StudentAnswers",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<ushort>(
                name: "Order",
                table: "Sections",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "CourseId",
                table: "Sections",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Sections",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "TimeLimitSeconds",
                table: "Quizzes",
                type: "int unsigned",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "SectionId",
                table: "Quizzes",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "PassingThresholdPercent",
                table: "Quizzes",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<ushort>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "smallint unsigned",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Quizzes",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "StudentId",
                table: "QuizAttempts",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "ScorePercent",
                table: "QuizAttempts",
                type: "decimal(65,30)",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "double");

            migrationBuilder.AlterColumn<uint>(
                name: "QuizId",
                table: "QuizAttempts",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "QuizAttempts",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "QuizId",
                table: "Questions",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<ushort>(
                name: "Points",
                table: "Questions",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Questions",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "SectionId",
                table: "Lessons",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<ushort>(
                name: "Order",
                table: "Lessons",
                type: "smallint unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Lessons",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "LessonId",
                table: "LessonAttachments",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "LessonAttachments",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "StudentId",
                table: "Enrollments",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "CourseId",
                table: "Enrollments",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Enrollments",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "Courses",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<uint>(
                name: "InstructorsId",
                table: "CourseInstructors",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "CourseId",
                table: "CourseInstructors",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "QuestionId",
                table: "AnswerOptions",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<uint>(
                name: "Id",
                table: "AnswerOptions",
                type: "int unsigned",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
            
            // Vztahy k Users
            migrationBuilder.AddForeignKey(name: "FK_CourseInstructors_Users_InstructorsId", table: "CourseInstructors", column: "InstructorsId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_Users_StudentId", table: "Enrollments", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_QuizAttempts_Users_StudentId", table: "QuizAttempts", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentLessonProgresses_Users_StudentId", table: "StudentLessonProgresses", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Courses
            migrationBuilder.AddForeignKey(name: "FK_CourseInstructors_Courses_CourseId", table: "CourseInstructors", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_Courses_CourseId", table: "Enrollments", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Sections_Courses_CourseId", table: "Sections", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Sections
            migrationBuilder.AddForeignKey(name: "FK_Lessons_Sections_SectionId", table: "Lessons", column: "SectionId", principalTable: "Sections", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Quizzes_Sections_SectionId", table: "Quizzes", column: "SectionId", principalTable: "Sections", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Lessons
            migrationBuilder.AddForeignKey(name: "FK_LessonAttachments_Lessons_LessonId", table: "LessonAttachments", column: "LessonId", principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentLessonProgresses_Lessons_LessonId", table: "StudentLessonProgresses", column: "LessonId", principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Quizzes
            migrationBuilder.AddForeignKey(name: "FK_Questions_Quizzes_QuizId", table: "Questions", column: "QuizId", principalTable: "Quizzes", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_QuizAttempts_Quizzes_QuizId", table: "QuizAttempts", column: "QuizId", principalTable: "Quizzes", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Questions
            migrationBuilder.AddForeignKey(name: "FK_AnswerOptions_Questions_QuestionId", table: "AnswerOptions", column: "QuestionId", principalTable: "Questions", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentAnswers_Questions_QuestionId", table: "StudentAnswers", column: "QuestionId", principalTable: "Questions", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k QuizAttempts
            migrationBuilder.AddForeignKey(name: "FK_StudentAnswers_QuizAttempts_QuizAttemptId", table: "StudentAnswers", column: "QuizAttemptId", principalTable: "QuizAttempts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_CourseInstructors_Users_InstructorsId", table: "CourseInstructors");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_Users_StudentId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_QuizAttempts_Users_StudentId", table: "QuizAttempts");
            migrationBuilder.DropForeignKey(name: "FK_StudentLessonProgresses_Users_StudentId", table: "StudentLessonProgresses");

            migrationBuilder.DropForeignKey(name: "FK_CourseInstructors_Courses_CourseId", table: "CourseInstructors");
            migrationBuilder.DropForeignKey(name: "FK_Enrollments_Courses_CourseId", table: "Enrollments");
            migrationBuilder.DropForeignKey(name: "FK_Sections_Courses_CourseId", table: "Sections");

            migrationBuilder.DropForeignKey(name: "FK_Lessons_Sections_SectionId", table: "Lessons");
            migrationBuilder.DropForeignKey(name: "FK_Quizzes_Sections_SectionId", table: "Quizzes");

            migrationBuilder.DropForeignKey(name: "FK_LessonAttachments_Lessons_LessonId", table: "LessonAttachments");
            migrationBuilder.DropForeignKey(name: "FK_StudentLessonProgresses_Lessons_LessonId", table: "StudentLessonProgresses");

            migrationBuilder.DropForeignKey(name: "FK_Questions_Quizzes_QuizId", table: "Questions");
            migrationBuilder.DropForeignKey(name: "FK_QuizAttempts_Quizzes_QuizId", table: "QuizAttempts");

            migrationBuilder.DropForeignKey(name: "FK_AnswerOptions_Questions_QuestionId", table: "AnswerOptions");
            migrationBuilder.DropForeignKey(name: "FK_StudentAnswers_Questions_QuestionId", table: "StudentAnswers");

            migrationBuilder.DropForeignKey(name: "FK_StudentAnswers_QuizAttempts_QuizAttemptId", table: "StudentAnswers");
            
            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Users",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "StudentLessonProgresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "StudentLessonProgresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StudentLessonProgresses",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "QuizAttemptId",
                table: "StudentAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "StudentAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "AwardedPoints",
                table: "StudentAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "StudentAnswers",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Sections",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Sections",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Sections",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "TimeLimitSeconds",
                table: "Quizzes",
                type: "int",
                nullable: true,
                oldClrType: typeof(uint),
                oldType: "int unsigned",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "Quizzes",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<double>(
                name: "PassingThresholdPercent",
                table: "Quizzes",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<int>(
                name: "MaxAttempts",
                table: "Quizzes",
                type: "int",
                nullable: true,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Quizzes",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<double>(
                name: "ScorePercent",
                table: "QuizAttempts",
                type: "double",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(65,30)");

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "QuizAttempts",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "QuizId",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Points",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Questions",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "SectionId",
                table: "Lessons",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Order",
                table: "Lessons",
                type: "int",
                nullable: false,
                oldClrType: typeof(ushort),
                oldType: "smallint unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Lessons",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "LessonAttachments",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "LessonAttachments",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "StudentId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "Enrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Enrollments",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Courses",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<int>(
                name: "InstructorsId",
                table: "CourseInstructors",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                table: "CourseInstructors",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "QuestionId",
                table: "AnswerOptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "AnswerOptions",
                type: "int",
                nullable: false,
                oldClrType: typeof(uint),
                oldType: "int unsigned")
                .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                .OldAnnotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn);
            
            // Vztahy k Users
            migrationBuilder.AddForeignKey(name: "FK_CourseInstructors_Users_InstructorsId", table: "CourseInstructors", column: "InstructorsId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_Users_StudentId", table: "Enrollments", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_QuizAttempts_Users_StudentId", table: "QuizAttempts", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentLessonProgresses_Users_StudentId", table: "StudentLessonProgresses", column: "StudentId", principalTable: "Users", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Courses
            migrationBuilder.AddForeignKey(name: "FK_CourseInstructors_Courses_CourseId", table: "CourseInstructors", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Enrollments_Courses_CourseId", table: "Enrollments", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Sections_Courses_CourseId", table: "Sections", column: "CourseId", principalTable: "Courses", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Sections
            migrationBuilder.AddForeignKey(name: "FK_Lessons_Sections_SectionId", table: "Lessons", column: "SectionId", principalTable: "Sections", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_Quizzes_Sections_SectionId", table: "Quizzes", column: "SectionId", principalTable: "Sections", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Lessons
            migrationBuilder.AddForeignKey(name: "FK_LessonAttachments_Lessons_LessonId", table: "LessonAttachments", column: "LessonId", principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentLessonProgresses_Lessons_LessonId", table: "StudentLessonProgresses", column: "LessonId", principalTable: "Lessons", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Quizzes
            migrationBuilder.AddForeignKey(name: "FK_Questions_Quizzes_QuizId", table: "Questions", column: "QuizId", principalTable: "Quizzes", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_QuizAttempts_Quizzes_QuizId", table: "QuizAttempts", column: "QuizId", principalTable: "Quizzes", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k Questions
            migrationBuilder.AddForeignKey(name: "FK_AnswerOptions_Questions_QuestionId", table: "AnswerOptions", column: "QuestionId", principalTable: "Questions", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
            migrationBuilder.AddForeignKey(name: "FK_StudentAnswers_Questions_QuestionId", table: "StudentAnswers", column: "QuestionId", principalTable: "Questions", principalColumn: "Id", onDelete: ReferentialAction.Cascade);

            // Vztahy k QuizAttempts
            migrationBuilder.AddForeignKey(name: "FK_StudentAnswers_QuizAttempts_QuizAttemptId", table: "StudentAnswers", column: "QuizAttemptId", principalTable: "QuizAttempts", principalColumn: "Id", onDelete: ReferentialAction.Cascade);
        }
    }
}
