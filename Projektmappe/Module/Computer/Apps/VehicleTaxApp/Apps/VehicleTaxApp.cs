using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.VehicleTaxApp.Apps
{
    public class VehicleTaxApp : SimpleApp
    {
        public VehicleTaxApp() : base("VehicleTaxApp") { }

        [RemoteEvent]
        public void requestVehicleTaxByModel(Client client, String searchString)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TakeBankMoney(500))
            {
                var overview = VehicleTaxFunctions.GetVehicleTaxOverviews(dbPlayer, searchString);
                TriggerEvent(client, "responseVehicleTax", NAPI.Util.ToJson(overview));
            }
            else
            {
                dbPlayer.SendNewNotification("Eine KFZ-Informationsabfrage kostet 500$");
            }
        }
    }
}
