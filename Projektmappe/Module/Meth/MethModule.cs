using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP.Module.Meth
{
    public sealed class MethModule : Module<MethModule>
    {
        public static Vector3 CamperInteriorPosition = new Vector3(1973.07, 3816.15, 33.4287);

        public static float CamperDrugAirRange = 60.0f;

        public static float DrugLabIncreaseRange = 20.0f;

        public static List<DbPlayer> CookingPlayers = new List<DbPlayer>();

        public static List<SxVehicle> CookingVehicles = new List<SxVehicle>();

        public int ephi = 0;
        public int meth = 0;
        public int cooker = 0;

        public void ResetLogVariables()
        {
            ephi = 0;
            meth = 0;
            cooker = 0;
        }

        public override bool Load(bool reload = false)
        {
            CookingPlayers = new List<DbPlayer>();
            CookingVehicles = new List<SxVehicle>();
            return base.Load(reload);
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer.DimensionType[0] != DimensionType.Camper) return;
            dbPlayer.DimensionType[0] = DimensionType.World;
            var sxVehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);
            dbPlayer.Player.Dimension = 0;
            dbPlayer.Dimension[0] = 0;
            if (sxVehicle == null)
            {
                if (!dbPlayer.HasData("CamperEnterPos")) return;
                Vector3 enterPosition = dbPlayer.GetData("CamperEnterPos");
                dbPlayer.Player.SetPosition(new Vector3(enterPosition.X + 3, enterPosition.Y,
                    enterPosition.Z));
            }
            else
            {
                try
                {
                    if (sxVehicle.Visitors.Contains(dbPlayer)) sxVehicle.Visitors.Remove(dbPlayer);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }

                dbPlayer.Player.SetPosition(new Vector3(sxVehicle.entity.Position.X + 3, sxVehicle.entity.Position.Y,
                    sxVehicle.entity.Position.Z));
            }
        }

        public override void OnMinuteUpdate()
        {
            // Reset Cooking State
            CookingVehicles.Clear();


            Dictionary<Vector3, string> Messages = new Dictionary<Vector3, string>();
            var sendMethVehs = new List<uint>();
            foreach (var iPlayer in CookingPlayers.ToList())
            {
                if (iPlayer == null || !iPlayer.IsValid()) return;

                if (!iPlayer.IsValid())
                    continue;
                var random = new Random();

                //Meth Cooking
                if (iPlayer.HasData("cooking") && iPlayer.DimensionType[0] == DimensionType.Camper)
                {
                    if(iPlayer.Player.Position.DistanceTo(CamperInteriorPosition) > 20.0f)
                    {
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                            iPlayer.GetName() + " Camper glitch (kochend entfernt) wurde gekickt!");
                        //DBLogging.LogAdminAction(iPlayer.Player, iPlayer.Player.Name, adminLogTypes.kick, "Camper kochend entfernt", 0, Configuration.Instance.DevMode);
                        iPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                        iPlayer.Kick("Du musst im Camper bleiben");
                        continue;
                    }

                    if (iPlayer.Container.GetItemAmount(13) >
                        0 && iPlayer.Container.GetItemAmount(15) > 0 && // Kocheq, Batter
                        iPlayer.Container.GetItemAmount(14) >
                        0 && iPlayer.Container.GetItemAmount(16) > 0) // Toilet , Ephe
                    {
                        var explode = random.Next(1, 30);
                        if (explode == 1)
                        {
                            iPlayer.SendNewNotification("1337Sexuakbar$explode");
                            iPlayer.Container.RemoveItem(13, 1);
                            cooker++;

                            iPlayer.SetHealth(iPlayer.Player.Health - 20);

                            iPlayer.ResetData("cooking");
                            if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);

                            var journeyDbId = iPlayer.Player.Dimension;
                            if (!sendMethVehs.Contains(journeyDbId))
                            {
                                sendMethVehs.Add(journeyDbId);
                                var sxveh =
                                    VehicleHandler.Instance.GetByVehicleDatabaseId(journeyDbId);
                                if (sxveh != null && sxveh.entity != null)
                                {
                                    Messages.Add(sxveh.entity.Position, "1337Sexuakbar$explode");
                                }
                            }
                        }
                        else
                        {
                            var meth = random.Next(3, 7); //3 included, 7 excluded
                            
                            iPlayer.Container.RemoveItem(15, 1);
                            iPlayer.Container.RemoveItem(14, 1);
                            iPlayer.Container.RemoveItem(16, 1);

                            iPlayer.SendNewNotification("Sie haben erfolgreich Meth gekocht: " + meth +
                                " Kristalle");
                            iPlayer.IncreasePlayerDrugInfection();

                            if (!iPlayer.Container.CanInventoryItemAdded(1, meth))
                            {
                                iPlayer.SendNewNotification("Dein Inventar ist voll!");
                                iPlayer.ResetData("cooking");
                                if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                            }
                            else
                            {
                                iPlayer.Container.AddItem(1, meth);
                                ephi++;
                                this.meth += meth;

                                var journeyDbId = iPlayer.Player.Dimension;
                                if (!sendMethVehs.Contains(journeyDbId))
                                {
                                    sendMethVehs.Add(journeyDbId);
                                    var sxveh =
                                        VehicleHandler.Instance.GetByVehicleDatabaseId(journeyDbId);
                                    
                                    if (sxveh != null && sxveh.entity != null)
                                    {
                                        if (!CookingVehicles.Contains(sxveh)) CookingVehicles.Add(sxveh);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Da sie keine Materialien mehr haben, ist der Kocher ausgegangen!");
                        iPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                    }
                }
            }

            // Send Messages together....
            foreach (var xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                foreach(KeyValuePair<Vector3, string> kvp in Messages)
                {
                    if (xPlayer.Player.Position.DistanceTo(kvp.Key) < 30)
                    {
                        xPlayer.SendNewNotification(kvp.Value, title: "Chemikalien");
                    }
                }
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;

            switch (key)
            {
                case Key.E:
                    // Check Vehicle in Range (Wohnwagen)
                    SxVehicle sxVeh;

                    if (dbPlayer.DimensionType[0] == DimensionType.Camper && dbPlayer.Player.Dimension != 0)
                    {
                        if (dbPlayer.Player.Position.DistanceTo(CamperInteriorPosition) > 2.5f) return false;

                        sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);
                        if (sxVeh == null) return false;
                        if (sxVeh.SyncExtension.Locked) return false;
                        if (sxVeh.entity.Model != (uint) VehicleHash.Journey &&
                            sxVeh.entity.Model != (uint) VehicleHash.Camper)

                            if (sxVeh.Visitors.Contains(dbPlayer))
                                sxVeh.Visitors.Remove(dbPlayer);
                        dbPlayer.DimensionType[0] = DimensionType.World;
                        dbPlayer.Dimension[0] = 0;
                        dbPlayer.Player.Dimension = 0;

                        // Reset Cooking on Exit
                        if (dbPlayer.HasData("cooking"))
                        {
                            dbPlayer.ResetData("cooking");
                        }
                        if (CookingPlayers.Contains(dbPlayer)) CookingPlayers.Remove(dbPlayer);

                        
                        dbPlayer.Player.SetPosition(new Vector3(sxVeh.entity.Position.X + 3.0f, sxVeh.entity.Position.Y,
                            sxVeh.entity.Position.Z + 0.5f));
                        dbPlayer.ResetData("CamperEnterPos");
                        return true;
                    }

                    sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);

                    if (sxVeh == null || sxVeh.databaseId == 0) return false;
                    if (sxVeh.entity.Model != (uint) VehicleHash.Journey &&
                        sxVeh.entity.Model != (uint) VehicleHash.Camper)
                        return false;
                    if (sxVeh.SyncExtension.Locked) return false;
                    
                    Task.Run(async () =>
                    {
                        dbPlayer.SetData("CamperEnterPos", dbPlayer.Player.Position);
                        dbPlayer.Player.Dimension = sxVeh.databaseId;
                        dbPlayer.DimensionType[0] = DimensionType.Camper;
                        dbPlayer.Dimension[0] = sxVeh.databaseId;
                        sxVeh.Visitors.Add(dbPlayer);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.Player.SetPosition(CamperInteriorPosition);

                        await Task.Delay(1000);
                        dbPlayer.Player.SetPosition(CamperInteriorPosition);

                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    });
                    // Set Player INTO
                    sxVeh.Visitors.Add(dbPlayer);
                    return true;
                case Key.L:
                    if (dbPlayer.DimensionType[0] == DimensionType.Camper)
                    {
                        var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbPlayer.Player.Dimension);

                        if (vehicle == null) return false;

                        // player has no right to operate vehicle
                        if (!dbPlayer.CanControl(vehicle)) return false;

                        if (vehicle.SyncExtension.Locked)
                        {
                            // closed to opene
                            vehicle.SyncExtension.SetLocked(false);
                            dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        }
                        else
                        {
                            // open to closed
                            vehicle.SyncExtension.SetLocked(true);
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