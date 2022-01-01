using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Vehicles
{
    public class VehicleTaxModule : Module<VehicleTaxModule>
    {

        public void DoVehicleTaxes()
        {
            /*
            // Try to get Owner on server
            DbPlayer dbPlayer = Players.Players.Instance.FindPlayerById(sxVehicle.ownerId);
            if (dbPlayer != null && dbPlayer.IsValid())
            {
                dbPlayer.VehicleTaxSum += sxVehicle.Data.Tax / 12; // Steuern / 60
            }
            
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                dbPlayer.VehicleTaxSum += GetPlayerVehicleTaxesForGarages(dbPlayer) / 24; // hälfte der Steuern wenn in garage
            }*/
        }

    }
}
