// ReSharper disable AnnotateCanBeNullTypeMember
// ReSharper disable AnnotateNotNullTypeMember
// ReSharper disable AnnotateNotNullParameter

extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.API.Users;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    [UsedImplicitly]
    [PluginServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class PlayerInfoRepositoryServiceImplementation : IPlayerInfoRepository
    {
        private readonly IServiceProvider m_ServiceProvider;

        public PlayerInfoRepositoryServiceImplementation(IServiceProvider serviceProvider)
        {
            m_ServiceProvider = serviceProvider;
        }

        #region Database Operation Handlers

        private PlayerInfoLibDbContext GetDbContext()
        {
            return m_ServiceProvider.GetRequiredService<PlayerInfoLibDbContext>();
        }

        private async Task RunOperation(Func<PlayerInfoLibDbContext, Task> action)
        {
            await using var dbContext = GetDbContext();

            await action(dbContext);
        }

        private async Task<T> RunOperation<T>(Func<PlayerInfoLibDbContext, Task<T>> action)
        {
            await using var dbContext = GetDbContext();

            return await action(dbContext);
        }

        private void RunOperation(Action<PlayerInfoLibDbContext> action)
        {
            using var dbContext = GetDbContext();

            action(dbContext);
        }

        private T RunOperation<T>(Func<PlayerInfoLibDbContext, T> action)
        {
            using var dbContext = GetDbContext();

            return action(dbContext);
        }

        #endregion

        #region Get Servers

        public async Task<Server> GetCurrentServerAsync()
        {
            return await RunOperation(GetCurrentServerInternalAsync);
        }

        public Server GetCurrentServer()
        {
            return RunOperation(GetCurrentServerInternal);
        }

        public async Task<Server> GetServerAsync(int id)
        {
            await using var dbContext = GetDbContext();

            return await dbContext.Servers.FirstOrDefaultAsync(k => k.Id == id);
        }

        public Server GetServer(int id)
        {
            using var dbContext = GetDbContext();

            return dbContext.Servers.FirstOrDefault(k => k.Id == id);
        }

        #endregion

        #region Check Current Server Existance

        public async Task<bool> CheckCurrentServerExistsAsync()
        {
            return await RunOperation(async dbContext =>
            {
                var server = await GetCurrentServerInternalAsync(dbContext);

                return server != null;
            });
        }

        public bool CheckCurrentServerExists()
        {
            return RunOperation(dbContext =>
            {
                var server = GetCurrentServerInternal(dbContext);

                return server != null;
            });
        }

        public async Task<Server> CheckAndRegisterCurrentServerAsync()
        {
            return await RunOperation(async dbContext =>
            {
                var server = await GetCurrentServerInternalAsync(dbContext);

                if (server == null)
                {
                    server = new Server
                    {
                        Instance = Provider.serverID,
                        Name = Provider.serverName
                    };

                    await dbContext.Servers.AddAsync(server);
                }
                else
                {
                    server.Name = Provider.serverName;
                }

                await dbContext.SaveChangesAsync();

                return server;
            });
        }

        public Server CheckAndRegisterCurrentServer()
        {
            return RunOperation(dbContext =>
            {
                var server = GetCurrentServerInternal(dbContext);

                if (server == null)
                {
                    server = new Server
                    {
                        Instance = Provider.serverID,
                        Name = Provider.serverName
                    };

                    dbContext.Servers.Add(server);
                }
                else
                {
                    server.Name = Provider.serverName;
                }

                dbContext.SaveChanges();

                return server;
            });
        }

        #endregion

        #region Find Players

        public async Task<PlayerData> FindPlayerAsync(string searchTerm, UserSearchMode searchMode)
        {
            return await RunOperation(dbContext =>
                FindMultiplePlayersInternal(dbContext, searchTerm, searchMode).FirstOrDefaultAsync());
        }

        public PlayerData FindPlayer(string searchTerm, UserSearchMode searchMode)
        {
            return RunOperation(dbContext =>
                FindMultiplePlayersInternal(dbContext, searchTerm, searchMode).FirstOrDefault());
        }

        public async Task<List<PlayerData>> FindMultiplePlayersAsync(string searchTerm, UserSearchMode searchMode)
        {
            return await RunOperation(dbContext =>
                FindMultiplePlayersInternal(dbContext, searchTerm, searchMode).ToListAsync());
        }

        public List<PlayerData> FindMultiplePlayers(string searchTerm, UserSearchMode searchMode)
        {
            return RunOperation(dbContext => FindMultiplePlayersInternal(dbContext, searchTerm, searchMode).ToList());
        }

        private IQueryable<PlayerData> FindMultiplePlayersInternal(PlayerInfoLibDbContext dbContext, string searchTerm, UserSearchMode searchMode)
        {
            return searchMode switch
            {
                UserSearchMode.FindById => GetPlayerByIdInternal(dbContext, searchTerm),
                UserSearchMode.FindByName => GetPlayerByNameInternal(dbContext, searchTerm),
                UserSearchMode.FindByNameOrId => GetPlayerByIdInternal(dbContext, searchTerm)
                    .Concat(GetPlayerByNameInternal(dbContext, searchTerm)),
                _ => dbContext.Players.Take(0)
            };
        }

        #endregion

        #region Internal Checks

        private async Task<Server> GetCurrentServerInternalAsync(PlayerInfoLibDbContext dbContext)
        {
            return await dbContext.Servers.FirstOrDefaultAsync(k =>
                k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }

        private Server GetCurrentServerInternal(PlayerInfoLibDbContext dbContext)
        {
            return dbContext.Servers.FirstOrDefault(k =>
                k.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
        }


        private IQueryable<PlayerData> GetPlayerByIdInternal(PlayerInfoLibDbContext dbContext, string searchTerm)
        {
            if (!ulong.TryParse(searchTerm, out var id) || id < 76561197960265728 || id > 103582791429521408)
                return dbContext.Players.Take(0);

            return dbContext.Players.Where(k => k.Id == id);
        }

        private IQueryable<PlayerData> GetPlayerByNameInternal(PlayerInfoLibDbContext dbContext, string searchTerm)
        {
            var trimmed = searchTerm.Trim();
            if (string.IsNullOrEmpty(trimmed))
                return dbContext.Players.Take(0);

            var searchLowered = trimmed.ToLower();
            return dbContext.Players.Where(k =>
                k.CharacterName.ToLower().Contains(searchLowered) || k.SteamName.ToLower().Contains(searchLowered));
        }

        #endregion

        public async Task AddPlayerDataAsync(PlayerData playerData)
        {
            await RunOperation(async dbContext =>
            {
                await dbContext.Players.AddAsync(playerData);

                await dbContext.SaveChangesAsync();
            });
        }

        public void AddPlayerData(PlayerData playerData)
        {
            RunOperation(dbContext =>
            {
                dbContext.Players.Add(playerData);
                dbContext.SaveChanges();
            });
        }

        public async Task UpdatePlayerDataAsync(PlayerData playerData)
        {
            await RunOperation(async dbContext =>
            {
                dbContext.Entry(playerData).State = EntityState.Modified;

                await dbContext.SaveChangesAsync();
            });
        }

        public void UpdatePlayerData(PlayerData playerData)
        {
            RunOperation(dbContext =>
            {
                dbContext.Entry(playerData).State = EntityState.Modified;

                dbContext.SaveChanges();
            });
        }

        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(0);
        }

        public int SaveChanges()
        {
            return 0;
        }
    }
}
