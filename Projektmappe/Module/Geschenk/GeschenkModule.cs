using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Geschenk
{
    public class GeschenkModule : SqlModule<GeschenkModule, Geschenk, uint>
    {
        public List<uint> BeschenktePlayer = new List<uint>();
        public static int gameDesignRank = 13;
        public static int supporterRank = 1;
        public static int moderatorRank = 2;
        public static int adminRank = 3;

        protected override string GetQuery()
        {
            return "SELECT * FROM `log_teamgeschenke`;";
        }

        protected override void OnItemLoaded(Geschenk geschenk)
        {
            BeschenktePlayer.Add(geschenk.PlayerId);
        }

        public bool GenerateRandomReward(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;
            int money = 0;
            int carId = 0;
            int itemId = 0;
            int value = 0;
            string reward = "";

            if (!(dbPlayer.Rank.Id == supporterRank || dbPlayer.Rank.Id == moderatorRank || dbPlayer.Rank.Id == adminRank || dbPlayer.Rank.Id == gameDesignRank))
            {
                reward = "Was bist du denn für ein Vogel!?";
                dbPlayer.SendNewNotification(reward);
                MySQLHandler.ExecuteAsync($"INSERT INTO log_teamgeschenke SET `player_id` = '{dbPlayer.Id}',`player_rank` = '{dbPlayer.Rank.Id}',`message` = '{reward}'");
                return false;
            }

            if (GeschenkModule.Instance.BeschenktePlayer.Contains(dbPlayer.Id))
            {
                reward = "Du hast bereits ein Geschenk erhalten!";
                dbPlayer.SendNewNotification(reward);
                MySQLHandler.ExecuteAsync($"INSERT INTO log_teamgeschenke SET `player_id` = '{dbPlayer.Id}',`player_rank` = '{dbPlayer.Rank.Id}',`message` = '{reward}'");
                return false;
            }

            int randomNumber = Utils.RandomNumber(1, 100);

            switch (randomNumber)
            {
                case int number when number <= 2:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast einen FMJ erhalten (Pillbox Garage)!";
                        carId = 575;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast einen Bullet erhalten (Pillbox Garage)!";
                        carId = 571;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast einen Thrax erhalten (Pillbox Garage)!";
                        carId = 989;
                    }
                    break;
                case int number when number <= 4:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast eine Banshee2 erhalten (Pillbox Garage)!";
                        carId = 492;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast einen Viseris erhalten (Pillbox Garage)!";
                        carId = 692;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast einen Zorrusso erhalten (Pillbox Garage)!";
                        carId = 991;
                    }
                    break;
                case int number when number <= 29:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast einen Issi7 erhalten (Pillbox Garage)!";
                        carId = 992;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast einen Schwarzer erhalten (Pillbox Garage)!";
                        carId = 530;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast einen Novak erhalten (Pillbox Garage)!";
                        carId = 990;
                    }
                    break;
                case int number when number <= 39:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast 500.000$ erhalten";
                        money = 500000;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast 750.000$ erhalten";
                        money = 750000;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast 1.000.000$ erhalten";
                        money = 1000000;
                    }
                    break;
                case int number when number <= 89:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast 300.000$ erhalten";
                        money = 300000;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast 400.000$ erhalten";
                        money = 400000;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast 500.000$ erhalten";
                        money = 500000;
                    }
                    break;
                case int number when number <= 100:
                    if (dbPlayer.Rank.Id == gameDesignRank || dbPlayer.Rank.Id == supporterRank)
                    {
                        reward = "Du hast einen Tuninggutschein im Wert von 500.000$ erhalten.";
                        itemId = 692;
                        value = 500000;
                    }
                    else if (dbPlayer.Rank.Id == moderatorRank)
                    {
                        reward = "Du hast einen Tuninggutschein im Wert von 1.000.000$ erhalten.";
                        itemId = 693;
                        value = 1000000;
                    }
                    else if (dbPlayer.Rank.Id == adminRank)
                    {
                        reward = "Du hast einen Tuninggutschein im Wert von 1.500.000$ erhalten.";
                        itemId = 694;
                        value = 1500000;
                    }
                    break;
            }
            if (carId != 0)
            {
                VehicleData vehModel = VehicleDataModule.Instance.GetDataById((uint)carId);
                AddVehicleToPlayer(dbPlayer, vehModel);
                value = vehModel.Price;
            }
            else if (money != 0)
            { 
                dbPlayer.GiveMoney(money);
                value = money;
            }
            else if(itemId != 0)
            {
                dbPlayer.Container.AddItem((uint)itemId, 1);
            }
            dbPlayer.Container.RemoveItem(695, 1);
            dbPlayer.SendNewNotification(reward);
            BeschenktePlayer.Add(dbPlayer.Id);
            MySQLHandler.ExecuteAsync($"INSERT INTO log_teamgeschenke SET `player_id` = '{dbPlayer.Id}',`player_rank` = '{dbPlayer.Rank.Id}',`value` = '{value}',`message` = '{reward}'");
            return true;
        }

        private void AddVehicleToPlayer(DbPlayer dbPlayer, VehicleData vehModel)
        {

            string query = $"INSERT INTO `vehicles` (`owner`, `garage_id`, `inGarage`, `plate`, `model`, `vehiclehash`, `note`) VALUES ('{dbPlayer.Id}', '1', '1', 'GESCHENK', '{vehModel.Id}', '{vehModel.Model}', 'Teamgeschenk');";
            
            MySQLHandler.ExecuteAsync(query);
        }
    }
}
