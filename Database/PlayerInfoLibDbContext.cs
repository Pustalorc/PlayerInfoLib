using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore.Configurator;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    public class PlayerInfoLibDbContext : OpenModDbContext<PlayerInfoLibDbContext>
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<PlayerData> Players { get; set; }

        public PlayerInfoLibDbContext(IDbContextConfigurator configurator, IServiceProvider serviceProvider) : base(configurator, serviceProvider)
        {
        }
    }
}
