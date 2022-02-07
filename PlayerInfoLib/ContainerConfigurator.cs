using OpenMod.API.Plugins;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using OpenMod.EntityFrameworkCore.MySql.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class ContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.AddMySqlDbContext<PlayerInfoLibDbContext>(ServiceLifetime.Transient);
        }
    }
}
