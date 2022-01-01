using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.Items
{
    public static class ContainerValidations
    {
        public static bool CanUseAction(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return false;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract()) return false;

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv")) return false; // Show Inventory
                        
            return true;
        }
    }
}
