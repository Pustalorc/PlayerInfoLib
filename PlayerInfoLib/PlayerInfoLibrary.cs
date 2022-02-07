extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using System;

[assembly:
    PluginMetadata("Pustalorc.PlayerInfoLib.Unturned", Author = "Pustalorc, Nuage",
        DisplayName = "Player Info Library Unturned",
        Website = "https://github.com/Pustalorc/PlayerInfoLib/")]

namespace Pustalorc.PlayerInfoLib.Unturned
{
    [UsedImplicitly]
    public class PlayerInfoLibrary : OpenModUnturnedPlugin
    {
        private readonly IPlayerInfoRepository m_PlayerInfoRepository;
        private readonly IServiceProvider m_ServiceProvider;

        public PlayerInfoLibrary(IServiceProvider serviceProvider, IPlayerInfoRepository playerInfoRepository) : base(serviceProvider)
        {
            m_PlayerInfoRepository = playerInfoRepository;
            m_ServiceProvider = serviceProvider;
        }

        protected override async UniTask OnLoadAsync()
        {
            await using var dbContext = m_ServiceProvider.GetRequiredService<PlayerInfoLibDbContext>();
            await dbContext.Database.MigrateAsync();

            await m_PlayerInfoRepository.CheckAndRegisterCurrentServerAsync();

            Logger.LogInformation("Player Info Library for Unturned by Pustalorc was loaded correctly.");
        }

        protected override UniTask OnUnloadAsync()
        {
            Logger.LogInformation("Player Info Library for Unturned by Pustalorc was unloaded correctly.");

            return UniTask.CompletedTask;
        }
    }
}
