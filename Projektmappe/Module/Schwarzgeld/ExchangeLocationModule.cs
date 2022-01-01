using System.Linq;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Schwarzgeld.Menu;

namespace GVRP.Module.Schwarzgeld
{
    public class ExchangeLocationModule : SqlModule<ExchangeLocationModule, ExchangeLocation, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `exchange_locations`;";
        }

        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new ExchangeMenuBuilder());
            MenuManager.Instance.AddBuilder(new ExchangeVehicleMenuBuilder());
            return base.OnLoad();
        }

        public ExchangeLocation getTeamExchangeLocation(uint teamId)
        {
            return Instance.GetAll().Where(d => d.Value.Team_id == teamId).FirstOrDefault().Value;
        }

        public ExchangeLocation getTeamExchangeLocationByPosition(DbPlayer dbPlayer)
        {
            return Instance.GetAll().Where(d => d.Value.Position.DistanceTo(dbPlayer.Player.Position) <= 2.0f && d.Value.Team_id == dbPlayer.TeamId).FirstOrDefault().Value;
        }

        public ExchangeLocation getExchangeDestroyLocationByPosition(DbPlayer dbPlayer)
        {
            return Instance.GetAll().Where(d => d.Value.ExchangeDestroyLocation.DistanceTo(dbPlayer.Player.Position) <= 2.0f && d.Value.Team_id == dbPlayer.TeamId).FirstOrDefault().Value;
        }

        public ExchangeLocation getAlertedExchangeLocation()
        {
            return Instance.GetAll().Where(d => d.Value.Alerted == true).FirstOrDefault().Value;
        }

        public void UpdateTeamExchangeAlert(uint teamId)
        {
            ExchangeLocation location = getTeamExchangeLocation(teamId);
            location.Alerted = !location.Alerted;
        }

        public void UpdateBestochen(uint teamId)
        {
            ExchangeLocation location = getTeamExchangeLocation(teamId);
            location.Bestochen = true;
        }

        public void UpdateExchangeValue(uint teamId)
        {
            ExchangeLocation location = getTeamExchangeLocation(teamId);
            location.ExchangedAmount += ExchangeModule.Instance.ExchangeValue;

            string query = $"UPDATE `exchange_locations` SET `exchanged_amount` = '{location.ExchangedAmount}' WHERE `team_id` = '{teamId}'";
            MySQLHandler.ExecuteAsync(query);
        }

        //public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        //{
        //    if (dbPlayer.Player.IsInVehicle) return false;
        //    if (!dbPlayer.IsAGangster()) return false;
        //    if (key != Key.E) return false;

        //    ExchangeLocation location = Instance.getTeamExchangeLocationByPosition(dbPlayer);

        //    if (location != null)
        //    {
        //        MenuManager.Instance.Build(PlayerMenu.ExchangeMenu, dbPlayer).Show(dbPlayer);
        //    }

        //    return false;
        //}
    }
}
