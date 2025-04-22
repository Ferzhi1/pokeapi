using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class update8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPokemon_PedidoPokemon_PedidoPokemonid",
                table: "ProductoPokemon");

            migrationBuilder.RenameColumn(
                name: "PedidoPokemonid",
                table: "ProductoPokemon",
                newName: "PedidoPokemonId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPokemon_PedidoPokemonid",
                table: "ProductoPokemon",
                newName: "IX_ProductoPokemon_PedidoPokemonId");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "PedidoPokemon",
                newName: "Id");

            migrationBuilder.AlterColumn<int>(
                name: "PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                column: "PedidosUsuariosPokeId",
                principalTable: "PedidoUsuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPokemon_PedidoPokemon_PedidoPokemonId",
                table: "ProductoPokemon",
                column: "PedidoPokemonId",
                principalTable: "PedidoPokemon",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductoPokemon_PedidoPokemon_PedidoPokemonId",
                table: "ProductoPokemon");

            migrationBuilder.RenameColumn(
                name: "PedidoPokemonId",
                table: "ProductoPokemon",
                newName: "PedidoPokemonid");

            migrationBuilder.RenameIndex(
                name: "IX_ProductoPokemon_PedidoPokemonId",
                table: "ProductoPokemon",
                newName: "IX_ProductoPokemon_PedidoPokemonid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PedidoPokemon",
                newName: "id");

            migrationBuilder.AlterColumn<int>(
                name: "PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoPokemon_PedidoUsuario_PedidosUsuariosPokeId",
                table: "PedidoPokemon",
                column: "PedidosUsuariosPokeId",
                principalTable: "PedidoUsuario",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductoPokemon_PedidoPokemon_PedidoPokemonid",
                table: "ProductoPokemon",
                column: "PedidoPokemonid",
                principalTable: "PedidoPokemon",
                principalColumn: "id");
        }
    }
}
