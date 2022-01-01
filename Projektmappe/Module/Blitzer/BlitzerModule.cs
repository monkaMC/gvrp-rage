using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Crime;
using GVRP.Module.Logging;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;
using GVRP.Module.Voice;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Blitzer
{
    public sealed class BlitzerModule : Module<BlitzerModule>
    {

        public Dictionary<int, Blitzer> BlitzerList;
        public int cInterval = 1;
        public int aufgestellt = 0;

        public override bool Load(bool reload = false)
        {
            BlitzerList = new Dictionary<int, Blitzer>();

            // WP Blitzer
            AddBlitzer(new Vector3(160.71, -1019.77, 28.9997), "Staat", 0, 50);
            //Yakuza
            AddBlitzer(new Vector3(-1968.4, 337.137, 90.2929), "Staat", 0, 100);
            AddBlitzer(new Vector3(-1959.8, 337.432, 90.2539), "Staat", 0, 100);
            //Vespucci
            AddBlitzer(new Vector3(-793.039, -1123.54, 10.7143), "Staat", 0, 100);
            AddBlitzer(new Vector3(-787.561, -1132.44, 10.7017), "Staat", 0, 100);
            //Staatsbank
            AddBlitzer(new Vector3(212.851, 217.575, 105.606), "Staat", 0, 100);
            AddBlitzer(new Vector3(204.817, 220.113, 105.602), "Staat", 0, 100);
            //Harmony
            AddBlitzer(new Vector3(286.226, 2635.61, 44.6714), "Staat", 0, 150);
            //PD
            AddBlitzer(new Vector3(400.054, -967.313, 29.4051), "Staat", 0, 100);
            //KH3
            AddBlitzer(new Vector3(-402.424, -293.611, 34.6449), "Staat", 0, 100);
            AddBlitzer(new Vector3(-406.004, -298.456, 34.5711), "Staat", 0, 100);
            //Tequi-lala
            AddBlitzer(new Vector3(-539.805, 274.556, 83.0248), "Staat", 0, 100);
            AddBlitzer(new Vector3(-535.404, 273.899, 83.0123), "Staat", 0, 100);
            //Westhighway
            AddBlitzer(new Vector3(-1765.67, -550.847, 35.7809), "Staat", 0, 100);
            AddBlitzer(new Vector3(-1762.95, -546.538, 35.8002), "Staat", 0, 100);
            //Liveinvader
            AddBlitzer(new Vector3(-1123.54, -237.24, 37.8912), "Staat", 0, 100);
            AddBlitzer(new Vector3(-1116.87, -249.027, 37.7792), "Staat", 0, 100);

            return true;
        }

        public void AddBlitzer(Vector3 position, string owner, int teamid, int speedlimit)
        {
            float range = 15.0f;
            ColShape shape = Spawners.ColShapes.Create(position, range);
            BlitzerList.Add(cInterval, new Blitzer(cInterval, range, owner, teamid, speedlimit, shape));

            shape.SetData("blitzer", cInterval);
            cInterval++;
            aufgestellt++;

            Logger.Debug($"{cInterval - 1} Blitzer created");
        }

        public void RemoveBlitzer(Blitzer item)
        {
            if (item == null) return;
            NAPI.ColShape.DeleteColShape(item.Shape);
            BlitzerList.Remove(item.Id);
            aufgestellt--;
        }

        public Blitzer GetNearestBlitzer(DbPlayer iPlayer)
        {
            if (iPlayer.HasData("inBlitzerRange"))
            {
                int data = iPlayer.GetData("inBlitzerRange");
                if (BlitzerList.ContainsKey(data))
                {
                    return BlitzerList[data];
                }
            }
            return null;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShapeState == ColShapeState.Exit)
            {
                if(colShape.HasData("inBlitzerRange"))
                {
                    dbPlayer.ResetData("inBlitzerRange");
                }
            }

            if(colShapeState == ColShapeState.Enter)
            {
                if (colShape.HasData("blitzer"))
                {
                    if (dbPlayer.HasData("inBlitzerRange")) return false;
                    dbPlayer.SetData("inBlitzerRange", colShape.GetData("blitzer"));

                    Blitzer xBlitzer = BlitzerModule.Instance.BlitzerList[colShape.GetData("blitzer")];
                    if (xBlitzer == null)
                    {
                        dbPlayer.ResetData("inBlitzerRange");
                        return false;
                    }

                    // in Fahrzeug, kein cop medic oder regierung
                    if (dbPlayer.Player.IsInVehicle)
                    {
                        if (((dbPlayer.IsACop() || dbPlayer.IsGoverment() || dbPlayer.IsAMedic()) && (dbPlayer.IsInDuty() || dbPlayer.IsInGuideDuty())) || dbPlayer.Dimension[0] != 0 || dbPlayer.IsInAdminDuty() || dbPlayer.DimensionType[0] != DimensionType.World)
                        {
                            dbPlayer.ResetData("inBlitzerRange");
                            return false;
                        }

                        // Nur wenn fahrer
                        if (dbPlayer.Player.VehicleSeat == -1)
                        {
                            SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                            if (sxVeh == null)
                            {
                                dbPlayer.ResetData("inBlitzerRange");
                                return false;
                            }

                            int speed = sxVeh.GetSpeed()-20; 
                            if (speed > xBlitzer.SpeedLimit)
                            {
                                int differenz = speed - xBlitzer.SpeedLimit;
                                int wantedReasonId = 1; // Standard-Fall (0 - 20)

                                // 20-50 Überschreitung Strafe
                                if(differenz > 20 && differenz < 50)
                                {
                                    wantedReasonId = 2;
                                }
                                else if (differenz >= 50 && differenz < 100) // 50-100 Überschreitung Strafe
                                {
                                    wantedReasonId = 3;
                                }
                                else if (differenz > 100) // 100+ Überschreitung Strafe
                                {
                                    wantedReasonId = 4;
                                }

                                try
                                {
                                    if (dbPlayer.HasData("BlitzerTimestamp"))
                                    {
                                        DateTime date = (DateTime)dbPlayer.GetData("BlitzerTimestamp");
                                        if (date.AddMinutes(1) >= DateTime.Now)
                                        {
                                            dbPlayer.ResetData("inBlitzerRange");
                                            dbPlayer.ResetData("BlitzerTimestamp");
                                            return false;
                                        }
                                    }

                                    dbPlayer.SetData("BlitzerTimestamp", DateTime.Now);
                                    string wantedstring = $"{sxVeh.Data.Model} ({sxVeh.databaseId}) mit {speed}/{xBlitzer.SpeedLimit} geblitzt - { DateTime.Now.Hour}:{ DateTime.Now.Minute} { DateTime.Now.Day}/{ DateTime.Now.Month}/{ DateTime.Now.Year}";
                                    dbPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get((uint)wantedReasonId), wantedstring);
                                    dbPlayer.SendNewNotification($"Fahrzeug {sxVeh.Data.Model} ({sxVeh.databaseId}) wurde mit {speed}/{xBlitzer.SpeedLimit} km/h geblitzt!", PlayerNotification.NotificationType.ERROR, title: "Blitzer", duration: 10000);
                                }
                                catch(Exception e)
                                {
                                    Logger.Crash(e);
                                    dbPlayer.ResetData("inBlitzerRange");
                                    return false;
                                }
                            }

                            dbPlayer.ResetData("inBlitzerRange");
                            return true;
                        }
                    }
                    else
                    {
                        dbPlayer.ResetData("BlitzerTimestamp");
                        dbPlayer.ResetData("inBlitzerRange");
                    }
                }
            }
            return false;
        }
    }
}