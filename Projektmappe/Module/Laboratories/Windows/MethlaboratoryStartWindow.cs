using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Laboratories.Windows
{
    public class MethlaboratoryStartWindow : Window<Func<DbPlayer, Methlaboratory, bool>>
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

            public ShowEvent(DbPlayer dbPlayer, Methlaboratory methlaboratory) : base(dbPlayer)
            {
                var par = methlaboratory.Parameters[0];
                Temperature = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = methlaboratory.Parameters[1];
                Pressure = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = methlaboratory.Parameters[2];
                Stirring = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                par = methlaboratory.Parameters[3];
                Amount = new Parameter
                {
                    current = par.ActValue,
                    min = par.MinValue,
                    max = par.MaxValue,
                    step = 1.0f
                };

                Status = methlaboratory.ProzessingPlayers.Contains(dbPlayer);
            }
        }

        public MethlaboratoryStartWindow() : base("MethLabor")
        {

        }

        public override Func<DbPlayer, Methlaboratory, bool> Show()
        {
            return (player, methlaboratory) => OnShow(new ShowEvent(player, methlaboratory));
        }



        [RemoteEvent]
        public void saveMethLabor(Client client, float temperature, float pressure, float stirring, float amount)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsAGangster())
            {
                return;
            }
            
            Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null)  return;

            if (methlaboratory.LastQualityChanged.AddMinutes(5) > DateTime.Now && !Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification("Die Einstellungen können nur alle 5 Minuten geändert werden!");
                return;
            }


            methlaboratory.Parameters[0].ActValue = temperature;
            methlaboratory.Parameters[1].ActValue = pressure;
            methlaboratory.Parameters[2].ActValue = stirring;
            methlaboratory.Parameters[3].ActValue = amount;
            methlaboratory.LastQualityChanged = DateTime.Now;


            methlaboratory.CalculateNewQuality();

            string query = $"UPDATE `team_methlaboratories` SET temperatur = '{temperature}', druck =  '{pressure}', ruehrgeschwindigkeit = '{stirring}', menge = '{amount}' WHERE `teamid` = '{methlaboratory.TeamId}';";
            MySQLHandler.ExecuteAsync(query);
        }

        [RemoteEvent]
        public void toggleMethLabor(Client client, bool result)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null)
            {
                return;
            }

            if (result)
            {
                methlaboratory.StartProcess(dbPlayer);
            }
            else
            {
                methlaboratory.StopProcess(dbPlayer);
            }
        }
    }
}