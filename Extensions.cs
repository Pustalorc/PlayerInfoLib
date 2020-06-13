using System;
using System.Collections;
using System.Text;
using JetBrains.Annotations;
using SDG.Unturned;
using Steamworks;
using UnityEngine.Networking;

namespace PlayerInfoLibrary
{
    public static class Extensions
    {
        public static DateTime FromTimeStamp(this long timestamp)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp).ToLocalTime();
        }

        public static long ToTimeStamp(this DateTime datetime)
        {
            return (long) datetime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))
                .TotalSeconds;
        }

        public static void GetGroupName([NotNull] this Player player, PlayerData playerData)
        {
            var group = player.quests.groupID;

            if (group.ToString().Length == 18)
                PlayerInfoLib.Instance.StartCoroutine(SteamGroupRequest(group.ToString(), playerData));
        }

        public static IEnumerator SteamGroupRequest(string group, PlayerData playerData)
        {
            var www = UnityWebRequest.Get("http://steamcommunity.com/gid/" + group + "/memberslistxml?xml=1");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError) yield break;

            var result = Encoding.UTF8.GetString(www.downloadHandler.data);

            var data = result.GetBetween("<groupName>", "</groupName>").Replace(" ", "");

            data = data.Replace("<![CDATA[", "").Replace("]]>", "");

            playerData.GroupName = data;
            PlayerInfoLib.Instance.database.SaveToDb(playerData);
        }

        [NotNull]
        public static string GetBetween([NotNull] this string strSource, [NotNull] string strStart, string strEnd)
        {
            if (!strSource.Contains(strStart) || !strSource.Contains(strEnd)) return "";

            var start = strSource.IndexOf(strStart, 0, StringComparison.Ordinal) + strStart.Length;
            var end = strSource.IndexOf(strEnd, start, StringComparison.Ordinal);
            return strSource.Substring(start, end - start);
        }

        // Grab an active players ip address from CSteamID.
        public static uint GetIp(this CSteamID cSteamId, uint fallback = uint.MaxValue)
        {
            SteamGameServerNetworking.GetP2PSessionState(cSteamId, out var sessionState);
            return sessionState.m_nRemoteIP == 0 ? fallback : sessionState.m_nRemoteIP;
        }

        // Returns a Steamworks.CSteamID on out from a string, and returns true if it is a CSteamID.
        public static bool IsCSteamId(this string sCSteamId, out CSteamID cSteamId)
        {
            cSteamId = (CSteamID) 0;
            if (!ulong.TryParse(sCSteamId, out var ulCSteamId)) return false;
            if ((ulCSteamId < 0x0110000100000000 || ulCSteamId > 0x0170000000000000) && ulCSteamId != 0) return false;

            cSteamId = (CSteamID) ulCSteamId;
            return true;
        }

        [CanBeNull]
        public static string Truncate([CanBeNull] this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        // Returns formatted string with how long they've played on the server in d, h, m, s.
        [NotNull]
        public static string FormatTotalTime(this ulong totalTime)
        {
            var totalTimeFormatted = "";
            if (totalTime >= 60 * 60 * 24) totalTimeFormatted = totalTime / (60 * 60 * 24) + "d ";
            if (totalTime >= 60 * 60) totalTimeFormatted += totalTime / (60 * 60) % 24 + "h ";
            if (totalTime >= 60) totalTimeFormatted += totalTime / 60 % 60 + "m ";
            totalTimeFormatted += totalTime % 60 + "s";
            return totalTimeFormatted;
        }
    }
}