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
                "Pustalorc_PlayerInfoLib_Unturned_Servers",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Instance = table.Column<string>(maxLength: 128, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_Pustalorc_PlayerInfoLib_Unturned_Servers", x => x.Id); });

            migrationBuilder.CreateTable(
                "Pustalorc_PlayerInfoLib_Unturned_Players",
                table => new
                {
                    Id = table.Column<long>("BIGINT UNSIGNED", nullable: false),
                    SteamName = table.Column<string>(maxLength: 64, nullable: false),
                    CharacterName = table.Column<string>(maxLength: 64, nullable: false),
                    LastQuestGroupId = table.Column<long>("BIGINT UNSIGNED", nullable: false),
                    SteamGroup = table.Column<long>("BIGINT UNSIGNED", nullable: false),
                    SteamGroupName = table.Column<string>(maxLength: 64, nullable: false),
                    Hwid = table.Column<string>(nullable: false),
                    Ip = table.Column<int>("INT UNSIGNED", nullable: false),
                    TotalPlaytime = table.Column<double>(nullable: false),
                    LastLoginGlobal = table.Column<DateTime>(nullable: false),
                    ServerId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pustalorc_PlayerInfoLib_Unturned_Players", x => x.Id);
                    table.ForeignKey(
                        "FK_Pustalorc_PlayerInfoLib_Unturned_Players_Pustalorc_PlayerInf~",
                        x => x.ServerId,
                        "Pustalorc_PlayerInfoLib_Unturned_Servers",
                        "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                "IX_Pustalorc_PlayerInfoLib_Unturned_Players_ServerId",
                "Pustalorc_PlayerInfoLib_Unturned_Players",
                "ServerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                "Pustalorc_PlayerInfoLib_Unturned_Players");

            migrationBuilder.DropTable(
                "Pustalorc_PlayerInfoLib_Unturned_Servers");
        }
    }
}