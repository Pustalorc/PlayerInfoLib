using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using PlayerInfoLibrary.Database;
using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;
using Math = System.Math;

namespace PlayerInfoLibrary.Commands
{
    public class CommandInvestigate : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        [NotNull] public string Name => "investigate";
        [NotNull] public string Help => "Returns info for players matching the search query.";
        [NotNull] public string Syntax => "<player> [page]";
        [NotNull] public List<string> Aliases => new List<string>();
        [NotNull] public List<string> Permissions => new List<string> {"PlayerInfoLib.Investigate"};

        public void Execute(IRocketPlayer caller, [NotNull] string[] command)
        {
            switch (command.Length)
            {
                case 1:
                    PrintInformation(caller, command[0]);
                    break;
                case 2:
                    if (!uint.TryParse(command[1], out var page))
                    {
                        UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("invalid_page"));
                        break;
                    }

                    PrintInformation(caller, command[0], page);
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("investigate_help"));
                    break;
            }
        }

        private static async Task PrintInformation(IRocketPlayer caller, string target, uint page = 1)
        {
            const uint totalRecords = 1;
            var perPage = caller is ConsolePlayer ? 10u : 4u;
            var pInfo = new List<PlayerData>();

            if (target.IsCSteamId(out var cSteamId))
            {
                var pData = await PlayerInfoLib.Instance.database.QueryById(cSteamId);

                if (pData?.IsValid() == true)
                    pInfo.Add(pData);
            }
            else if (Parser.checkIP(target))
            {
                pInfo = await PlayerInfoLib.Instance.database.QueryByName(target, QueryType.Ip);
            }
            else
            {
                pInfo = await PlayerInfoLib.Instance.database.QueryByName(target, QueryType.Both);
            }

            if (pInfo.Count <= 0)
            {
                UnturnedChat.Say(caller, "No players found by that name.");
                return;
            }

            var start = (page - 1) * perPage;
            UnturnedChat.Say(caller,
                PlayerInfoLib.Instance.Translate("number_of_records_found", totalRecords, target, page,
                    Math.Ceiling(totalRecords / (float) perPage)), Color.red);
            foreach (var pData in pInfo)
            {
                start++;

                UnturnedChat.Say(caller,
                    $"{start}: {(caller is ConsolePlayer ? pData.CharacterName : pData.CharacterName.Truncate(12))} [{(caller is ConsolePlayer ? pData.SteamName : pData.SteamName.Truncate(12))}] ({pData.SteamId}), Group ID: {pData.LastQuestGroupId}, Group Name: {pData.GroupName}",
                    Color.yellow);
                UnturnedChat.Say(caller,
                    $"Seen: {pData.LastLoginGlobal}, TT: {pData.TotalPlaytime.FormatTotalTime()}",
                    Color.yellow);
            }
        }
    }
}