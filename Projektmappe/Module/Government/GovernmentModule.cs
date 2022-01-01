using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Barber.Windows;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.Configurations;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo.Windows;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;
using static GVRP.Module.Chat.Chats;

namespace GVRP.Module.Government
{
    public class DefconLevel
    {
        public int Level { get; set; }
        public int Caller { get; set; }
        
    }

    public sealed class GovernmentModule : Module<GovernmentModule>
    {
        public static DefconLevel Defcon = null;
        //cleanup

        public override bool Load(bool reload = false)
        {
            Defcon = new DefconLevel() { Level = 5, Caller = 0};

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM defcon ORDER BY date DESC LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Defcon = new DefconLevel() { Level = reader.GetInt32("level"), Caller = reader.GetInt32("caller") };
                        }
                    }
                }
                conn.Close();
            }
            
            SetDefcon(Defcon.Level, Defcon.Caller);
            
            return reload;
        }

        public void SetDefcon(int Level, int CallerId, bool save = true)
        {

            if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 14)
            {
                if (Level > 3)
                {
                    Level = 3;
                    save = false;
                }
            }


            Defcon.Caller = CallerId;
            Defcon.Level = Level;
            
            if(save) MySQLHandler.ExecuteAsync($"INSERT INTO defcon (`level`, `caller`) VALUES ('{Level}', '{CallerId}')");

            Chats.SendGlobalMessage($"Regierungsmitteilung: Die aktuelle Defcon Stufe {Level} wurde ausgerufen!", COLOR.LIGHTBLUE, ICON.GOV);
        }
        
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsetdefcon(Client player, string level)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (!Int32.TryParse(level, out int defcons)) return;

            if (defcons <= 0 || defcons > 5) return;

            switch(defcons)
            {
                case 5:
                case 4:
                case 3:
                    if ((iPlayer.TeamId == (int)teams.TEAM_POLICE || iPlayer.TeamId == (int)teams.TEAM_COUNTYPD || iPlayer.TeamId == (int)teams.TEAM_FIB) && iPlayer.TeamRank >= 12 ||
                        (iPlayer.TeamId == (int)teams.TEAM_GOV && iPlayer.TeamRank >= 9) || iPlayer.Rank.Id == 4 || iPlayer.Rank.Id == 6 || iPlayer.Rank.Id == 5)
                    {
                        SetDefcon(defcons, (int)iPlayer.Id);
                        return;
                    }
                    else return;
                case 2:
                    if ((iPlayer.TeamId == (int)teams.TEAM_POLICE || iPlayer.TeamId == (int)teams.TEAM_COUNTYPD || iPlayer.TeamId == (int)teams.TEAM_FIB) && iPlayer.TeamRank >= 12 ||
                        (iPlayer.TeamId == (int)teams.TEAM_GOV && iPlayer.TeamRank >= 11) || iPlayer.Rank.Id == 4 || iPlayer.Rank.Id == 6 || iPlayer.Rank.Id == 5)
                    {
                        SetDefcon(defcons, (int)iPlayer.Id);
                        return;
                    }
                    else return;
                case 1:
                    if (iPlayer.Rank.Id == 4 || iPlayer.Rank.Id == 6 || iPlayer.Rank.Id == 5)
                    {
                        SetDefcon(defcons, (int)iPlayer.Id);
                        return;
                    }
                    else return;
                default:
                    return;
            }
        }

    }
}