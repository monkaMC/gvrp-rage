using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Vehicles.InteriorVehicles
{
    public class InteriorVehiclesModule : Module<InteriorVehiclesModule>
    {
        public static uint Dimension = 444;
        public static uint AirforceDataId = 953;
        public static Vector3 AirforceInteriorPos = new Vector3(-1350.06, -2822.82, 14.0365);
        public static float AirforceInteriorHeading = 61.174f;
        public static Vector3 AirforceCockpitPos = new Vector3(-1347.34, -2817.87, 17.4466);
        public static float AirforceCokpitFloat = 333.596f;

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            // Cockpit nach hinten
            if (dbPlayer.Player.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();

                if (sxVeh == null || sxVeh.databaseId == 0) return false;
                if (sxVeh.Data.Id != AirforceDataId)
                    return false;
                if (sxVeh.SyncExtension.Locked) return false;

                if (sxVeh.Data.Id == AirforceDataId)
                {
                    Task.Run(async () =>
                    {
                        dbPlayer.SetData("AirforceEnterPos", dbPlayer.Player.Position);
                        dbPlayer.Player.Dimension = Dimension;
                        dbPlayer.Dimension[0] = Dimension;
                        sxVeh.Visitors.Add(dbPlayer);

                        DbPlayer coPilot = null;
                        // Check if pilot is existing
                        if (dbPlayer.Player.VehicleSeat == -1 && sxVeh.Occupants.ContainsKey(0) && sxVeh.Occupants[0] != null && sxVeh.Occupants[0].IsValid())
                        {
                            coPilot = sxVeh.Occupants[0];
                        }

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.Player.SetPosition(AirforceCockpitPos);
                        dbPlayer.Player.SetRotation(AirforceCokpitFloat);

                        await Task.Delay(1000);
                        dbPlayer.Player.SetPosition(AirforceCockpitPos);
                        dbPlayer.Player.SetRotation(AirforceCokpitFloat);

                        if (coPilot != null && coPilot.IsValid())
                        {
                            if (sxVeh.Occupants.ContainsKey(0)) sxVeh.Occupants.Remove(0);
                            // Setze copilot in pilotensitz
                            coPilot.Player.SetIntoVehicle(sxVeh.entity, -1);
                        }

                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    });
                    return true;
                }
                return false;
            }

            switch (key)
            {
                case Key.E:
                    SxVehicle sxVeh;
                    if (dbPlayer.Player.Dimension == Dimension)
                    {
                        if (dbPlayer.Player.Position.DistanceTo(AirforceInteriorPos) < 1.0f)
                        {
                            sxVeh = VehicleHandler.Instance.GetTeamVehicles((uint)teams.TEAM_GOV).Where(v => v.Data.Id == AirforceDataId).FirstOrDefault();

                            if (sxVeh == null) return false;
                            if (sxVeh.SyncExtension.Locked) return false;
                            if (sxVeh.Visitors.Contains(dbPlayer))
                                sxVeh.Visitors.Remove(dbPlayer);

                            dbPlayer.DimensionType[0] = DimensionType.World;
                            dbPlayer.Dimension[0] = 0;
                            dbPlayer.Player.Dimension = 0;

                            dbPlayer.Player.SetPosition(new Vector3(sxVeh.entity.Position.X + 3.0f, sxVeh.entity.Position.Y,
                                sxVeh.entity.Position.Z - 4.0f));
                            dbPlayer.ResetData("AirforceEnterPos");
                            return true;
                        }
                        // Ins Cockpit
                        else if (dbPlayer.Player.Position.DistanceTo(AirforceCockpitPos) < 1.0f)
                        {
                            sxVeh = VehicleHandler.Instance.GetTeamVehicles((uint)teams.TEAM_GOV).Where(v => v.Data.Id == AirforceDataId).FirstOrDefault();

                            if (sxVeh == null) return false;
                            if (sxVeh.SyncExtension.Locked) return false;

                            if(!sxVeh.Occupants.ContainsKey(-1)) // pilot
                            {
                                if (sxVeh.Visitors.Contains(dbPlayer))
                                    sxVeh.Visitors.Remove(dbPlayer);
                                
                                Task.Run(async () =>
                                {
                                    dbPlayer.DimensionType[0] = DimensionType.World;
                                    dbPlayer.Dimension[0] = 0;
                                    dbPlayer.Player.Dimension = 0;

                                    dbPlayer.Player.SetPosition(new Vector3(sxVeh.entity.Position.X, sxVeh.entity.Position.Y,
                                        sxVeh.entity.Position.Z));
                                    dbPlayer.ResetData("AirforceEnterPos");
                                    dbPlayer.Player.TriggerEvent("freezePlayer", true);

                                    await Task.Delay(1200);

                                    dbPlayer.Player.SetIntoVehicle(sxVeh.entity, -1);
                                    dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                });
                                return true;
                            }
                            else if(!sxVeh.Occupants.ContainsKey(0)) // copilot
                            {
                                Task.Run(async () =>
                                {
                                    dbPlayer.DimensionType[0] = DimensionType.World;
                                    dbPlayer.Dimension[0] = 0;
                                    dbPlayer.Player.Dimension = 0;

                                    dbPlayer.Player.SetPosition(new Vector3(sxVeh.entity.Position.X, sxVeh.entity.Position.Y,
                                        sxVeh.entity.Position.Z));
                                    dbPlayer.ResetData("AirforceEnterPos");
                                    dbPlayer.Player.TriggerEvent("freezePlayer", true);

                                    await Task.Delay(1200);

                                    dbPlayer.Player.SetIntoVehicle(sxVeh.entity, 0);
                                    dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                });
                                return true;
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Das Cockpit ist derzeit besetzt!");
                                return false;
                            }
                        }
                        return false;
                    }

                    sxVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(dbPlayer.Player.Position, (int)teams.TEAM_GOV, 17.0f);
                    

                    if (sxVeh != null && sxVeh.IsValid() && sxVeh.databaseId != 0 && !sxVeh.SyncExtension.Locked && sxVeh.Data.Id == AirforceDataId)
                    {
                        Task.Run(async () =>
                        {
                            dbPlayer.SetData("AirforceEnterPos", dbPlayer.Player.Position);
                            dbPlayer.Player.Dimension = Dimension;
                            dbPlayer.Dimension[0] = Dimension;
                            sxVeh.Visitors.Add(dbPlayer);

                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.Player.SetPosition(AirforceInteriorPos);
                            dbPlayer.Player.SetRotation(AirforceInteriorHeading);

                            await Task.Delay(1000);
                            dbPlayer.Player.SetPosition(AirforceInteriorPos);
                            dbPlayer.Player.SetRotation(AirforceInteriorHeading);

                            await Task.Delay(1500);
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        });
                        return true;
                    }
                    return false;
                case Key.L:
                    if (dbPlayer.Player.Dimension == 444)
                    {
                        if (dbPlayer.Player.Position.DistanceTo(AirforceInteriorPos) > 2.5f) return false;

                        sxVeh = VehicleHandler.Instance.GetTeamVehicles((uint)teams.TEAM_GOV).Where(v => v.Data.Id == AirforceDataId).FirstOrDefault();
                        if (sxVeh == null) return false;

                        // player has no right to operate vehicle
                        if (!dbPlayer.CanControl(sxVeh)) return false;

                        if (sxVeh.SyncExtension.Locked)
                        {
                            // closed to opene
                            sxVeh.SyncExtension.SetLocked(false);
                            dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        }
                        else
                        {
                            // open to closed
                            sxVeh.SyncExtension.SetLocked(true);
                            dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                        }

                        return true;
                    }
                    break;
            }

            return false;
        }
    }
}
