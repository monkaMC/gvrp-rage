using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.Permission
{
    public static class PlayerTeamRankPermission
    {
        public static void SetTeamRankPermission(this DbPlayer iPlayer, bool bank, int manage, bool inventory, string title)
        {
            if (iPlayer == null) return;
            iPlayer.TeamRankPermission.Bank = bank;
            iPlayer.TeamRankPermission.Manage = manage;
            iPlayer.TeamRankPermission.Inventory = inventory;
            iPlayer.TeamRankPermission.Title = title;

            iPlayer.TeamRankPermission.Save();
        }
        
        public static void Save(this TeamRankPermission trp)
        {
            var query =
                string.Format(
                    $"UPDATE `player_rights` SET `r_bank` = {(trp.Bank ? 1 : 0)}, `r_manage` = {trp.Manage}, `r_inventory` = {(trp.Inventory ? 1 : 0)}, `title` = '{trp.Title}' WHERE `accountid` = '{trp.PlayerId}'");

            MySQLHandler.ExecuteAsync(query);
        }

        public static TeamRankPermission CreateTeamRankPermission(this DbPlayer iPlayer)
        {
            var key = new TeamRankPermission(iPlayer);
            var query =
                string.Format(
                    $"INSERT INTO `player_rights` (`accountid`, `r_bank`, `r_manage`, `r_inventory`, `title`) VALUES ('{iPlayer.Id}', '0', '0', '0', '')");

            MySQLHandler.ExecuteAsync(query);
            return key;
        }
    }
}