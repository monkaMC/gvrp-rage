namespace GVRP.Module.Teams.Spawn
{
    public class TeamSpawnModule : SqlModule<TeamSpawnModule, TeamSpawn, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `team_spawn`;";
        }
    }
}