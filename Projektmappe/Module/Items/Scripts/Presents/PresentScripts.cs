using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items.Scripts.Presents;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool PresentScript(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Container.GetInventoryFreeSpace() < 30000 || iPlayer.Container.MaxSlots - iPlayer.Container.GetUsedSlots() < 2)
            {
                iPlayer.SendNewNotification("Du benoetigst mehr Platz in den Taschen! (30kg & 2 Platz)");
                return false;
            }

            List<Present> presents = PresentModule.Instance.GetByItemId(ItemData.Id);

            if(presents.Count > 0)
            {
                Present ResultPresent = presents[new Random().Next(presents.Count)];
                ItemModel Result = ResultPresent.ResultItem;

                iPlayer.Container.AddItem(Result, ResultPresent.Amount);

                iPlayer.SendNewNotification("Du hast ein " + ItemData.Name + " geoeffnet und " + Result.Name + " erhalten!");
                return true;
            }
            
            // RefreshInventory
            return false;
        }
    }
}
