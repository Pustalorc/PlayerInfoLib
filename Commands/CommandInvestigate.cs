using Microsoft.Extensions.Localization;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using System;
using System.Linq;
using System.Threading.Tasks;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Pustalorc.PlayerInfoLib.Unturned.Commands
{
    [Command("investigate")]
    [CommandSyntax("<player>")]
    [CommandDescription("Investigates and gets information about a player.")]
    public class CommandInvestigate : Command
    {
        private readonly IServiceProvider m_ServiceProvider;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandInvestigate(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer) : base(serviceProvider)
        {
            m_ServiceProvider = serviceProvider;
            m_StringLocalizer = stringLocalizer;
        }

        protected override async Task OnExecuteAsync()
        {
            var actor = Context.Actor;
            var targetPlayer = await Context.Parameters.GetAsync<string>(0);

            await using var playerInfoRepository = m_ServiceProvider.GetRequiredService<IPlayerInfoRepository>();

            var players = await playerInfoRepository.FindMultiplePlayersAsync(targetPlayer, UserSearchMode.FindByNameOrId);

            if (!players.Any())
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:no_results", new {Target = targetPlayer}]);
            }
            else
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_count", new {players.Count}]);
                var firstResult = players.First();
                var server = firstResult.Server ?? await playerInfoRepository.GetServerAsync(firstResult.ServerId);
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