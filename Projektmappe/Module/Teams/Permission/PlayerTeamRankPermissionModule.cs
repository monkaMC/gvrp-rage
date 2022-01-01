using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.Permission
{
    public class
        PlayerTeamRankPermissionModule : SqlSingleItemModule<PlayerTeamRankPermissionModule, TeamRankPermission,
            DbPlayer, uint>
    {
        protected override string OnItemRequest(DbPlayer dbPlayer)
        {
            return $"SELECT * FROM `player_rights` WHERE accountid = {dbPlayer.Id};";
        }

        public override int GetOrder()
        {
            return 1;
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.TeamRankPermission = RequestItem(dbPlayer) ?? dbPlayer.CreateTeamRankPermission();
        }
    }
}