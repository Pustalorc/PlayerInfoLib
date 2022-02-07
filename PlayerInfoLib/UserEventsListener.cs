// ReSharper disable AnnotateNotNullParameter
// ReSharper disable AnnotateNotNullTypeMember

using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using OpenMod.API.Eventing;
using OpenMod.API.Users;
using OpenMod.Core.Helpers;
using OpenMod.Unturned.Players.Connections.Events;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes.SteamWebApiClasses;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;
using Steamworks;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class UserEventsListener : IEventListener<UnturnedPlayerConnectedEvent>, IEventListener<UnturnedPlayerDisconnectedEvent>
    {
        private readonly IServiceProvider m_ServiceProvider;
        private readonly IConfiguration m_Configuration;

        public UserEventsListener(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            m_ServiceProvider = serviceProvider;
            m_Configuration = configuration;
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerConnectedEvent @event)
        {
            var joinTime = DateTime.Now;
            AsyncHelper.Schedule("PlayerInfoLib_PlayerConnected", async () =>
            {
                var player = @event.Player;
                var playerId = player.SteamPlayer.playerID;
                var steamId = player.SteamId;
                var pfpHash = await GetProfilePictureHashAsync(steamId);
                var groupName = await GetSteamGroupNameAsync(playerId.group);
                var hwid = string.Join("", playerId.hwid);
                if (!player.SteamPlayer.transportConnection.TryGetIPv4Address(out var ip))
                    ip = uint.MinValue;
                var questGroupId = player.Player.quests.groupID.m_SteamID;

                await using var playerInfoRepository = m_ServiceProvider.GetRequiredService<IPlayerInfoRepository>();

                var pData = await playerInfoRepository.FindPlayerAsync(steamId.ToString(), UserSearchMode.FindById);
                var server = await playerInfoRepository.GetCurrentServerAsync() ??
                             await playerInfoRepository.CheckAndRegisterCurrentServerAsync();

                if (pData == null)
                {
                    pData = BuildPlayerData(steamId.m_SteamID, player.SteamPlayer.playerID.characterName,
                        playerId.playerName, hwid, ip,
                        pfpHash, questGroupId, playerId.group.m_SteamID, groupName, 0,
                        joinTime, server);

                    await playerInfoRepository.AddPlayerDataAsync(pData);
                }
                else
                {
                    pData.ProfilePictureHash = pfpHash;
                    pData.CharacterName = player.SteamPlayer.playerID.characterName;
                    pData.Hwid = hwid;
                    pData.Ip = ip;
                    pData.LastLoginGlobal = joinTime;

                    if (questGroupId != 0)
                        pData.LastQuestGroupId = questGroupId;

                    pData.SteamGroup = playerId.group.m_SteamID;
                    pData.SteamGroupName = groupName;
                    pData.SteamName = playerId.playerName;
                    pData.Server = server;
                    pData.ServerId = server.Id;

                    await playerInfoRepository.SaveChangesAsync();
                }
            });
        }

        public async Task HandleEventAsync(object sender, UnturnedPlayerDisconnectedEvent @event)
        {
            var leaveTime = DateTime.Now;
            AsyncHelper.Schedule("PlayerInfoLib_PlayerDisconnected", async () =>
            {
                var player = @event.Player;
                var playerId = player.SteamPlayer.playerID;
                var steamId = player.SteamId;
                var pfpHash = await GetProfilePictureHashAsync(steamId);
                var groupName = await GetSteamGroupNameAsync(playerId.group);
                var hwid = string.Join("", playerId.hwid);

                await using var playerInfoRepository = m_ServiceProvider.GetRequiredService<IPlayerInfoRepository>();

                var pData = await playerInfoRepository.FindPlayerAsync(player.SteamId.ToString(), UserSearchMode.FindById);
                var server = await playerInfoRepository.GetCurrentServerAsync() ??
                             await playerInfoRepository.CheckAndRegisterCurrentServerAsync();

                if (pData == null)
                {
                    pData = BuildPlayerData(steamId.m_SteamID, player.SteamPlayer.playerID.characterName,
                        playerId.playerName, hwid, uint.MinValue,
                        pfpHash, player.Player.quests.groupID.m_SteamID, playerId.group.m_SteamID, groupName, 0,
                        leaveTime, server);

                    await playerInfoRepository.AddPlayerDataAsync(pData);
                }
                else
                {
                    pData.ProfilePictureHash = pfpHash;
                    pData.LastQuestGroupId = player.Player.quests.groupID.m_SteamID;
                    pData.SteamGroup = playerId.group.m_SteamID;
                    pData.SteamGroupName = groupName;
                    pData.SteamName = playerId.playerName;
                    pData.TotalPlaytime += leaveTime.Subtract(pData.LastLoginGlobal).TotalSeconds;

                    await playerInfoRepository.SaveChangesAsync();
                }
            });
        }

        private static PlayerData BuildPlayerData(ulong steamId, string characterName, string steamName, string hwid, uint ip,
            string profileHash, ulong questGroup, ulong steamGroup, string steamGroupName, double totalPlaytime,
            DateTime lastLogin, Server server)
        {
            return new PlayerData
            {
                Id = steamId,
                CharacterName = characterName,
                SteamName = steamName,
                Hwid = hwid,
                Ip = ip,
                ProfilePictureHash = profileHash,
                LastQuestGroupId = questGroup,
                SteamGroup = steamGroup,
                SteamGroupName = steamGroupName,
                TotalPlaytime = totalPlaytime,
                LastLoginGlobal = lastLogin,
                ServerId = server.Id,
                Server = server
            };
        }

        private async Task<string> GetProfilePictureHashAsync(CSteamID user)
        {
            var apiKey = m_Configuration["steamWebApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return "";

            using var web = new WebClient();
            var result =
                await web.DownloadStringTaskAsync(
                    $"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={user.m_SteamID}");

            var deserialized = JsonConvert.DeserializeObject<PlayerSummaries>(result);

            return deserialized.response.players
                       .FirstOrDefault(k => k.steamid.Equals(user.ToString(), StringComparison.Ordinal))?.avatarhash ??
                   "";
        }

        private static async Task<string> GetSteamGroupNameAsync(CSteamID groupId)
        {
            using var web = new WebClient();
            var result =
                await web.DownloadStringTaskAsync("http://steamcommunity.com/gid/" + groupId +
                                                  "/memberslistxml?xml=1");

            if (!result.Contains("<groupName>") || !result.Contains("</groupName>")) return "";

            var start = result.IndexOf("<groupName>", 0, StringComparison.Ordinal) + "<groupName>".Length;
            var end = result.IndexOf("</groupName>", start, StringComparison.Ordinal);

            var data = result.Substring(start, end - start);
            data = data.Trim();
            data = data.Replace("<![CDATA[", "").Replace("]]>", "");
            return data;
        }
    }
}