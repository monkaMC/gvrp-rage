using GVRP.Module.Assets.Tattoo;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationCategoryModule : SqlModule<AnimationCategoryModule, AnimationCategory, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `animationmenu_category` ORDER by `order`;";
        }
    }
}
