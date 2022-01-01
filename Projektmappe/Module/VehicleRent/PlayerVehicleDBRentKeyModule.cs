using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.VehicleRent
{
    public class PlayerVehicleDBRentKeyModule : SqlModule<PlayerVehicleDBRentKeyModule, PlayerVehicleDBRentKey, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(VehicleRentModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `player_vehicle_rent`;";
        }

        protected override void OnItemLoaded(PlayerVehicleDBRentKey loadable)
        {
            // Add To global Dictionary on Loading...
            VehicleRentModule.PlayerVehicleRentKeys.Add(new PlayerVehicleRentKey()
            {
                OwnerId = loadable.OwnerId,
                PlayerId = loadable.PlayerId,
                VehicleId = loadable.VehicleId,
                BeginDate = loadable.BeginDate,
                EndingDate = loadable.EndingDate
            });
        }
    }
}
