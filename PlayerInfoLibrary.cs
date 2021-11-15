using System;
using Cysharp.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;
using Pustalorc.PlayerInfoLib.Unturned.Database;

[assembly:
    PluginMetadata("Pustalorc.PlayerInfoLib.Unturned", Author = "Pustalorc, Nuage",
        DisplayName = "Player Info Library Unturned",
        Website = "https://github.com/Pustalorc/PlayerInfoLib/")]

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class PlayerInfoLibrary : OpenModUnturnedPlugin
    {
        private readonly ILogger<PlayerInfoLibrary> m_Logger;
        private readonly IServiceProvider m_ServiceProvider;

        public PlayerInfoLibrary(ILogger<PlayerInfoLibrary> logger, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_Logger = logger;
            m_ServiceProvider = serviceProvider;
        }

        protected override async UniTask OnLoadAsync()
        {
            await using var dbContext = m_ServiceProvider.GetRequiredService<PlayerInfoLibDbContext>();
            await dbContext.Database.MigrateAsync();

            await using var playerInfoRepository = m_ServiceProvider.GetRequiredService<IPlayerInfoRepository>();
            await playerInfoRepository.CheckAndRegisterCurrentServerAsync();

            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was loaded correctly.");
        }

        protected override UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was unloaded correctly.");

            return UniTask.CompletedTask;
        }
    }
}
