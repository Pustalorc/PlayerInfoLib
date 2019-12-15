using System.Collections.Generic;
using Rocket.API;
using Rocket.Unturned.Chat;

namespace PlayerInfoLibrary.Commands
{
    public class CommandRnInstance : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Console;
        public string Name => "rnint";
        public string Help => "Renames this instance in the database.";
        public string Syntax => "<InstanceName>";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> {"PlayerInfoLib.rnint"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            switch (command.Length)
            {
                case 1:
                    var newName = command[0].ToLower();
                    PlayerInfoLib.Instance.database.SetInstanceName(newName,
                        output => UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_success")));
                    break;
                default:
                    UnturnedChat.Say(caller, PlayerInfoLib.Instance.Translate("rnint_help"));
                    break;
            }
        }
    }
}