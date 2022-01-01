
namespace GVRP.Module.Assets.Tattoo
{
    public class AssetsTattooModule : SqlModule<AssetsTattooModule, AssetsTattoo, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_tattoo`;";
        }
    }
}
