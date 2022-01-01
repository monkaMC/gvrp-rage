using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Spawners;

namespace GVRP.Module.NpcSpawner
{
    public class AdditionallyNpcModule : SqlModule<AdditionallyNpcModule, AdditionallyNpc, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `additionally_npcs`;";
        }
    }
}
