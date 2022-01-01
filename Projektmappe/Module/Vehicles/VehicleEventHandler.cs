using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Handler;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.Chat;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Node;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.Vehicles
{
    public class VehicleEventHandler : Script
    {
        private const int RepairkitId = 38;

        // Wenn ihr das mal in den Client einbinden wollt, müsst ihr nur [RemoteEvent] anfügen und dieses Event mit der Tür ID anpeilen
  
        [RemoteEvent]
        public void VehicleSirenToggled(Client p_Client, Vehicle p_Vehicle, bool p_State)
        {
            SxVehicle l_Vehicle = p_Vehicle.GetVehicle();
            if (l_Vehicle == null)
                return;

            l_Vehicle.SirensActive = p_State;
        }

        [RemoteEvent]
        public void requestVehicleSyncData(Client p_Client, Vehicle p_RequestedVehicle)
        {
            DbPlayer l_DbPlayer = p_Client.GetPlayer();
            if (l_DbPlayer == null)
                return;

            SxVehicle l_SxVehicle = p_RequestedVehicle.GetVehicle();
            if (l_SxVehicle == null || !l_SxVehicle.IsValid() || l_SxVehicle.databaseId == 0)
                return;

            var l_Tuning        = l_SxVehicle.Mods;
            var l_Sirens        = l_SxVehicle.SirensActive;
            var l_SirenSound    = l_SxVehicle.SilentSiren;
            var l_DoorStates    = l_SxVehicle.DoorStates;

            try
            {
                string l_SerializedTuning = JsonConvert.SerializeObject(l_Tuning);
                string l_SerializedDoor = JsonConvert.SerializeObject(l_DoorStates);

                p_Client.TriggerEvent("responseVehicleSyncData", p_RequestedVehicle, JsonConvert.SerializeObject(l_Tuning), l_Sirens, l_SirenSound, JsonConvert.SerializeObject(l_DoorStates));
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_INFORMATION(Client client, Vehicle vehicle)
        {
            var dbPlayer = client.GetPlayer();
            if(dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (dbPlayer.HasData("cooldown222"))
            {
                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;


            }
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var dbVehicle = vehicle.GetVehicle();
            if (dbVehicle == null)
            {
                return;
            }
            if (!dbVehicle.IsValid()) return;

            // Respawnstate
            dbVehicle.respawnInteractionState = true;

            var msg = "";

            //vehicle information

            // number plate
            msg += "Nummernschild: " + dbVehicle.entity.NumberPlate;
            
            // vehicle model name
            if (dbVehicle.Data.modded_car == 1)
                msg += " Modell: " + dbVehicle.Data.mod_car_name;
            else
                msg += " Modell: " + dbVehicle.Data.Model;
            
            // vehicle serial number
            if (dbVehicle.Undercover)
            {
                msg += " Seriennummer: " + dbVehicle.entity.GetData("nsa_veh_id");

                if (dbVehicle.teamid == (uint)teams.TEAM_FIB && dbPlayer.TeamId == (uint)teams.TEAM_FIB && dbPlayer.TeamRank >= 11)
                {
                    dbPlayer.SendNewNotification($"Interne Nummer: {dbVehicle.databaseId.ToString()}");
                }
                else if (dbPlayer.TeamId == dbVehicle.teamid)
                {
                    msg += $" Interne Nummer: {dbVehicle.databaseId.ToString()}";
                }
            }
            else
            {
                msg += " Seriennummer: " + dbVehicle.databaseId;
            }

            if(dbVehicle.CarsellPrice > 0)
            {
                msg += " VB $" + dbVehicle.CarsellPrice;
            }

            dbPlayer.SendNewNotification(msg, PlayerNotification.NotificationType.INFO, "KFZ", 10000);
            if (!dbPlayer.HasData("cooldown222"))
            {
                dbPlayer.SetData("cooldown222", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown222");


            }
        }
                
        //[RemoteEventPermission]
        //[RemoteEvent]
        //public void REQUEST_VEHICLE_FlATBED_LOAD(Client client, Vehicle vehicle)
        //{
        //    return;
            /*var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (!dbPlayer.IsInDuty() || dbPlayer.TeamId != (int) teams.TEAM_DPOS) return;
            var dbVehicle = vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;
            
            var offsetFlatbed = vehicle.GetModel().GetFlatbedVehicleOffset();
            if (offsetFlatbed == null)
                return;
            
            if (offsetFlatbed == null) return;

            // Respawnstate
            dbVehicle.respawnInteractionState = true;
            
            foreach (var dposVehicle in VehicleHandler.Instance.GetAllVehicles())
            {
                if (dposVehicle == null || dposVehicle.entity == null) continue;
                Vector3 offset = new Vector3(0,0,0);
                if (dposVehicle.entity.GetModel() == VehicleHash.Flatbed && offsetFlatbed != null
                                                                         && vehicle.Position.DistanceTo(
                                                                             dposVehicle.entity.Position) <=
                                                                         12.0f)
                {
                    offset = offsetFlatbed;
                }
                else
                {
                    continue;
                }
                
                if (dposVehicle.entity.HasData("loadedVehicle")) continue;
                
                var call = new NodeCallBuilder("attachTo").AddVehicle(dposVehicle.entity).AddInt(0).AddFloat(offset.X).AddFloat(offset.Y).AddFloat(offset.Z).AddFloat(0).AddFloat(0).AddFloat(0).AddBool(true).AddBool(false).AddBool(false).AddBool(false).AddInt(0).AddBool(false).Build();
                vehicle.Call(call);

                dposVehicle.entity.SetData("loadedVehicle", vehicle);
                vehicle.SetData("isLoaded", true);
                return;
            }*/
        //}

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_FlATBED_UNLOAD(Client client)
        {
            return;
            /*var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || dbPlayer.isInjured() || !client.IsInVehicle) return;
            if (!dbPlayer.IsInDuty() || dbPlayer.TeamId != (int) teams.TEAM_DPOS) return;
            if ((VehicleHash)client.Vehicle.Model != VehicleHash.Flatbed &&
                (VehicleHash)client.Vehicle.Model != VehicleHash.Wastelander) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!client.Vehicle.HasData("loadedVehicle")) return;
            Vehicle loadedVehicle = client.Vehicle.GetData("loadedVehicle");

            var call = new NodeCallBuilder("detach").Build();
            loadedVehicle.Call(call);

            client.Vehicle.ResetData("loadedVehicle");
            loadedVehicle.ResetData("isLoaded");*/
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_TOGGLE_ENGINE(Client client)
        {
            try {
            var dbPlayer = client.GetPlayer();
             if(dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;
                if (!dbPlayer.HasData("spawncool22"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown22", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown22");
                    dbPlayer.SetData("spawncool22", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown22"))
            {
                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;


            }
            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;
            var dbVehicle = client.Vehicle.GetVehicle();
                if (dbVehicle == null) return;

                if (!dbVehicle.IsValid()) return;

            // player is not in driver seat
            if (client.VehicleSeat != -1) return;
            if (!dbVehicle.CanInteract) return;
            if (!dbPlayer.CanControl(dbVehicle)) return;
            
            // Respawnstate
            dbVehicle.respawnInteractionState = true;



            if (dbVehicle == null)
            {
                return;
            }
            if (!dbVehicle.IsValid())
            {
                return;
            }

            // EMP
            if (dbVehicle.IsInAntiFlight())
            {
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(false);
                dbVehicle.SyncExtension.SetEngineStatus(false);
                return;
            }
            
            if (dbVehicle.fuel == 0 && dbVehicle.SyncExtension.EngineOn == false)
            {
                dbPlayer.SendNewNotification("Dieses Fahrzeug hat kein Benzin mehr!", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (dbVehicle.WheelClamp > 0)
            {
                dbPlayer.SendNewNotification("Dein Fahrzeug wurde mit einer Parkkralle festgesetzt und rührt sich keinen Meter mehr vom Fleck...", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }


            if (dbVehicle.SyncExtension.EngineOn == false)
            {
                dbPlayer.SendNewNotification("Motor eingeschaltet!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(true);

                if (dbVehicle.entity.HasData("paintCar"))
                {
                    if (dbVehicle.entity.HasData("origin_color1") && dbVehicle.entity.HasData("origin_color2"))
                    {
                        int color1 = dbVehicle.entity.GetData("origin_color1");
                        int color2 = dbVehicle.entity.GetData("origin_color2");
                        dbVehicle.entity.PrimaryColor = color1;
                        dbVehicle.entity.SecondaryColor = color2;
                        dbVehicle.entity.ResetData("color1");
                        dbVehicle.entity.ResetData("color2");
                        dbVehicle.entity.ResetData("p_color1");
                        dbVehicle.entity.ResetData("p_color2");
                    }

                    dbVehicle.entity.ResetData("paintCar");
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Motor ausgeschaltet!", notificationType: PlayerNotification.NotificationType.ERROR);
                client.Vehicle.GetVehicle().SyncExtension.SetEngineStatus(false);
            }
            if (!dbPlayer.HasData("cooldown22"))
            {
                dbPlayer.SetData("cooldown22", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown22");


            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_INDICATORS(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle || client.VehicleSeat != -1) return;
            var dbVehicle = client.Vehicle.GetVehicle();
            if (!dbVehicle.IsValid()) return;

            if (!client.Vehicle.HasSharedData("INDICATOR_0"))
            {
                client.Vehicle.SetSharedData("INDICATOR_0", true);
            }
            else
            {
                client.Vehicle.ResetSharedData("INDICATOR_0");
            }

            if (!client.Vehicle.HasSharedData("INDICATOR_1"))
            {
                client.Vehicle.SetSharedData("INDICATOR_1", true);
            }
            else
            {
                client.Vehicle.ResetSharedData("INDICATOR_1");
            }
        }

        public async void handleVehicleLockInside(Client client)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if(dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!dbPlayer.HasData("spawncool23"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown23", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown23");
                    dbPlayer.SetData("spawncool23", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown23"))
            {

                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;

            }
            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;
            var dbVehicle = client.Vehicle.GetVehicle();
                if (dbVehicle == null) return;





                if (!dbVehicle.IsValid()) return;

            if (!dbVehicle.CanInteract) return;

            if (!dbPlayer.CanControl(dbVehicle)) return;
            if (client.Vehicle.GetVehicle().SyncExtension.Locked)
            {
                // closed to open
                client.Vehicle.GetVehicle().SyncExtension.SetLocked(false);
                dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);

            }
            else
            {
                // open to closed
                client.Vehicle.GetVehicle().SyncExtension.SetLocked(true);
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);

            }
            if (!dbPlayer.HasData("cooldown23"))
            {
                dbPlayer.SetData("cooldown23", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown23");

            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_TOGGLE_LOCK(Client client)
        {
            try {

                handleVehicleLockInside(client);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }



        public async void handleVehicleLockOutside(Client client, Vehicle vehicle)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
                if (!dbPlayer.HasData("spawncool555"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown555", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown555");
                    dbPlayer.SetData("spawncool555", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown555"))
            {


                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
                        
            }
          
            var dbVehicle = vehicle.GetVehicle();
                if (dbVehicle == null) return;

                if (!dbVehicle.IsValid()) return;
            if (dbPlayer.Player.Position.DistanceTo(vehicle.Position) > 20f) return;

            if (!dbVehicle.CanInteract) return;

            // check Users rights to toogle Locked state
            if (!dbPlayer.CanControl(dbVehicle)) return;
            if (vehicle.GetVehicle().SyncExtension.Locked)
            {
                // closed to open
                vehicle.GetVehicle().SyncExtension.SetLocked(false);
                dbPlayer.SendNewNotification("Fahrzeug aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);


            }
            else
            {
                // open to closed
                vehicle.GetVehicle().SyncExtension.SetLocked(true);
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);

            }
            if (!dbPlayer.HasData("cooldown555"))
            {
                dbPlayer.SetData("cooldown555", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown555");

            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_VEHICLE_TOGGLE_LOCK_OUTSIDE(Client client, Vehicle vehicle)
        {
            try { 
            handleVehicleLockOutside(client, vehicle);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        public async void handleVehicleDoorInside(Client client, int door)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!dbPlayer.HasData("spawncool33"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown33", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown33");
                    dbPlayer.SetData("spawncool33", true);

                    return;


                }
                if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;

            if (dbPlayer.HasData("cooldown33"))
            {
    dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;

            }
            var dbVehicle = client.Vehicle.GetVehicle();
                if (dbVehicle == null || !dbVehicle.IsValid())
                {
                    return;
                }
                if (!dbVehicle.IsValid()) return;

            if (!dbVehicle.CanInteract) return;
            // validate player opens a doors with permission
            var userseat = client.VehicleSeat;

            // validate player can open right doors
            if (userseat != -1 && userseat != door)
            {
                return;
            }
            // trunk handling
            if (door == 5)
            {
                // Locked vehicle can only close open doors
                if (dbVehicle.SyncExtension.Locked)
                {
                    dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);

                        if (!dbPlayer.HasData("cooldown33"))
                        {
                            dbPlayer.SetData("cooldown33", true);
                            await Task.Delay(3000);
                            dbPlayer.ResetData("cooldown33");

                        }
                        return;
                }

                if (dbVehicle.isDoorOpen[door])
                {
                    // trunk was opened    
                    dbPlayer.SendNewNotification("Kofferraum zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbVehicle.entity.SetData("Door_KRaum", 0);
                }
                else
                {
                    // trunk was closed
                    dbPlayer.SendNewNotification("Kofferraum aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    dbVehicle.entity.SetData("Door_KRaum", 1);
                }

            }
            var newstate = !dbVehicle.isDoorOpen[door];

            dbVehicle.isDoorOpen[door] = newstate;

            if (!dbPlayer.HasData("cooldown33"))
            {
                dbPlayer.SetData("cooldown33", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown33");

            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_TOGGLE_DOOR(Client client, int door)
        {
           try { 
            handleVehicleDoorInside(client, door);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }

        public async void handleVehicleDoorOutside(Client client, Vehicle vehicle, int door)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!dbPlayer.CanAccessRemoteEvent()) return;
                if (!dbPlayer.HasData("spawncool66"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown66", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown66");
                    dbPlayer.SetData("spawncool66", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown66"))
            {
                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;


            }

            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var dbVehicle = vehicle.GetVehicle();
                if (dbVehicle == null) return;

                if (!dbVehicle.IsValid()) return;
            if (dbPlayer.Player.Position.DistanceTo(vehicle.Position) > 20f) return;

            if (!dbVehicle.CanInteract) return;

            if (dbVehicle.SyncExtension.Locked)
            {
                dbPlayer.SendNewNotification("Fahrzeug zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    if (!dbPlayer.HasData("cooldown66"))
                    {


                        dbPlayer.SetData("cooldown66", true);
                        await Task.Delay(3000);
                        dbPlayer.ResetData("cooldown66");
                    }
                    return;
            }

            // trunk handling
            if (door == 5)
            {
                if (dbVehicle.isDoorOpen[door])
                {
                    // trunk was opened
                    dbVehicle.entity.SetData("Door_KRaum", 0);
                    dbPlayer.SendNewNotification("Kofferraum zugeschlossen!", notificationType: PlayerNotification.NotificationType.ERROR);
                }
                else
                {
                    // trunk was closed
                    dbVehicle.entity.SetData("Door_KRaum", 1);
                    dbPlayer.SendNewNotification("Kofferraum aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }

            }

            // faction vehicle
            if (dbVehicle.teamid > 0)
            {
                if (dbPlayer.TeamId != dbVehicle.teamid)
                {
                    return;
                }
            }

            bool newstate = !dbVehicle.isDoorOpen[door];

            dbVehicle.isDoorOpen[door] = newstate;
            if (!dbPlayer.HasData("cooldown66"))
            {


                dbPlayer.SetData("cooldown66", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown66");
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_TOGGLE_DOOR_OUTSIDE(Client client, Vehicle vehicle, int door)
        {
            try { 
            handleVehicleDoorOutside(client, vehicle, door);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        
        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_REPAIR(Client client, Vehicle vehicle)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;
                if (!dbPlayer.HasData("spawncool777"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown777", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown777");
                    dbPlayer.SetData("spawncool777", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown777"))
            {


                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
            if (!dbPlayer.CanAccessRemoteEvent() || client.IsInVehicle) return;
            var dbVehicle = vehicle.GetVehicle();
                if (dbVehicle == null) return;

                if (!dbVehicle.IsValid()) return;

            uint repairKitItem = RepairkitId;

            // verify player has required item
            if (dbPlayer.Container.GetItemAmount(repairKitItem) < 1)
            {
                return;
            }

            var x = new ItemsModuleEvents();
            await x.useInventoryItem(client, dbPlayer.Container.GetSlotOfSimilairSingleItems(repairKitItem));

            // verfiy player can interact
            if (dbPlayer.isInjured() || dbPlayer.IsCuffed)
            {
                dbPlayer.SendNewNotification(
                    "Sie koennen diese Funktion derzeit nicht benutzen.");
            }
            if (!dbPlayer.HasData("cooldown777"))
            {
                dbPlayer.SetData("cooldown777", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown777");
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_VEHICLE_TOGGLE_SEATBELT(Client client)
        {
            try { 
            var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;
                if (!dbPlayer.HasData("spawncool77"))
                {
                    dbPlayer.SendNewNotification("Du kannst dies in 5 Sekunden benutzen!", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.SetData("cooldown77", true);
                    await Task.Delay(5000);
                    dbPlayer.ResetData("cooldown77");
                    dbPlayer.SetData("spawncool77", true);

                    return;


                }
                if (dbPlayer.HasData("cooldown77"))
            {


                dbPlayer.SendNewNotification("Anti-Spam!", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!dbPlayer.CanAccessRemoteEvent() || !client.IsInVehicle) return;

            if (client.Seatbelt)
            {
                // seatbelt on to off
                client.Seatbelt = false;
                dbPlayer.SendNewNotification("Sitzgurt geöffnet!", title: "", notificationType: PlayerNotification.NotificationType.ERROR);
            }
            else
            {
                // seatbelt off to on
                client.Seatbelt = true;
                dbPlayer.SendNewNotification("Sitzgurt geschlossen!", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            if (!dbPlayer.HasData("cooldown77"))
            {
                dbPlayer.SetData("cooldown77", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldown77");
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        [RemoteEvent]
        public void syncSirens(Client p_Player, Vehicle p_Vehicle)
        {
            if (p_Player == null || p_Vehicle == null)
                return;

            var l_Vehicle = p_Vehicle.GetVehicle();
            if (l_Vehicle == null || !l_Vehicle.IsValid() || !l_Vehicle.Team.IsCops())
                return;
            
            l_Vehicle.SilentSiren = !l_Vehicle.SilentSiren;
            var l_SurroundingPlayers = NAPI.Player.GetPlayersInRadiusOfPlayer(50.0f, p_Player);
            foreach (var l_User in l_SurroundingPlayers)
            {
                if (l_User.Dimension == p_Player.Dimension)
                {
                    l_User.TriggerEvent("syncSirenState", p_Vehicle, l_Vehicle.SilentSiren);
                }
            }
        }
    }
}