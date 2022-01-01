using System;
using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Banks.BankHistory;
using GVRP.Module.Clothes.Character;
using GVRP.Module.Crime;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Phone.Contacts;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Players.Ranks;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Permission;
using GVRP.Module.Pet;
using GVRP.Module.Players.Phone.Apps;
using GVRP.Module.Injury;
using GVRP.Module.Customization;
using GVRP.Module.Items;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Weapons;
using GVRP.Module.Voice;
using System.Threading.Tasks;
using GVRP.Module.Telefon.App.Settings;
using GVRP.Module.Telefon.App.Settings.Wallpaper;
using GVRP.Module.Telefon.App.Settings.Ringtone;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Clothes.Outfits;
using GVRP.Module.Business;
using GVRP.Module.Delivery;
using GVRP.Module.Delivery.Menu;
using GVRP.Module.Freiberuf;
using GVRP.Module.Vehicles;
using GVRP.Module.Gangwar;
using GVRP.Module.Houses;
using static GVRP.Module.Players.Events.EventStateModule;

namespace GVRP.Module.Players.Db
{
    public enum DimensionType : uint
    {
        World = 0,
        House = 1,
        Basement = 2,
        Labor = 3,
        Business = 4,
        Storage = 6,
        WeaponFactory = 7,
        Camper = 8,
        House_Shop_Interior = 9,
        Methlaboratory = 10,
        MoneyKeller = 11,
        Gangwar = 12,
        Weaponlaboratory = 13,
        Cannabislaboratory = 14,
        RacingArea = 15,
    }

    public enum ArmorType : uint
    {
        Normal = 0,
        Strong = 1,
        Admin = 2,
    }

    public class MetaDataObject
    {
        public Vector3 Position { get; set; }
        public uint Dimension { get; set; }
        public float Heading { get; set; }

        public int Health { get; set; }
        public int Armor { get; set; }
        public bool SaveBlocked { get; set; }

        public MetaDataObject()
        {
            Position = new Vector3();
            Dimension = 0;
            Heading = 0.0f;
            Health = 100;
            Armor = 0;
            SaveBlocked = false;
        }
    }

    //Todo: runtime property indexer for db columns
    public class DbPlayer
    {
        public enum Value
        {
            Duty = 0,
            RankId = 1,
            TeamId = 2,
            Level = 3,
            TeamRang = 4,
            DeathStatus = 5,
            IsCuffed = 6,
            IsTied = 7,
            Hp = 8,
            Armor = 9,
            Swat = 10,
            UHaftTime = 11,
            Einwanderung = 12,
            SwatDuty = 13,
            Teamfight = 14,
            Suspension = 15,
            WDutyTime = 16,
        }
        public DateTime LastQueryBreak = DateTime.Now;

        public enum RankDutyStatus
        {
            OffDuty = 0,
            AdminDuty = 1,
            GuideDuty = 2,
            CasinoDuty = 3,
            GameDesignDuty = 4
        }

        public static readonly string[] DbColumns =
        {
            "duty",
            "rankId",
            "team", //Todo: needs db rename
            "Level",
            "rang", //Todo: needs db rename
            "Deadstatus",
            "isCuffed",
            "isTied",
            "hp",
            "armor",
            "swat",
            "uhaft",
            "einwanderung",
            "swatduty",
            "teamfight",
            "suspendate",
            "w_dutytime",
        };

        public Client Player { get; set; }

        // Character
        public Character Character { get; set; }

        // Customization
        public CharacterCustomization Customization { get; set; }

        // PlayerDataObject
        public PlayerBuffs Buffs { get; set; }

        // PlayerRights
        public TeamRankPermission TeamRankPermission { get; set; }

        public Dictionary<string, dynamic> PlayerData { get; set; }
        public MetaDataObject MetaData { get; set; }

        public uint Id { get; set; }
        public string Password { get; set; }
        public string Salt { get; set; }

