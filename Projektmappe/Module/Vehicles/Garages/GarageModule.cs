using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.GTAN;
using GVRP.Module.Helper;
using GVRP.Module.Logging;
using GVRP.Module.Vehicles.Data;
using static GVRP.Main;
using GVRP.Module.Players.Db;
using GVRP.Module.Players;
using GVRP.Module.Spawners;

namespace GVRP.Module.Vehicles.Garages
{
    public sealed class GarageModule : SqlModule<GarageModule, Garage, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] {typeof(VehicleClassificationModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `garages` WHERE (npc_pos_x != 0 AND npc_pos_y != 0) OR house_id > 0";
        }

        protected override bool OnLoad()
        {
            if (GetAll() != null)
            {
                foreach (var garage in GetAll().Values)
                {
                    if (garage.Blip != null)
                    {
                        garage.Blip.Delete();
                    }
                }
            }
            return base.OnLoad();
        }

        public override void OnFiveMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                foreach (var sxVehicle in VehicleHandler.Instance.GetAllVehicles().ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid()) return;

                    if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.LastInteracted.AddMinutes(15) > DateTime.Now) continue;
                    if (sxVehicle.Occupants.Count > 0) continue;

                    // Wird quasi für Team & Player Vehicles gemacht um Fahrzeug an der Ausparkgarage autom einzuparken
                    if (sxVehicle.LastGarage > 0)
                    {
                        // Finde letzte Garage
                        Garage garage = Get(sxVehicle.LastGarage);
                        if (garage == null)
                            continue;

                        if (garage.DisableAutomaticCarInsertion) continue;

                        if (garage.Classifications.Contains(sxVehicle.Data.ClassificationId))
                        {
                            if (sxVehicle.entity.Position.DistanceTo(garage.Position) < 100.0f)
                            {
                                //einparken
                                if (sxVehicle.IsTeamVehicle())
                                    sxVehicle.SetTeamCarGarage(true);
                                else
                                    sxVehicle.SetPrivateCarGarage(1, garage.Id);

                                continue;
                            }
                        }
                    }

                    // Player Vehicle
                    if (sxVehicle.IsPlayerVehicle())
                    {
                        // Öffentl garagen
                        foreach (var garage in GetAll().Values.Where(garage => garage.HouseId == 0 && garage.Teams.Count() == 0))
                        {
                            if (garage.DisableAutomaticCarInsertion) continue;
                            
                            if (garage.Classifications.Contains(sxVehicle.Data.ClassificationId))
                            {
                                if (sxVehicle.entity.Position.DistanceTo(garage.Position) < 50.0f)
                                {
                                    //einparken
                                    sxVehicle.SetPrivateCarGarage(1, garage.Id);
                                    break;
                                }
                            }
                        }
                    }
                }
            }));
        }


        protected override void OnItemLoaded(Garage garage)
        {
            if (garage.Marker)
            {
                garage.Blip = Blips.Create(garage.Position, garage.Name, 357, 1.0f, true, 0, 255);
                Main.ServerBlips.Add(garage.Blip);
            }
        }

        public Garage GetHouseGarage(uint houseId)
        {
            return (from kvp in GetAll() where kvp.Value.HouseId == houseId select kvp.Value)
                .FirstOrDefault();
        }
    }
}