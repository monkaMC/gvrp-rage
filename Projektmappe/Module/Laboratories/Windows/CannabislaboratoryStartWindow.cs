using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Laboratories.Windows
{
    public class CannabislaboratoryStartWindow : Window<Func<DbPlayer, Cannabislaboratory, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "temperature")]
            private Parameter Temperature { get; }

            [JsonProperty(PropertyName = "pressure")]
            private Parameter Pressure { get; }

            [JsonProperty(PropertyName = "stirring")]
            private Parameter Stirring { get; }

            [JsonProperty(PropertyName = "amount")]
            private Parameter Amount { get; }

            [JsonProperty(PropertyName = "status")]
            private bool Status { get; }

            public class Parameter
            {
                public float min { get; set; }
                public float max { get; set; }
                public float current { get; set; }
                public float step { get; set; }
            }

            public ShowEvent(DbPlayer dbPlayer, Cannabislaboratory cannabislaboratory) : base(dbPlayer)
            {
                var par = cannabislaboratory.Parameters[0];
                Temperature = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = cannabislaboratory.Parameters[1];
                Pressure = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = cannabislaboratory.Parameters[2];
                Stirring = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = cannabislaboratory.Parameters[3];
                Amount = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                Status = cannabislaboratory.ActingPlayers.Contains(dbPlayer);
            }
        }

        public CannabislaboratoryStartWindow() : base("CannabisLabor")
        {

        }

        public override Func<DbPlayer, Cannabislaboratory, bool> Show()
        {
            return (player, methlaboratory) => OnShow(new ShowEvent(player, methlaboratory));
        }



        [RemoteEvent]
        public void saveCannabisLabor(Client client, float temperature, float pressure, float stirring, float amount)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsAGangster())
            {
                return;
            }

            Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (cannabislaboratory == null) return;

            if (cannabislaboratory.LastQualityChanged.AddMinutes(5) > DateTime.Now && !Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification("Die Einstellungen können nur alle 5 Minuten geändert werden!");
                return;
            }


            cannabislaboratory.Parameters[0].ActValue = temperature;
            cannabislaboratory.Parameters[1].ActValue = pressure;
            cannabislaboratory.Parameters[2].ActValue = stirring;
            cannabislaboratory.Parameters[3].ActValue = amount;
            cannabislaboratory.LastQualityChanged = DateTime.Now;


            cannabislaboratory.CalculateNewQuality();

            string query = $"UPDATE `team_cannabislaboratories` SET temperatur = '{temperature}', uvenergy =  '{pressure}', luftfeuchtigkeit = '{stirring}', duenger = '{amount}' WHERE `teamid` = '{cannabislaboratory.TeamId}';";
            MySQLHandler.ExecuteAsync(query);
        }

        [RemoteEvent]
        public void toggleCannabisLabor(Client client, bool result)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (cannabislaboratory == null)
            {
                return;
            }

            if (result)
            {
                cannabislaboratory.StartProcess(dbPlayer);
            }
            else
            {
                cannabislaboratory.StopProcess(dbPlayer);
            }
        }
    }
}