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
                name: "PedidoPokemon",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreMazo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FechaPedido = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoPokemon", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ProductoPokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rareza = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PedidoPokemonid = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoPokemon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductoPokemon_PedidoPokemon_PedidoPokemonid",
                        column: x => x.PedidoPokemonid,
                        principalTable: "PedidoPokemon",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "StatPokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Valor = table.Column<int>(type: "int", nullable: false),
                    ProductoPokemonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatPokemon", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StatPokemon_ProductoPokemon_ProductoPokemonId",
                        column: x => x.ProductoPokemonId,
                        principalTable: "ProductoPokemon",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPokemon_PedidoPokemonid",
                table: "ProductoPokemon",
                column: "PedidoPokemonid");

            migrationBuilder.CreateIndex(
                name: "IX_StatPokemon_ProductoPokemonId",
                table: "StatPokemon",
                column: "ProductoPokemonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StatPokemon");

            migrationBuilder.DropTable(
                name: "ProductoPokemon");

            migrationBuilder.DropTable(
                name: "PedidoPokemon");
        }
    }
}
