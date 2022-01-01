using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Warehouse
{
    public class WarehouseItemModule : SqlModule<WarehouseItemModule, WarehouseItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `warehouses_items`;";
        }
    }
}
