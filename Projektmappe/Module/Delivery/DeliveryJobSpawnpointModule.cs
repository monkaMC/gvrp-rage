using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Dealer;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Delivery
{
    public class DeliveryJobSpawnpointModule : SqlModule<DeliveryJobSpawnpointModule, DeliveryJobSpawnpoint, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `delivery_jobs_spawnpoints`;";
        }


    }
}
