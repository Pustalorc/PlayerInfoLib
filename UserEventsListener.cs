using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenMod.API.Eventing;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Users;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using Pustalorc.PlayerInfoLib.Unturned.SteamWebApiClasses;
using SDG.Unturned;
using Steamworks;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class UserEventsListener : IEventListener<UserConnectedEvent>, IEventListener<UserDisconnectedEvent>
    {
        private readonly PlayerInfoLibDbContext m_DbContext;
        private readonly IConfiguration m_Configuration;

        public UserEventsListener(PlayerInfoLibDbContext dbContext, IConfiguration configuration)
        {
            m_DbContext = dbContext;
            m_Configuration = configuration;
        }

        public async Task HandleEventAsync(object sender, UserConnectedEvent @event)
        {
            if (!(@event.User is UnturnedUser player)) return;

            var playerId = player.SteamPlayer.playerID;
            SteamGameServerNetworking.GetP2PSessionState(player.SteamId, out var sessionState);

            var pData = await m_DbContext.Players.FirstOrDefaultAsync(d => d.Id.Equals(player.SteamId.m_SteamID));
            var server =
                await m_DbContext.Servers.FirstAsync(
                    k => k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
            if (pData == null)
            {
                pData = new PlayerData
                {
                    Id = player.SteamId.m_SteamID,
                    CharacterName = player.DisplayName,
                    ProfilePictureHash = await GetProfilePictureHash(player.SteamId),
                    Hwid = string.Join("", playerId.hwid),
                    Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP,
                    LastLoginGlobal = DateTime.Now,
                    LastQuestGroupId = player.Player.quests.groupID.m_SteamID,
                    SteamGroup = playerId.group.m_SteamID,
                    SteamGroupName = await GetSteamGroupName(playerId.group),
                    SteamName = playerId.playerName,
                    TotalPlaytime = 0,
                    ServerId = server.Id
                };

                await m_DbContext.Players.AddAsync(pData);
            }
            else
            {
                pData.ProfilePictureHash = await GetProfilePictureHash(player.SteamId);
                pData.CharacterName = player.DisplayName;
                pData.Hwid = string.Join("", playerId.hwid);
                pData.Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP;
                pData.LastLoginGlobal = DateTime.Now;
                pData.LastQuestGroupId = player.Player.quests.groupID.m_SteamID;
                pData.SteamGroup = playerId.group.m_SteamID;
                pData.SteamGroupName = await GetSteamGroupName(playerId.group);
                pData.SteamName = playerId.playerName;
                pData.ServerId = server.Id;
            }

            await m_DbContext.SaveChangesAsync();
        }

        public async Task HandleEventAsync(object sender, UserDisconnectedEvent @event)
        {
            if (!(@event.User is UnturnedUser player)) return;

            var playerId = player.SteamPlayer.playerID;
            SteamGameServerNetworking.GetP2PSessionState(player.SteamId, out var sessionState);

            var pData = await m_DbContext.Players.FirstOrDefaultAsync(d => d.Id.Equals(player.SteamId.m_SteamID));
            var server =
                await m_DbContext.Servers.FirstAsync(
                    k => k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
            if (pData == null)
            {
                pData = new PlayerData
                {
                    Id = player.SteamId.m_SteamID,
                    CharacterName = player.DisplayName,
                    SteamName = playerId.playerName,
                    ProfilePictureHash = await GetProfilePictureHash(player.SteamId),
                    Hwid = string.Join("", playerId.hwid),
                    Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP,
                    LastLoginGlobal = DateTime.Now,
                    LastQuestGroupId = player.Player.quests.groupID.m_SteamID,
                    SteamGroup = playerId.group.m_SteamID,
                    SteamGroupName = await GetSteamGroupName(playerId.group),
                    TotalPlaytime = 0,
                    ServerId = server.Id
                };

                await m_DbContext.Players.AddAsync(pData);
            }
            else
            {
                pData.ProfilePictureHash = await GetProfilePictureHash(player.SteamId);
                pData.LastQuestGroupId = player.Player.quests.groupID.m_SteamID;
                pData.SteamGroup = playerId.group.m_SteamID;
                pData.SteamGroupName = await GetSteamGroupName(playerId.group);
                pData.TotalPlaytime += DateTime.Now.Subtract(pData.LastLoginGlobal).TotalSeconds;
                pData.ServerId = server.Id;
            }

            await m_DbContext.SaveChangesAsync();
        }

        private async Task<string> GetProfilePictureHash(CSteamID user)
        {
            var apiKey = m_Configuration["steamWebApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                return "";

            using var web = new WebClient();
            var result =
                await web.DownloadStringTaskAsync($"http://api.steampowered.com/ISteamUser/GetPlayerSummaries/v2/?key={apiKey}&steamids={user.m_SteamID}");

            var deserialized = JsonConvert.DeserializeObject<PlayerSummaries>(result);

            return deserialized.response.players.FirstOrDefault(k => k.steamid.Equals(user.ToString(), StringComparison.Ordinal))?.avatarhash ?? "";
        }

        [ItemNotNull]
        private static async Task<string> GetSteamGroupName(CSteamID groupId)
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