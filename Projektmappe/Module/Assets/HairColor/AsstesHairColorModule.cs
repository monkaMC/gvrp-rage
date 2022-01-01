using System;
using GVRP.Module.Barber;

namespace GVRP.Module.Assets.HairColor
{
    public class AssetsHairColorModule : SqlModule<AssetsHairColorModule, AssetsHairColor, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_hair_color`;";
        }
    }
}
