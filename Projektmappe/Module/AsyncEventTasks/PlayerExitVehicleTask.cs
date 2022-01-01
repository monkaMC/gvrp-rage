using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Armory;
using GVRP.Module.Banks;
using GVRP.Module.Clothes;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;

namespace GVRP.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void PlayerExitVehicleTask(Client player, Vehicle handle)
        {
            
                var vehicle = NAPI.Entity.GetEntityFromHandle<Vehicle>(handle);
                //Todo: maybe save vehicle and player position here
                DbPlayer iPlayer = player.GetPlayer();

                if (iPlayer == null || !iPlayer.IsValid()) return;

                Modules.Instance.OnPlayerExitVehicle(iPlayer, handle);
                
                if (iPlayer.HasData("paintCar"))
                {
                    if (vehicle.HasData("color1") && vehicle.HasData("color2"))
                    {
                        int color1 = vehicle.GetData("color1");
                        int color2 = vehicle.GetData("color2");
                        vehicle.PrimaryColor = color1;
                        vehicle.SecondaryColor = color2;
                        vehicle.ResetData("color1");
                        vehicle.ResetData("color2");
                        iPlayer.ResetData("p_color1");
                        iPlayer.ResetData("p_color2");
                    }

                    iPlayer.ResetData("paintCar");
                }

                if (vehicle != null)
                {
                    SxVehicle sxVeh = vehicle.GetVehicle();
                    if (sxVeh != null)
                    {
                        // Respawnstate
                        sxVeh.respawnInteractionState = true;
                        sxVeh.DynamicMotorMultiplier = sxVeh.Data.Multiplier;

                        if (iPlayer.HasData("neonCar"))
                        {
                            if (sxVeh.neon != "")
                            {
                                sxVeh.LoadNeon();
                                iPlayer.ResetData("neonCar");
                            }
                        }

                        if (iPlayer.HasData("hornCar") || iPlayer.HasData("perlCar"))
                        {
                            // ResetMods
                            iPlayer.ResetData("hornCar");
                            iPlayer.ResetData("perlCar");
                        }

                        if (iPlayer.HasData("tuneSlot"))
                        {
                            // ResetMods
                            iPlayer.ResetData("tuneIndex");
                            iPlayer.ResetData("tuneSlot");
                            iPlayer.ResetData("tuneVeh");
                        }

                        if (sxVeh.Occupants.ContainsValue(iPlayer))
                        {
                            sxVeh.Occupants.Remove(sxVeh.Occupants.First(x => x.Value == iPlayer).Key);
                        }
                    }
                }
            
        }
    }
}
