using System;
using System.Linq;
using GVRP.Module.Logging;

namespace GVRP.Module.Items
{
    public class ItemOrderNpcItemModule : SqlModule<ItemOrderNpcItemModule, ItemOrderNpcItem, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }
        
        protected override string GetQuery()
        {
            return "SELECT * FROM `itemorder_npc_items`;";
        }
    }
}
