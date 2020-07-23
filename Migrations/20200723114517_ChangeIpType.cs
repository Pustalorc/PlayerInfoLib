using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace Pustalorc.PlayerInfoLib.Unturned.Migrations
{
    public partial class ChangeIpType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "Ip",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT UNSIGNED");

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "BIGINT UNSIGNED",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "BIGINT UNSIGNED")
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Ip",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "INT UNSIGNED",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<long>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "BIGINT UNSIGNED",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "BIGINT UNSIGNED")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }
    }
}
