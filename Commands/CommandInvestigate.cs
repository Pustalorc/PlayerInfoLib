using Microsoft.Extensions.Localization;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using System;
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

        public CommandInvestigate(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer,
            PlayerInfoLibDbContext dbContext) : base(serviceProvider)
        {
            m_DbContext = dbContext;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var actor = Context.Actor;
            var targetPlayer = await Context.Parameters.GetAsync<string>(0);

            var players = m_DbContext.FindMultiplePlayers(targetPlayer, UserSearchMode.NameOrId);

            if (!players.Any())
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:no_results", new {Target = targetPlayer}]);
            }
            else
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_count", new {players.Count}]);
                var firstResult = players.First();
                var server = await m_DbContext.GetServerAsync(firstResult.ServerId);
                var timeSpan = TimeSpan.FromSeconds(firstResult.TotalPlaytime);

                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_text",
                    new
                    {
                        Data = firstResult, ServerName = server?.Name ?? "",
                        TotalPlaytimeFormatted = m_StringLocalizer["timestamp_format", new {Span = timeSpan}]
                    }]);
            }
        }
    }
}