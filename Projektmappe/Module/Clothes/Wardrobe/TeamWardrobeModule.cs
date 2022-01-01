using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Spawners;

namespace GVRP.Module.Clothes.Wardrobe
{
    public class TeamWardrobeModule : Module<TeamWardrobeModule>
    {
        private List<TeamWardrobe> teamWardrobes;

        protected override bool OnLoad()
        {
            teamWardrobes = new List<TeamWardrobe>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `team_wardrobes`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return false;
                    while (reader.Read())
                    {
                        var teamWardrobe = new TeamWardrobe
                        {
                            Id = reader.GetInt32("id"),
                            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")),
                            Range = reader.GetFloat("range")
                        };
                        string teamString = reader.GetString("team_id");

                        teamWardrobe.Teams = new HashSet<int>();
                        if (!string.IsNullOrEmpty(teamString))
                        {
                            var splittedTeams = teamString.Split(',');
                            foreach (var teamIdString in splittedTeams)
                            {
                                if (!int.TryParse(teamIdString, out var teamId)) continue;
                                teamWardrobe.Teams.Add(teamId);
                            }
                        }
                        teamWardrobe.Colshape = ColShapes.Create(teamWardrobe.Position, teamWardrobe.Range);
                        teamWardrobe.Colshape.SetData("teamWardrobe", teamWardrobe.Teams);
                        teamWardrobes.Add(teamWardrobe);
                    }
                }
                conn.Close();
            }

            return true;
        }

        public List<TeamWardrobe> GetAll()
        {
            return teamWardrobes;
        }
    }
}