using System;
using Cysharp.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.Extensions;
using OpenMod.Unturned.Plugins;
using Pustalorc.PlayerInfoLib.Unturned.Database;

[assembly:
    PluginMetadata("Pustalorc.PlayerInfoLib.Unturned", Author = "Pustalorc",
        DisplayName = "Player Info Library Unturned",
        Website = "https://github.com/Pustalorc/PlayerInfoLib/")]

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
            await m_DbContext.CheckServerExistsAsync();

            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was loaded correctly.");
        }

        protected override UniTask OnUnloadAsync()
        {
            m_Logger.LogInformation("Player Info Library for Unturned by Pustalorc was unloaded correctly.");

            return UniTask.CompletedTask;
        }
    }
}