        public int Level { get; set; }
        public RankDutyStatus RankDuty { get; set; }
        public uint RankId { get; set; }
        public AccountStatus AccountStatus { get; set; }
        public int PassAttempts { get; set; }
        public uint TeamId { get; private set; }
        public int[] money { get; set; }
        public int[] bank_money { get; set; }

        public int[] blackmoney { get; set; }
        public int[] blackmoneybank { get; set; }
        public uint TeamRank { get; set; }
        public int[] payday { set; get; }
        public bool Firstspawn { get; set; }
        public bool IsSwimmingOrDivingDoNotUse { get; set; }
        public int[] rp { get; set; }
        public uint[] ownHouse { get; set; }
        public int[] wanteds { get; set; }
        public uint WatchMenu { get; set; }
        public bool Freezed { get; set; }
        public uint watchDialog { get; set; }
        public int[] Lic_Car { get; set; }
        public int[] Lic_LKW { get; set; }
        public int[] Lic_Bike { get; set; }
        public int[] Lic_PlaneA { get; set; }
        public int[] Lic_PlaneB { get; set; }
        public int[] Lic_Boot { get; set; }
        public int[] Lic_Gun { get; set; }
        public int[] Lic_Biz { get; set; }
        public int marryLic { get; set; }
        public int[] spawnchange { get; set; }
        public int[] job { get; set; }
        public int[] jobskill { get; set; }
        public string[] job_skills { get; set; }
        public int[] jailtime { get; set; }
        public int[] uni_points { get; set; }
        public int[] uni_economy { get; set; }
        public int[] uni_business { get; set; }
        public int[] uni_workaholic { get; set; }
        public int[] deadtime { get; set; }
        public InjuryType Injury { get; set; }
        public float[] dead_x { get; set; }
        public float[] dead_z { get; set; }
        public float[] dead_y { get; set; }
        public int[] hasPerso { get; set; }
        public bool fakePerso { get; set; }
        public string fakeName { get; set; }
        public string fakeSurname { get; set; }
        public uint[] fspawn { get; set; }

        public string[] birthday { get; set; }
        public int[] donator { get; set; }
        public string[] hasPed { get; set; }
        public int[] Lic_FirstAID { get; set; }
        public int[] timeban { get; set; }
        public int tmpPlayerId { get; set; }
        public int[] warns { get; set; }
        public int[] fgehalt { get; set; }
        public int[] paycheck { get; set; }
        public uint[] handy { get; set; }
        public int[] guthaben { get; set; }
        public int[] Lic_Transfer { get; set; }
        public uint[] married { get; set; }
        public int[] Lic_Taxi { get; set; }
        public float[] pos_x { get; set; }
        public float[] pos_y { get; set; }
        public float[] pos_z { get; set; }
        public float[] pos_heading { get; set; }
        public bool Duty { get; set; }
        public int Hp { get; set; }
        public int[] Armor { get; set; }

        public ArmorType ArmorType { get; set; }
        public uint VisibleArmorType { get; set; }

        public bool CanSeeNames { get; set; }
        public bool visibleArmor { get; set; }
        public bool IsCuffed { get; set; }
        public bool IsTied { get; set; }
        public bool IsFarming { get; set; }
        public bool IsInRob { get; set; }

        public int[] grade { get; set; }
        public int UHaftTime { get; set; }

        public int Einwanderung { get; set; }

        public int SwatDuty { get; set; }
        public int Teamfight { get; set; }
        public int WDutyTime { get; set; }

        public bool Suspension { get; set; }

        public string UndercoverName { get; set; }

        public string SocialClubName { get; set; }

        public string VoiceHash { get; set; }

        public int[] drink { get; set; }
        public int[] food { get; set; }
        public int[] fitness { get; set; }

        public int ForumId { get; set; }

        public bool IsNSA { get; set; }

        public GTANetworkAPI.Object adminObject { get; set; }
        public double adminObjectSpeed { get; set; }

        public string saveQuery { get; set; }

        public DateTime lastKeySend { get; set; }
        public DateTime spawnProtection { get; set; }

