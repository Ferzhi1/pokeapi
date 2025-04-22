using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailUsuario",
                table: "ProductoPokemon",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "EnVenta",
                table: "ProductoPokemon",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MazoPokemonId",
                table: "ProductoPokemon",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MazoPokemon",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImagenUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MazoPokemon", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PedidoUsuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MazoSeleccionado = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoUsuario", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductoPokemon_MazoPokemonId",
                table: "ProductoPokemon",
                column: "MazoPokemonId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoPokemon_PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                column: "PedidosUsuariosPokeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                column: "PedidosUsuariosPokeId",
                principalTable: "PedidoUsuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPokemon_MazoPokemon_MazoPokemonId",
                table: "ProductoPokemon",
                column: "MazoPokemonId",
                principalTable: "MazoPokemon",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPokemon_MazoPokemon_MazoPokemonId",
                table: "ProductoPokemon");

            migrationBuilder.DropTable(
                name: "MazoPokemon");

            migrationBuilder.DropTable(
                name: "PedidoUsuario");

            migrationBuilder.DropIndex(
                name: "IX_ProductoPokemon_MazoPokemonId",
                table: "ProductoPokemon");

            migrationBuilder.DropIndex(
                name: "IX_PedidoPokemon_PedidosUsuariosPokeId",
                table: "PedidoPokemon");

            migrationBuilder.DropColumn(
                name: "EmailUsuario",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "EnVenta",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "MazoPokemonId",
                table: "ProductoPokemon");

            migrationBuilder.DropColumn(
                name: "PedidosUsuariosPokeId",
                table: "PedidoPokemon");
        }
    }
}
