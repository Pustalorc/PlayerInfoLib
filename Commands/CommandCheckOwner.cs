using System;
using System.Collections.Generic;
using PlayerInfoLibrary.Database;
using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using UnityEngine;
using Math = System.Math;

namespace PlayerInfoLibrary.Commands
{
    public class CommandCheckOwner : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;
        public string Name => "checkowner";
        public string Help => "Checks the owner of the buildable you are looking at.";
        public string Syntax => "";
        public List<string> Aliases => new List<string>();
        public List<string> Permissions => new List<string> { "PlayerInfoLib.Checkowner" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer player = (UnturnedPlayer)caller;
            RaycastInfo thingLocated = DamageTool.raycast(new Ray(player.Player.look.aim.position, player.Player.look.aim.forward), 2048f, RayMasks.VEHICLE | RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.BARRICADE_INTERACT | RayMasks.STRUCTURE_INTERACT);

            if (thingLocated.vehicle != null)
            {
                InteractableVehicle vehicle = thingLocated.vehicle;

                if (vehicle.lockedOwner != CSteamID.Nil)
                {
                    TellInfo(player, vehicle.lockedOwner);
                    return;
                }
                UnturnedChat.Say(caller, "Vehicle does not have an owner.");
                return;
            }
            else
            {
                if (!(thingLocated.transform != null)) { return; }
                byte x;
                byte y;
                Interactable2 component = thingLocated.transform.GetComponent<Interactable2>();
                if (!(component.transform != null)) { return; }
                CSteamID val2 = (CSteamID)component.owner;
                Interactable2SalvageBarricade interactable2SalvageBarricade = component as Interactable2SalvageBarricade;
                if (interactable2SalvageBarricade != null)
                {
                    if (!BarricadeManager.tryGetInfo(interactable2SalvageBarricade.root, out x, out y, out ushort _, out ushort index, out BarricadeRegion region))
                    {
                        return;
                    }
                    ItemBarricadeAsset asset = region.barricades[index].barricade.asset;
                    TellInfo(player, val2);
                }
                else
                {
                    if (!(component is Interactable2SalvageStructure))
                    {

                        return;
                    }
                    if (!StructureManager.tryGetInfo(thingLocated.transform, out y, out x, out ushort index2, out StructureRegion region2))
                    {

                        return;
                    }
                    ItemStructureAsset asset = region2.structures[index2].structure.asset;
                    TellInfo(player, val2);
                }
            }
        }

        public static void TellInfo(UnturnedPlayer caller, CSteamID owner)
        {
            var pData = PlayerInfoLib.Instance.database.QueryById(owner);

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