using System;
using Steamworks;

namespace PlayerInfoLibrary
{
    public sealed class PlayerData
    {
        // IDENTIFYABLE PERSONAL DATA
        public CSteamID SteamId { get; private set; }
        public ulong LastQuestGroupId { get; internal set; }
        public string GroupName { get; internal set; }
        public string SteamName { get; internal set; }
        public string CharacterName { get; internal set; }
        public string Hwid { get; internal set; }
        public uint Ip { get; internal set; }

        // SERVER RELATED DATA
        public ulong TotalPlaytime { get; internal set; }
        public ushort ServerId { get; private set; }
        public string ServerName { get; internal set; }
        public DateTime LastLoginGlobal { get; internal set; }

        /// <summary>
        /// Checks to see if the data is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return SteamId != CSteamID.Nil;
        }

        internal PlayerData(CSteamID steamId, string steamName, string characterName, ulong groupId, string groupName,
            uint ip, string hwid, string serverName, ushort serverId, ulong totalPlaytime, DateTime lastLoginGlobal)
        {
            SteamId = steamId;
            SteamName = steamName;
            CharacterName = characterName;
            LastQuestGroupId = groupId;
            GroupName = groupName;
            Ip = ip;
            Hwid = hwid;
            LastLoginGlobal = lastLoginGlobal;
            ServerName = serverName;
            ServerId = serverId;
            TotalPlaytime = totalPlaytime;
        }

        internal PlayerData(CSteamID steamId, string steamName, string characterName, ulong groupId, uint ip,
            string hwid, string serverName, ushort serverId, ulong totalPlaytime, DateTime lastLoginGlobal)
        {
            SteamId = steamId;
            SteamName = steamName;
            CharacterName = characterName;
            LastQuestGroupId = groupId;
            GroupName = "";
            Ip = ip;
            Hwid = hwid;
            LastLoginGlobal = lastLoginGlobal;
            ServerName = serverName;
            ServerId = serverId;
            TotalPlaytime = totalPlaytime;
        }
    }
}