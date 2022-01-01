using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;
using GVRP.Module.Menu;

namespace GVRP.Module.Clothes.Outfits
{
    public class Outfit
    {
        public uint PlayerId { get; set; }
        public string Name { get; set; }
        public Dictionary<int, uint> Clothes { get; set; }
        public Dictionary<int, uint> Props { get; set; }

    }

    public class OutfitsModule : Module<OutfitsModule>
    {
        protected override bool OnLoad()
        {

            MenuManager.Instance.AddBuilder(new OutfitsMenuBuilder());
            MenuManager.Instance.AddBuilder(new OutfitsSubMenuBuilder());
            return base.OnLoad();
        }

        public void AddOutfit(DbPlayer dbPlayer, Outfit Outfit)
        {
            dbPlayer.Outfits.Add(Outfit);
            MySQLHandler.ExecuteAsync($"INSERT INTO player_clothes_outfits (`player_id`, `name`, `clothes`, `props`) VALUES ('{Outfit.PlayerId}', '{Outfit.Name}', '{NAPI.Util.ToJson(Outfit.Clothes)}' , '{NAPI.Util.ToJson(Outfit.Props)}')");
        }

        public void DeleteOutfit(DbPlayer dbPlayer, Outfit Outfit)
        {
            dbPlayer.Outfits.Remove(Outfit);
            MySQLHandler.ExecuteAsync($"DELETE FROM player_clothes_outfits WHERE player_id = '{Outfit.PlayerId}' AND name = '{Outfit.Name}'");
        }


        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.Outfits = new List<Outfit>();
            string query = $"SELECT * FROM `player_clothes_outfits` WHERE `player_id` = '{dbPlayer.Id}';";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = query;
                using (var xreader = cmd.ExecuteReader())
                {
                    if (xreader.HasRows)
                    {
                        while (xreader.Read())
                        {
                            Outfit outfit = new Outfit()
                            {
                                PlayerId = dbPlayer.Id,
                                Name = xreader.GetString("name"),
                                Clothes = NAPI.Util.FromJson<Dictionary<int, uint>>(xreader.GetString("clothes")),
                                Props = NAPI.Util.FromJson<Dictionary<int, uint>>(xreader.GetString("props"))
                            };
                            dbPlayer.Outfits.Add(outfit);
                        }
                    }
                }
                conn.Close();
            }
        }
    }
}
