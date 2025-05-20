using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CorreoValidado",
                table: "UsuariosPokemonApi",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorreoValidado",
                table: "UsuariosPokemonApi");
        }
    }
}
