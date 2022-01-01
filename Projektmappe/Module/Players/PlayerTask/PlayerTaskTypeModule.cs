namespace GVRP.Module.Players.PlayerTask
{
    public sealed class PlayerTaskTypeModule : SqlModule<PlayerTaskTypeModule, PlayerTaskType, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `task_types`";
        }
    }
}