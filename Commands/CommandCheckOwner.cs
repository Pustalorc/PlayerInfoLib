using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;

namespace PlayerInfoLibrary.Commands
{
    public class CommandCheckOwner : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        [NotNull] public string Name => "checkowner";
        [NotNull] public string Help => "Checks the owner of the buildable you are looking at.";
        [NotNull] public string Syntax => "";
        [NotNull] public List<string> Aliases => new List<string>();
        [NotNull] public List<string> Permissions => new List<string> {"PlayerInfoLib.Checkowner"};

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var player = (UnturnedPlayer) caller;
            var thingLocated = DamageTool.raycast(
                new Ray(player.Player.look.aim.position, player.Player.look.aim.forward), 2048f,
                RayMasks.VEHICLE | RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.BARRICADE_INTERACT |
                RayMasks.STRUCTURE_INTERACT);

            if (thingLocated.vehicle != null)
            {
                var vehicle = thingLocated.vehicle;

                if (vehicle.lockedOwner != CSteamID.Nil)
                {
                    TellInfo(player, vehicle.lockedOwner);
                    return;
                }

                UnturnedChat.Say(caller, "Vehicle does not have an owner.");
            }
            else
            {
                if (!(thingLocated.transform != null)) return;

                var component = thingLocated.transform.GetComponent<Interactable2>();
                if (!(component.transform != null)) return;

                var val2 = (CSteamID) component.owner;
                var interactable2SalvageBarricade = component as Interactable2SalvageBarricade;
                if (interactable2SalvageBarricade != null)
                {
                    if (!BarricadeManager.tryGetInfo(interactable2SalvageBarricade.root, out _, out _, out _, out _,
                        out _)) return;

                    TellInfo(player, val2);
                }
                else
                {
                    if (!(component is Interactable2SalvageStructure)) return;
                    if (!StructureManager.tryGetInfo(thingLocated.transform, out _, out _, out _, out _)) return;

                    TellInfo(player, val2);
                }
            }
        }

        public static async Task TellInfo(UnturnedPlayer caller, CSteamID owner)
        {
            var pData = await PlayerInfoLib.Instance.database.QueryById(owner);

            if (pData == null)
            {
                UnturnedChat.Say(PlayerInfoLib.Instance.Translate("no_info", owner));
                return;
            }

            UnturnedChat.Say(caller,
                $"{pData.CharacterName.Truncate(12)} [{pData.SteamName.Truncate(12)}] ({pData.SteamId}), Group ID: {pData.LastQuestGroupId}, Group Name: {pData.GroupName}",
                Color.yellow);
            UnturnedChat.Say(caller,
                $"Seen: {pData.LastLoginGlobal}, TT: {pData.TotalPlaytime.FormatTotalTime()}",
                Color.yellow);

            caller.Player.sendBrowserRequest("SteamProfile", "http://steamcommunity.com/profiles/" + owner);
        }
    }
}