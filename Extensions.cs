using System;
using MySql.Data.MySqlClient;
using SDG.Unturned;
using Steamworks;

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

        public static bool IsDbNull(this MySqlDataReader reader, string fieldname)
        {
            return reader.IsDBNull(reader.GetOrdinal(fieldname));
        }

        public static string GetIp(this CSteamID cSteamId)
        {
            // Grab an active players ip address from CSteamID.
            SteamGameServerNetworking.GetP2PSessionState(cSteamId, out var sessionState);
            return Parser.getIPFromUInt32(sessionState.m_nRemoteIP);
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
        public static string FormatTotalTime(this int totalTime)
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