using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class numberpokedex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PokemonIdOriginal",
                table: "ProductoPokemon",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PokemonIdOriginal",
                table: "ColeccionPokemon",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PokemonIdOriginal",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "PokemonIdOriginal",
                table: "ColeccionPokemon");
        }
    }
}
