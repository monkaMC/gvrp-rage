using System;
using GVRP.Module.Logging;
using GVRP.Module.Spawners;

namespace GVRP.Module.Injury
{
    public class InjuryDeliverIntPointModule : SqlModule<InjuryDeliverIntPointModule, InjuryDeliverIntPoint, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_deliver_int_points`;";
        }
    }
}