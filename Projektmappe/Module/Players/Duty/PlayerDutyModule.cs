using MySql.Data.MySqlClient;
using GVRP.Module.GTAN;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Duty
{
    public class PlayerDutyModule : Module<PlayerDutyModule>
    {
        private const string DutyEventName = "setDuty";
        
        //Todo: this was previously in onSpawn check if its fine to do just one time as well
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            // handle player duty states
            if (!dbPlayer.Duty) return;
            if (dbPlayer.Team.HasDuty && dbPlayer.Team != null)
            {
                dbPlayer.Player.TriggerEvent(DutyEventName, true);
            }
            else
            {
                dbPlayer.Duty = false;
                dbPlayer.RemoveWeapons();
            }
        }
    }
}