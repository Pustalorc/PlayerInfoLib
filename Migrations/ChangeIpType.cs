using Microsoft.EntityFrameworkCore.Migrations;
using MySql.Data.EntityFrameworkCore.Metadata;

namespace Pustalorc.PlayerInfoLib.Unturned.Migrations
{
    public partial class ChangeIpType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                "Ip",
                "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INT UNSIGNED");

            migrationBuilder.AlterColumn<long>(
                    "Id",
                    "Pustalorc_PlayerInfoLib_Unturned_Players",
                    "BIGINT UNSIGNED",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "BIGINT UNSIGNED")
                .OldAnnotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                "Ip",
                "Pustalorc_PlayerInfoLib_Unturned_Players",
                "INT UNSIGNED",
                nullable: false,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<long>(
                    "Id",
                    "Pustalorc_PlayerInfoLib_Unturned_Players",
                    "BIGINT UNSIGNED",
                    nullable: false,
                    oldClrType: typeof(long),
                    oldType: "BIGINT UNSIGNED")
                .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn);
        }
    }
}