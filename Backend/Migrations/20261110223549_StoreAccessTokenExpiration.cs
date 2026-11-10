using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class StoreAccessTokenExpiration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AccessTokenExpiresAtUtc",
                table: "RefreshTokens",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.Sql($"UPDATE `RefreshTokens` SET `AccessTokenExpiresAtUtc` = `ExpiresAtUtc` WHERE `AccessTokenExpiresAtUtc` IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccessTokenExpiresAtUtc",
                table: "RefreshTokens");
        }
    }
}
