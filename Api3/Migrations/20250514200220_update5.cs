using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "passwordResetTokens");

            migrationBuilder.AddColumn<string>(
                name: "ResetPasswordToken",
                table: "UsuariosPokemonApi",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResetPasswordTokenExpiry",
                table: "UsuariosPokemonApi",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResetPasswordToken",
                table: "UsuariosPokemonApi");

            migrationBuilder.DropColumn(
                name: "ResetPasswordTokenExpiry",
                table: "UsuariosPokemonApi");

            migrationBuilder.CreateTable(
                name: "passwordResetTokens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TokenHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Used = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_passwordResetTokens", x => x.Id);
                });
        }
    }
}