        // Temp Wanted
        public int TempWanteds { get; set; }

        public uint[] Dimension { get; set; }
        public DimensionType[] DimensionType { get; set; }

        public DateTime LastInteracted { get; set; }
        public DateTime LastEInteract { get; set; }

        //TOOD: move to Phone object
        //Phone
        public PhoneContacts PhoneContacts { get; set; }

        public PhoneApps PhoneApps { get; set; }

        public Dictionary<uint, String> VehicleKeys { get; set; }

        public Dictionary<uint, String> OwnVehicles { get; set; }

        public Dictionary<int, int> Attachments { get; set; }

        public Dictionary<uint, PlayerTask.PlayerTask> PlayerTasks { get; set; }

        public HashSet<uint> HouseKeys { get; set; }
        public HashSet<uint> StorageKeys { get; set; }

        public List<WeaponDetail> Weapons { get; set; }

        public bool IsInTask { get; set; }
        public Rank Rank { get; set; }

        // Reworked Stuff
        public Team Team { get; private set; }

        public List<Banks.BankHistory.BankHistory> BankHistory { get; set; }
        public AnimationScenario AnimationScenario { get; set; }

        //BusinessId, Member
        public Business.Business.Member BusinessMembership { get; set; }

        public Business.Business ActiveBusiness { get; set; }

        public List<CrimePlayerReason> Crimes { get; set; }

        public bool[] PedLicense { get; set; }

        public DateTime? FreezedUntil { get; set; }

        public dynamic[] DbValues { get; }

        public int CurrentSeat { get; set; } = -1;
        public int CurrentSeatIndex { get; set; } = -1;

        public PlayerPet PlayerPet { get; set; }
        public Container Container { get; set; }
        public Container TeamFightContainer { get; set; }
        public FunkStatus funkStatus { get; set; }

        public int Swat { get; set; }
        public DateTime xmasLast { get; set; }
        public DateTime DrugCreateLast { get; set; }
        public DateTime LastPhoneNumberChange { get; set; }

        public PhoneSetting phoneSetting { get; set; }

        public Wallpaper wallpaper { get; set; }
        public Ringtone ringtone { get; set; }
        public List<DbPlayer> playerWhoHearRingtone { get; set; }

        public CustomData CustomData { get; set; }

        public int VehicleTaxSum { get; set; }

        public bool IsSpeaking = false;

        public int TeamfightKillCounter = 0;

        public DateTime LastUninvite { get; set; }

        public Dictionary<uint, uint> AnimationShortcuts { get; set; }

        public List<Outfit> Outfits { get; set; }

        public DateTime LastReport { get; set; }
        public Container PrisonLockerContainer { get; set; }
        public Container MethlaboratoryInputContainer { get; set; }
        public Container MethlaboratoryOutputContainer { get; set; }
        public Container WeaponlaboratoryInputContainer { get; set; }
        public Container WeaponlaboratoryOutputContainer { get; set; }
        public Container CannabislaboratoryInputContainer { get; set; }
        public Container CannabislaboratoryOutputContainer { get; set; }

        public Container WorkstationFuelContainer { get; set; }
        public Container WorkstationSourceContainer { get; set; }
        public Container WorkstationEndContainer { get; set; }

        public uint WorkstationId { get; set; }
        
        public Dictionary<uint, int> DeliveryJobSkillPoints { get; set; }

        public bool ParamedicLicense { get; set; }

        public string GovLevel { get; set; }

        public int RacingBestTimeSeconds { get; set; }

        public Dictionary<EventListIds, int> EventDoneList = new Dictionary<EventListIds, int>();

