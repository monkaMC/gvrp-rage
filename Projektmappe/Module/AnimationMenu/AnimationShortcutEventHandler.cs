using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Injury;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationShortcutEventHandler : Script
    {
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_ANIMATION_USE(Client client, int slot)
        {

            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.Player.IsInVehicle || !dbPlayer.CanInteract()) return;

            if (slot == 0) // Stop Animation
            {
                dbPlayer.Player.StopAnimation();
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
            }
            else if (slot == 1) // Configure
            {
                MenuManager.Instance.Build(PlayerMenu.AnimationShortCutSlotMenu, dbPlayer).Show(dbPlayer);
                return;
            }
            else
            {
                if (dbPlayer.AnimationShortcuts.ContainsKey((uint)slot))
                {
                    if (AnimationItemModule.Instance.Contains((uint)dbPlayer.AnimationShortcuts[(uint)slot]))
                    {
                        AnimationExtension.StartAnimation(dbPlayer, AnimationItemModule.Instance.Get((uint)dbPlayer.AnimationShortcuts[(uint)slot]));
                        return;
                    }
                }
                return;
            }

        }
    }

}
