using System;
using Steamworks;

namespace PlayerInfoLibrary
{
    public sealed class PlayerData
    {
        public CSteamID SteamId { get; private set; }
        public string SteamName { get; internal set; }
        public string CharacterName { get; internal set; }
        public string Ip { get; internal set; }
        public ushort ServerId { get; private set; }
        public DateTime LastLoginGlobal { get; internal set; }
        public DateTime LastLoginLocal { get; internal set; }
        public int TotalPlayime { get; internal set; }
        public ushort LastServerId { get; internal set; }
        public string LastServerName { get; internal set; }

        /// <summary>
        /// Checks to see if the server specific data stored in this class is from this server(local).
        /// </summary>
        /// <returns>true if the data is from this server.</returns>
        public bool IsLocal()
        {
            return IsValid() && ServerId == PlayerInfoLib.Instance.database.InstanceId;
        }

        /// <summary>
        /// Checks to see if the data is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValid()
        {
            return SteamId != CSteamID.Nil;
        }

        internal PlayerData()
        {
            SteamId = CSteamID.Nil;
            TotalPlayime = 0;
        }

        internal PlayerData(CSteamID steamId, string steamName, string characterName, string ip,
            DateTime lastLoginGlobal, ushort lastServerId, string lastServerName, ushort serverId,
            DateTime lastLoginLocal, int totalPlayTime)
        {
            SteamId = steamId;
            SteamName = steamName;
            CharacterName = characterName;
            Ip = ip;
            LastLoginGlobal = lastLoginGlobal;
            LastServerId = lastServerId;
            LastServerName = lastServerName;
            ServerId = serverId;
            LastLoginLocal = lastLoginLocal;
            TotalPlayime = totalPlayTime;
        }
    }
}