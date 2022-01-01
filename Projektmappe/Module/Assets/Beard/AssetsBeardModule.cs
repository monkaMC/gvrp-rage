
namespace GVRP.Module.Assets.Beard
{
    public class AssetsBeardModule : SqlModule<AssetsBeardModule, AssetsBeard, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_beard`;";
        }
    }
}
