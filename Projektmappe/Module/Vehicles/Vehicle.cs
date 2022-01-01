using System;
using GVRP.Handler;
using GVRP.Module.Items;

namespace GVRP.Module.Vehicles
{
    /*
     * Derzeit kommt es vor, dass SxVehicle obwohl es nur aufgerufen werdeen kann, wenn es tatsächlich existiert trotzdem null ist.
     * Mit den Abfragen in den jeweiligen Funktionen wird der crash vorgebeugt. Fixt aber die genaue Ursache nicht.
     * Beispiel: (14.10.2018)
     * System.NullReferenceException: Object reference not set to an instance of an object.
        at void GVRP.Module.Vehicles.VehicleFunctions.SetTeamCarGarage(SxVehicle sxVeh, bool inGarage) in C:\Users\Jefferson\source\repos\server - Kopie\server\server\Module\Vehicles\Vehicle.cs:line 38
     */
    public static class VehicleFunctions
    {
        public static bool IsPlayerVehicle(this SxVehicle sxVeh)
        {
            if (sxVeh == null) return false;
            return sxVeh.jobid == 0 && sxVeh.ownerId > 0;
        }

        public static bool IsJobVehicle(this SxVehicle sxVeh)
        {
            if (sxVeh == null) return false;
            return sxVeh.jobid > 0;
        }
        
        public static bool IsTeamVehicle(this SxVehicle sxVeh)
        {
            if (sxVeh == null) return false;
            return sxVeh.teamid > 0;
        }

        public static void SetTuningState(this SxVehicle p_SxVeh, bool p_State)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                if (p_SxVeh == null) return;
                p_SxVeh.InTuningProcess = p_State;
                uint l_BoolID = (uint)(p_State == true ? 1 : 0);
                // Resett garage to id 1 in case of automatically warp in from server
                string l_Query = $"UPDATE `vehicles` SET `TuningState`={l_BoolID.ToString()}, `garage_id`='1' WHERE `id`={p_SxVeh.databaseId.ToString()};";
                if (p_SxVeh.IsTeamVehicle())
                {
                    l_Query = $"UPDATE `fvehicles` SET `TuningState`={l_BoolID.ToString()} WHERE `id`={p_SxVeh.databaseId.ToString()};";
                }

                MySQLHandler.ExecuteAsync(l_Query);
            }));
        }
        
        public static void SetPrivateCarGarage(this SxVehicle sxVeh, uint? inGarage = null, uint? garageId = null)
        {
            if (sxVeh == null) return;
            if (!sxVeh.IsPlayerVehicle() || sxVeh.databaseId <= 0) return;
            sxVeh.entity.Position.X = 0;
            sxVeh.entity.Position.Y = 0;
            sxVeh.entity.Position.Z = 0;
            sxVeh.Save(false, inGarage, garageId);
            
            if (sxVeh.entity != null && inGarage >= 1)
            {
                VehicleHandler.Instance.DeleteVehicle(sxVeh, false);
            }
        }

        public static void SetTeamCarGarage(this SxVehicle sxVeh, bool inGarage)
        {
            if (sxVeh == null) return;
            if (sxVeh.teamid <= 0 || sxVeh.databaseId <= 0) return;
            string query = inGarage ? $"UPDATE `fvehicles` SET inGarage = '1', fuel = '{sxVeh.fuel}', pos_x = '0', pos_y = '0', pos_z = '0' WHERE id = '{sxVeh.databaseId}';" : $"UPDATE `fvehicles` SET inGarage = '0', fuel = '{sxVeh.fuel}' WHERE id = '{sxVeh.databaseId}';";
            MySQLHandler.ExecuteAsync(query);
            
            if (sxVeh.entity != null && inGarage)
            {
                VehicleHandler.Instance.DeleteVehicle(sxVeh);
            }
        }

        public static void ChangeVirtualGarageStatus(this SxVehicle sxVehicle, VirtualGarageStatus virtualGarageStatus)
        {
            sxVehicle.GarageStatus = virtualGarageStatus;
            if (sxVehicle == null || !sxVehicle.IsValid()) return;
            if (sxVehicle.databaseId <= 0) return;

            // Update Status To DB
            string query = sxVehicle.IsTeamVehicle() ? $"UPDATE `fvehicles` SET vgarage = '{(int)virtualGarageStatus}' WHERE id = '{sxVehicle.databaseId}';" :
                $"UPDATE `vehicles` SET vgarage = '{(int)virtualGarageStatus}' WHERE id = '{sxVehicle.databaseId}';";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}