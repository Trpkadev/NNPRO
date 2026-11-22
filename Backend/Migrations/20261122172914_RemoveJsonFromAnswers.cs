using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveJsonFromAnswers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                    name: "SelectedOptionIds",
                    table: "StudentAnswers",
                    type: "longtext",
                    nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Pokud je v SelectedOptionIdsJson už validní JSON pole jako "[1,2]", stačí prostý přesun:
            migrationBuilder.Sql("""
                                 UPDATE `StudentAnswers`
                                 SET `SelectedOptionIds` = `SelectedOptionIdsJson`
                                 WHERE `SelectedOptionIdsJson` IS NOT NULL AND `SelectedOptionIdsJson` <> '';
                                 """);

            migrationBuilder.DropColumn(
                name: "SelectedOptionIdsJson",
                table: "StudentAnswers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                    name: "SelectedOptionIdsJson",
                    table: "StudentAnswers",
                    type: "varchar(1024)",
                    maxLength: 1024,
                    nullable: false,
                    defaultValue: "[]")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                                 UPDATE `StudentAnswers`
                                 SET `SelectedOptionIdsJson` = COALESCE(`SelectedOptionIds`, '[]');
                                 """);

            migrationBuilder.DropColumn(
                name: "SelectedOptionIds",
                table: "StudentAnswers");
        }
    }
}
