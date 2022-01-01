using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using static GVRP.Module.Laboratories.Methlaboratory;

namespace GVRP.Module.Laboratories
{
    public class MethlaboratoryEvents : Script
    {
        [RemoteEvent]
        public void ChangeMethlaboratoryTemperatur(Client player, string returnString)
        {
            ChangeMethlaboratoryParameter(player, returnString, "Temperatur");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryDruck(Client player, string returnString)
        {
            ChangeMethlaboratoryParameter(player, returnString, "Druck");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryRuehrgeschwindigkeit(Client player, string returnString)
        {
            ChangeMethlaboratoryParameter(player, returnString, "Ruehrgeschwindigkeit");
        }
        [RemoteEvent]
        public void ChangeMethlaboratoryMenge(Client player, string returnString)
        {
            ChangeMethlaboratoryParameter(player, returnString, "Menge");
        }

        [RemoteEvent]
        public void updateMethlaboratoryParameter(Client client, string returnString)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

        }

        public void ChangeMethlaboratoryParameter(Client player, string returnString, string parameterName)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!Configurations.Configuration.Instance.MethLabEnabled) return;

            if (!Regex.IsMatch(returnString, @"^[0-9]*$"))
            {
                dbPlayer.SendNewNotification("Nur Zahlen sind erlaubt");
                return;
            }
            if (!float.TryParse(returnString, out float value)) return;

            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return;
            Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return;
            Parameter parameter = methlaboratory.Parameters.Find(delegate (Parameter para) { return para.Name == parameterName; });
            if (parameter.MinValue <= value && value <= parameter.MaxValue)
            {
                parameter.ActValue = value;
                MySQLHandler.ExecuteAsync(
                    $"UPDATE `team_methlaboratories` SET `{parameter.Name.ToLower()}` = '{value}' WHERE `team_methlaboratories`.`teamid` = '{methlaboratory.TeamId}';");
                dbPlayer.SendNewNotification($"{parameter.Name} wurde auf {value} {parameter.Einheit} eingestellt.");
            }
            else
            {
                dbPlayer.SendNewNotification("Das ist kein gültiger Einstellparameter. Minimal- und Maximalwert beachten!");
            }
        }
    }
}
