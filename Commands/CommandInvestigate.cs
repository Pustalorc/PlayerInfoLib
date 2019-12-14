using System;
using System.Collections.Generic;
using PlayerInfoLibrary.Database;
using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using UnityEngine;

namespace PlayerInfoLibrary.Commands
{
    public class CommandInvestigate : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Both;
        public string Name => "investigate";
        public string Help => "Returns info for players matching the search query.";
        public string Syntax => "<player> [page]";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> {"PlayerInfoLib.Investigate"};

        public void Execute(IRocketPlayer caller, string[] command)
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

        private void PrintInformation(IRocketPlayer caller, string target, uint page = 1)
        {
            uint totalRecods = 1;
            var perPage = caller is ConsolePlayer ? 10u : 4u;
            var pInfo = new List<PlayerData>();

            if (target.IsCSteamId(out var cSteamId))
            {
                var pData = PlayerInfoLib.Instance.database.QueryById(cSteamId);
                if (pData.IsValid())
                    pInfo.Add(pData);
            }
            else if (Parser.checkIP(target))
            {
                pInfo = PlayerInfoLib.Instance.database.QueryByName(target, QueryType.Ip, out totalRecods, true,
                    page, perPage);
            }
            else
            {
                pInfo = PlayerInfoLib.Instance.database.QueryByName(target, QueryType.Both, out totalRecods, true,
                    page, perPage);
            }

            if (pInfo.Count <= 0)
            {
                UnturnedChat.Say(caller, "No players found by that name.");
                return;
            }

            var start = (page - 1) * perPage;
            UnturnedChat.Say(caller,
                PlayerInfoLib.Instance.Translate("number_of_records_found", totalRecods, target, page,
                    Math.Ceiling(totalRecods / (float) perPage)), Color.red);
            foreach (var pData in pInfo)
            {
                start++;
                if (pData.IsLocal())
                {
                    UnturnedChat.Say(caller,
                        $"{start}: {(caller is ConsolePlayer ? pData.CharacterName : pData.CharacterName.Truncate(12))} [{(caller is ConsolePlayer ? pData.SteamName : pData.SteamName.Truncate(12))}] ({pData.SteamId}), IP: {pData.Ip}, Local: {pData.IsLocal()}",
                        Color.yellow);
                    UnturnedChat.Say(caller,
                        $"Seen: {pData.LastLoginLocal}, TT: {pData.TotalPlayime.FormatTotalTime()}",
                        Color.yellow);
                }
                else
                {
                    UnturnedChat.Say(caller,
                        $"{start}: {(caller is ConsolePlayer ? pData.CharacterName : pData.CharacterName.Truncate(12))} [{(caller is ConsolePlayer ? pData.SteamName : pData.SteamName.Truncate(12))}] ({pData.SteamId}), IP: {pData.Ip}, Local: {pData.IsLocal()}",
                        Color.yellow);
                    UnturnedChat.Say(caller,
                        $"Seen: {pData.LastLoginLocal}, TT: {pData.TotalPlayime.FormatTotalTime()}, on: {pData.LastServerId}:{pData.LastServerName}",
                        Color.yellow);
                }
            }
        }
    }
}