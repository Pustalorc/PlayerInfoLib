﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using SDG.Unturned;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    [ServiceImplementation(Lifetime = ServiceLifetime.Scoped, Priority = Priority.Normal)]
    public class PlayerInfoRepositoryServiceImplementation : IPlayerInfoRepository
    {
        private readonly PlayerInfoLibDbContext m_DbContext;

        public PlayerInfoRepositoryServiceImplementation(PlayerInfoLibDbContext dbContext)
        {
            m_DbContext = dbContext;
        }

        #region Get Servers

        public async Task<Server> GetCurrentServerAsync()
        {
            if (!await CheckCurrentServerExistsAsync())
                return null;

            return await GetCurrentServerInternalAsync();
        }

        public Server GetCurrentServer()
        {
            return !CheckCurrentServerExists() ? null : GetCurrentServerInternal();
        }

        public async Task<Server> GetServerAsync(int id)
        {
            return await m_DbContext.Servers.FirstOrDefaultAsync(k => k.Id == id);
        }

        public Server GetServer(int id)
        {
            return m_DbContext.Servers.FirstOrDefault(k => k.Id == id);
        }

        #endregion

        #region Check Current Server Existance

        public async Task<bool> CheckCurrentServerExistsAsync()
        {
            var server = await GetCurrentServerInternalAsync();

            return server != null;
        }

        public bool CheckCurrentServerExists()
        {
            var server = GetCurrentServerInternal();

            return server != null;
        }

        public async Task<Server> CheckAndRegisterCurrentServerAsync()
        {
            var server = await GetCurrentServerInternalAsync();
            if (server == null)
                await m_DbContext.Servers.AddAsync(new Server
                {
                    Instance = Provider.serverID,
                    Name = Provider.serverName
                });
            else
                server.Name = Provider.serverName;

            await m_DbContext.SaveChangesAsync();

            return server;
        }

        public Server CheckAndRegisterCurrentServer()
        {
            var server = GetCurrentServerInternal();
            if (server == null)
                m_DbContext.Servers.Add(new Server
                {
                    Instance = Provider.serverID,
                    Name = Provider.serverName
                });
            else
                server.Name = Provider.serverName;

            m_DbContext.SaveChanges();
            return server;
        }

        #endregion

        #region Find Players

        public async Task<PlayerData> FindPlayerAsync(string searchTerm, UserSearchMode searchMode)
        {
            return await FindMultiplePlayersInternal(searchTerm, searchMode).FirstOrDefaultAsync();
        }

        public PlayerData FindPlayer(string searchTerm, UserSearchMode searchMode)
        {
            return FindMultiplePlayersInternal(searchTerm, searchMode).FirstOrDefault();
        }

        public async Task<List<PlayerData>> FindMultiplePlayersAsync(string searchTerm, UserSearchMode searchMode)
        {
            return await FindMultiplePlayersInternal(searchTerm, searchMode).ToListAsync();
        }

        public List<PlayerData> FindMultiplePlayers(string searchTerm, UserSearchMode searchMode)
        {
            return FindMultiplePlayersInternal(searchTerm, searchMode).ToList();
        }

        private IQueryable<PlayerData> FindMultiplePlayersInternal(string searchTerm, UserSearchMode searchMode)
        {
            return searchMode switch
            {
                UserSearchMode.Id => GetPlayerByIdInternal(searchTerm),
                UserSearchMode.Name => GetPlayerByNameInternal(searchTerm),
                UserSearchMode.NameOrId => GetPlayerByIdInternal(searchTerm)
                    .Concat(GetPlayerByNameInternal(searchTerm)),
                _ => Array.Empty<PlayerData>().AsQueryable()
            };
        }

        #endregion

        #region Internal Checks

        private async Task<Server> GetCurrentServerInternalAsync()
        {
            return await m_DbContext.Servers.FirstOrDefaultAsync(k =>
                k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }

        private Server GetCurrentServerInternal()
        {
            return m_DbContext.Servers.FirstOrDefault(k =>
                k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }


        private IQueryable<PlayerData> GetPlayerByIdInternal(string searchTerm)
        {
            if (!ulong.TryParse(searchTerm, out var id) || id < 76561197960265728 || id > 103582791429521408)
                return Array.Empty<PlayerData>().AsQueryable();

            return m_DbContext.Players.Where(k => k.Id == id);
        }

        private IQueryable<PlayerData> GetPlayerByNameInternal(string searchTerm)
        {
            var searchLowered = searchTerm.ToLower();
            return m_DbContext.Players.Where(k =>
                k.CharacterName.ToLower().Contains(searchLowered) || k.SteamName.ToLower().Contains(searchLowered));
        }

        #endregion

        public async Task AddPlayerDataAsync(PlayerData playerData)
        {
            await m_DbContext.Players.AddAsync(playerData);
            await m_DbContext.SaveChangesAsync();
        }

        public void AddPlayerData(PlayerData playerData)
        {
            m_DbContext.Players.Add(playerData);
            m_DbContext.SaveChanges();
        }

        public PlayerData BuildPlayerData(ulong steamId, string characterName, string steamName, string hwid, uint ip,
            string profileHash, ulong questGroup, ulong steamGroup, string steamGroupName, double totalPlaytime,
            DateTime lastLogin, Server server)
        {
            return new PlayerData
            {
                Id = steamId,
                CharacterName = characterName,
                SteamName = steamName,
                Hwid = hwid,
                Ip = ip,
                ProfilePictureHash = profileHash,
                LastQuestGroupId = questGroup,
                SteamGroup = steamGroup,
                SteamGroupName = steamGroupName,
                TotalPlaytime = totalPlaytime,
                LastLoginGlobal = lastLogin,
                ServerId = server.Id,
                Server = server
            };
        }

        public async Task<int> SaveChangesAsync()
        {
            return await m_DbContext.SaveChangesAsync();
        }

        public int SaveChanges()
        {
            return m_DbContext.SaveChanges();
        }
    }
}