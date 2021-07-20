using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenMod.EntityFrameworkCore;
using Pustalorc.PlayerInfoLib.Unturned.API.Classes;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    public class PlayerInfoLibDbContext : OpenModDbContext<PlayerInfoLibDbContext>
    {
        public DbSet<Server> Servers { get; set; }
        public DbSet<PlayerData> Players { get; set; }

        public PlayerInfoLibDbContext(DbContextOptions<PlayerInfoLibDbContext> options,
            IServiceProvider serviceProvider) : base((OpenMod.EntityFrameworkCore.Configurator.IDbContextConfigurator)options, serviceProvider)
        {
        }

        internal Task OpenModMigrateAsync()
        {
            throw new NotImplementedException();
        }
    }
}
