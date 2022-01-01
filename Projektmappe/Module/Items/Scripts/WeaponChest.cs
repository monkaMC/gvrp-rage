using System;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool WeaponChest(DbPlayer iPlayer, ItemModel ItemData)
        {
            // Restrict 4 Cops and Brigada
            if (iPlayer.Team.IsCops() || iPlayer.Team.Id == (int)teams.TEAM_HUSTLER || iPlayer.Team.Id == (int)teams.TEAM_NNM || iPlayer.Team.Id == (int)teams.TEAM_BRATWA || iPlayer.Team.Id == (int)teams.TEAM_ICA)
                return false;

            if (iPlayer.Container.GetItemAmount(301) < 1)
            {
                iPlayer.SendNewNotification(
                    "Um eine Waffenkiste aufzubrechen benötigen Sie ein Brecheisen!");
                return false;
            }
            
            // Choose Items
            float size = 0;
            
            while (size < ItemData.Weight)
            {
                // Results: 
                // 72 SMG, 211 AmmoSMG, 
                // 82 SpecialCarbine, 221 Ammo SpecialCarb
                // 54 Flashlight
                // 83 Bullpup, 222 Ammo Bullpup

                Random rnd = new Random();
                int rand = rnd.Next(1, 11);
                uint xItem = 0;
                int amount = 1;
                switch (rand)
                {
                    case 1:
                    case 2:
                        xItem = 72;
                        break;
                    case 3:
                    case 4:
                        xItem = 211;
                        amount = 3;
                        break;
                    case 5:
                        xItem = 82;
                        break;
                    case 6:
                        xItem = 221;
                        amount = 3;
                        break;
                    case 7:
                    case 8:
                        xItem = 54;
                        break;
                    case 9:
                        xItem = 83;
                        break;
                    case 10:
                        xItem = 222;
                        amount = 3;
                        break;
                    default:
                        xItem = 72;
                        break;
                }

                if (!iPlayer.Container.AddItem(xItem, amount))
                {
                    iPlayer.SendNewNotification("Dein Inventar ist leider voll!");
                }
                size += ItemModelModule.Instance.Get(xItem).Weight * amount;
            }

            iPlayer.SendNewNotification("Sie haben eine Waffenkiste entpackt!", title: "Waffenkiste entpackt");
            return true;
        }
    }
}
