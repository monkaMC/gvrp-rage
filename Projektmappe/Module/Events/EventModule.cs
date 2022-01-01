using System.Collections.Generic;

namespace GVRP.Module.Events
{
    public sealed class EventModule : SqlModule<EventModule, Event, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `events`;";
        }
    }
}