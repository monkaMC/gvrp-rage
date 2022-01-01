using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GTANetworkAPI;
using GVRP.Module;
using GVRP.Module.Configurations;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Helper;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Tuning;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.InteriorVehicles;
using GVRP.Module.Vehicles.RegistrationOffice;
using VehicleData = GVRP.Module.Vehicles.Data.VehicleData;

namespace GVRP.Handler
{
    public sealed class VehicleHandler : Module<VehicleHandler>
    {
        public const int MaxVehicleHealth = 2000;

        public static Dictionary<uint, SxVehicle> SxVehicles;

        public static Dictionary<uint, List<SxVehicle>> TeamVehicles;
        public static Dictionary<uint, List<SxVehicle>> PlayerVehicles;

        private uint unique;

        public override bool Load(bool reload = false)
        {
            SxVehicles = new Dictionary<uint, SxVehicle>();
            TeamVehicles = new Dictionary<uint, List<SxVehicle>>();
            PlayerVehicles = new Dictionary<uint, List<SxVehicle>>();
            return reload;
        }
        
        public void AddContextTeamVehicle(uint teamid, SxVehicle sxVehicle)
        {
            if (!TeamVehicles.ContainsKey(teamid)) TeamVehicles.Add(teamid, new List<SxVehicle>());

            TeamVehicles[teamid].Add(sxVehicle);
        }

        public void AddContextPlayerVehicle(uint playerId, SxVehicle sxVehicle)
        {
            if (!PlayerVehicles.ContainsKey(playerId)) PlayerVehicles.Add(playerId, new List<SxVehicle>());

            PlayerVehicles[playerId].Add(sxVehicle);
        }

        public List<SxVehicle> GetPlayerVehicles(uint playerid)
        {
            return PlayerVehicles.ContainsKey(playerid) ? PlayerVehicles[playerid].ToList() : new List<SxVehicle>();
        }

