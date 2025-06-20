using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class newmethodcollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Puja_ProductoPokemon_ProductoPokemonId",
                table: "Puja");

            migrationBuilder.DropIndex(
                name: "IX_Puja_ProductoPokemonId",
                table: "Puja");

            migrationBuilder.DropColumn(
                name: "ProductoPokemonId",
                table: "Puja");

            migrationBuilder.AddColumn<int>(
                name: "NumeroAlbum",
                table: "ColeccionPokemon",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Puja_PokemonId",
                table: "Puja",
                column: "PokemonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Puja_ProductoPokemon_PokemonId",
                table: "Puja",
                column: "PokemonId",
                principalTable: "ProductoPokemon",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Puja_ProductoPokemon_PokemonId",
                table: "Puja");

            migrationBuilder.DropIndex(
                name: "IX_Puja_PokemonId",
                table: "Puja");

            migrationBuilder.DropColumn(
                name: "NumeroAlbum",
                table: "ColeccionPokemon");

            migrationBuilder.AddColumn<int>(
                name: "ProductoPokemonId",
                table: "Puja",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Puja_ProductoPokemonId",
                table: "Puja",
                column: "ProductoPokemonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Puja_ProductoPokemon_ProductoPokemonId",
                table: "Puja",
                column: "ProductoPokemonId",
                principalTable: "ProductoPokemon",
                principalColumn: "Id");
        }
    }
}
