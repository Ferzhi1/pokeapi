using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)

        {
           migrationBuilder.AlterColumn<bool>(
             name: "CorreoValidado",
             table: "UsuariosPokemonApi",
             nullable: false,
             defaultValue: true);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
             name: "CorreoValidado",
             table: "UsuariosPokemonApi",
             nullable: false,
             defaultValue: false);

        }
    }
}
