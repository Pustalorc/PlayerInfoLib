using System;
using Cysharp.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.Extensions;
using OpenMod.Unturned.Plugins;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using SDG.Unturned;

[assembly:
    PluginMetadata("Pustalorc.PlayerInfoLib.Unturned", Author = "Pustalorc",
        DisplayName = "Player Info Library Unturned",
        Website = "https://github.com/Pustalorc/PlayerInfoLib/tree/OpenMod")]

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class PlayerInfoLibrary : OpenModUnturnedPlugin
    {
        private readonly ILogger<PlayerInfoLibrary> m_Logger;
        private readonly PlayerInfoLibDbContext m_DbContext;

        public PlayerInfoLibrary(ILogger<PlayerInfoLibrary> logger, IServiceProvider serviceProvider,
            PlayerInfoLibDbContext dbContext) : base(serviceProvider)
        {
            m_Logger = logger;
            m_DbContext = dbContext;
        }

        protected override async UniTask OnLoadAsync()
        {
            await m_DbContext.OpenModMigrateAsync();

            var server = await m_DbContext.Servers.FirstOrDefaultAsync(d =>
                d.Instance.Equals(Provider.serverID, StringComparison.Ordinal));
            if (server == null)
                await m_DbContext.Servers.AddAsync(new Server
                {
                    Instance = Provider.serverID,
                    Name = Provider.serverName
                });
            else
                server.Name = Provider.serverName;
            await m_DbContext.SaveChangesAsync();

            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was loaded correctly.");
        }

        protected override UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was unloaded correctly.");

            return UniTask.CompletedTask;
        }
    }
}