using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.FahrzeugUebersichtApp;
using GVRP.Module.LeitstellenPhone;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.KennzeichenUebersichtApp.Apps
{
    public class KennzeichenUebersichtApp : SimpleApp
    {
        public KennzeichenUebersichtApp() : base("KennzeichenUebersichtApp") { }

        public enum SearchType
        {
            PLATE = 0,
            VEHICLEID = 1
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByPlate(Client client, String plate)
        {
            await HandleVehicleOverview(client, plate, SearchType.PLATE);
        }

        [RemoteEvent]
        public async Task requestVehicleOverviewByVehicleId(Client client, int vehicleId)
        {
            await HandleVehicleOverview(client, vehicleId.ToString(), SearchType.VEHICLEID);
            
        }


        private async Task HandleVehicleOverview(Client p_Client, String information, SearchType type)
        {
            DbPlayer p_DbPlayer = p_Client.GetPlayer();
            if (p_DbPlayer == null || !p_DbPlayer.IsValid())
                return;

            if (LeitstellenPhoneModule.Instance.GetByAcceptor(p_DbPlayer) == null)
            {
                p_DbPlayer.SendNewNotification("Sie müssen als Leitstelle angemeldet sein", PlayerNotification.NotificationType.ERROR);
                return;
            }

            var l_Overview = KennzeichenUebersichtFunctions.GetVehicleInfoByPlateOrId(p_DbPlayer, type, information);
            TriggerEvent(p_Client, "responsePlateOverview", NAPI.Util.ToJson(l_Overview));
        }


    }
}
