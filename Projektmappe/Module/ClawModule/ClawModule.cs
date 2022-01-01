using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.ClawModule
{
    public sealed class ClawModule : SqlModule<ClawModule, Claw, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT log_vehicleclaw.*, player.Name FROM `log_vehicleclaw` INNER JOIN player ON player.id = player_id WHERE (reason <> '' AND status = 1) OR (status = 0 AND reason <> '');";
        }
    }
}
