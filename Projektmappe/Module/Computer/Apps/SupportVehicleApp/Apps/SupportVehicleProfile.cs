using GTANetworkAPI;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Commands;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using static GVRP.Module.Computer.Apps.SupportVehicleApp.SupportVehicleFunctions;

namespace GVRP.Module.Computer.Apps.SupportVehicleApp.Apps
{
    class SupportVehicleProfile : SimpleApp
    {
        public SupportVehicleProfile() : base("SupportVehicleProfile") { }

        [RemoteEvent]
        public async void requestVehicleData(Client client, int id)
        {
            
                DbPlayer iPlayer = client.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;

                VehicleCategory category = VehicleCategory.ID;
                var vehicleData = SupportVehicleFunctions.GetVehicleData(category, id);

                var vehicleDataJson = NAPI.Util.ToJson(vehicleData);
                TriggerEvent(client, "responseVehicleData", vehicleDataJson);
            
        }

        [RemoteEvent]
        public async void SupportSetGarage(Client client, uint vehicleId)
        {
            
                DbPlayer iPlayer = client.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;
                if (!iPlayer.CanAccessMethod()) return;

                SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                if (Vehicle == null) return;

                if (Vehicle.IsPlayerVehicle())
                {
                    Vehicle.SetPrivateCarGarage(1);
                    iPlayer.SendNewNotification("Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
                else
                {
                    iPlayer.SendNewNotification("Fahrzeug ist kein privat Fahrzeug!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            
        }

        [RemoteEvent]
        public async void SupportGoToVehicle(Client client, uint vehicleId)
        {
            
                DbPlayer iPlayer = client.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;
                if (!iPlayer.CanAccessMethod()) return;

                SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                if (Vehicle == null) return;

                Vector3 pos = Vehicle.entity.Position;

                if (client.IsInVehicle)
                {
                    client.Vehicle.Position = pos;
                }
                else
                {
                    client.SetPosition(pos);
                }

                iPlayer.SendNewNotification($"Sie haben sich zu Fahrzeug {Vehicle.databaseId} teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            
        }
    }
}
