using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.VehicleImpoundApp.Apps
{
    public class VehicleImpoundApp : SimpleApp
    {
        public VehicleImpoundApp() : base ("VehicleImpoundApp") { }


        [RemoteEvent]
        public void requestVehicleConfiscationById(Client client, uint vehicleId)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var overview = VehicleImpoundFunctions.GetVehicleImpoundOverviews(dbPlayer, vehicleId);
            TriggerEvent(client, "responseVehicleImpound", NAPI.Util.ToJson(overview));
        }

        [RemoteEvent]
        public void requestVehicleImpoundMember(Client player, string member)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (member == null) return;

            var overview = VehicleImpoundFunctions.GetVehicleImpoundOverviewsByMember(dbPlayer, member);
            TriggerEvent(player, "responseVehicleImpound", NAPI.Util.ToJson(overview));
        }
    }
}