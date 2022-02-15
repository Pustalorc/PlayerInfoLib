using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.MySql.Extensions;
using Pustalorc.PlayerInfoLib.Unturned.Database;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    [UsedImplicitly]
    public class ContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.AddMySqlDbContext<PlayerInfoLibDbContext>(ServiceLifetime.Transient);
        }
    }
}
