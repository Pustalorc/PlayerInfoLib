using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace Pustalorc.PlayerInfoLib.Unturned.Migrations
{
    public partial class ChangeUlongToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SteamGroup",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "BIGINT UNSIGNED");

            migrationBuilder.AlterColumn<string>(
                name: "LastQuestGroupId",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "BIGINT UNSIGNED");

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "BIGINT UNSIGNED")
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "SteamGroup",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "BIGINT UNSIGNED",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<long>(
                name: "LastQuestGroupId",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "BIGINT UNSIGNED",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "BIGINT UNSIGNED",
                nullable: false,
                oldClrType: typeof(string))
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }
    }
}
