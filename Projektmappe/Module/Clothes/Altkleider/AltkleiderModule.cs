using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Clothes.Altkleider
{
    public class AltkleiderModule : Module<AltkleiderModule>
    {
        public static uint AltkleiderSackId = 949;
        public static string PriceInfoString = "pValue";

        public static Vector3 AltkleiderTonne = new Vector3(-325.149, -1511.14, 27.7259);

        public override bool Load(bool reload = false)
        {
            MenuManager.Instance.AddBuilder(new AltkleiderMenuBuilder());
            return base.Load(reload);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle) return false;

            if(dbPlayer.Player.Position.DistanceTo(AltkleiderTonne) < 4.0f)
            {
                if(dbPlayer.Container.GetItemAmount(AltkleiderSackId) >= 1)
                {
                    // Select Item
                    Item item = dbPlayer.Container.GetItemById((int)AltkleiderSackId);
                    if (!item.Data.ContainsKey(PriceInfoString)) return false;


                    int price = (int)item.Data[PriceInfoString];

                    dbPlayer.Container.RemoveItem(AltkleiderSackId);
                    dbPlayer.GiveMoney(price);
                    dbPlayer.SendNewNotification($"Du hast deinen Altkleidersack für ${price} abgegeben!");
                    return true;
                }
                return true;
            }
            return false;
        }

    }
}
