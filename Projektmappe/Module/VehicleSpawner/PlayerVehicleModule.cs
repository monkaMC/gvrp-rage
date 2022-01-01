using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Items;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.VehicleSpawner
{
    class PlayerVehicleModule : SqlModule<PlayerVehicleModule, PlayerVehicle, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemsModule), typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `vehicles` WHERE !(`pos_x` = '0')  AND `inGarage` = '0';";
        }

        protected override void OnItemLoaded(PlayerVehicle playerVehicle)
        {
            var data = VehicleDataModule.Instance.GetDataById((uint)playerVehicle.Model);
            if (data == null) return;
            if (data.Disabled) return;
            
            SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(data.Id, playerVehicle.Registered,
                                    playerVehicle.Position, playerVehicle.Rotation,
                                    playerVehicle.ColorPrimary, playerVehicle.ColorSecondary, 0, playerVehicle.GpsTracker, true, true,
                                    0, "", playerVehicle.Id, 0, (uint)playerVehicle.PlayerId, (int)playerVehicle.Fuel,
                                    playerVehicle.Health, playerVehicle.Tuning, playerVehicle.Neon, playerVehicle.Mileage,
                                    ContainerManager.LoadContainer(playerVehicle.Id, ContainerTypes.VEHICLE, data.InventorySize, data.InventoryWeight), playerVehicle.Plate, false, playerVehicle.TuningState, WheelClamp:playerVehicle.WheelClamp, AlarmSystem:playerVehicle.AlarmSystem, lastgarageId: (uint)playerVehicle.GarageId);
            Logging.Logger.Debug($"PLAYER VEHICLE {playerVehicle.Model} {playerVehicle.Plate} loaded");

            xVeh.SetPrivateCarGarage(0);
        }
    }
}
