using System;
using GVRP.Module.Logging;

namespace GVRP.Module.Items
{
    public class StaticContainerModule : SqlModule<StaticContainerModule, StaticContainer, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemsModule), typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `container_static_data`;";
        }
    }
}