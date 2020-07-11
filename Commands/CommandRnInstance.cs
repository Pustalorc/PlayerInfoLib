using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;

namespace PlayerInfoLibrary.Commands
{
    public class CommandRnInstance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        [NotNull] public string Name => "rnint";
        [NotNull] public string Help => "Renames this instance in the database.";
        [NotNull] public string Syntax => "<InstanceName>";
        [NotNull] public List<string> Aliases => new List<string>();
        [NotNull] public List<string> Permissions => new List<string> { "PlayerInfoLib.rnint" };

        public void Execute(IRocketPlayer caller, [NotNull] string[] command)
        {
            switch (command.Length)
            {
                case 1:
                    var newName = command[0];
                    GetAndDisplayResult(caller, newName);
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_help"));
                    break;
            }
        }

        public async Task GetAndDisplayResult(IRocketPlayer caller, string newName)
        {
            var changed = await PlayerInfoLib.Instance.database.SetInstanceName(newName);

            UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate(changed ? "rnint_success" : "rnint_fail"));
        }
    }
}