        public DbPlayer(MySqlDataReader reader)
        {
            DbValues = new dynamic[DbColumns.Length];

            IsInTask = false;

            Crimes = new List<CrimePlayerReason>();

            RankDuty = RankDutyStatus.OffDuty;
            
            RankId = reader.GetUInt32("rankId");
            TeamId = reader.GetUInt32("team");
            TeamRank = reader.GetUInt32("rang");
            Level = reader.GetInt32("Level");
            Duty = reader.GetUInt32("duty") == 1;
            IsCuffed = reader.GetInt32("isCuffed") == 1;
            IsTied = reader.GetInt32("isTied") == 1;
            Hp = reader.GetInt32("hp");
            Swat = reader.GetInt32("swat");
            xmasLast = reader.GetDateTime("xmasLast");
            DrugCreateLast = reader.GetDateTime("drugcreatelast");
            UHaftTime = reader.GetInt32("uhaft");
            Einwanderung = reader.GetInt32("einwanderung");
            SwatDuty = reader.GetInt32("swatduty");
            Teamfight = reader.GetInt32("teamfight");
            IsNSA = reader.GetInt32("nsalic") == 1;
            Suspension = reader.GetInt32("suspendate") == 1;
            WDutyTime = reader.GetInt32("w_dutytime");
            marryLic = reader.GetInt32("marrylic");
            ParamedicLicense = reader.GetInt32("mediclic") == 1;
            GovLevel = reader.GetString("gov_level");
            RacingBestTimeSeconds = reader.GetInt32("racing_besttime");

            int[] temp = new int[] { 0, 0 };

            temp[0] = reader.GetInt32("armor");
            VisibleArmorType = reader.GetUInt32("visibleArmorType");
            IsSwimmingOrDivingDoNotUse = false;

            DbValues[(uint)Value.Duty] = Duty;
            DbValues[(uint)Value.RankId] = RankId;
            DbValues[(uint)Value.TeamId] = TeamId;
            DbValues[(uint)Value.Level] = Level;
            DbValues[(uint)Value.TeamRang] = TeamRank;
            DbValues[(uint)Value.IsCuffed] = IsCuffed;
            DbValues[(uint)Value.IsTied] = IsTied;
            DbValues[(uint)Value.Hp] = Hp;
            DbValues[(uint)Value.Armor] = temp[0];
            DbValues[(uint)Value.Swat] = Swat;
            DbValues[(uint)Value.UHaftTime] = UHaftTime;
            DbValues[(uint)Value.Einwanderung] = Einwanderung;
            DbValues[(uint)Value.SwatDuty] = SwatDuty;
            DbValues[(uint)Value.Teamfight] = Teamfight;
            DbValues[(uint)Value.Suspension] = Suspension;
            DbValues[(uint)Value.WDutyTime] = WDutyTime;

            Armor = temp;

            PlayerData = new Dictionary<string, dynamic>();
            VehicleKeys = new Dictionary<uint, string>();
            Attachments = new Dictionary<int, int>();
            LastReport = DateTime.Now.Subtract(TimeSpan.FromMinutes(10));
            DeliveryJobSkillPoints = new Dictionary<uint, int>();
        }

        public void UpdateDeliveryJobSkillPoints()
        {
            String updateString = "";
            foreach (var deliveryJob in DeliveryJobSkillPoints)
            {
                if (!updateString.Equals("")) updateString += ",";
                updateString += deliveryJob.Key + ":" + deliveryJob.Value;
            }

      //      MySQLHandler.ExecuteAsync($"UPDATE player SET delivery_job_skillpoints = '{updateString}' WHERE id = '{Id}'");
        }

        public void IncreaseDeliveryJobSkillPoints(DeliveryJob deliveryJob, int amount)
        {
            int oldAmount = DeliveryJobSkillPoints.GetValueOrDefault(deliveryJob.SkillpointType);
            DeliveryJobSkillPoints.Remove(deliveryJob.SkillpointType);
            DeliveryJobSkillPoints.Add(deliveryJob.SkillpointType, oldAmount + amount);
            UpdateDeliveryJobSkillPoints();
        }

