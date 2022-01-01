using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.FahrzeugUebersichtApp;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.FahrzeuguebersichtApp.Apps
{
    public class FahrzeugUebersichtApp : SimpleApp
    {
        public FahrzeugUebersichtApp() : base("FahrzeugUebersichtApp") { }


        public enum OverviewCategory
        {
            OWN=0,
            KEY=1,
            BUSINESS=2,
            RENT = 3
        }



        [RemoteEvent]
        public void requestVehicleOverviewByCategory(Client client, int id)
        {
            DbPlayer p_DbPlayer = client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            OverviewCategory l_Category = (OverviewCategory)id;
            var l_Overview = FahrzeugUebersichtFunctions.GetOverviewVehiclesForPlayerByCategory(p_DbPlayer, l_Category);
            TriggerEvent(client, "responseVehicleOverview", NAPI.Util.ToJson(l_Overview));
        }
    }
}
