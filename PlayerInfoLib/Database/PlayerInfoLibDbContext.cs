using Microsoft.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore.Configurator;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes;
using System;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    public class PlayerInfoLibDbContext : OpenModDbContext<PlayerInfoLibDbContext>
    {
        public DbSet<Server> Servers => Set<Server>();
        public DbSet<PlayerData> Players => Set<PlayerData>();

        public PlayerInfoLibDbContext(IDbContextConfigurator configurator, IServiceProvider serviceProvider) : base(configurator, serviceProvider)
        {
        }
    }
}
