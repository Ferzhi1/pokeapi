using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioInicial",
                table: "ProductoPokemon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PujaActual",
                table: "ProductoPokemon",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "TiempoExpiracion",
                table: "ProductoPokemon",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateTable(
                name: "Puja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PokemonId = table.Column<int>(type: "int", nullable: false),
                    UsuarioEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CantidadMonedas = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPuja = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProductoPokemonId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Puja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Puja_ProductoPokemon_ProductoPokemonId",
                        column: x => x.ProductoPokemonId,
                        principalTable: "ProductoPokemon",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Puja_ProductoPokemonId",
                table: "Puja",
                column: "ProductoPokemonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Puja");

            migrationBuilder.DropColumn(
                name: "PrecioInicial",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "PujaActual",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "TiempoExpiracion",
                table: "ProductoPokemon");
        }
    }
}
