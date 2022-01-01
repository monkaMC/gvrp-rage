
namespace GVRP.Module.Assets.Hair
{
    public class AssetsHairModule : SqlModule<AssetsHairModule, AssetsHair, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_hair`;";
        }
    }
}
