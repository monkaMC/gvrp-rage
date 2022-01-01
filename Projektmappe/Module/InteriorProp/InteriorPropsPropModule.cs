using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.InteriorProp
{
    public class InteriorPropsPropModule : SqlModule<InteriorPropsPropModule, InteriorPropsProp, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `interiors_props_props`;";
        }
    }
}
