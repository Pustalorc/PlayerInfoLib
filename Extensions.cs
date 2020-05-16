using System;
using System.Collections;
using MySql.Data.MySqlClient;
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

        public static void GetGroupName(this Player player, PlayerData playerData)
        {
            var group = player.quests.groupID;

            if (group.ToString().Length == 18)
            {
                PlayerInfoLib.Instance.StartCoroutine(SteamGroupRequest(group.ToString(), playerData));
            }
        }

        public static IEnumerator SteamGroupRequest(string group, PlayerData playerData)
        {
            var www = UnityWebRequest.Get("http://steamcommunity.com/gid/" + group + "/memberslistxml?xml=1");
            yield return www.SendWebRequest();

            if (!www.isNetworkError && !www.isHttpError)
            {
                string result = System.Text.Encoding.UTF8.GetString(www.downloadHandler.data);

                string data = result.getBetween("<groupName>", "</groupName>").Replace(" ", "");

                data = data.Replace("<![CDATA[", "").Replace("]]>", "");

                playerData.GroupName = data;
                PlayerInfoLib.Instance.database.SaveToDb(playerData);
            }
        }

        public static string getBetween(this string strSource, string strStart, string strEnd)
        {
            int Start, End;
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                Start = strSource.IndexOf(strStart, 0) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                return strSource.Substring(Start, End - Start);
            }
            else
            {
                return "";
            }
        }

        // Grab an active players ip address from CSteamID.
        public static uint GetIp(this CSteamID cSteamId, uint fallback = uint.MaxValue)
        {
            SteamGameServerNetworking.GetP2PSessionState(cSteamId, out var sessionState);
            if (sessionState.m_nRemoteIP == 0)
                return fallback;
            return sessionState.m_nRemoteIP;
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

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;

            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        // Returns formatted string with how long they've played on the server in d, h, m, s.
        public static string FormatTotalTime(this ulong totalTime)
        {
            var totalTimeFormated = "";
            if (totalTime >= 60 * 60 * 24) totalTimeFormated = totalTime / (60 * 60 * 24) + "d ";
            if (totalTime >= 60 * 60) totalTimeFormated += totalTime / (60 * 60) % 24 + "h ";
            if (totalTime >= 60) totalTimeFormated += totalTime / 60 % 60 + "m ";
            totalTimeFormated += totalTime % 60 + "s";
            return totalTimeFormated;
        }
    }
}