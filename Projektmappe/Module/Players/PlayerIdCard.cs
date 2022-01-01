using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Kasino;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerIdCard
    {
        public static string GetPlayerDutyStuff(this DbPlayer dbPlayer)
        {
            string returnString = "";
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = $"SELECT title FROM player_rights WHERE accountid = @playerid LIMIT 1;";
                cmd.Parameters.AddWithValue("@playerid", dbPlayer.Id);
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            returnString = reader.GetString("title");
                        }
                    }
                }
            }

            return returnString;
        }

        public static void ShowIdCard(this DbPlayer dbPlayer, Client destinationPlayer)
        {

            bool isCasinoGuest = KasinoModule.Instance.CasinoGuests.Contains(dbPlayer);
            DbPlayer destinationDbPlayer = destinationPlayer.GetPlayer();
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;

            //Wenn Spieler undercover
            if (dbPlayer.fakePerso)
            {
                destinationPlayer.TriggerEvent("showPerso", dbPlayer.fakeName, dbPlayer.fakeSurname, dbPlayer.birthday[0], "Haus " + dbPlayer.ownHouse[0], dbPlayer.Level, dbPlayer.Id, isCasinoGuest, "");
            }
            //Wenn Spieler COP || FIB und OnDuty -> Dienstausweis
            else if ((dbPlayer.TeamId == (int)teams.TEAM_MEDIC || dbPlayer.TeamId == (int) teams.TEAM_FIB || dbPlayer.TeamId == (int)teams.TEAM_COUNTYPD || dbPlayer.TeamId == (int) teams.TEAM_POLICE || dbPlayer.TeamId == (int) teams.TEAM_GOV || dbPlayer.TeamId == (int) teams.TEAM_NEWS || dbPlayer.TeamId == (int) teams.TEAM_ARMY || dbPlayer.TeamId == (int) teams.TEAM_DRIVINGSCHOOL || dbPlayer.TeamId == (int) teams.TEAM_DPOS || dbPlayer.TeamId == (int) teams.TEAM_SWAT) && dbPlayer.IsInDuty())
            {
                var crumbs = dbPlayer.GetName().Split('_');

                // Dienstnummer , Govlevel opt
                string GovLevelDescription = dbPlayer.GetPlayerDutyStuff();

                if(dbPlayer.GovLevel.Length > 0)
                {
                    GovLevelDescription += " Sicherheitsfreigabe " + dbPlayer.GovLevel;
                }

                destinationPlayer.TriggerEvent("showDienstausweis", dbPlayer.Team.ShortName, dbPlayer.TeamRank, isCasinoGuest, crumbs[0], crumbs[1], dbPlayer.IsInCasinoDuty() ? 1 : 0, GovLevelDescription);
            }
            //Anderenfalls normaler Personalausweis
            else
            {
                var crumbs = dbPlayer.GetName().Split('_');
                if (dbPlayer.IsInCasinoDuty())
                {
                    destinationPlayer.TriggerEvent("showDienstausweis", "", "", isCasinoGuest, crumbs[0], crumbs[1], dbPlayer.IsInCasinoDuty() ? 1 : 0, "");
                }
                else
                {
                    if (destinationDbPlayer.Team.IsCops() || destinationDbPlayer.TeamId == (int)teams.TEAM_GOV || destinationDbPlayer.GovLevel.Length > 0)
                    {
                        destinationPlayer.TriggerEvent("showPerso", crumbs[0], crumbs[1], dbPlayer.birthday[0],
                        "Haus " + dbPlayer.ownHouse[0], dbPlayer.Level, dbPlayer.Id, isCasinoGuest, dbPlayer.GovLevel);
                    }
                    else
                    {
                        destinationPlayer.TriggerEvent("showPerso", crumbs[0], crumbs[1], dbPlayer.birthday[0],
                        "Haus " + dbPlayer.ownHouse[0], dbPlayer.Level, dbPlayer.Id, isCasinoGuest, "");
                    }
                }
            }
        }
    }
}