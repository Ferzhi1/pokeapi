using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api3.Migrations
{
    /// <inheritdoc />
    public partial class finalsubatas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fechaInicioSubasta",
                table: "ProductoPokemon",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fechaInicioSubasta",
                table: "ProductoPokemon");
        }
    }
}
