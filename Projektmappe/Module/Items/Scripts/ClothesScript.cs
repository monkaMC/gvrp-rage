using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Items.Scripts.Presents;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool ItemToCloth(DbPlayer iPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if (!uint.TryParse(itemScript.Split('_')[1], out uint clothId))
            {
                return false;
            }

            Cloth cloth = ClothModule.Instance.GetAll().Values.Where(c => c.Id == clothId).FirstOrDefault();
            if (cloth == null) return false;

            if(iPlayer.Character.Wardrobe.Contains(clothId))
            {
                iPlayer.SendNewNotification("Sie besitzen dieses Kleidungsstück bereits!");
                return false;
            }

            ClothModule.AddNewCloth(iPlayer, clothId);
            iPlayer.SendNewNotification($"Sie haben {cloth.Name} zu Ihrem Kleiderschrank hinzugefügt!");
            return true;
        }

        public static bool ItemToProp(DbPlayer iPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if (!uint.TryParse(itemScript.Split('_')[1], out uint clothId))
            {
                return false;
            }

            Prop prop = PropModule.Instance.GetAll().Values.Where(c => c.Id == clothId).FirstOrDefault();
            if (prop == null) return false;

            if (iPlayer.Character.Props.Contains(clothId))
            {
                iPlayer.SendNewNotification("Sie besitzen dieses Kleidungsstück bereits!");
                return false;
            }

            ClothModule.AddNewProp(iPlayer, clothId);
            iPlayer.SendNewNotification($"Sie haben {prop.Name} zu Ihrem Kleiderschrank hinzugefügt!");
            return true;
        }
    }
}
