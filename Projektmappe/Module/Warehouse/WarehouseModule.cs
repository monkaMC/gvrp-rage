using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Warehouse
{
    public class WarehouseModule : SqlModule<WarehouseModule, Warehouse, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(WarehouseItemModule) };
        }
        protected override string GetQuery()
        {
            return "SELECT * FROM `warehouses`;";
        }

        public Warehouse GetThis(Vector3 position)
        {
            return Instance.GetAll().Values.FirstOrDefault(fs => fs.Position.DistanceTo(position) < 3.0f);
        }
        
        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new WarehouseBuyMenuBuilder());
            MenuManager.Instance.AddBuilder(new WarehouseSellMenuBuilder());
            MenuManager.Instance.AddBuilder(new WarehouseMenuBuilder());
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;

            Warehouse warehouse = WarehouseModule.Instance.GetThis(dbPlayer.Player.Position);
            if (warehouse != null)
            {
                MenuManager.Instance.Build(PlayerMenu.WarehouseMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }
    }
}
