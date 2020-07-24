using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenMod.API.Users;
using OpenMod.EntityFrameworkCore;
using SDG.Unturned;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    public class PlayerInfoLibDbContext : OpenModDbContext<PlayerInfoLibDbContext>
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<PlayerData> Players { get; set; }

        public PlayerInfoLibDbContext(DbContextOptions<PlayerInfoLibDbContext> options,
            IServiceProvider serviceProvider) : base(options, serviceProvider)
        {
        }

        /// <summary>
        /// Gets the current server from the Servers DbSet.
        /// </summary>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        public async Task<Server> GetCurrentServerAsync()
        {
            await CheckServerExistsAsync();
            return await GetCurrentServerInternalAsync();
        }

        /// <summary>
        /// Gets the current server from the Servers DbSet.
        /// </summary>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        public Server GetCurrentServer()
        {
            CheckServerExists();
            return GetCurrentServerInternal();
        }

        /// <summary>
        /// Used to get the current server without verifying if it exists in the DbSet.
        /// </summary>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        private async Task<Server> GetCurrentServerInternalAsync()
        {
            return await Servers.FirstOrDefaultAsync(
                k => k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }

        /// <summary>
        /// Used to get the current server without verifying if it exists in the DbSet.
        /// </summary>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        private Server GetCurrentServerInternal()
        {
            return Servers.FirstOrDefault(k => k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }

        /// <summary>
        /// Checks the Servers DbSet to see if the current server exists in it. Also updates the server name.
        /// </summary>
        public async Task CheckServerExistsAsync()
        {
            var server = await GetCurrentServerInternalAsync();
            if (server == null)
                await Servers.AddAsync(new Server
                {
                    Instance = Provider.serverID,
                    Name = Provider.serverName
                });
            else
                server.Name = Provider.serverName;

            await SaveChangesAsync();
        }

        /// <summary>
        /// Checks the Servers DbSet to see if the current server exists in it. Also updates the server name.
        /// </summary>
        public void CheckServerExists()
        {
            var server = GetCurrentServerInternal();
            if (server == null)
                Servers.Add(new Server
                {
                    Instance = Provider.serverID,
                    Name = Provider.serverName
                });
            else
                server.Name = Provider.serverName;

            SaveChanges();
        }

        /// <summary>
        /// Retrieves a Server instance from the DbSet with the specified id.
        /// </summary>
        /// <param name="id">The auto-incrementing ID related to the server.</param>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        public async Task<Server> GetServerAsync(int id)
        {
            return await Servers.FirstOrDefaultAsync(k => k.Id == id);
        }

        /// <summary>
        /// Retrieves a Server instance from the DbSet with the specified id.
        /// </summary>
        /// <param name="id">The auto-incrementing ID related to the server.</param>
        /// <returns>An instance of Server if found, null otherwise.</returns>
        public Server GetServer(int id)
        {
            return Servers.FirstOrDefault(k => k.Id == id);
        }

        /// <summary>
        /// Searches for a players' data within the Players DbSet
        /// </summary>
        /// <param name="searchTerm">The term (a steam64ID or a name) to search for.</param>
        /// <param name="searchMode">How the term should be interpreted when searching</param>
        /// <returns>A PlayerData instance if anything is found, null otherwise.</returns>
        public async Task<PlayerData> FindPlayerAsync(string searchTerm, UserSearchMode searchMode)
        {
            return (await FindMultiplePlayersAsync(searchTerm, searchMode)).FirstOrDefault();
        }

        /// <summary>
        /// Searches for a players' data within the Players DbSet
        /// </summary>
        /// <param name="searchTerm">The term (a steam64ID or a name) to search for.</param>
        /// <param name="searchMode">How the term should be interpreted when searching</param>
        /// <returns>A PlayerData instance if anything is found, null otherwise.</returns>
        public PlayerData FindPlayer(string searchTerm, UserSearchMode searchMode)
        {
            return FindMultiplePlayers(searchTerm, searchMode).FirstOrDefault();
        }

        /// <summary>
        /// Searches for multiple players data within the Players DbSet
        /// </summary>
        /// <param name="searchTerm">The term (a steam64ID or a name) to search for.</param>
        /// <param name="searchMode">How the term should be interpreted when searching</param>
        /// <returns>A List of PlayerData instances. Never is null, only empty if nothing is found.</returns>
        public async Task<List<PlayerData>> FindMultiplePlayersAsync(string searchTerm, UserSearchMode searchMode)
        {
            var searchLowered = searchTerm.ToLower();
            switch (searchMode)
            {
                case UserSearchMode.Id:
                    if (!ulong.TryParse(searchTerm, out var id) || id < 76561197960265728 || id > 103582791429521408)
                        return new List<PlayerData>();

                    return await Players.Where(k => k.Id == id).ToListAsync();
                case UserSearchMode.Name:
                    return await Players.Where(k =>
                        k.CharacterName.ToLower().Contains(searchLowered) ||
                        k.SteamName.ToLower().Contains(searchLowered)).ToListAsync();
                case UserSearchMode.NameOrId:
                    if (!ulong.TryParse(searchTerm, out id) || id < 76561197960265728 || id > 103582791429521408)
                        return await Players.Where(k =>
                            k.CharacterName.ToLower().Contains(searchLowered) ||
                            k.SteamName.ToLower().Contains(searchLowered)).ToListAsync();

                    var result = Players.Where(k => k.Id == id);

                    if (result.Any())
                        return await Players.Where(k =>
                            k.CharacterName.ToLower().Contains(searchLowered) ||
                            k.SteamName.ToLower().Contains(searchLowered)).ToListAsync();

                    return await result.ToListAsync();
                default:
                    return new List<PlayerData>();
            }
        }

        /// <summary>
        /// Searches for multiple players data within the Players DbSet
        /// </summary>
        /// <param name="searchTerm">The term (a steam64ID or a name) to search for.</param>
        /// <param name="searchMode">How the term should be interpreted when searching</param>
        /// <returns>A List of PlayerData instances. Never is null, only empty if nothing is found.</returns>
        public List<PlayerData> FindMultiplePlayers(string searchTerm, UserSearchMode searchMode)
        {
            var searchLowered = searchTerm.ToLower();
            switch (searchMode)
            {
                case UserSearchMode.Id:
                    if (!ulong.TryParse(searchTerm, out var id) || id < 76561197960265728 || id > 103582791429521408)
                        return new List<PlayerData>();

                    return Players.Where(k => k.Id == id).ToList();
                case UserSearchMode.Name:
                    return Players.Where(k =>
                        k.CharacterName.ToLower().Contains(searchLowered) ||
                        k.SteamName.ToLower().Contains(searchLowered)).ToList();
                case UserSearchMode.NameOrId:
                    if (!ulong.TryParse(searchTerm, out id) || id < 76561197960265728 || id > 103582791429521408)
                        return Players.Where(k =>
                            k.CharacterName.ToLower().Contains(searchLowered) ||
                            k.SteamName.ToLower().Contains(searchLowered)).ToList();

                    var result = Players.Where(k => k.Id == id);

                    if (result.Any())
                        return Players.Where(k =>
                            k.CharacterName.ToLower().Contains(searchLowered) ||
                            k.SteamName.ToLower().Contains(searchLowered)).ToList();

                    return result.ToList();
                default:
                    return new List<PlayerData>();
            }
        }
    }
}