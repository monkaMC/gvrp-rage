using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Outfits
{
    public class OutfitModule : SqlModule<OutfitModule, Outfit, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(OutfitComponentModule), typeof(OutfitPropModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `outfits`;";
        }

    }
}
