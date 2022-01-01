using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.InteriorProp
{
    public class InteriorsPropModule : SqlModule<InteriorsPropModule, InteriorsProp, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(InteriorPropsPropModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `interiors_props`;";
        }
        
        protected override void OnItemLoaded(InteriorsProp loadable)
        {
            foreach (InteriorPropsProp interiorPropsProp in InteriorPropsPropModule.Instance.GetAll().Values.Where(ipp => ipp.InteriorsPropId == loadable.Id))
            {
                loadable.Props.Add(interiorPropsProp.ObjectName);
            }

            // Create Colshape
            if(loadable.AutoLoad)
            {
                ColShape col = Spawners.ColShapes.Create(loadable.Position, loadable.Range);
                col.SetData("interiorPropId", loadable.Id);
            }
        }
    }
}
