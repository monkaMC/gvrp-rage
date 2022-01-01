
namespace GVRP.Module.Assets.Chest
{
    public class AssetsChestModule : SqlModule<AssetsChestModule, AssetsChest, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `assets_chest`;";
        }
    }
}
