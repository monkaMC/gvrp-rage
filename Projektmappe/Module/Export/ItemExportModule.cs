using System;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Export.Menu;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Export
{
    public class ItemExportModule : SqlModule<ItemExportModule, ItemExport, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] {typeof(ItemModelModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `item_exports`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new ExportItemMenu());
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer == null || !dbPlayer.IsValid() || key != Key.E) return false;

            ItemExportNpc itemExportNpc = ItemExportNpcModule.Instance.GetAll().Values.FirstOrDefault(ie => ie.Position.DistanceTo(dbPlayer.Player.Position) < 3.0f);

            if (itemExportNpc != null)
            {
                // open
                Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.ItemExportsMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }
    }
}