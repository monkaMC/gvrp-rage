using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer
{
    public static class ComputerPlayerEvents
    {
        public static bool CanAccessComputerApp(this DbPlayer iPlayer, ComputerApp computerApp)
        {
            if (computerApp.Type == ComputerTypes.Computer)
            {
                // Wenn nicht im benötigten Team
                if (computerApp.Teams.Count > 0 && !computerApp.Teams.Contains(iPlayer.TeamId)) return false;

                // Wenn nicht benötigter Rang
                if (computerApp.Rang > 0 && iPlayer.TeamRank < computerApp.Rang) return false;

                // Wenn Duty vorrausgesetzt wird und nicht duty ist
                if (computerApp.Duty && !iPlayer.Duty && iPlayer.TeamId != (int)teams.TEAM_LSC) return false;

                return true;
            }
            else if (computerApp.Type == ComputerTypes.AdminTablet)
            {
                return (iPlayer.Rank.CanAccessFeature(computerApp.AppName) || iPlayer.Rank.CanAccessFeature("allApps"));
            }

            return false;
        }
    }
}
