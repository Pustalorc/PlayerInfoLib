using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace Pustalorc.PlayerInfoLib.Unturned.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Instance = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pustalorc_PlayerInfoLib_Unturned_Servers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Pustalorc_PlayerInfoLib_Unturned_Players",
                columns: table => new
                {
                    Id = table.Column<long>(type: "BIGINT UNSIGNED", nullable: false),
                    SteamName = table.Column<string>(maxLength: 64, nullable: false),
                    CharacterName = table.Column<string>(maxLength: 64, nullable: false),
                    ProfilePictureHash = table.Column<string>(maxLength: 64, nullable: false),
                    LastQuestGroupId = table.Column<long>(type: "BIGINT UNSIGNED", nullable: false),
                    SteamGroup = table.Column<long>(type: "BIGINT UNSIGNED", nullable: false),
                    SteamGroupName = table.Column<string>(maxLength: 64, nullable: false),
                    Hwid = table.Column<string>(nullable: false),
                    Ip = table.Column<long>(nullable: false),
                    TotalPlaytime = table.Column<double>(nullable: false),
                    LastLoginGlobal = table.Column<DateTime>(nullable: false),
                    ServerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pustalorc_PlayerInfoLib_Unturned_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pustalorc_PlayerInfoLib_Unturned_Players_Pustalorc_PlayerInf~",
                        column: x => x.ServerId,
                        principalTable: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pustalorc_PlayerInfoLib_Unturned_Players_ServerId",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                column: "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pustalorc_PlayerInfoLib_Unturned_Players");

            migrationBuilder.DropTable(
                name: "Pustalorc_PlayerInfoLib_Unturned_Servers");
        }
    }
}
