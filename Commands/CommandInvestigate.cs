using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OpenMod.Core.Commands;
using Org.BouncyCastle.Asn1.X509;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pustalorc.PlayerInfoLib.Unturned.Commands
{
    [Command("investigate")]
    [CommandSyntax("<player>")]
    [CommandDescription("Renames the current instance in the DB ")]
    public class CommandInvestigate : Command
    {
        private readonly IStringLocalizer m_StringLocalizer;
        private readonly PlayerInfoLibDbContext m_DbContext;

        public CommandInvestigate(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer, PlayerInfoLibDbContext dbContext) : base(serviceProvider)
        {
            m_DbContext = dbContext;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var actor = Context.Actor;
            var targetPlayer = await Context.Parameters.GetAsync<string>(0);

            List<PlayerData> players = new List<PlayerData>();

            if (ulong.TryParse(targetPlayer, out var id) && id >= 76561197960265728 && id <= 103582791429521408)
            {
                var data = await m_DbContext.Players.FirstOrDefaultAsync(k => k.Id == id);

                if (data != null)
                    players.Add(data);
            }
            else
            {
                var data = m_DbContext.Players.Where(k => k.CharacterName.ToLower().Contains(targetPlayer.ToLower()) || k.SteamName.ToLower().Contains(targetPlayer.ToLower()));

                if (data != null)
                    players.AddRange(data);
            }

            if (players.Count == 0)
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:no_results", new { Target = targetPlayer }]);
            }
            else
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_count", new { players.Count }]);
                var firstResult = players.First();
                var server = await m_DbContext.Servers.FirstOrDefaultAsync(k => k.Id == firstResult.ServerId);
                var timeSpan = TimeSpan.FromSeconds(firstResult.TotalPlaytime);

                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_text", new { Data = firstResult, ServerName = server.Name ?? "", TotalPlaytimeFormatted = m_StringLocalizer["timestamp_format", new { Span = timeSpan }] }]);
            }
        }
    }
}
