using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SpotifyApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlaylistsAndUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    IsPublic = table.Column<int>(type: "integer", nullable: false),
                    OwnerId = table.Column<int>(type: "integer", nullable: false),
                    SongIds = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistCoCreators",
                columns: table => new
                {
                    CollaboratingPlaylistsId = table.Column<int>(type: "integer", nullable: false),
                    CollaboratorsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistCoCreators", x => new { x.CollaboratingPlaylistsId, x.CollaboratorsId });
                    table.ForeignKey(
                        name: "FK_PlaylistCoCreators_Playlists_CollaboratingPlaylistsId",
                        column: x => x.CollaboratingPlaylistsId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistCoCreators_Users_CollaboratorsId",
                        column: x => x.CollaboratorsId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserFavoritePlaylists",
                columns: table => new
                {
                    FavoritePlaylistsId = table.Column<int>(type: "integer", nullable: false),
                    FavoritedByUsersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavoritePlaylists", x => new { x.FavoritePlaylistsId, x.FavoritedByUsersId });
                    table.ForeignKey(
                        name: "FK_UserFavoritePlaylists_Playlists_FavoritePlaylistsId",
                        column: x => x.FavoritePlaylistsId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavoritePlaylists_Users_FavoritedByUsersId",
                        column: x => x.FavoritedByUsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistCoCreators_CollaboratorsId",
                table: "PlaylistCoCreators",
                column: "CollaboratorsId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_OwnerId",
                table: "Playlists",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavoritePlaylists_FavoritedByUsersId",
                table: "UserFavoritePlaylists",
                column: "FavoritedByUsersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistCoCreators");

            migrationBuilder.DropTable(
                name: "UserFavoritePlaylists");

            migrationBuilder.DropTable(
                name: "Playlists");
        }
    }
}