        public SxVehicle FindPlayerVehicle(uint playerid, uint vehicleDatabaseId)
        {
            return GetPlayerVehicles(playerid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public List<SxVehicle> GetTeamVehicles(uint teamid)
        {
            return TeamVehicles.ContainsKey(teamid) ? TeamVehicles[teamid].Where(v => !v.PlanningVehicle).ToList() : new List<SxVehicle>();
        }

        public List<SxVehicle> GetTeamPlanningVehicles(uint teamid)
        {
            return TeamVehicles.ContainsKey(teamid) ? TeamVehicles[teamid].Where(v => v.PlanningVehicle).ToList() : new List<SxVehicle>();
        }

        public bool PlanningVehicleCheckByModel(uint teamid, string model)
        {
            return GetTeamPlanningVehicles(teamid).Where(v => v.Data.Model == model).Count() >= 0;
        }

        public SxVehicle FindTeamVehicle(uint teamid, uint vehicleDatabaseId)
        {
            return GetTeamVehicles(teamid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public SxVehicle FindTeamPlanningVehicle(uint teamid, uint vehicleDatabaseId)
        {
            return GetTeamPlanningVehicles(teamid).Where(v => v.databaseId == vehicleDatabaseId).FirstOrDefault();
        }

        public void DeletePlayerJobVehicle(DbPlayer iPlayer)
        {
            foreach (var sxVeh in VehicleHandler.Instance.GetJobVehicles())
            {
                if (sxVeh.ownerId == iPlayer.Id)
                {
                    DeleteVehicle(sxVeh, false);
                }
            }
        }

        public SxVehicle GetByVehicleDatabaseId(uint dbId)
        {
            try
            {
                SxVehicle result = GetAllVehicles().FirstOrDefault(veh => veh != null && veh.databaseId == dbId && veh.IsPlayerVehicle());
                result = result != null ? result : GetAllVehicles().FirstOrDefault(veh => veh.databaseId == dbId && veh.IsTeamVehicle());
                return result;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(dbPlayer != null && dbPlayer.IsValid() && dbPlayer.Player.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && dbPlayer.IsValid())
                {
                    RemovePlayerFromOccupants(sxVeh, dbPlayer);
                }
            }
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            SxVehicle sxVeh = vehicle.GetVehicle();
            if(sxVeh != null && dbPlayer.IsValid())
            {
                RemovePlayerFromOccupants(sxVeh, dbPlayer);
            }
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (dbPlayer != null && dbPlayer.IsValid() && dbPlayer.Player.IsInVehicle)
            {
                SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVeh != null && dbPlayer.IsValid())
                {
                    RemovePlayerFromOccupants(sxVeh, dbPlayer);
                }
            }
        }

        public SxVehicle GetByVehicleDatabaseId(uint dbId, uint teamId)
        {
            try
            {
                return GetAllVehicles().FirstOrDefault(veh => veh.databaseId == dbId && veh.teamid == teamId);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetAllVehicles()
        {
            try
            {
                var vehicles = SxVehicles.Values.ToList();

                return vehicles.Where(sx => sx != null && sx.IsValid());
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetJobVehicles()
        {
            try
            {
                return SxVehicles.Values.ToList().Where(sx => sx != null && sx.IsValid() && sx.jobid != 0);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public IEnumerable<SxVehicle> GetClosestJobVehicles(Vector3 positon, float range = 7.0f)
        {
            try
            {
                return SxVehicles.Values.ToList().Where(sx => sx != null && sx.IsValid() && sx.jobid != 0 && sx.entity.Position.DistanceTo(positon) < range);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        
        public bool isAJobVeh(SxVehicle sxVeh)
        {
            if (sxVeh.jobid > 0 && sxVeh.databaseId == 0 && sxVeh.teamid == 0)
            {
                return true;
            }

            return false;
        }

        public bool isJobVeh(SxVehicle sxVeh, int jobid)
        {
            if (sxVeh.jobid == jobid && sxVeh.databaseId == 0 && sxVeh.teamid == 0)
            {
                return true;
            }

            return false;
        }

        public List<SxVehicle> GetClosestVehiclesPlayerCanControl(DbPlayer dbPlayer, float range = 4.0f)
        {
            try
            {
                return GetAllVehicles().Where(sx => sx.IsValid() && sx.entity.Position.DistanceTo(dbPlayer.Player.Position) < range && dbPlayer.CanControl(sx)).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        
        public SxVehicle GetClosestVehicle(Vector3 position, float range = 4.0f, UInt32 dimension = 0)
        {
            var dictionary = new Dictionary<float, SxVehicle>();

            foreach (var vehicle in GetAllVehicles())
            {
                if (vehicle.entity == null || vehicle.entity.Dimension != dimension) continue;

                var _range = vehicle.entity.Position.DistanceTo(position);

                if (_range <= range && !dictionary.ContainsKey(_range))
                {
                    dictionary.Add(_range, vehicle);
                }
            }

            var list = dictionary.Keys.ToList();
            list.Sort();

            return (dictionary.Count() > 0 && dictionary.ContainsKey(list[0])) ? dictionary[list[0]] : null;
        }
        
        public List<SxVehicle> GetClosestVehicles(Vector3 position, float range = 4.0f)
        {
            try
            {
                return GetAllVehicles().Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetClosestTeamVehicles(Vector3 position, float range = 4.0f)
        {
            try
            {
                return GetAllTeamVehicles().Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range).ToList();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetAllTeamVehicles()
        {
            var l_List = new List<SxVehicle>();
            foreach (var l_TeamVehicles in TeamVehicles)
            {
                foreach (var l_Veh in l_TeamVehicles.Value)
                {
                    if (l_List.Contains(l_Veh))
                        continue;

                    l_List.Add(l_Veh);
                }
            }

            return l_List;
        }

        public SxVehicle GetClosestVehicleFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range);
                return sxVehicleList.Count() > 0 ? sxVehicleList.FirstOrDefault() : null;
            }
            catch(Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public List<SxVehicle> GetClosestVehiclesFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range).ToList();
            return sxVehicles;
        }

        public List<SxVehicle> GetClosestPlanningVehiclesFromTeam(Vector3 position, int teamid, float range = 4.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamPlanningVehicles((uint)teamid).Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range).ToList();
            return sxVehicles;
        }

        public List<SxVehicle> GetClosestVehiclesFromTeamWithContainerOpen(Vector3 position, int teamid, float range = 8.0f)
        {
            List<SxVehicle> sxVehicles = new List<SxVehicle>();
            sxVehicles = GetTeamVehicles((uint)teamid).Where(sxVeh => sxVeh.entity.Position.DistanceTo(position) <= range && !sxVeh.SyncExtension.Locked && sxVeh.entity.HasData("Door_KRaum") && sxVeh.entity.GetData("Door_KRaum") == 1).ToList();
            return sxVehicles;
        }

        public SxVehicle GetClosestVehicleFromTeamFilter(Vector3 position, int teamid, float range = 4.0f, int seats = 2)
        {
            try
            {
                IEnumerable<SxVehicle> sxVehicleList = GetClosestVehiclesFromTeam(position, teamid, range);
                if (sxVehicleList.Count() == 0) return null;
                SxVehicle sxVehicle = sxVehicleList.FirstOrDefault();
                var pos = sxVehicle.entity.Position.DistanceTo(position);
                foreach(var sx in sxVehicleList)
                {
                    if (sx.entity.GetNextFreeSeat() == -2) continue;
                    if (sx.Data.Slots < seats) continue;
                    if(pos > sx.entity.Position.DistanceTo(position))
                    {
                        pos = sx.entity.Position.DistanceTo(position);
                        sxVehicle = sx;
                    }
                }
                return sxVehicle;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public void AddPlayerToVehicleOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer, int seat)
        {
            try
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (sxVehicle == null || !sxVehicle.IsValid()) return;

                var occupants = sxVehicle.Occupants;

                if (occupants.ContainsValue(dbPlayer))
                {
                    occupants.Remove(occupants.FirstOrDefault(x => x.Value == dbPlayer).Key);
                }
                if (occupants.ContainsKey(seat))
                {
                    occupants.Remove(seat);
                }

                occupants.TryAdd(seat, dbPlayer);
                sxVehicle.Occupants = occupants;
            }
            catch(Exception e)
            {
                Logger.Crash(e);
            }
        }

        public void RemovePlayerFromOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer)
        {
            try
            {
                if (sxVehicle.Occupants.ContainsValue(dbPlayer))
                {
                    sxVehicle.Occupants.Remove(sxVehicle.Occupants.FirstOrDefault(x => x.Value == dbPlayer).Key);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public bool TrySetPlayerIntoVehicleOccupants(SxVehicle sxVehicle, DbPlayer dbPlayer)
        {
            if (sxVehicle == null || dbPlayer == null || !sxVehicle.IsValid() || !dbPlayer.IsValid()) return false;
            if (sxVehicle.Data.Slots > 1)
            {
                int key = 1;

                // Check ab hinten links ALLE Sitze...
                while (key < sxVehicle.Data.Slots-1)
                {
                    if (!sxVehicle.Occupants.ContainsKey(key))
                    {
                        Main.WarpPlayerIntoVehicle(dbPlayer.Player, sxVehicle.entity, key);
                        return true;
                    }
                    key++;
                }
                
                return false;
            }
            return false;
        }

        //Todo: instead of isIdSet do an task that sleeps 2 sec and spawn the car Locked and invincible and frozen first
        public SxVehicle CreateServerVehicle(uint model, bool registered, Vector3 pos, float rotation, int color1, int color2, uint dimension, bool gpsTracker,
            bool spawnClosed = true, bool engineOff = false, uint teamid = 0, string owner = "", uint databaseId = 0,
            int jobId = 0, uint ownerId = 0, int fuel = 69, int zustand = 1000,
            string tuning = "", string neon = "", float km = 0f, Container container = null, string plate = "", bool disableTuning = false, bool InTuningProcess = false, 
            int WheelClamp = 0, bool AlarmSystem = false, uint lastgarageId = 0, bool planningvehicle = false, int carSellPrice = 0)
        {
            // Cannot spawn duplicatings
            if (VehicleHandler.SxVehicles != null && databaseId != 0 && GetAllVehicles().Where(veh => veh.databaseId == databaseId && veh.teamid == teamid).Count() > 0) return null;

            var xVeh = new SxVehicle();
            var data = VehicleDataModule.Instance.GetDataById(model);
            if (data == null)
            {
                //Prevent crash by using faggio as default, should never happen
                data = VehicleDataModule.Instance.GetDataById(219);
            }

            xVeh.Occupants = new Dictionary<int, DbPlayer>();
            xVeh.LastInteracted = DateTime.Now;

            float motorMultiplier = data.Multiplier;
            xVeh.Data = data;
            xVeh.uniqueServerId = unique++;

            if (data.modded_car == 0)
            {
                if (data.Hash < 0)
                {
                    int.TryParse(data.Hash.ToString(), out int l_IntHash);
                    xVeh.entity = NAPI.Vehicle.CreateVehicle(l_IntHash, pos, rotation, color1, color2);
                }
                else
                {
                    uint.TryParse(data.Hash.ToString(), out uint l_IntHash);
                    xVeh.entity = NAPI.Vehicle.CreateVehicle(l_IntHash, pos, rotation, color1, color2);
                }
            }
            else
            {
                var l_Hash = NAPI.Util.GetHashKey(data.Model);
                xVeh.entity = NAPI.Vehicle.CreateVehicle(l_Hash, pos, rotation, color1, color2);
            }

            xVeh.PlanningVehicle = planningvehicle;
            xVeh.LastGarage = lastgarageId;
            xVeh.spawnRot = rotation;
            xVeh.spawnPos = pos;
            xVeh.teamid = teamid;
            xVeh.jobid = jobId;
            xVeh.zustand = zustand;
            xVeh.databaseId = databaseId;
            xVeh.ownerId = ownerId;
            xVeh.saveQuery = "";
            xVeh.respawnInteractionState = true;
            xVeh.Mods = TuningVehicleExtension.ConvertModsToDictonary(tuning);
            xVeh.neon = neon;
            xVeh.Container = container;
            xVeh.Distance = km;
            xVeh.respawnInterval = 0;
            xVeh.spawnPosInterval = 0;
            xVeh.entity.SetData("vehicle", xVeh);
            xVeh.GpsTracker = gpsTracker;
            xVeh.Undercover = false;
            xVeh.Registered = registered;
            
            xVeh.SpawnTime = DateTime.Now;
            xVeh.RepairState = false;
            xVeh.GarageStatus = VirtualGarageStatus.IN_WORLD;
            xVeh.Visitors = new List<DbPlayer>();

            xVeh.Team = TeamModule.Instance.Get(teamid);

            xVeh.isDoorOpen = new[] { false, false, false, false, false, false, false };
            
            xVeh.entity.NumberPlateStyle = 1;

            xVeh.color1 = color1;
            xVeh.color2 = color2;

            xVeh.entity.Dimension = dimension;
            xVeh.CanInteract = true;

            xVeh.SyncExtension = new VehicleEntitySyncExtension(xVeh.entity, spawnClosed, !engineOff);
            xVeh.SilentSiren = false;
            xVeh.InTuningProcess = InTuningProcess;
            xVeh.WheelClamp = WheelClamp;
            xVeh.AlarmSystem = AlarmSystem;

            xVeh.DynamicMotorMultiplier = Convert.ToInt32(data.Multiplier);

            xVeh.CarsellPrice = carSellPrice;

            xVeh.Attachments = new Dictionary<int, int>();

            if (teamid > 0)
            {                
                if (teamid == (int)teams.TEAM_FIB && color1 == -1 && color2 == -1)
                {
                    xVeh.Undercover = true;
                }
            }

            if (plate == null)
            {
                if (owner != "" && owner.Contains("_"))
                {
                    var crumbs = owner.Split('_');

                    var firstLetter = crumbs[0][0].ToString();

                    var secondLetter = crumbs[1][0].ToString();

                    xVeh.entity.NumberPlate = firstLetter + secondLetter + " " + PlayerNameModule.Instance.Get(ownerId).ForumId;

                    xVeh.plate = plate;
                }
            }
            else
            {
                xVeh.entity.NumberPlate = plate;
                xVeh.plate = plate;
            }                  
            
            if (engineOff)
            {
                xVeh.SyncExtension.SetEngineStatus(false);
            }

            if (spawnClosed && data.Id != InteriorVehiclesModule.AirforceDataId)
            {
                xVeh.SyncExtension.SetLocked(true);
                xVeh.SyncExtension.SetEngineStatus(false);
            }
            else
            {
                xVeh.SyncExtension.SetLocked(false);
            }

            if (fuel <= 0)
            {
                xVeh.SyncExtension.SetEngineStatus(false);
                xVeh.fuel = 0;
            }

            if (fuel > 0)
            {
                if (fuel > data.Fuel) fuel = data.Fuel;
                xVeh.fuel = fuel;
            }

            if (jobId > 0)
            {
                xVeh.fuel = data.Fuel;
            }

            if (data.Hash == (uint)VehicleHash.Mule)
            {
                for (var i = 0; i < 7; i++)
                {
                    xVeh.entity.SetExtra(i, false);
                }

                xVeh.entity.SetExtra(1, true);
            }

            if (zustand > 0 && zustand < MaxVehicleHealth)
            {
                if (zustand < 50) zustand = 50;
                xVeh.SetHealth(zustand);
            }
            else
            {
                xVeh.SetHealth(MaxVehicleHealth);
            }
            
            xVeh.entity.SetData("Door_KRaum", 0);
            // Set Anticheat Data
            xVeh.entity.SetData("serverhash", "1312asdbncaw13JADGWSh1");
            xVeh.entity.SetData("lastSavedPos", xVeh.entity.Position);

            if (xVeh.Undercover)
            {
                var l_Rand = new Random();
                if (xVeh.teamid == (int) teams.TEAM_FIB)
                {
                    var color = l_Rand.Next(0, 150);
                    xVeh.color1 = color;
                    xVeh.color2 = color;
                    xVeh.entity.PrimaryColor = color;
                    xVeh.entity.SecondaryColor = color;
                }
                
                xVeh.Distance = l_Rand.Next(2000, 3000);

                var l_ID = l_Rand.Next(630000, 650000);
                xVeh.entity.SetData("nsa_veh_id", l_ID);

                l_ID = l_Rand.Next(60000, 90000);
                xVeh.entity.NumberPlate = RegistrationOfficeFunctions.GetRandomPlate(true);
            }

            SxVehicles.TryAdd(xVeh.uniqueServerId, xVeh);

            // Add to Contextlists
            if(xVeh.IsTeamVehicle())
            {
                AddContextTeamVehicle(xVeh.teamid, xVeh);
            }
            else if(xVeh.IsPlayerVehicle())
            {
                AddContextPlayerVehicle(xVeh.ownerId, xVeh);
            }

            Modules.Instance.OnVehicleSpawn(xVeh);

            xVeh.entity.SetSharedData("silentMode", false);
            xVeh.SetNeon(neon);
            xVeh.ClearMods();

            foreach (var l_Pair in xVeh.Mods)
            {
                xVeh.SetMod(l_Pair.Key, l_Pair.Value);
            }
            
            return xVeh;
        }

        public SxVehicle GetByUniqueServerId(uint id)
        {
            return SxVehicles.TryGetValue(id, out var vehicle) ? vehicle : null;
        }

        public string GetPlayerVehicleNameByDatabaseId(uint dbId)
        {
            if (dbId == 0) return "";
            foreach (var vehicle in GetAllVehicles())
            {
                if (vehicle == null) continue;

                if (vehicle.databaseId != dbId) continue;
                if (vehicle.jobid == 0)
                    return vehicle.Data.Model;
            }

            return "";
        }

        public void DeleteVehicleByEntity(Vehicle vehicle, bool save = true)
        {
            var sxVeh = vehicle.GetVehicle();

            if (sxVeh == null)
            {
                vehicle.DeleteVehicle();
                return;
            }

            DeleteVehicle(sxVeh, save);
        }

        public void DeleteVehicle(SxVehicle sxVehicle, bool save = true)
        {
            if (sxVehicle == null) return;

            if (save) sxVehicle.Save();
            
            if (sxVehicle.IsTeamVehicle())
            {
                try
                {
                    TeamVehicles[sxVehicle.teamid].Remove(sxVehicle);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            SxVehicles.Remove(sxVehicle.uniqueServerId);
            sxVehicle.entity.DeleteVehicle();
        }

        public bool isFuelCar(SxVehicle sxVehicle)
        {
            var model = sxVehicle.Data.Model.ToLowerInvariant();
            if (model.Equals("tanker") ||model.Equals("Benson")||model.Equals("armytanker") || model.Equals("tanker2") || model.Equals("oiltanker")) return true;
            else return false;
        }
    }
    public enum VirtualGarageStatus
    {
        IN_WORLD = 0,
        IN_VGARAGE = 1,
    }

    public class SxVehicle
    {
        public uint uniqueServerId { get; set; }
        public Vehicle entity { get; set; }
        public Vector3 spawnPos { get; set; }
        public float spawnRot { get; set; }
        public double fuel { get; set; }
        public int color1 { get; set; }
        public int color2 { get; set; }
        public uint teamid { get; set; }
        public int jobid { get; set; }
        public uint databaseId { get; set; }
        public int zustand { get; set; }
        public uint ownerId { get; set; }
        public int respawnInterval { get; set; }
        public int spawnPosInterval { get; set; }
        public string saveQuery { get; set; }
        public string plate { get; set; }
        public VirtualGarageStatus GarageStatus { get; set; }
        public DateTime SpawnTime { get; set; }
        public bool RepairState { get; set; }

        public double Distance { get; set; }

        public Team Team { get; set; }
        
        public Dictionary<int, int> Attachments { get; set; }
        public string LastDriver { get; set; }

        public Container Container { get; set; }

        public Dictionary<int, int> Mods { get; set; }

        public string neon { get; set; }

        public bool[] isDoorOpen { get; set; }

        public bool respawnInteractionState { get; set; }

        public VehicleData Data { get; set; }

        public List<DbPlayer> Visitors { get; set; }

        public VehicleEntitySyncExtension SyncExtension { get; set; }

        public bool CanInteract { get; set; }
        
        public DateTime LastInteracted { get; set; }
        public Dictionary<int, DbPlayer> Occupants { get; set; }

        public bool GpsTracker { get; set; }
        
        public bool Undercover { get; set; }
        
        public bool SilentSiren { get; set; }

        public bool SirensActive { get; set; }

        public bool Registered { get; set; }

        public uint LastGarage { get; set; }
        public bool PlanningVehicle { get; set; }

        public bool InTuningProcess { get; set; }

        //0 -> keine // 1 -> Staat // 2 -> Gang 
        public int WheelClamp { get; set; }
        public bool AlarmSystem { get; set; }

        public float DynamicMotorMultiplier { get; set; }

        public int CarsellPrice { get; set; }

        public string GetName()
        {
            return (Data.modded_car == 1) ? Data.mod_car_name : Data.Model;
        }

        public Dictionary<uint, bool> DoorStates = new Dictionary<uint, bool>()
        {
            { 0, false }, // Vorne links
            { 1, false }, // Vorne rechts
            { 2, false }, // hinten link
            { 3, false }, // hinten rechts
            { 4, false }, // motorhaube
            { 5, false }, // kofferraum
            { 6, false }, // back (??)
            { 7, false }  // back2 (??)
        };
    }
}