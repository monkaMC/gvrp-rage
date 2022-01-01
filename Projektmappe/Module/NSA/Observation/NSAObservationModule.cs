using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.NSA.Observation
{
    public class NSAObservationModule : SqlModule<NSAObservationModule, NSADBObservation, uint>
    {

        public static Dictionary<uint, NSAObservation> ObservationList = new Dictionary<uint, NSAObservation>();

        public static List<NSAPeilsender> NSAPeilsenders = new List<NSAPeilsender>();

        protected override bool OnLoad()
        {
            ObservationList = new Dictionary<uint, NSAObservation>();
            NSAPeilsenders = new List<NSAPeilsender>();
            return base.OnLoad();
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `nsa_observation_players`;";
        }

        protected override void OnItemLoaded(NSADBObservation loadable)
        {
            // Add To global Dictionary on Loading...
            if(!ObservationList.ContainsKey((uint)loadable.PlayerId))
            {
                ObservationList.Add((uint)loadable.PlayerId, new NSAObservation()
                {
                    PlayerId = loadable.PlayerId,
                    AcceptedPlayerId = loadable.AcceptedPlayerId,
                    Added = loadable.Added,
                    Agreed = loadable.Agreed
                });
            }
        }

        public void AddObservation(DbPlayer dbPlayer, DbPlayer l_FindPlayer, string reason)
        {
            NSAObservationModule.ObservationList.Add(l_FindPlayer.Id, new NSAObservation()
            {
                PlayerId = (int)l_FindPlayer.Id,
                AcceptedPlayerId = (int)dbPlayer.Id,
                Added = DateTime.Now,
                Agreed = false,
                Reason = reason,
            });

            // Add to DB for after restart compatiblity ... :)
            MySQLHandler.ExecuteAsync($"INSERT INTO `nsa_observation_players` (`player_id`, `accepted_player_id`, `added`, `agreed`, `reason`) VALUES('{l_FindPlayer.Id}', '{dbPlayer.Id}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '0', '{reason}');");
        }

        public void AgreeObservation(NSAObservation nSAObservation)
        {
            nSAObservation.Agreed = true;
            MySQLHandler.ExecuteAsync($"UPDATE `nsa_observation_players` SET agreed = '1' WHERE `player_id` = '{nSAObservation.PlayerId}';");
        }

        public void RemoveObservation(DbPlayer dbPlayer, uint l_FindPlayerId)
        {
            if (NSAObservationModule.ObservationList.ContainsKey(l_FindPlayerId)) {
                ObservationList.Remove(l_FindPlayerId);
            }

            // Add to DB for after restart compatiblity ... :)
            MySQLHandler.ExecuteAsync($"DELETE FROM `nsa_observation_players` WHERE `player_id` = '{l_FindPlayerId}';");
        }

        public static void CancelPhoneHearing(int number)
        {
            foreach(DbPlayer xPlayer in TeamModule.Instance.Get((int)teams.TEAM_FIB).Members.Values.Where(p => p.HasData("nsa_activePhone") && p.GetData("nsa_activePhone") == number).ToList()) 
            {
                xPlayer.Player.TriggerEvent("setCallingPlayer", "");
                xPlayer.ResetData("nsa_activePhone");
                xPlayer.SendNewNotification("Mithören beendet, Anruf wurde beendet!");
            }
            return;
        }
    }
}
