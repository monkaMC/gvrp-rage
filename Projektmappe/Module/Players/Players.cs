using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Banks.BankHistory;
using GVRP.Module.Crime;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Players.Ranks;
using GVRP.Module.Weapons;

namespace GVRP.Module.Players
{
    public sealed class Players
    {
        public static Players Instance { get; } = new Players();
        public readonly List<DbPlayer> players;
        private readonly HashSet<uint> playerIds;
        private const float SyncRange = 125f;

        private Players()
        {
            players = new List<DbPlayer>();
            playerIds = new HashSet<uint>();
        }

        public DbPlayer GetByDbId(uint id)
        {
            var playerDb = GetValidPlayers().FirstOrDefault(player => player.Id == id && player.IsValid());
            return playerDb?.Player.GetPlayer();
        }
        
        public DbPlayer GetByName(string name)
        {
            var playerDb = GetValidPlayers().FirstOrDefault(player => player.GetName() == name && player.IsValid());
            return playerDb?.Player.GetPlayer();
        }

        public List<DbPlayer> GetValidPlayers()
        {
            return players.ToList().Where(p => p != null && p.IsValid()).ToList();
        }

        public List<DbPlayer> GetJailedPlayers()
        {
            return players.ToList().Where(p => p != null && p.IsValid() && p.jailtime[0] > 0).ToList();
        }
        
