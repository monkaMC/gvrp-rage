using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;

namespace GVRP.Module.Warrants
{
    public class WarrantModule : Module<WarrantModule>
    {
        public List<Warrant> warrants = new List<Warrant>();

        protected override bool OnLoad()
        {
            warrants.Clear();

            var query = $"SELECT * FROM `warrants`";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            warrants.Add(new Warrant(reader));
                        }
                    }
                }
            }

            return base.OnLoad();
        }

        
    }
    public static class WarrantFunctions
    {
        public static void Insert(this Warrant warrant)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO warrants (warrant_player_id, acceptor_player_id, creator_player_id, reason, accepted) VALUES " +
                $"('{warrant.WarrantedPlayerId}', '{warrant.AcceptorPlayerId}', '{warrant.CreatorPlayerId}', '{warrant.Reason}', '{warrant.Accepted}';");
        }

        public static void Update(this Warrant warrant)
        {
            MySQLHandler.ExecuteAsync($"UPDATE warrants SET acceptor_player_id = '{warrant.AcceptorPlayerId}', accepted = '{warrant.Accepted}' WHERE warrant_player_id = '{warrant.WarrantedPlayerId}' AND acceptor_player_id ='{warrant.AcceptorPlayerId}';");
        }
    }
}
