using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Vehicles
{
    public class VehicleEntitySyncExtension
    {
        public bool Locked { get; set; }
        public bool EngineOn { get; set; }
        public bool Freezed { get; set; }

        public Vehicle Entity { get; set; }

        public VehicleEntitySyncExtension(Vehicle entity, bool locked = false, bool engineOn = false, bool freeze = false)
        {
            Entity = entity;
            Locked = locked;
            EngineOn = engineOn;
            Freezed = freeze;

        }

        public void SetLocked(bool status)
        {
            NAPI.Vehicle.SetVehicleLocked(this.Entity, status);
            this.Locked = status;
            this.Entity.GetVehicle().LastInteracted = DateTime.Now;
            Entity.SetSharedData("lockedStatus", status);
        }
        public void SetFreezed(bool status)
        {

        }
        public void SetEngineStatus(bool status)
        {
            NAPI.Vehicle.SetVehicleEngineStatus(this.Entity, status);
            
            this.EngineOn = status;
            Entity.SetSharedData("engineStatus", status);
        }
    }
}