        public void SendNotificationToAllUsers(string command, int duration = 5000)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (var dbPlayer in players)
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    dbPlayer.SendNewNotification(command, PlayerNotification.NotificationType.STANDARD, "", duration);
                }
            }));
        }

        public DbPlayer GetPlayerByPhoneNumber(uint HandyNummer)
        {
            return players.ToList().Where(p => p != null && p.IsValid() && p.handy[0] == HandyNummer).FirstOrDefault();
        }

        public DbPlayer GetClosestPlayerForPlayer(DbPlayer source, float range = 4.0f)
        {
            try
            {
                return GetPlayersInRange(source.Player.Position, range).Where(pl => pl.Player.Position.DistanceTo(source.Player.Position) <= range && pl.Id != source.Id).FirstOrDefault();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }
        
        public void SendMessageToAuthorizedUsers(string feature, string command, int time = 10000)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (var dbPlayer in GetValidPlayers().Where(p => p.Rank.Id != 0))
                {
                    if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                    if (!dbPlayer.Rank.CanAccessFeature(feature)) continue;
                    if (dbPlayer.Player.HasFeatureIgnored(feature)) continue;
                    if (feature.Equals("teamchat"))
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.TEAM);
                    else if (feature.Equals("support"))
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.SERVER);
                    else if (feature.Equals("highteamchat"))                    
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.HIGH);
                    else
                        dbPlayer.SendNewNotification(command, title: GetTextExtensionForFeature(feature), duration: time, notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            }));
        }

        public void TriggerEventInRange(DbPlayer dbPlayer, string eventName, params object[] args)
        {
            GetPlayersInRange(dbPlayer.Player.Position).TriggerEvent(eventName, args);
        }

        public void SendChatMessageToAuthorizedUsers(string feature, DbPlayer dbPlayer, string message)
        {
            SendMessageToAuthorizedUsers(feature, $"{dbPlayer.Player.Name}: {message}");
        }

        public DbPlayer FindPlayer(object search, bool p_IgnoreFakeName = false)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();

                var searchString = search.ToString();
                if (string.IsNullOrEmpty(searchString)) return null;
                if (int.TryParse(searchString, out var playerId))
                {
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.Id != playerId) continue;
                        return user;
                    }
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.ForumId != playerId) continue;
                        return user;
                    }
                }
                else
                {
                    foreach (var user in playerList)
                    {
                        if (user == null || !user.IsValid() || user.Player == null) continue;

                        var l_Name = user.GetName();
                        if (p_IgnoreFakeName)
                        {
                            if (l_Name.ToLower().Contains(search.ToString().ToLower()))
                                return user;
                            
                            if (user.Player.Name.ToLower().Contains(search.ToString().ToLower()))
                                return user;
                        }
                        else if (l_Name.ToLower().Contains(search.ToString().ToLower())) return user;
                    }
                }
                return null;
            }
            catch(Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public DbPlayer FindByVoiceHash(string search)
        {
            try
            {
                List<DbPlayer> playerList = GetValidPlayers();
                
                if (string.IsNullOrEmpty(search) || !Int32.TryParse(search, out int searchVH)) return null;
                foreach (var user in playerList)
                {
                    if (user == null || !user.IsValid() || user.Player == null) continue;

                    if (Int32.TryParse(user.VoiceHash, out int userVh))
                    {
                        if (userVh == searchVH) return user;
                    }
                }
                return null;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }
        }

        public DbPlayer FindPlayerById(object search, bool valid = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                foreach (var dbPlayerx in GetValidPlayers())
                {
                    if (dbPlayerx.Id != playerId) continue;
                    if (valid && !dbPlayerx.IsValid()) return null;
                    return dbPlayerx;
                }
            }
            
            return null;
        }
        public DbPlayer FindPlayerByForumId(object search, bool valid = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                foreach (var dbPlayerx in GetValidPlayers())
                {
                    if (dbPlayerx.ForumId != playerId) continue;
                    if (valid && !dbPlayerx.IsValid()) return null;
                    return dbPlayerx;
                }
            }

            return null;
        }
        
        public IEnumerable<DbPlayer> GetPlayersInRange(Vector3 position, float range = SyncRange)
        {
            return GetValidPlayers().Where((player) => player.Player.Position.DistanceTo(position) < range);
        }

        private static string GetTextExtensionForFeature(string feature)
        {
            string name;
            switch (feature)
            {
                case "adminchat":
                    name = "AdminChat";
                    break;
                case "teamchat":
                    name = "TeamChat";
                    break;
                default:
                    name = feature;
                    break;
            }

            return $"[{name}]: ";
        }

        public DbPlayer Load(MySqlDataReader reader, Client player)
        {
            try { 
            var iPlayer = new DbPlayer(reader);
            iPlayer.Player = player;
            iPlayer.Id = reader.GetUInt32("id");
    //        Console.WriteLine(reader.GetString("Pass"));
            iPlayer.Password = reader.GetString("Pass");
            iPlayer.Salt = reader.GetString("Salt");

            // Add Verweis
            player.SetData("player", iPlayer);

            // Forumid
            iPlayer.ForumId = reader.GetInt32("forumid");
            iPlayer.tmpPlayerId = GetFreeId();

            iPlayer.AnimationScenario = new AnimationScenario();

            iPlayer.money = new int[2];
            iPlayer.money[1] = reader.GetInt32("Money");
            iPlayer.money[0] = reader.GetInt32("Money");
            iPlayer.bank_money = new int[2];
            iPlayer.bank_money[1] = reader.GetInt32("BankMoney");
            iPlayer.bank_money[0] = reader.GetInt32("BankMoney");
            iPlayer.blackmoney = new int[2];
            iPlayer.blackmoney[1] = reader.GetInt32("blackmoney");
            iPlayer.blackmoney[0] = reader.GetInt32("blackmoney");
            iPlayer.blackmoneybank = new int[2];
            iPlayer.blackmoneybank[1] = reader.GetInt32("blackmoneybank");
            iPlayer.blackmoneybank[0] = reader.GetInt32("blackmoneybank");
            iPlayer.payday = new int[2];
            iPlayer.payday[1] = reader.GetInt32("payday");
            iPlayer.payday[0] = reader.GetInt32("payday");
            iPlayer.rp = new int[2];
            iPlayer.rp[1] = reader.GetInt32("rp");
            iPlayer.rp[0] = reader.GetInt32("rp");
            iPlayer.ownHouse = new uint[2];
            iPlayer.ownHouse[1] = reader.GetUInt32("ownHouse");
            iPlayer.ownHouse[0] = reader.GetUInt32("ownHouse");
            iPlayer.wanteds = new int[2];
            iPlayer.wanteds[1] = reader.GetInt32("wanteds");
            iPlayer.wanteds[0] = reader.GetInt32("wanteds");

            //Licenses
            iPlayer.Lic_Car = new int[2];
            iPlayer.Lic_Car[1] = reader.GetInt32("Lic_Car");
            iPlayer.Lic_Car[0] = reader.GetInt32("Lic_Car");
            iPlayer.Lic_LKW = new int[2];
            iPlayer.Lic_LKW[1] = reader.GetInt32("Lic_LKW");
            iPlayer.Lic_LKW[0] = reader.GetInt32("Lic_LKW");
            iPlayer.Lic_Bike = new int[2];
            iPlayer.Lic_Bike[1] = reader.GetInt32("Lic_Bike");
            iPlayer.Lic_Bike[0] = reader.GetInt32("Lic_Bike");
            iPlayer.Lic_PlaneA = new int[2];
            iPlayer.Lic_PlaneA[1] = reader.GetInt32("Lic_PlaneA");
            iPlayer.Lic_PlaneA[0] = reader.GetInt32("Lic_PlaneA");
            iPlayer.Lic_PlaneB = new int[2];
            iPlayer.Lic_PlaneB[1] = reader.GetInt32("Lic_PlaneB");
            iPlayer.Lic_PlaneB[0] = reader.GetInt32("Lic_PlaneB");
            iPlayer.Lic_Boot = new int[2];
            iPlayer.Lic_Boot[1] = reader.GetInt32("Lic_Boot");
            iPlayer.Lic_Boot[0] = reader.GetInt32("Lic_Boot");
            iPlayer.Lic_Gun = new int[2];
            iPlayer.Lic_Gun[1] = reader.GetInt32("Lic_Gun");
            iPlayer.Lic_Gun[0] = reader.GetInt32("Lic_Gun");
            iPlayer.Lic_Biz = new int[2];
            iPlayer.Lic_Biz[1] = reader.GetInt32("Lic_Biz");
            iPlayer.Lic_Biz[0] = reader.GetInt32("Lic_Biz");
            iPlayer.spawnchange = new int[2];
            iPlayer.spawnchange[1] = reader.GetInt32("spawnchange");
            iPlayer.spawnchange[0] = reader.GetInt32("spawnchange");
            iPlayer.job = new int[2];
            iPlayer.job[1] = reader.GetInt32("job");
            iPlayer.job[0] = reader.GetInt32("job");
           iPlayer.jobskill = new int[2];
            iPlayer.jobskill[1] = reader.GetInt32("jobskills");
            iPlayer.jobskill[0] = reader.GetInt32("jobskills");
            iPlayer.jailtime = new int[2];
            iPlayer.jailtime[1] = reader.GetInt32("jailtime");
            iPlayer.jailtime[0] = reader.GetInt32("jailtime");
            iPlayer.hasPerso = new int[2];
            iPlayer.hasPerso[1] = reader.GetInt32("Perso");
            iPlayer.hasPerso[0] = reader.GetInt32("Perso");
            iPlayer.fakePerso = false;
            iPlayer.fakeName = "";
            iPlayer.fakeSurname = "";
            iPlayer.donator = new int[2];
            iPlayer.donator[1] = reader.GetInt32("Donator");
            iPlayer.donator[0] = reader.GetInt32("Donator");
            iPlayer.uni_points = new int[2];
            iPlayer.uni_points[1] = reader.GetInt32("uni_points");
            iPlayer.uni_points[0] = reader.GetInt32("uni_points");
            iPlayer.uni_economy = new int[2];
            iPlayer.uni_economy[1] = reader.GetInt32("uni_economy");
            iPlayer.uni_economy[0] = reader.GetInt32("uni_economy");
            iPlayer.uni_business = new int[2];
            iPlayer.uni_business[1] = reader.GetInt32("uni_business");
            iPlayer.uni_business[0] = reader.GetInt32("uni_business");
            iPlayer.uni_workaholic = new int[2];
            iPlayer.uni_workaholic[1] = reader.GetInt32("uni_workaholic");
            iPlayer.uni_workaholic[0] = reader.GetInt32("uni_workaholic");
            iPlayer.birthday = new string[2];
            iPlayer.birthday[1] = reader.GetString("birthday");
            iPlayer.birthday[0] = reader.GetString("birthday");
            iPlayer.fspawn = new uint[2];
            iPlayer.fspawn[1] = reader.GetUInt32("fspawn");
            iPlayer.fspawn[0] = reader.GetUInt32("fspawn");

            iPlayer.hasPed = new string[2];
            iPlayer.hasPed[1] = reader.GetString("hasPed");
            iPlayer.hasPed[0] = reader.GetString("hasPed");
            iPlayer.Lic_FirstAID = new int[2];
            iPlayer.Lic_FirstAID[1] = reader.GetInt32("Lic_FirstAID");
            iPlayer.Lic_FirstAID[0] = reader.GetInt32("Lic_FirstAID");
            iPlayer.timeban = new int[2];
            iPlayer.timeban[1] = reader.GetInt32("timeban");
            iPlayer.timeban[0] = reader.GetInt32("timeban");
            iPlayer.job_skills = new string[2];
            iPlayer.job_skills[1] = reader.GetString("job_skills");
            iPlayer.job_skills[0] = reader.GetString("job_skills");
            iPlayer.warns = new int[2];
            iPlayer.warns[1] = reader.GetInt32("warns");
            iPlayer.warns[0] = reader.GetInt32("warns");
            iPlayer.fgehalt = new int[2];
            iPlayer.fgehalt[1] = reader.GetInt32("fgehalt");
            iPlayer.fgehalt[0] = reader.GetInt32("fgehalt");
            iPlayer.paycheck = new int[2];
            iPlayer.paycheck[1] = reader.GetInt32("paycheck");
            iPlayer.paycheck[0] = reader.GetInt32("paycheck");

            iPlayer.PedLicense = new bool[2];
            iPlayer.PedLicense[1] = reader.GetInt32("pedlicense") == 1;
            iPlayer.PedLicense[0] = reader.GetInt32("pedlicense") == 1;
         
            var handy = 1275 + iPlayer.Id;

            iPlayer.handy = new uint[2];
            iPlayer.handy[1] = handy;
            iPlayer.handy[0] = handy;

            iPlayer.guthaben = new int[2];
            iPlayer.guthaben[1] = reader.GetInt32("guthaben");
            iPlayer.guthaben[0] = reader.GetInt32("guthaben");
            iPlayer.Lic_Transfer = new int[2];
            iPlayer.Lic_Transfer[1] = reader.GetInt32("lic_transfer");
            iPlayer.Lic_Transfer[0] = reader.GetInt32("lic_transfer");
            iPlayer.married = new uint[2];
            iPlayer.married[1] = reader.GetUInt32("married");
            iPlayer.married[0] = reader.GetUInt32("married");
            iPlayer.Lic_Taxi = new int[2];
            iPlayer.Lic_Taxi[1] = reader.GetInt32("Lic_Taxi");
            iPlayer.Lic_Taxi[0] = reader.GetInt32("Lic_Taxi");

            // Setting SavedPos Params
            iPlayer.pos_x = new float[2];
            iPlayer.pos_x[1] = reader.GetFloat("pos_x");
            iPlayer.pos_x[0] = reader.GetFloat("pos_x");
            iPlayer.pos_y = new float[2];
            iPlayer.pos_y[1] = reader.GetFloat("pos_y");
            iPlayer.pos_y[0] = reader.GetFloat("pos_y");
            iPlayer.pos_z = new float[2];
            iPlayer.pos_z[1] = reader.GetFloat("pos_z");
            iPlayer.pos_z[0] = reader.GetFloat("pos_z");
            iPlayer.pos_heading = new float[2];
            iPlayer.pos_heading[1] = reader.GetFloat("pos_heading");
            iPlayer.pos_heading[0] = reader.GetFloat("pos_heading");
            iPlayer.Armor = new int[2];
            iPlayer.Armor[1] = reader.GetInt32("armor");
            iPlayer.Armor[0] = reader.GetInt32("armor");


            iPlayer.Dimension = new uint[2];
            iPlayer.Dimension[0] = reader.GetUInt32("dimension");
            iPlayer.Dimension[1] = reader.GetUInt32("dimension");


            iPlayer.MetaData = new MetaDataObject();
            iPlayer.MetaData.Position = new Vector3(iPlayer.pos_x[0], iPlayer.pos_y[0], iPlayer.pos_z[0]);
            iPlayer.MetaData.Dimension = iPlayer.Dimension[0];
            iPlayer.MetaData.Heading = 0f;
            iPlayer.MetaData.Armor = iPlayer.Armor[0];
            iPlayer.MetaData.Health = iPlayer.Hp;
            iPlayer.ApplyPlayerHealth();

            iPlayer.CanSeeNames = false;

            iPlayer.grade = new int[2];
            iPlayer.grade[1] = reader.GetInt32("grade");
            iPlayer.grade[0] = reader.GetInt32("grade");

            // drink
            iPlayer.drink = new int[2];
            iPlayer.drink[1] = reader.GetInt32("drink");
            iPlayer.drink[0] = reader.GetInt32("drink");

            // food
            iPlayer.food = new int[2];
            iPlayer.food[1] = reader.GetInt32("food");
            iPlayer.food[0] = reader.GetInt32("food");

            // fitness
            iPlayer.fitness = new int[2];
            iPlayer.fitness[1] = reader.GetInt32("fitness");
            iPlayer.fitness[0] = reader.GetInt32("fitness");


            iPlayer.SocialClubName = reader.GetString("SCName");

            iPlayer.DimensionType = new DimensionType[2];
            iPlayer.DimensionType[0] = (DimensionType) reader.GetInt32("dimensionType");
            iPlayer.DimensionType[1] = (DimensionType) reader.GetInt32("dimensionType");

            iPlayer.LastInteracted = DateTime.Now;

            iPlayer.VehicleKeys = new Dictionary<uint, string>();
            iPlayer.OwnVehicles = new Dictionary<uint, string>();
            
            iPlayer.HouseKeys = new HashSet<uint>();

            iPlayer.Weapons = new List<WeaponDetail>();
            if (reader.GetString("weapons") != "")
                iPlayer.Weapons = NAPI.Util.FromJson<List<WeaponDetail>>(reader.GetString("weapons"));





            // Add to Global Playerlist
            players.Add(iPlayer);

            // Rework module system needed
            iPlayer.LoadCrimes();

            iPlayer.VoiceHash = "";

            if (!playerIds.Contains(iPlayer.Id))
                playerIds.Add(iPlayer.Id);

            Modules.Instance.OnPlayerLoadData(iPlayer, reader);

            Modules.Instance.OnPlayerConnect(iPlayer);


            return iPlayer;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public void RemovePlayerId(uint id)
        {
            playerIds.Remove(id);
        }

        public bool DoesPlayerExists(uint id)
        {
            return playerIds.Contains(id);
        }

        private int GetFreeId()
        {
            var freeId = 0;
            while (players.ToList().FirstOrDefault(player => player != null && player.IsValid() && player.tmpPlayerId == freeId) != null)
            {
                freeId++;
            }

            return freeId;
        }
    }

    public static class PlayerListExtensions
    {
        public static void TriggerEvent(this IEnumerable<DbPlayer> players, string eventName, params object[] args)
        {
            foreach (var player in players)
            {
                player.Player.TriggerEvent(eventName, args);
            }
        }
    }
}