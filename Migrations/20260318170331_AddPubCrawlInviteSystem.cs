using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace aletrail_api.Migrations
{
    /// <inheritdoc />
    public partial class AddPubCrawlInviteSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InviteCode",
                table: "PubCrawlRoutes",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""PubCrawlRoutes""
                SET ""InviteCode"" = substring(md5(random()::text || ""Id""::text) from 1 for 8)
                WHERE ""InviteCode"" IS NULL;
            ");

            migrationBuilder.AlterColumn<string>(
                name: "InviteCode",
                table: "PubCrawlRoutes",
                type: "text",
                nullable: false);

            migrationBuilder.CreateTable(
                name: "PubCrawlParticipants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PubCrawlRouteId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PubCrawlParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PubCrawlParticipants_PubCrawlRoutes_PubCrawlRouteId",
                        column: x => x.PubCrawlRouteId,
                        principalTable: "PubCrawlRoutes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PubCrawlParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PubCrawlRoutes_InviteCode",
                table: "PubCrawlRoutes",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PubCrawlParticipants_PubCrawlRouteId_UserId",
                table: "PubCrawlParticipants",
                columns: new[] { "PubCrawlRouteId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PubCrawlParticipants_UserId",
                table: "PubCrawlParticipants",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PubCrawlParticipants");

            migrationBuilder.DropIndex(
                name: "IX_PubCrawlRoutes_InviteCode",
                table: "PubCrawlRoutes");

            migrationBuilder.DropColumn(
                name: "InviteCode",
                table: "PubCrawlRoutes");
        }
    }
}
