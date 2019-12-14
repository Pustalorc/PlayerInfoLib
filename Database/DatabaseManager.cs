using System;
using System.Collections.Generic;
using System.Linq;
using I18N.West;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI.Relational;
using PlayerInfoLibrary.Configuration;
using Pustalorc.Libraries.MySqlConnectorWrapper;
using Pustalorc.Libraries.MySqlConnectorWrapper.Queries;
using Rocket.API;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace PlayerInfoLibrary.Database
{
    public sealed class DatabaseManager : ConnectorWrapper<PlayerInfoLibConfig>
    {
        private Dictionary<Query, Query> _createTableQueries;
        private Query _getPlayerDataQuery;

        private Dictionary<Query, Query> CreateTableQueries => _createTableQueries ??= new Dictionary<Query, Query>
        {
            {
                new Query($"SHOW TABLES LIKE '{Configuration.TableNamePlayers}';", EQueryType.Scalar),
                new Query(
                    $"CREATE TABLE `{Configuration.TableNamePlayers}` (`SteamID` bigint(24) unsigned NOT NULL, `SteamName` varchar(255) COLLATE utf8_unicode_ci NOT NULL, `CharName` varchar(255) COLLATE utf8_unicode_ci NOT NULL, `IP` varchar(16) COLLATE utf8_unicode_ci NOT NULL, `LastLoginGlobal` bigint(32) NOT NULL, `LastServerID` smallint(5) unsigned NOT NULL, PRIMARY KEY (`SteamID`), KEY `LastServerID` (`LastServerID`), KEY `IP` (`IP`));",
                    EQueryType.NonQuery)
            },
            {
                new Query($"SHOW TABLES LIKE '{Configuration.TableNameInstances}';", EQueryType.Scalar),
                new Query(
                    $"CREATE TABLE `{Configuration.TableNameInstances}` (`ServerID` smallint(5) unsigned NOT NULL AUTO_INCREMENT, `ServerInstance` varchar(128) COLLATE utf8_unicode_ci NOT NULL, `ServerName` varchar(60) COLLATE utf8_unicode_ci NOT NULL, PRIMARY KEY(`ServerID`), UNIQUE KEY `ServerInstance` (`ServerInstance`)));",
                    EQueryType.NonQuery)
            }
        };

        public Query GetPlayerDataQuery => _getPlayerDataQuery ??= new Query(
            $"SELECT * FROM `{Configuration.TableNamePlayers}`;",
            EQueryType.Reader, PlayerDataFetched, true);

        private List<PlayerData> _allPlayerData = new List<PlayerData>();
        private readonly object _memory = new object();

        public bool Initialized { get; private set; }
        public ushort InstanceId { get; private set; }

        internal DatabaseManager(PlayerInfoLibConfig config) : base(config)
        {
            new CP1250();

            var output = ExecuteTransaction(CreateTableQueries.Keys.ToArray()).ToList();
            var execute = (from queryResult in output
                where queryResult.Output == null
                select CreateTableQueries[queryResult.Query]).ToArray();

            if (execute.Length > 0)
                ExecuteTransaction(execute);

            for (var i = 0; i < 10; i++)
            {
                if (GetInstanceId())
                    break;

                if (i != 9) continue;

                Logger.LogError("Unable to get instance Id.");
                return;
            }

            Initialized = true;
        }

        private void PlayerDataFetched(QueryOutput queryOutput)
        {
            if (queryOutput.Query.QueryType != EQueryType.Reader) return;

            lock (_memory)
            {
                _allPlayerData = (from row in (List<Row>) queryOutput.Output select BuildPlayerData(row)).ToList();
            }
        }

        private bool GetInstanceId()
        {
            var output = ExecuteQuery(new Query(
                $"SELECT `ServerID`, `ServerName` FROM `{Configuration.TableNameInstances}` WHERE `ServerInstance` = @instance;",
                EQueryType.Reader, queryParameters: new MySqlParameter("@instance", Provider.serverID.ToLower())));

            var rows = (List<Row>) output.Output;
            if (rows.Count <= 0)
            {
                ExecuteQuery(new Query(
                    $"INSERT INTO `{Configuration.TableNameInstances}` (`ServerInstance`, `ServerName`) VALUES (@instance, @name);",
                    EQueryType.NonQuery, null, false, new MySqlParameter("@instance", Provider.serverID.ToLower()),
                    new MySqlParameter("name", Provider.serverName)));
                return false;
            }

            var row = rows[0];
            if (row["ServerID"] == null)
            {
                ExecuteQuery(new Query(
                    $"INSERT INTO `{Configuration.TableNameInstances}` (`ServerInstance`, `ServerName`) VALUES (@instance, @name);",
                    EQueryType.NonQuery, null, false, new MySqlParameter("@instance", Provider.serverID.ToLower()),
                    new MySqlParameter("name", Provider.serverName)));
                return false;
            }

            InstanceId = ushort.Parse(row["ServerID"].ToString());

            if (row["ServerName"].ToString() != Provider.serverName)
                ExecuteQuery(new Query(
                    $"UPDATE `{Configuration.TableNameInstances}` SET `ServerName` = @servername WHERE `ServerID` = {InstanceId};",
                    EQueryType.NonQuery, queryParameters: new MySqlParameter("name", Provider.serverName)));

            return true;
        }

        internal void SetInstanceName(string newName, IRocketPlayer caller)
        {
            RequestQueryExecute(false,
                new Query(
                    $"UPDATE `{Configuration.TableNameInstances}` SET `ServerInstance` = @name WHERE `ServerID` = @instance;",
                    EQueryType.NonQuery,
                    output => UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_success")), false,
                    new MySqlParameter("@name", newName), new MySqlParameter("@instance", InstanceId)));
        }

        public PlayerData QueryById(CSteamID steamId)
        {
            if (Initialized) return _allPlayerData.FirstOrDefault(k => k.SteamId == steamId);

            Logger.LogError("Error: Cant load player info from DB, plugin hasn't initialized properly.");
            return null;
        }

        public List<PlayerData> QueryByName(string playerName, QueryType queryType, out uint records,
            bool pagination = true, uint page = 1, uint limit = 4)
        {
            var playerList = new List<PlayerData>();
            MySqlDataReader reader = null;
            records = 0;
            var limitStart = (page - 1) * limit;
            try
            {
                if (!Initialized)
                {
                    Logger.LogError("Error: Cant load player info from DB, plugin hasn't initialized properly.");
                    return playerList;
                }

                if (page == 0 || limit == 0)
                {
                    Logger.LogError("Error: Invalid pagination values, these must be above 0.");
                    return playerList;
                }

                if (playerName.Trim() == string.Empty)
                {
                    Logger.LogWarning("Warning: Need at least one character in the player name.");
                    return playerList;
                }

                var command = Connection.CreateCommand();
                command.Parameters.AddWithValue("@name", "%" + playerName + "%");
                command.Parameters.AddWithValue("@instance", InstanceId);
                string type;
                switch (queryType)
                {
                    case QueryType.Both:
                        type = "AND (a.SteamName LIKE @name OR a.CharName LIKE @name)";
                        break;
                    case QueryType.CharName:
                        type = "AND a.CharName LIKE @name";
                        break;
                    case QueryType.SteamName:
                        type = "AND a.SteamName LIKE @name";
                        break;
                    case QueryType.Ip:
                        type = "AND a.IP = " + Parser.getUInt32FromIP(playerName);
                        break;
                    default:
                        type = string.Empty;
                        break;
                }

                if (pagination)
                    command.CommandText = "SELECT COUNT(*) AS count FROM (SELECT * FROM (SELECT a.SteamID FROM `" +
                                          Table + "` AS a LEFT JOIN `" + TableServer +
                                          "` AS b ON a.SteamID = b.SteamID WHERE (b.ServerID = @instance OR b.ServerID = a.LastServerID OR b.ServerID IS NULL) " +
                                          type + " ORDER BY b.LastLoginLocal ASC) AS g GROUP BY g.SteamID) AS c;";
                command.CommandText +=
                    "SELECT * FROM (SELECT a.SteamID, a.SteamName, a.CharName, a.IP, a.LastLoginGlobal, a.TotalPlayTime, a.LastServerID, b.ServerID, b.LastLoginLocal, b.CleanedBuildables, b.CleanedPlayerData, c.ServerName AS LastServerName FROM `" +
                    Table + "` AS a LEFT JOIN `" + TableServer + "` AS b ON a.SteamID = b.SteamID LEFT JOIN `" +
                    TableInstance +
                    "` AS c ON a.LastServerID = c.ServerID WHERE (b.ServerID = @instance OR b.ServerID = a.LastServerID OR b.ServerID IS NULL) " +
                    type + " ORDER BY b.LastLoginLocal ASC) AS g GROUP BY g.SteamID ORDER BY g.LastLoginGlobal DESC" +
                    (pagination ? " LIMIT " + limitStart + ", " + limit + ";" : ";");
                reader = command.ExecuteReader();
                if (pagination)
                {
                    if (reader.Read())
                        records = reader.GetUInt32("count");
                    if (!reader.NextResult()) return playerList;
                }

                if (!reader.HasRows) return playerList;
                while (reader.Read())
                {
                    var record = BuildPlayerData(reader);
                    record.CacheTime = DateTime.Now;
                    playerList.Add(record);
                }

                if (!pagination)
                    records = (uint) playerList.Count;
            }
            catch (MySqlException ex)
            {
                HandleException(ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

            return playerList;
        }

        private PlayerData BuildPlayerData(Row row)
        {
            return new PlayerData(new CSteamID(ulong.Parse(row["SteamID"].ToString())),
                row["SteamName"].ToString(), row["CharName"].ToString(), row["IP"].ToString(),
                long.Parse(row["LastLoginGlobal"].ToString()).FromTimeStamp(), ushort.Parse(row["LastServerID"].ToString()),
                row["LastServerName"].ToString(), ushort.Parse(row["ServerID"].ToString()),
                long.Parse(row["LastLoginLocal"].ToString()).FromTimeStamp(), int.Parse(row["TotalPlayTime"].ToString()));
        }

        internal bool RemoveInstance(ushort instanceId)
        {
            if (!Initialized) return false;

            MySqlDataReader reader = null;
            var records = new Dictionary<ulong, object[]>();
            try
            {
                var command = Connection.CreateCommand();
                command.Parameters.AddWithValue("@forinstance", instanceId);
                command.CommandText = "SELECT ServerID FROM `" + TableInstance + "` WHERE ServerID = @forinstance;";
                var result = command.ExecuteScalar();
                if (result == null) return false;
                command.CommandText = "SELECT SteamID FROM `" + TableServer + "` WHERE ServerID = @forinstance";
                reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read()) records.Add(reader.GetUInt64("SteamID"), new object[] { });
                    reader.Close();
                    reader.Dispose();
                    ProcessRecordRemoval(command, records, instanceId);
                    command.CommandText =
                        "DELETE FROM `" + TableInstance + "` WHERE ServerID = " + instanceId + ";";
                    command.ExecuteNonQuery();
                    if (instanceId == this.InstanceId)
                        Initialized = false;
                    return true;
                }
            }
            catch (MySqlException ex)
            {
                HandleException(ex);
                return false;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                    reader.Dispose();
                }
            }

            return true;
        }

        private void ProcessRecordRemoval(MySqlCommand command, Dictionary<ulong, object[]> records, ushort instanceId,
            bool instanceRemoval = true)
        {
            try
            {
                var totalRemoved = 0;
                var totalRemovedServer = 0;
                var recordNum = 0;
                if (instanceRemoval)
                    Logger.Log(
                        $"Starting player info removal process For the entered Instance ID, number of records to process: {records.Count}.",
                        ConsoleColor.Yellow);
                else
                    Logger.Log(
                        $"Starting expired player info cleanup process, number of records to cleanup in this batch: {records.Count}",
                        ConsoleColor.Yellow);

                foreach (var val in records)
                {
                    var count = 0;
                    recordNum++;
                    if (recordNum % 1000 == 0)
                        Logger.Log($"Processing record: {recordNum} of {records.Count}");
                    command.CommandText = "SELECT COUNT(*) as count FROM `" + TableServer + "` WHERE SteamID = " +
                                          val.Key + ";";
                    var resultc = command.ExecuteScalar();
                    if (resultc != null && resultc != DBNull.Value)
                        if (int.TryParse(resultc.ToString(), out count))
                        {
                            if (!instanceRemoval)
                                Logger.Log(
                                    $"Removing Player info for: {val.Value[0]} [{val.Value[1]}] ({val.Key})");
                            if (count <= 1)
                            {
                                command.CommandText = "DELETE FROM `" + Table + "` WHERE SteamID = " + val.Key + ";";
                                command.ExecuteNonQuery();
                                totalRemoved++;
                            }

                            command.CommandText = "DELETE FROM `" + TableServer + "` WHERE SteamID = " + val.Key +
                                                  " AND ServerID = " + instanceId + ";";
                            command.ExecuteNonQuery();
                            totalRemovedServer++;
                        }
                }

                Logger.Log(
                    $"Finished player info cleanup. Number cleaned: {TableServer}: {totalRemovedServer}, {Table}: {totalRemoved}.",
                    ConsoleColor.Yellow);
            }
            catch (MySqlException ex)
            {
                HandleException(ex);
            }
        }

        public void SaveToDb(PlayerData pdata)
        {
            if (!Initialized)
            {
                Logger.LogError("Error: Cant save player info, plugin hasn't initialized properly.");
                return;
            }

            if (!pdata.IsValid())
            {
                Logger.LogError("Error: Invalid player data information.");
                return;
            }

            RequestQueryExecute(false,
                new Query(
                    $"INSERT INTO `{Configuration.TableNamePlayers}` (`SteamID`, `SteamName`, `CharName`, `IP`, `LastLoginGlobal`, `TotalPlayTime`, `LastServerID`) VALUES (@steamid, @steamname, @charname, @ip, @lastloginglobal, @totalplaytime, @lastinstanceid) ON DUPLICATE KEY UPDATE `SteamName` = VALUES(`SteamName`), `CharName` = VALUES(`CharName`), `IP` = VALUES(`IP`), `LastLoginGlobal` = VALUES(`LastLoginglobal`), `TotalPlayTime` = VALUES(`TotalPlayTime`), `LastServerID` = VALUES(`LastServerID`);",
                    EQueryType.NonQuery, null, false, new MySqlParameter("@steamid", pdata.SteamId), new MySqlParameter("@steamname", pdata.SteamName.Truncate(200)), new MySqlParameter("@charname", pdata.CharacterName.Truncate(200)),
                    new MySqlParameter("@ip", Parser.getUInt32FromIP(pdata.Ip)), new MySqlParameter("@lastinstanceid", pdata.LastServerId), new MySqlParameter("@lastloginglobal", pdata.LastLoginGlobal.ToTimeStamp()), new MySqlParameter("@totalplaytime", pdata.TotalPlayime),
                    new MySqlParameter(), new MySqlParameter()));
            
                
                command.Parameters.AddWithValue();
                command.Parameters.AddWithValue("@instanceid", pdata.ServerId);
                command.Parameters.AddWithValue();
                command.Parameters.AddWithValue();
                command.Parameters.AddWithValue();
                command.Parameters.AddWithValue("@lastloginlocal", pdata.LastLoginLocal.ToTimeStamp());
        }
    }
}