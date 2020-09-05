using Autofac;
using Microsoft.Extensions.Configuration;
using OpenMod.API.Plugins;
using OpenMod.EntityFrameworkCore.Extensions;
using Pustalorc.PlayerInfoLib.Unturned.Database;

namespace Pustalorc.PlayerInfoLib.Unturned
{
    public class ContainerConfigurator : IPluginContainerConfigurator
    {
        public void ConfigureContainer(IPluginServiceConfigurationContext context)
        {
            context.ContainerBuilder.AddEntityFrameworkCoreMySql();
            context.ContainerBuilder.AddDbContext<PlayerInfoLibDbContext>();
        }
    }
}