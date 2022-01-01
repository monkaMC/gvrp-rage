using System;
using GVRP.Module.Assets.Tattoo;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationItemModule : SqlModule<AnimationItemModule, AnimationItem, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(AnimationCategoryModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `animationmenu_item` ORDER by `name`;";
        }
    }
}
