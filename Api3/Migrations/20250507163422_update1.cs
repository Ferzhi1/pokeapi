using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "UsuariosPokemonApi",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuariosPokemonApi", x => x.Id);
                });
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatPokemon");

            migrationBuilder.DropTable(
                name: "UsuariosPokemonApi");

            migrationBuilder.DropTable(
                name: "ColeccionPokemon");

            migrationBuilder.DropTable(
                name: "ProductoPokemon");

            migrationBuilder.DropTable(
                name: "MazoPokemon");

            migrationBuilder.DropTable(
                name: "PedidoPokemon");

            migrationBuilder.DropTable(
                name: "PedidoUsuario");
        }
    }
}
