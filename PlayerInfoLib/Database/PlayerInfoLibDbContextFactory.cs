extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.EntityFrameworkCore.MySql;

namespace Pustalorc.PlayerInfoLib.Unturned.Database
{
    [UsedImplicitly]
    public class PlayerInfoLibDbContextFactory : OpenModMySqlDbContextFactory<PlayerInfoLibDbContext>
    {
    }
}
