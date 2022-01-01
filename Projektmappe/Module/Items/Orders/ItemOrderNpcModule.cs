using System;
using System.Linq;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items
{
    public class ItemOrderNpcModule : SqlModule<ItemOrderNpcModule, ItemOrderNpc, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemOrderNpcItemModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `itemorder_npc`;";
        }

        public ItemOrderNpc GetByPlayerPosition(DbPlayer dbPlayer)
        {
            foreach (ItemOrderNpc itemOrderNpc in GetAll().Values)
            {
                if (dbPlayer.Player.Position.DistanceTo(itemOrderNpc.Position) < 3.0f)
                {
                    return itemOrderNpc;
                }
            }
            return null;
        }
    }
}
