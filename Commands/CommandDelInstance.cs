using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace PlayerInfoLibrary.Commands
{
    public class CommandDelInstance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "delint";

        public string Help =>
            "Uses the numerical Instance ID for a server to remove all player data saved for that server.";

        public string Syntax => "<InstanceId>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> {"PlayerInfoLib.delint"};

        public void Execute(IRocketPlayer caller, string[] command)
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
                        output => UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("delint_success")));
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("delint_help"));
                    break;
            }
        }
    }
}