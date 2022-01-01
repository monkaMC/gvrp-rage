using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.MetaData
{
    public class TeamMetaData
    {
        public uint TeamId { get; set; }
        public int Respect { get; set; }

        public Container Container { get; set; }

        public int OrderedPackets { get; set; }

        public void SaveRespect()
        {
            MySQLHandler.ExecuteAsync($"UPDATE team_metadata SET respect = '{Respect}' WHERE team_id = '{TeamId}'");
        }

        public void SaveOrderedPackets()
        {
            MySQLHandler.ExecuteAsync($"UPDATE team_metadata SET ordered_packets = '{OrderedPackets}' WHERE team_id = '{TeamId}'");
        }

        public void AddRespect(int respect)
        {
            if (Respect+respect < -10000 || Respect+respect > 10000) // Cap
                return; 
            else
                Respect += respect;
            
            SaveRespect();
        }

        public async void CheckPlayerCopDeath(DbPlayer dbPlayer, DbPlayer iKiller)
        { 
            
                if (!dbPlayer.Team.IsGangsters() || dbPlayer.Team.TeamMetaData == null) return;
                if (iKiller == null || !iKiller.IsValid()) return;

                // Killer is cop && team is not in active rob
                if (iKiller.IsACop() && !dbPlayer.Team.IsInRobbery() && dbPlayer.jailtime[0] == 0)
                {
                    if (!dbPlayer.Team.IsNearSpawn(dbPlayer.Player.Position))
                    {
                        // minus respect
                        dbPlayer.Team.TeamMetaData.AddRespect(-30);
                        dbPlayer.SendNewNotification("Durch dein Gefecht, hat dein Team an Ansehen verloren!");
                    }
                }
            
        }

        public TeamMetaData(uint dbTeamId)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `team_metadata` WHERE `team_id` = '{dbTeamId}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            TeamId = reader.GetUInt32("team_id");
                            Respect = reader.GetInt32("respect");
                            OrderedPackets = reader.GetInt32("ordered_packets");

                            Container = ContainerManager.LoadContainer(TeamId, ContainerTypes.TeamOrder);
                        }
                    }
                }
            }
        }
    }
}
