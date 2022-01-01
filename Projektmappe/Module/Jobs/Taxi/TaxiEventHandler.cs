using System;
using GTANetworkAPI;
using GVRP.Module.Players;

namespace GVRP.Module.Jobs.Taxi
{
    public class TaxiEventHandler : Script
    {
        [RemoteEvent]
        public void resultTaxometer(Client client, double distance, int price)
        {
            var iPlayer = client.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            iPlayer.SendNewNotification(
                "Taxometer lief fuer " + distance + "km. Gesamtpreis: " + Math.Round(distance * price) +
                "$");
        }
    }
}