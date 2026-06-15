using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MitHra.API.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCodigoBarras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoBarras",
                table: "Productos",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoBarras",
                table: "Productos");
        }
    }
}
