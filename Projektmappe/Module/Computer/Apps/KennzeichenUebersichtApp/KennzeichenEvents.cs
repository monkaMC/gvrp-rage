using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Computer.Apps.KennzeichenUebersichtApp
{
    public class KennzeichenEvents : Script
    {
        [RemoteEvent]
        public void SetKennzeichen(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnString.Length > 8 || returnString.Length < 1)
            {
                dbPlayer.SendNewNotification("Das Kennzeichen muss zwischen 1 und 8 Zeichen lang sein");
                return;
            }            

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }

            dbPlayer.Container.RemoveItem(12227, 1);
            dbPlayer.Container.AddItem(12256, 1, new Dictionary<string, dynamic>() { { "Plate", returnString.Trim() } });
            dbPlayer.SendNewNotification("Sie haben das Kennzeichen bedruckt.");
        }

        [RemoteEvent]
        public void SetVehicleNote(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnString.Length > 15 || returnString.Length < 1)
            {
                dbPlayer.SendNewNotification("Die Notiz muss zwischen 1 und 10 Zeichen lang sein");
                return;
            }

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }

            if (player.Vehicle == null) return;

            SxVehicle sxVehicle = player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            if (!sxVehicle.IsPlayerVehicle()) return;
            if (sxVehicle.IsTeamVehicle()) return;

            MySQLHandler.ExecuteAsync($"UPDATE vehicles SET note = '{returnString}' WHERE id = {sxVehicle.databaseId}");

            dbPlayer.SendNewNotification("Sie haben die Notiz angebracht.");
        }



    }
}
