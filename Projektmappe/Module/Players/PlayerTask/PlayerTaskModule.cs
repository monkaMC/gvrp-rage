using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.PlayerTask
{
    public sealed class PlayerTaskModule : Module<PlayerTaskModule>
    {
        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.LoadTasks();
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

        }
    }
}