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
            migrationBuilder.AddColumn<int>(
                name: "ColeccionPokemonId",
                table: "StatPokemon",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ColeccionPokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Rareza = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailUsuario = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ColeccionPokemon", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StatPokemon_ColeccionPokemonId",
                table: "StatPokemon",
                column: "ColeccionPokemonId");

            migrationBuilder.AddForeignKey(
                name: "FK_StatPokemon_ColeccionPokemon_ColeccionPokemonId",
                table: "StatPokemon",
                column: "ColeccionPokemonId",
                principalTable: "ColeccionPokemon",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StatPokemon_ColeccionPokemon_ColeccionPokemonId",
                table: "StatPokemon");

            migrationBuilder.DropTable(
                name: "ColeccionPokemon");

            migrationBuilder.DropIndex(
                name: "IX_StatPokemon_ColeccionPokemonId",
                table: "StatPokemon");

            migrationBuilder.DropColumn(
                name: "ColeccionPokemonId",
                table: "StatPokemon");
        }
    }
}
