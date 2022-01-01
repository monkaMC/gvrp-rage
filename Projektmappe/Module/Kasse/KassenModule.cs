using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;

namespace GVRP.Module.Staatskasse
{
    public sealed class KassenModule : Module<KassenModule>
    {
        private int StaatsKasse { get; set; } = 0;
        private int WeazleNewsKasse { get; set; } = 0;

        public int StaatsKassenPaycheckAmountAll = 0;

        public enum Kasse
        {
            STAATSKASSE = 14,
            WEAZLENEWS = 4,
            BLACKBUDGET = 23,
        }

        private void AddMoney(Kasse kasse, int Amount)
        {
            if (Amount > 2000000000)
                return;

            UpdateFBank(kasse, GetMoney(kasse) + Amount);
        }

        private void RemoveMoney(Kasse kasse, int Amount)
        {
            UpdateFBank(kasse, GetMoney(kasse) + Amount);
        }

        public bool ChangeMoney(Kasse kasse, int Amount)
        {
            if (Amount > 0)
            {
                AddMoney(kasse, Amount);
            }
            else
            {
                if (GetMoney(kasse) + Amount < 0) 
                    return false;

                RemoveMoney(kasse, Amount);
            }
            return true;
        }

        public int GetMoney(Kasse kasse)
        {
            if (kasse == Kasse.STAATSKASSE)
            {
                return StaatsKasse;
            }
            else
            {
                return WeazleNewsKasse;
            }
        }


        public void UpdateFBank(Kasse kasse, int value)
        {
            if (value >= 2000000000)
                value = 2000000000;

            switch (kasse)
            {
                case Kasse.STAATSKASSE:
                    StaatsKasse = value;
                    break;
                case Kasse.WEAZLENEWS:
                    WeazleNewsKasse = value;
                    break;
            }
            return;
        }

        public override void OnMinuteUpdate()
        {
            string query;

            query = $"UPDATE `fbank` SET money = '{StaatsKasse}' WHERE teamid = '{(int)Kasse.STAATSKASSE}';";
            query += $"UPDATE `fbank` SET money = '{WeazleNewsKasse}' WHERE teamid = '{(int)Kasse.WEAZLENEWS}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public override bool Load(bool reload = false)
        {
            StaatsKassenPaycheckAmountAll = 0;
            string query = $"SELECT * FROM `fbank`;";

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
                            int Money = reader.GetInt32(2);

                            switch (reader.GetInt32(1))
                            {
                                case (int)Kasse.WEAZLENEWS://Weale News
                                    WeazleNewsKasse = Money;
                                    break;
                                case (int)Kasse.STAATSKASSE://Staatskasse
                                    StaatsKasse = Money;
                                    break;
                            }
                        }
                    }
                }
            }
            return true;
        }

    }
}
