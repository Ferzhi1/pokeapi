using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreguntaSeguridad",
                table: "UsuariosPokemonApi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RespuestaSeguridad",
                table: "UsuariosPokemonApi",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreguntaSeguridad",
                table: "UsuariosPokemonApi");

            migrationBuilder.DropColumn(
                name: "RespuestaSeguridad",
                table: "UsuariosPokemonApi");
        }
    }
}
