using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items.Scripts.Presents;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool PresentKFZBrief(DbPlayer iPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if (!uint.TryParse(itemScript.Split('_')[1], out uint VehicleModelId))
            {
                return false;
            }
            VehicleData vehModel = VehicleDataModule.Instance.GetDataById(VehicleModelId);

            if (vehModel == null) return false;

            var crumbs = iPlayer.GetName().Split('_');

            string firstLetter;
            string secondLetter;

            if (crumbs.Length == 2 && crumbs[0].Length > 0 && crumbs[1].Length > 0)
            {
                firstLetter = crumbs[0][0].ToString();
                secondLetter = crumbs[1][0].ToString();
            }
            else
            {
                firstLetter = "";
                secondLetter = "";
            }

            var query = $"INSERT INTO `vehicles` (`owner`, `garage_id`, `inGarage`, `plate`, `model`, `vehiclehash`) VALUES ('{iPlayer.Id}', '1', '1', '{firstLetter + secondLetter + " " + iPlayer.ForumId}', '{vehModel.Id}', '{vehModel.Model}');";

            MySQLHandler.ExecuteAsync(query);

            iPlayer.SendNewNotification("Du hast " + ItemData.Name + " eingelöst und ein " + vehModel.Model + " erhalten!");

            // RefreshInventory
            return true;
        }

        public static bool PresentMoney(DbPlayer iPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if (!Int32.TryParse(itemScript.Split('_')[1], out int Amount))
            {
                return false;
            }

            iPlayer.GiveMoney(Amount);
            iPlayer.SendNewNotification("Du hast " + ItemData.Name + " eingelöst und $" + Amount + " erhalten!");

            // RefreshInventory
            return true;
        }
    }
}
