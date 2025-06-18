using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class subastafinalizar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UltimoDueno",
                table: "ProductoPokemon",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimoDueno",
                table: "ProductoPokemon");
        }
    }
}
