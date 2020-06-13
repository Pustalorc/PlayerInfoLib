using System;
using System.Threading.Tasks;
using Rocket.Unturned.Player;
using SDG.Unturned;

namespace PlayerInfoLibrary
{
    public sealed class PlayerInfoComponent : UnturnedPlayerComponent
    {
        private bool _start;
        private DateTime _startTime;
        private PlayerData _pData;

        protected override void Load()
        {
            OnJoin();
        }

        protected override void Unload()
        {
            OnLeave();
        }

        private async Task OnJoin()
        {
            var pData = await PlayerInfoLib.Instance.database.QueryById(Player.CSteamID);
            var playtime = pData?.TotalPlaytime ?? 0;
            _startTime = DateTime.Now;
            _start = true;

            var hwid = string.Join("", Player.SteamPlayer().playerID.hwid);

            _pData = new PlayerData(Player.CSteamID, Player.SteamName, Player.CharacterName,
                Player.Player.quests.groupID.m_SteamID, Player.CSteamID.GetIp(pData?.Ip ?? 0), hwid,
                Provider.serverName, PlayerInfoLib.Instance.database.InstanceId, playtime, _startTime);
            PlayerInfoLib.Instance.database.SaveToDb(_pData);
            Player.Player.GetGroupName(_pData);
        }

        private async Task OnLeave()
        {
            if (Player == null || !_start) return;

            var pData = await PlayerInfoLib.Instance.database.QueryById(Player.CSteamID);

            if (!pData.IsValid()) return;

            var totalSessionTime = (ulong) DateTime.Now.Subtract(_startTime).TotalSeconds;
            pData.TotalPlaytime += totalSessionTime;
            PlayerInfoLib.Instance.database.SaveToDb(pData);
        }
    }
}