using System.Collections.Generic;
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
        [NotNull] public List<string> Permissions => new List<string> {"PlayerInfoLib.rnint"};

        public void Execute(IRocketPlayer caller, [NotNull] string[] command)
        {
            switch (command.Length)
            {
                case 1:
                    var newName = command[0].ToLower();
                    PlayerInfoLib.Instance.database.SetInstanceName(newName,
                        output => TaskDispatcher.QueueOnMainThread(() =>
                            UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_success"))));
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_help"));
                    break;
            }
        }
    }
}