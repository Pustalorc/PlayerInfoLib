using System.Collections.Generic;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Core.Utils;
using Rocket.Unturned.Chat;

namespace PlayerInfoLibrary.Commands
{
    public class CommandDelInstance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        [NotNull] public string Name => "delint";

        [NotNull]
        public string Help =>
            "Uses the numerical Instance ID for a server to remove all player data saved for that server.";

        [NotNull] public string Syntax => "<InstanceId>";
        [NotNull] public List<string> Aliases => new List<string>();
        [NotNull] public List<string> Permissions => new List<string> {"PlayerInfoLib.delint"};

        public void Execute(IRocketPlayer caller, [NotNull] string[] command)
        {
            switch (command.Length)
            {
                case 1:
                    if (!ushort.TryParse(command[0], out var id))
                    {
                        UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("delint_invalid"));
                        return;
                    }

                    PlayerInfoLib.Instance.database.RemoveInstance(id,
                        output => TaskDispatcher.QueueOnMainThread(() =>
                            UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("delint_success"))));
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("delint_help"));
                    break;
            }
        }
    }
}