        public void FinishDeliveryJob(bool finisedSuccessfully, DeliveryJob deliveryJob)
        {
            SxVehicle sxVehicle = FreiberufFunctions.GetJobVehicle(this, DeliveryJobModule.DeliverJobVehMarkId);

            if (finisedSuccessfully)
            {
                //Auftrag wurde erfolgreich abgeschlossen. Spieler bekommt Belohnung

                foreach (var delivery in DeliveryJobModule.Instance.DeliveryOrders.GetValueOrDefault(this).DeliveryPositions)
                {
                    if (delivery.Value == false)
                    {
                        this.SendNewNotification("Du hast noch nicht alle Lieferungen ausgeliefert!", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        return;
                    }
                }


                if (sxVehicle != null && sxVehicle.IsValid())
                {
                    if (sxVehicle.entity.Position.DistanceTo(this.Player.Position) < 20.0f)
                    {
                        this.SendNewNotification("Du hast deinen Auftrag erfolgreich beendet.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        this.IncreaseDeliveryJobSkillPoints(deliveryJob, 100);
                    }
                    else
                    {
                        this.SendNewNotification("Du musst dein Firmenfahrzeug wieder her bringen. Ansonsten kriegst du von mir gar nichts!", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        return;
                    }
                }
            }
            else
            {
                //Auftrag wurde abgebrochen
                this.SendNewNotification("Du hast deinen Auftrag abgebrochen", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
            }

            VehicleHandler.Instance.DeleteVehicle(sxVehicle);
            DeliveryJobModule.Instance.DeliveryOrders.Remove(this);
            ResetData("delivery_job_id");
            ResetData("delivery_tour_start");
            ResetData("delivery_has_package");
            ResetData("delivery_vehicle_package_amount");
        }

        public void SetData(string key, dynamic value)
        {
            try { 
            if (PlayerData.ContainsKey(key))
            {
                PlayerData[key] = value;
            }
            else
            {
                lock (PlayerData)
                {
                    PlayerData.Add(key, value);
                }
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public bool HasData(string key)
        {
            try { 
            return (PlayerData.ContainsKey(key));
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public void ResetData(string key)
        {
            try { 
            if (PlayerData.ContainsKey(key)) PlayerData.Remove(key);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public dynamic GetData(string key)
        {
        try { 
    var result = (PlayerData.ContainsKey(key)) ? PlayerData[key] : "";
            return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return null;
        }

        public bool TryData<T>(string key, out T value)
        {
            var tmpdata = PlayerData.ContainsKey(key);
            value = tmpdata ? (T)PlayerData[key] : default(T);
            return tmpdata;
        }

        public void SetRank(uint rank)
        {
            RankId = rank;
            Player?.TriggerEvent("updateTeamRank", rank);
        }

        public void SetTeam(uint teamid, bool resetweaponsonDuty = true)
        {
            if (Team != null && Team.HasDuty && resetweaponsonDuty)
            {
                if (Duty)
                {
                    Duty = false;
                    this.RemoveWeapons();
                }
            }

            if (TeamId != 0)
            {
                TeamModule.Instance[TeamId]?.RemoveMember(this);
            }

            TeamId = teamid;
            Team = TeamModule.Instance[teamid];
            Team?.AddMember(this);

            Player?.TriggerEvent("updateTeamId", teamid);
        }

        public void SetTeamfight()
        {
            Teamfight = Team.IsInTeamfight() ? 1 : 0;
            PlayerDb.Save(this);
        }

        public bool UpdateApps()
        {
            if (TeamId == (uint)TeamList.Zivilist)
            {
                this.PhoneApps.Remove("TeamApp");
            }
            else
            {
                this.PhoneApps.Add("TeamApp");
            }

            if (ActiveBusiness == null || ActiveBusiness.Id == 0)
            {
                this.PhoneApps.Remove("BusinessApp");
            }
            else
            {
                this.PhoneApps.Add("BusinessApp");
            }

            if (!VoiceModule.Instance.hasPlayerRadio(this))
            {
                this.funkStatus = FunkStatus.Deactive;
                VoiceModule.Instance.ChangeFrequenz(this, 0.0f);
                this.PhoneApps.Remove("FunkApp");
            }
            else
            {
                this.PhoneApps.Add("FunkApp");
            }

            return true;
        }

        public bool IsAMedic()
        {
            return Team != null && Team.IsMedics();
        }

        public bool IsCopPackGun()
        {
            return Team != null && Duty && (Team.IsCops() || Team.IsDpos() || Team.IsMedics() || Team.Id == (int)teams.TEAM_DRIVINGSCHOOL || Team.Id == (int)teams.TEAM_NEWS);
        }

        public bool IsACop()
        {
            return Team != null && Team.IsCops();
        }

        public bool IsGoverment()
        {
            return Team != null && Team.Id == 14;
        }

        public bool IsAGangster()
        {
            return Team.IsGangsters();
        }

        public bool IsHomeless()
        {
            return !this.IsTenant() && ownHouse[0] == 0;
        }
        public void UpdateWimmingOrDiving()
        {
            this.Player.TriggerEvent("isPlayerSwimming");
        }

        [RemoteEvent]
        public void swimmingOrDivingResponse(Client player, bool swimStatus)
        {
            IsSwimmingOrDivingDoNotUse = swimStatus;
        }

        public void Kick(string reason = "")
        {
            Player.SendNotification("Du wurdest gekickt. Grund: " + reason);
            Player.Kick(reason);
        }

        internal void PlayAnimation(int animationFlags, string animationDict, string animationName, float speed = 8f)
        {
            NAPI.Player.PlayPlayerAnimation(Player, animationFlags, animationDict, animationName, speed);
        }

        public string GetName()
        {
            var l_Name = Player.Name;
            if (fakePerso && fakeName.Length > 0 && fakeSurname.Length > 0)
                l_Name = $"{fakeName}_{fakeSurname}";

            return l_Name;
        }

        public async Task PlayInventoryInteractAnimation(int time = 1500)
        {
            if (Player.IsInVehicle) return;

            PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");
            Player.TriggerEvent("freezePlayer", true);
            await Task.Delay(time);
            if (Player == null || !NAPI.Pools.GetAllPlayers().Contains(Player) || !Player.Exists) return;
            Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(Player);
        }
        
        public void SaveArmorType(uint type)
        {
            string query = $"UPDATE `player` SET visibleArmorType = '{type}' WHERE id = '{Id}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public void SaveBlackMoneyBank()
        {
            string query = $"UPDATE `player` SET blackmoneybank = '{blackmoneybank[0]}' WHERE id = '{Id}';";
            blackmoneybank[1] = blackmoneybank[0];
            MySQLHandler.ExecuteAsync(query);
        }

        public bool IsOrtable(DbPlayer fromPlayer, bool ignoreTimer = false)
        {
            if (CrimeModule.Instance.CalcWantedStars(this.Crimes) == 0 && fromPlayer.TeamRank < 5)
            {
                fromPlayer.SendNewNotification("Es liegt kein Haftbefehl gegen den Spieler vor.");
                fromPlayer.SendNewNotification("Um ohne Orten zu koennen benoetigen Sie mind Rang 5.");
                return false;
            }

            if (this.IsAGangster() && GangwarTownModule.Instance.IsTeamInGangwar(this.Team) && this.Dimension[0] == GangwarModule.Instance.DefaultDimension)
            {
                fromPlayer.SendNewNotification("Person konnte nicht geortet werden!");
                return false;
            }

            if (!ignoreTimer && this.HasData("isOrted_" + fromPlayer.TeamId))
            {
                DateTime isOrted = this.GetData("isOrted_" + fromPlayer.TeamId);
                if (isOrted > DateTime.Now)
                {
                    fromPlayer.SendNewNotification(
                        "Bürger wurde bereits geortet! (Nur jede Minute 1x möglich)");
                    return false;
                }
            }

            if (this.Container.GetItemAmount(174) >= 1 && !this.phoneSetting.flugmodus)
            {
                return true;
            }
            return false;
        }
    }
}