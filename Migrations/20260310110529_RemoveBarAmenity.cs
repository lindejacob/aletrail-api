using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aletrail_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveBarAmenity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Amenity",
                table: "Bars");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Amenity",
                table: "Bars",
                type: "text",
                nullable: true);
        }
    }
}
