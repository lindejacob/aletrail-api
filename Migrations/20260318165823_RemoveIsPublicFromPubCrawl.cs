using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace aletrail_api.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIsPublicFromPubCrawl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "PubCrawlRoutes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "PubCrawlRoutes",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
