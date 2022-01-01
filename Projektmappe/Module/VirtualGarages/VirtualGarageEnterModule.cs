using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.Players.Db;

namespace GVRP.Module.VirtualGarages
{
    public class VirtualGarageEnterModule : SqlModule<VirtualGarageEnterModule, VirtualGarageEnter, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(VirtualGarageModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `virtual_garages_enters`;";
        }
    }
}
