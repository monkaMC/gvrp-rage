using System;
using GTANetworkAPI;

namespace GVRP.Module.Vehicles.Fuel
{
    public class VehicleFuelEventHandler : Script
    {
        [RemoteEvent]
        public void updateVehicleDistance(Client client, Vehicle vehicle, double distance, double fuelDistance)
        {
            if (client == null || vehicle == null || client.Vehicle == null) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;
            dbVehicle.Distance += distance;
            var consumedFuel = dbVehicle.Data.FuelConsumption * fuelDistance;
            dbVehicle.fuel -= consumedFuel;
            if (dbVehicle.fuel < 0) dbVehicle.fuel = 100;
            if (dbVehicle.fuel == 0) dbVehicle.fuel = 100;

            var newFuel = dbVehicle.fuel.ToString().Replace(",", ".");
            var newDistance = String.Format("{0:0.00}", dbVehicle.Distance).Replace(",", ".");
            var newVehicleHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle) + NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
            var newHealth = newVehicleHealth.ToString().Replace(",", ".");
            var newLockState = dbVehicle.entity.Locked?"true":"false";

            //ToDo: Workaround bis 0.4 alle Insassen haben ggf. eine kleine Differenz in der Anzeige, da occupants nicht die Insassen zurückliefert.
            client.TriggerEvent("updateVehicleData", newFuel, newDistance, newHealth, newLockState, dbVehicle.entity.EngineStatus ? "true":"false");
            client.TriggerEvent("setPlayerVehicleMultiplier", dbVehicle.DynamicMotorMultiplier);

            if (dbVehicle.fuel > 0.0) return;

            dbVehicle.SyncExtension.SetEngineStatus(false);
        }
    }
}