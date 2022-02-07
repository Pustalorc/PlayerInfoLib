using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Pustalorc.PlayerInfoLib.Unturned.Migrations
{
    public partial class SwitchToPomelo : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Instance",
                table: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "SteamName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "SteamGroupName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<ulong>(
                name: "SteamGroup",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePictureHash",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<ulong>(
                name: "LastQuestGroupId",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginGlobal",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime");

            migrationBuilder.AlterColumn<uint>(
                name: "Ip",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<string>(
                name: "Hwid",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CharacterName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<ulong>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(767)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Instance",
                table: "Pustalorc_PlayerInfoLib_Unturned_Servers",
                type: "varchar(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "SteamName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "SteamGroupName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "SteamGroup",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "text",
                nullable: false,
                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<string>(
                name: "ProfilePictureHash",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "LastQuestGroupId",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "text",
                nullable: false,
                oldClrType: typeof(ulong));

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastLoginGlobal",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "datetime",
                nullable: false,
                oldClrType: typeof(DateTime));

            migrationBuilder.AlterColumn<long>(
                name: "Ip",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(uint));

            migrationBuilder.AlterColumn<string>(
                name: "Hwid",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "text",
                nullable: false,
                oldClrType: typeof(string));

            migrationBuilder.AlterColumn<string>(
                name: "CharacterName",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "Pustalorc_PlayerInfoLib_Unturned_Players",
                type: "varchar(767)",
                nullable: false,
                oldClrType: typeof(ulong));
        }
    }
}
