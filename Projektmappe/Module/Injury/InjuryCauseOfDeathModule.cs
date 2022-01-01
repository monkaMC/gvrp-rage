using System;
using GVRP.Module.Logging;

namespace GVRP.Module.Injury
{
    public class InjuryCauseOfDeathModule : SqlModule<InjuryCauseOfDeathModule, InjuryCauseOfDeath, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(InjuryTypeModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `injury_causes_of_death`;";
        }
    }
}