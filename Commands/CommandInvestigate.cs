using Microsoft.Extensions.Localization;
using OpenMod.API.Users;
using OpenMod.Core.Commands;
using Pustalorc.PlayerInfoLib.Unturned.Database;
using System;
using System.Linq;
using System.Threading.Tasks;
using Pustalorc.PlayerInfoLib.Unturned.API.Services;

namespace Pustalorc.PlayerInfoLib.Unturned.Commands
{
    [Command("investigate")]
    [CommandSyntax("<player>")]
    [CommandDescription("Investigates and gets information about a player.")]
    public class CommandInvestigate : Command
    {
        private readonly IPlayerInfoRepository m_PlayerInfoRepository;
        private readonly IStringLocalizer m_StringLocalizer;

        public CommandInvestigate(IServiceProvider serviceProvider, IStringLocalizer stringLocalizer,
            IPlayerInfoRepository playerInfoRepository) : base(serviceProvider)
        {
            m_StringLocalizer = stringLocalizer;
            m_PlayerInfoRepository = playerInfoRepository;
        }

        protected override async Task OnExecuteAsync()
        {
            var actor = Context.Actor;
            var targetPlayer = await Context.Parameters.GetAsync<string>(0);

            var players = await m_PlayerInfoRepository.FindMultiplePlayersAsync(targetPlayer, UserSearchMode.NameOrId);

            if (!players.Any())
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:no_results", new {Target = targetPlayer}]);
            }
            else
            {
                await actor.PrintMessageAsync(m_StringLocalizer["investigate:result_count", new {players.Count}]);
                var firstResult = players.First();
                var server = firstResult.Server ?? await m_PlayerInfoRepository.GetServerAsync(firstResult.ServerId);
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