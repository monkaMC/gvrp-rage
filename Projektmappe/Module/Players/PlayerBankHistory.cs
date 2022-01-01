using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Module.Banks.BankHistory;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.Players
{
    //Todo: new module like player team rank
    public class PlayerBankHistoryModule : Module<PlayerBankHistoryModule>
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private static long ConvertToTimestamp(DateTime value)
        {
            var elapsedTime = value - Epoch;
            return (long) elapsedTime.TotalSeconds;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {

            dbPlayer.BankHistory = new List<Banks.BankHistory.BankHistory>();

            // Load Player Bank
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `player_bankhistory` WHERE player_id = '{dbPlayer.Id}' ORDER BY date DESC LIMIT 10;";
                using (var reader2 = cmd.ExecuteReader())
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            var bankHistory = new Banks.BankHistory.BankHistory
                            {
                                PlayerId = reader2.GetUInt32(1),
                                Name = reader2.GetString(2),
                                Value = reader2.GetInt32(3),
                                Date = reader2.GetDateTime(4)
                            };

                            dbPlayer.BankHistory.Add(bankHistory);
                        }
                    }
                }
            }

            return;
        }
        
    }

    public static class BankhistoryExtensions
    {

        public static void AddPlayerBankHistory(this DbPlayer iPlayer, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = iPlayer.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            iPlayer.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `player_bankhistory` (`player_id`, `description`, `value`) VALUES ('{iPlayer.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }

        public static void AddPlayerBankHistories(this DbPlayer iPlayer, List<Banks.BankHistory.BankHistory> bankHistories)
        {
            var query = "";
            foreach (var bankHistory in bankHistories)
            {
                var tmpBankHistory = new Banks.BankHistory.BankHistory
                {
                    PlayerId = iPlayer.Id,
                    Name = bankHistory.Name,
                    Value = bankHistory.Value,
                    Date = DateTime.Now
                };

                iPlayer.BankHistory.Insert(0, tmpBankHistory);

                query +=
                    $"INSERT INTO `player_bankhistory` (`player_id`, `description`, `value`) VALUES ('{iPlayer.Id}', '{MySqlHelper.EscapeString(bankHistory.Name)}', '{bankHistory.Value}');";
            }
            MySQLHandler.ExecuteAsync(query);
        }

        public static void AddBankHistory(this Team team, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = team.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            team.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `team_bankhistory` (`team_id`, `description`, `value`) VALUES ('{team.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }
        public static void AddBankHistory(this Business.Business business, int value, string description)
        {
            var bankHistory = new Banks.BankHistory.BankHistory
            {
                PlayerId = business.Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            business.BankHistory.Insert(0, bankHistory);

            var query =
                $"INSERT INTO `business_bankhistory` (`business_id`, `description`, `value`) VALUES ('{business.Id}', '{MySqlHelper.EscapeString(description)}', '{value}')";

            MySQLHandler.ExecuteAsync(query);
        }
    }
}