using System;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using OpenMod.API.Eventing;
using OpenMod.Core.Users.Events;
using OpenMod.Unturned.Users;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using SDG.Unturned;
using Steamworks;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class UserEventsListener : IEventListener<UserConnectedEvent>, IEventListener<UserDisconnectedEvent>
    {
        private readonly PlayerInfoLibDbContext m_DbContext;

        public UserEventsListener(PlayerInfoLibDbContext dbContext)
        {
            m_DbContext = dbContext;
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
                    Hwid = string.Join("", playerId.hwid),
                    Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP,
                    LastLoginGlobal = DateTime.Now,
                    LastQuestGroupId = player.Player.quests.groupID.m_SteamID,
                    SteamGroup = playerId.group.m_SteamID,
                    SteamGroupName = await GetGroupName(playerId.group),
                    SteamName = playerId.playerName,
                    TotalPlaytime = 0,
                    ServerId = server.Id
                };

                await m_DbContext.Players.AddAsync(pData);
            }
            else
            {
                pData.CharacterName = player.DisplayName;
                pData.Hwid = string.Join("", playerId.hwid);
                pData.Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP;
                pData.LastLoginGlobal = DateTime.Now;
                pData.LastQuestGroupId = player.Player.quests.groupID.m_SteamID;
                pData.SteamGroup = playerId.group.m_SteamID;
                pData.SteamGroupName = await GetGroupName(playerId.group);
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
                    Hwid = string.Join("", playerId.hwid),
                    Ip = sessionState.m_nRemoteIP == 0 ? uint.MinValue : sessionState.m_nRemoteIP,
                    LastLoginGlobal = DateTime.Now,
                    LastQuestGroupId = player.Player.quests.groupID.m_SteamID,
                    SteamGroup = playerId.group.m_SteamID,
                    SteamGroupName = await GetGroupName(playerId.group),
                    SteamName = playerId.playerName,
                    TotalPlaytime = 0,
                    ServerId = server.Id
                };

                await m_DbContext.Players.AddAsync(pData);
            }
            else
            {
                pData.LastQuestGroupId = player.Player.quests.groupID.m_SteamID;
                pData.SteamGroup = playerId.group.m_SteamID;
                pData.SteamGroupName = await GetGroupName(playerId.group);
                pData.TotalPlaytime += DateTime.Now.Subtract(pData.LastLoginGlobal).TotalSeconds;
                pData.ServerId = server.Id;
            }

            await m_DbContext.SaveChangesAsync();
        }

        [ItemNotNull]
        private static async Task<string> GetGroupName(CSteamID groupId)
        {
            using var web = new WebClient();
            var result =
                await web.DownloadStringTaskAsync("http://steamcommunity.com/gid/" + groupId +
                                                  "/memberslistxml?xml=1");

            if (!result.Contains("<groupName>") || !result.Contains("</groupName>")) return "";

            var start = result.IndexOf("<groupName>", 0, StringComparison.Ordinal) + "<groupName>".Length;
            var end = result.IndexOf("</groupName>", start, StringComparison.Ordinal);

            var data = result.Substring(start, end - start);
            data = data.Replace(" ", "");
            data = data.Replace("<![CDATA[", "").Replace("]]>", "");
            return data;
        }
    }
}