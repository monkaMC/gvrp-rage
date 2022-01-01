using System;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Farming;
using GVRP.Module.GTAN;
using GVRP.Module.Injury;
using GVRP.Module.Players.Db;
using GVRP.Module.VehicleRent;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Players
{
    public static class PlayerControl
    {
        public static void ChangeSeat(this DbPlayer dbPlayer, int newSeat)
        {
            if (!dbPlayer.Player.IsInVehicle) return;

            GTANetworkAPI.Vehicle vehicle = dbPlayer.Player.Vehicle;
            if (vehicle == null || !vehicle.IsSeatFree(newSeat)) return;
            // Carzy Workaroung
            dbPlayer.Player.WarpOutOfVehicle();
            dbPlayer.Player.SetIntoVehicle(vehicle, newSeat);
        }

        public static bool CanControl(this DbPlayer iPlayer, SxVehicle sxVehicle)
        {
            if (sxVehicle == null || !sxVehicle.IsValid()) return false;
            if (iPlayer == null || !iPlayer.IsValid()) return false;

            try
            {
                if (sxVehicle.InTuningProcess == true && iPlayer.TeamId == (int)teams.TEAM_LSC)
                {
                    return true;
                }
                if (sxVehicle.InTuningProcess == true && iPlayer.TeamId != (int)teams.TEAM_LSC)
                {
                    return false;
                }

                if (sxVehicle.IsTeamVehicle())
                {
                    if (sxVehicle.teamid == (int)teams.TEAM_GOV && iPlayer.TeamId == (int)teams.TEAM_FIB)
                    {
                        return true;
                    }

                    if (sxVehicle.Team.IsCops() && iPlayer.TeamId == (int)teams.TEAM_FIB && iPlayer.IsNSA)
                    {
                        return true;
                    }

                    if (sxVehicle.Team.IsCops() && iPlayer.TeamId == (int)teams.TEAM_SWAT)
                    {
                        return true;
                    }

                    // Ab Rang 3 kann Sheriff -> LSPD und LSPD -> Sheriff aufschließen
                    if (((sxVehicle.teamid == (int)teams.TEAM_POLICE && iPlayer.TeamId == (int)teams.TEAM_COUNTYPD) ||
                        (sxVehicle.teamid == (int)teams.TEAM_COUNTYPD && iPlayer.TeamId == (int)teams.TEAM_POLICE)) && iPlayer.TeamRank >= 3 && iPlayer.IsInDuty())
                    {
                        return true;
                    }

                    if (sxVehicle.teamid == (int)teams.TEAM_ARMY)
                    {
                        return iPlayer.TeamId == sxVehicle.teamid && (iPlayer.TeamRank >= 1 || sxVehicle.Data.Id == 185); // Rang 0 only freecrawler
                    }

                    return iPlayer.TeamId == sxVehicle.teamid && (!iPlayer.Team.IsStaatsfraktion() || iPlayer.Duty);
                }

                if (sxVehicle.ownerId == iPlayer.Id && VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == sxVehicle.databaseId).Count() <= 0)
                {
                    return true;
                }

                if (iPlayer.VehicleKeys != null && iPlayer.VehicleKeys.Count > 0 && iPlayer.VehicleKeys.ContainsKey(sxVehicle.databaseId))
                {
                    return true;
                }

                // Business Keys
                if (iPlayer.IsMemberOfBusiness() && iPlayer.ActiveBusiness != null && iPlayer.ActiveBusiness.VehicleKeys != null &&
                    iPlayer.ActiveBusiness.VehicleKeys.Count > 0 && iPlayer.ActiveBusiness.VehicleKeys.ContainsKey(sxVehicle.databaseId))
                {
                    return true;
                }

                // Vehicle Rent
                if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.VehicleId == sxVehicle.databaseId && k.PlayerId == iPlayer.Id).Count() > 0)
                {
                    return true;
                }
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }

        public static bool IsOwner(this DbPlayer iPlayer, SxVehicle sxVehicle)
        {
            if (sxVehicle.ownerId != iPlayer.Id) return false;
            return sxVehicle.teamid == 0 && sxVehicle.jobid == 0;
        }

        public static void SetCannotInteract(this DbPlayer iPlayer, bool status)
        {
            if(status) iPlayer.SetData("userCannotInterrupt", true);
            else iPlayer.ResetData("userCannotInterrupt");
        }
        public static bool CanInteract(this DbPlayer iPlayer, bool ignoreFarming = false)
        {
            if (!iPlayer.IsValid()) return false;
            if (iPlayer.IsInAnimation()) return false;
            if (iPlayer.IsCuffed || iPlayer.IsTied) return false;
            if (iPlayer.HasData("follow")) return false;
            if (iPlayer.HasData("userCannotInterrupt") && iPlayer.GetData("userCannotInterrupt")) return false;
            if (FarmingModule.FarmingList.Contains(iPlayer) && !ignoreFarming) return false;
            //Todo: can be removed soon
            if (iPlayer.HasData("disableAnim")) return false;
            return true;
        }
    }
}