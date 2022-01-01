using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Spawners;

namespace GVRP.Module.Shops
{
    public class ShopModule : SqlModule<ShopModule, Shop, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `shops` WHERE pos_x != 0 AND pos_y != 0;";
        }

        private static uint blip = 52;
        private static int color = 69;

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }

        protected override void OnItemLoaded(Shop u)
        {
            if (u.Marker) Main.ServerBlips.Add(Blips.Create(u.Position, "Store", blip, 1.0f, color: color));

        }
    }
}
