using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Talabat.Reposatory.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultPictureUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Product_PictureUrl",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Product_PictureUrl",
                table: "OrderItems");
        }
    }
}
