using System;
using GVRP.Module.Logging;
using GVRP.Module.Spawners;

namespace GVRP.Module.Injury
{
    public class InjuryDeliverModule : SqlModule<InjuryDeliverModule, InjuryDeliver, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(InjuryDeliverIntPointModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_deliver`;";
        }

        protected override void OnItemLoaded(InjuryDeliver loadable)
        {
            loadable.ColShape = ColShapes.Create(loadable.Position, 8f);
            loadable.ColShape.SetData("injuryDeliverId", loadable.Id);
        }
    }
}