using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Forum
{
    public sealed class TeamForumSync : SqlModule<TeamForumSync, TeamForumSyncItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `team_forumsync`;";
        }

        public override void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {

        }
    }
}