using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Gamescom
{
    public class GamescomModule : SqlModule<GamescomModule, GamescomCode, uint>
    {
        public Dictionary<string, GamescomCode> Codes = new Dictionary<string, GamescomCode>();


        protected override string GetQuery()
        {
            return "SELECT * FROM `gamescom_codes`;";
        }

        protected override void OnItemLoaded(GamescomCode code)
        {
            Codes.Add(code.Code, code);
        }


        public int GenerateRandomReward(DbPlayer dbPlayer, GamescomCode code)
        {
            int randomNumber = new Random().Next(1, 101);

            switch (randomNumber)
            {
                case int number when number <= 40:
                    // 15 * Level
                    code.RewardId = 1;
                    code.Text = "15.000 $ * dein aktuelles Level";
                    dbPlayer.GiveBankMoney(15000 * dbPlayer.Level);
                    break;
                case int number when number <= 60:
                    code.RewardId = 2;
                    code.Text = "25 % KFZ Gutschein";
                    dbPlayer.Container.AddItem(504);
                    break;
                case int number when number <= 70:
                    code.RewardId = 3;
                    code.Text = "50 % KFZ Gutschein";
                    dbPlayer.Container.AddItem(505);
                    break;
                case int number when number <= 80:
                    code.RewardId = 4;
                    code.Text = "1.000.000 $";
                    dbPlayer.GiveBankMoney(1000000);
                    break;
                case int number when number <= 90:
                    code.RewardId = 5;
                    code.Text = "1 x Fahrzeug tunen kostenlos";
                    dbPlayer.Container.AddItem(678);
                    break;
                case int number when number <= 92:
                    code.RewardId = 6;
                    code.Text = "Ein Fahrzeug : Zion 3";
                    dbPlayer.Container.AddItem(679);
                    break;
                case int number when number <= 94:
                    code.RewardId = 7;
                    code.Text = "Ein Fahrzeug : Zorrusso";
                    dbPlayer.Container.AddItem(680);
                    break;
                case int number when number <= 96:
                    code.RewardId = 8;
                    code.Text = "Ein Fahrzeug : S80";
                    dbPlayer.Container.AddItem(681);
                    break;
                case int number when number <= 98:
                    code.RewardId = 9;
                    code.Text = "Ein Fahrzeug : Tezeract";
                    dbPlayer.Container.AddItem(682);
                    break;
                case int number when number <= 100:
                    code.RewardId = 10;
                    code.Text = "Ein Fahrzeug : Thrax";
                    dbPlayer.Container.AddItem(683);
                    break;


            }
            dbPlayer.SendNewNotification($"Du hast den Code erfolgreich eingegeben und deine Überraschung erhalten : {code.Text}");


            return code.RewardId;
        }    



    }
}
