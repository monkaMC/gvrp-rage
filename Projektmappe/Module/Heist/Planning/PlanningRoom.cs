using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Items;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Spawners;

namespace GVRP.Module.Heist.Planning
{
    public class PlanningRoom : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint TeamId { get; set; }
        public bool Bought { get; set; }
        public int JumpPointId { get; set; }
        public int JumpPointVehicleId { get; set; }

        public int MainFloor { get; set; }
        public int MainFloorCleanup { get; set; }

        public int MainFloorItem1ItemCount { get; set; }
        public int MainFloorItem2ItemCount { get; set; }
        public int MainFloorItem3ItemCount { get; set; }
        public int MainFloorItem4ItemCount { get; set; }
        public int MainFloorItem5ItemCount { get; set; }

        public int MainFloorMirrorLevel { get; set; }
        public int MainFloorInteriorLevel { get; set; }
        public int MainFloorSlotMachineLevel { get; set; }

        public int MainFloorMirrorItemCount { get; set; }
        public int MainFloorInteriorItmeCount { get; set; }
        public int MainFloorSlotMachineItemCount { get; set; }

        public int MainFloorStyle { get; set; }

        public int BasementLevel { get; set; }
        public int BasementCleanUp { get; set; }

        public int BasementMechanicLevel { get; set; }
        public int BasementHackerLevel { get; set; }
        public int BasementWeaponsLevel { get; set; }
        public int BasementWardrobeLevel { get; set; }
        public int CasinoPlanLevel { get; set; }
        public Vector3 ContactPersonPosition { get; set; }
        public float ContactPersonHeading { get; set; }
        public int Prop_crafting { get; set; }
        public DateTime PropCrafting { get; set; }
        public int CasinoPropDoor { get; set; }

        public int BasementElectronicItemCount { get; set; }
        public int BasementMachanicItemCount { get; set; }
        public int BasementHackerItemCount { get; set; }
        public int BasementWeaponsItemCount { get; set; }
        public int BasementKellerWardrobeItemCount { get; set; }
        public Container PlanningroomWardrobeContainer { get; set; }

        public ColShape Interior { get; set; }
        public Vector3 InteriorPosition { get; set; }
        public List<uint> rewardIds = new List<uint>();

        public List<DbPlayer> PlayersInsideRoom = new List<DbPlayer>();
        public List<Marker> markers = new List<Marker>();
        public Marker GrundraumWoodMarker { get; set; }
        public Marker GrundraumIronMarker { get; set; }
        public Marker GrundraumCementMarker { get; set; }
        public Marker GrundraumPlasticMarker { get; set; }
        public Marker GrundraumGlasMarker { get; set; }
        public Marker SpiegelGlasMarker { get; set; }
        public Marker InneneinrichtungGlasMarker { get; set; }
        public Marker SpielautomatenGlasMarker { get; set; }
        public Marker KellerElectronicMarker { get; set; }
        public Marker KellerMechanicMarker { get; set; }
        public Marker KellerHackerMarker { get; set; }
        public Marker KellerWeaponMarker { get; set; }
        public Marker KellerWardrobeMarker { get; set; }
        public Marker KellerMechanicFinalMarker { get; set; }
        public Marker KellerMechanicGarageMarker { get; set; }
        public Marker KellerPreQuestMarker { get; set; }
        public Marker KellerHeistQuestMarker { get; set; }
        public Marker KellerWadrobeFinalMarker { get; set; }
        public Marker KellerWeaponFinalMarker { get; set; }

        public TextLabel GrundraumWoodLabel { get; set; }
        public TextLabel GrundraumIronLabel { get; set; }
        public TextLabel GrundraumCementLabel { get; set; }
        public TextLabel GrundraumPlasticLabel { get; set; }
        public TextLabel GrundraumGlasLabel { get; set; }
        public TextLabel SpiegelGlasLabel { get; set; }
        public TextLabel KellerElectronicLabel { get; set; }
        public TextLabel InneneinrichtungGlasLabel { get; set; }
        public TextLabel SpielautomatenGlasLabel { get; set; }
        public TextLabel KellerMechanicLabel { get; set; }
        public TextLabel KellerHackerLabel { get; set; }
        public TextLabel KellerWeaponLabel { get; set; }
        public TextLabel KellerWardrobeLabel { get; set; }

        public List<TextLabel> textLabels = new List<TextLabel>();
        public GTANetworkAPI.Object DoorObject;
        public Marker MainFloorMarker { get; set; }
        public Marker BasementMarker { get; set; }
        public int PlanningOutfitCounter { get; set; }
        public bool PlanningOutfitIsActive { get; set; }

        public PlanningRoom(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            TeamId = reader.GetUInt32("teamid");
            Bought = reader.GetInt32("bought") == 1;
            JumpPointId = reader.GetInt32("jumppoint_outside");
            JumpPointVehicleId = reader.GetInt32("jumppoint_outside_vehicle");
            MainFloor = reader.GetInt32("grundraum");
            MainFloorMirrorLevel = reader.GetInt32("spiegel");
            MainFloorStyle = reader.GetInt32("einrichtungsstyle");
            MainFloorInteriorLevel = reader.GetInt32("inneneinrichtung");
            MainFloorSlotMachineLevel = reader.GetInt32("spielautomaten");
            string rewardString = reader.GetString("rewards");
            BasementLevel = reader.GetInt32("kellerraum");
            BasementMechanicLevel = reader.GetInt32("mechanic");
            BasementHackerLevel = reader.GetInt32("hacker");
            BasementWeaponsLevel = reader.GetInt32("weapons");
            BasementWardrobeLevel = reader.GetInt32("wardrobe");
            MainFloorCleanup = reader.GetInt32("cleanup");
            BasementCleanUp = reader.GetInt32("kellerCleanUp");
            MainFloorItem1ItemCount = reader.GetInt32("grundraum_wood");
            MainFloorItem2ItemCount = reader.GetInt32("grundraum_iron");
            MainFloorItem3ItemCount = reader.GetInt32("grundraum_cement");
            MainFloorItem4ItemCount = reader.GetInt32("grundraum_plastic");
            MainFloorItem5ItemCount = reader.GetInt32("grundraum_glas");
            MainFloorMirrorItemCount = reader.GetInt32("spiegel_glas");
            MainFloorInteriorItmeCount = reader.GetInt32("inneneinrichtung_glas");
            MainFloorSlotMachineItemCount = reader.GetInt32("spielautomaten_glas");
            MainFloorSlotMachineItemCount = reader.GetInt32("spielautomaten_glas");
            BasementElectronicItemCount = reader.GetInt32("keller_electronic");
            BasementMachanicItemCount = reader.GetInt32("keller_mechanic");
            BasementHackerItemCount = reader.GetInt32("keller_hacker");
            BasementWeaponsItemCount = reader.GetInt32("keller_weapons");
            BasementKellerWardrobeItemCount = reader.GetInt32("keller_wardrobe");
            PlanningroomWardrobeContainer = ContainerManager.LoadContainer(reader.GetUInt32("wardrobe_container_id"), ContainerTypes.PLANNINGROOMWARDROBE, 0);
            CasinoPlanLevel = reader.GetInt32("casino_plan");
            ContactPersonPosition = new Vector3(reader.GetFloat("plan_pos_x"), reader.GetFloat("plan_pos_y"), reader.GetFloat("plan_pos_z"));
            ContactPersonHeading = reader.GetFloat("plan_heading");
            Prop_crafting = reader.GetInt32("prop_crafting");
            PropCrafting = reader.GetDateTime("prop_crafting_started");
            CasinoPropDoor = reader.GetInt32("casino_prop");

            if (!Bought)
            {
                JumpPoint jumpPoint = JumpPointModule.Instance.Get(JumpPointId);

                if (jumpPoint != null)
                {
                    jumpPoint.Disabled = true;
                    jumpPoint.HideInfos = true;
                }
            }

            if (BasementLevel < 2)
            {
                JumpPoint jumpPoint = JumpPointModule.Instance.Get(JumpPointVehicleId);

                if (jumpPoint != null)
                {
                    jumpPoint.Disabled = true;
                    jumpPoint.HideInfos = true;
                }
            }

            if (MainFloor < 2)
                DoorObject = NAPI.Object.CreateObject(3358237751, new Vector3(2727.282, -371.9337, -47.10417), new Vector3(0, 90, 0), 255, TeamId);
            if (MainFloor > 0)
                MainFloorMarker = Markers.Create(0, PlanningModule.Instance.PlanningRoomLaptopPosition, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 255, 0, TeamId, true);
            if (BasementLevel > 1)
                BasementMarker = Markers.Create(0, PlanningModule.Instance.PlanningRoomKellerLaptopLocation, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 255, 0, TeamId, true);

            if (!string.IsNullOrEmpty(rewardString))
            {
                string[] splittedRewardIds = rewardString.Split(',');
                foreach (string rewardIdstring in splittedRewardIds)
                {
                    if (!uint.TryParse(rewardIdstring, out uint rewardId)) return;
                    rewardIds.Add(rewardId);
                }
            }

            PlanningOutfitCounter = 0;
            PlanningOutfitIsActive = false;

            InteriorPosition = new Vector3(2707.41f, -369.817f, -54.7809f);
            Interior = ColShapes.Create(InteriorPosition, 100.0f, TeamId);
            Interior.SetData("planningroomInteriorColshape", TeamId);

            new Npc(Enum.TryParse("a_f_y_business_01", true, out PedHash casino) ? casino : PedHash.Business01AFY, ContactPersonPosition, ContactPersonHeading, 0);
        }

        public void SavePlanningRoom()
        {
            string rewardString = "";

            foreach (uint rewardId in rewardIds)
            {
                rewardString += rewardId.ToString() + ",";
            }

            if (rewardString.Length > 0)
                rewardString = rewardString.Substring(0, rewardString.Length - 1);
            int bought = Bought ? 1 : 0;
            string query = $"UPDATE `team_planningrooms` SET " +
            $"`bought` = '{bought}', `grundraum` = '{MainFloor}', `spiegel` = '{MainFloorMirrorLevel}', `inneneinrichtung` = '{MainFloorInteriorLevel}', `einrichtungsstyle` = '{MainFloorStyle}', `spielautomaten` = '{MainFloorSlotMachineLevel}', `rewards` = '{rewardString}', `kellerraum` = '{BasementLevel}', `mechanic` = '{BasementMechanicLevel}', `hacker` = '{BasementHackerLevel}', `weapons` = '{BasementWeaponsLevel}', `wardrobe` = '{BasementWardrobeLevel}', `cleanup` = '{MainFloorCleanup}', `kellerCleanUp` = '{BasementCleanUp}', `grundraum_wood` = '{MainFloorItem1ItemCount}' , `grundraum_iron` = '{MainFloorItem2ItemCount}' , `grundraum_cement` = '{MainFloorItem3ItemCount}' , `grundraum_plastic` = '{MainFloorItem4ItemCount}' , `grundraum_glas` = '{MainFloorItem5ItemCount}' , `spiegel_glas` = '{MainFloorMirrorItemCount}' , `inneneinrichtung_glas` = '{MainFloorInteriorItmeCount}' , `spielautomaten_glas` = '{MainFloorSlotMachineItemCount}', `keller_electronic` = '{BasementElectronicItemCount}', `keller_mechanic` = '{BasementMachanicItemCount}', `keller_hacker` = '{BasementHackerItemCount}', `keller_weapons` = '{BasementWeaponsItemCount}', `keller_wardrobe` = '{BasementKellerWardrobeItemCount}', `casino_plan` = '{CasinoPlanLevel}', `prop_crafting` = '{Prop_crafting}', `casino_prop` = '{CasinoPropDoor}'" +
            $"WHERE `teamid` = {TeamId};";
            MySQLHandler.ExecuteAsync(query);
        }

        public void PurchasePlanningRoom(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;

            int RequiredPlanningRoomPurchaseAmount = 2000000;

            if (dbPlayer.TeamRankPermission.Manage < 1)
            {
                dbPlayer.SendNewNotification("Du besitzt nicht die notwendigen Rechte für dieses Vorhaben!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (dbPlayer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) < RequiredPlanningRoomPurchaseAmount)
            {
                dbPlayer.SendNewNotification("Du hast nicht genügend Schwarzgeld dabei!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            dbPlayer.Container.RemoveItem(SchwarzgeldModule.SchwarzgeldId, RequiredPlanningRoomPurchaseAmount);
            dbPlayer.SendNewNotification("Hier hast du die Schlüssel. Vielleicht solltest du dort ein wenig aufräumen!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.SUCCESS);

            Vector3 location = JumpPointModule.Instance.jumpPoints.FirstOrDefault(point => point.Value.Id == PlanningModule.Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id).JumpPointId).Value.Position;
            dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", location.X, location.Y);

            Bought = true;
            SavePlanningRoom();
            ActivatePlanningroomJumppoint();
        }

        public void CraftCasinoDoor(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;

            int RequiredCraftPrice = 1000000;

            if (Prop_crafting == 0)
            {
                if (dbPlayer.TeamRankPermission.Manage < 1)
                {
                    dbPlayer.SendNewNotification("Du besitzt nicht die notwendigen Rechte für dieses Vorhaben!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                if (dbPlayer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) < RequiredCraftPrice)
                {
                    dbPlayer.SendNewNotification("Du hast nicht genügend Schwarzgeld dabei!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                dbPlayer.Container.RemoveItem(SchwarzgeldModule.SchwarzgeldId, RequiredCraftPrice);

                Prop_crafting = 1;
                PropCrafting = DateTime.Now;
                MySQLHandler.ExecuteAsync($"UPDATE team_planningrooms SET `prop_crafting_started` = '{PropCrafting.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `teamid` = '{dbPlayer.Team.Id}'");
                dbPlayer.SendNewNotification($"Die Tresortür wird nun hergestellt. Ihr könnt diese in 7 Tagen abholen!", PlayerNotification.NotificationType.SUCCESS);

                SavePlanningRoom();
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast bereits eine Tresortür in Arbeit gegeben!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public void DeliverCasinoDoor(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;

            if (Prop_crafting == 1)
            {
                DateTime actualDate = DateTime.Now;
                if (PropCrafting.AddHours(168) < actualDate)
                {
                    UpgradePlanningRoom(dbPlayer, 12, 1);
                }
                else
                {
                    dbPlayer.SendNewNotification($"Die Tresortür wurde noch nicht angefertigt, komme zu einem späterem Zeitpunkt wieder!", PlayerNotification.NotificationType.ERROR);
                    return;
                }
            }
            else
            {
                dbPlayer.SendNewNotification($"Die herstellung der Tresortür wurde noch nicht gestartet!", PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public void DeliverPlanningroomTrash(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.HasData("pressedEOnProcess")) return;

            int trashAmount = dbPlayer.Container.GetItemAmount(PlanningModule.Instance.Trash);

            if (trashAmount == 0)
            {
                dbPlayer.SendNewNotification("Du hast keinen Müll dabei den Ich entsorgen könnte!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                Chats.sendProgressBar(dbPlayer, PlanningModule.Instance.TimeToRecycleTrash / 10 * trashAmount);
                await Task.Delay(PlanningModule.Instance.TimeToRecycleTrash / 10 * trashAmount);

                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");
            }));

            dbPlayer.Container.RemoveItem(PlanningModule.Instance.Trash, trashAmount);

            MainFloorCleanup = (MainFloorCleanup - trashAmount) > 0 ? (MainFloorCleanup - trashAmount) : 0;

            if (MainFloorCleanup > 0)
            {
                dbPlayer.SendNewNotification($"Verbleibend: {MainFloorCleanup}, räume weiter auf!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.INFO);
            }
            else
            {
                dbPlayer.SendNewNotification("Ich denke das genügt mit dem aufräumen!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.SUCCESS);
                UpgradePlanningRoom(dbPlayer, 1, 1);

                PlayersInsideRoom.ForEach(player => LoadInterior(player));
            }

            Markers.Create(0, PlanningModule.Instance.PlanningRoomLaptopPosition, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 255, 0, TeamId, true);
            SavePlanningRoom();
        }

        public void RecyclePlanningRoomTrash(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.HasData("pressedEOnProcess")) return;

            int trashAmount = dbPlayer.Container.GetItemAmount(PlanningModule.Instance.Trash);

            if (trashAmount == 0)
            {
                dbPlayer.SendNewNotification("Du hast keinen Müll dabei der entsorgt werden könnte!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                Chats.sendProgressBar(dbPlayer, PlanningModule.Instance.TimeToRecycleTrash / 10 * trashAmount);
                await Task.Delay(PlanningModule.Instance.TimeToRecycleTrash / 10 * trashAmount);

                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");
            }));

            dbPlayer.Container.RemoveItem(PlanningModule.Instance.Trash, trashAmount);

            BasementCleanUp = (BasementCleanUp - trashAmount) > 0 ? (BasementCleanUp - trashAmount) : 0;

            if (BasementCleanUp > 0)
            {
                dbPlayer.SendNewNotification($"Verbleibend: {BasementCleanUp}, räume weiter auf!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.INFO);
            }
            else
            {
                dbPlayer.SendNewNotification("Ich denke das genügt mit dem aufräumen!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.SUCCESS);
                UpgradePlanningRoom(dbPlayer, 6, 1);

                PlayersInsideRoom.ForEach(player => LoadInterior(player));
            }

            SavePlanningRoom();
        }

        public void UpgradePlanningRoom(DbPlayer dbPlayer, int id, int index)
        {
            bool UpgradeStatus = false;

            switch (id)
            {
                case 1:
                    UpgradeStatus = UpgradeGrundraum(dbPlayer, index);
                    break;
                case 2:
                    UpgradeStatus = UpgradeSpiegel(dbPlayer, index);
                    break;
                case 3:
                    UpgradeStatus = UpgradeEinrichtungsstyle(dbPlayer, index);
                    break;
                case 4:
                    UpgradeStatus = UpgradeInneneinrichtung(dbPlayer, index);
                    break;
                case 5:
                    UpgradeStatus = UpgradeSpielautomaten(dbPlayer, index);
                    break;
                case 6:
                    UpgradeStatus = UpgradeKellerGrundraum(dbPlayer, index);
                    break;
                case 7:
                    UpgradeStatus = UpgradeMechanic(dbPlayer, index);
                    break;
                case 8:
                    UpgradeStatus = UpgradeHacker(dbPlayer, index);
                    break;
                case 9:
                    UpgradeStatus = UpgradeWeapons(dbPlayer, index);
                    break;
                case 10:
                    UpgradeStatus = UpgradeWardrobe(dbPlayer, index);
                    break;
                case 11:
                    UpgradeStatus = UpgradeCasinoPlan(dbPlayer, index);
                    break;
                case 12:
                    UpgradeStatus = UpgradeCasinoDoor(dbPlayer, index);
                    break;
                default:
                    break;
            }

            if (UpgradeStatus)
            {
                PlayersInsideRoom.ForEach(player => LoadInterior(player));
                SavePlanningRoom();
            }
        }

        private bool UpgradeCasinoDoor(DbPlayer dbPlayer, int index)
        {
            if (CasinoPropDoor == index)
            {
                dbPlayer.SendNewNotification($"Es wurde bereits ein Nachbau der Casino Tresortür an den Planningroom geliefert!", PlayerNotification.NotificationType.ERROR);
                return false;
            }

            CasinoPropDoor = index;

            dbPlayer.SendNewNotification($"Es wurde ein Nachbau der Casino Tresortür an den Planningroom geliefert!", PlayerNotification.NotificationType.SUCCESS);
            return true;
        }

        private bool UpgradeCasinoPlan(DbPlayer dbPlayer, int index)
        {
            if (CasinoPlanLevel == index)
            {
                dbPlayer.SendNewNotification($"Der Miniatur Bausatz wurde bereits an den Planningroom geliefert!", PlayerNotification.NotificationType.ERROR);
                return false;
            }

            CasinoPlanLevel = index;
            dbPlayer.SendNewNotification($"Es wurde ein Miniatur Bausatz an den Planningroom geliefert!", PlayerNotification.NotificationType.SUCCESS);
            return true;
        }

        private bool UpgradeHacker(DbPlayer dbPlayer, int index)
        {
            if (BasementHackerLevel == index) return false;

            if (BasementLevel == 2 && BasementHackerLevel == 0)
            {
                if (BasementHackerItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es wurden nocht nicht genuegend Elektronikteile verbaut!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            BasementHackerLevel = index;
            return true;
        }

        private bool UpgradeWeapons(DbPlayer dbPlayer, int index)
        {
            if (BasementWeaponsLevel == index) return false;

            if (BasementLevel == 2 && BasementWeaponsLevel == 0)
            {
                if (BasementWeaponsItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es wurden nocht nicht genuegend Eisenbarren verbaut!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            BasementWeaponsLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeWardrobe(DbPlayer dbPlayer, int index)
        {
            if (BasementWardrobeLevel == index) return false;

            if (BasementLevel == 2 && BasementWardrobeLevel == 0)
            {
                if (BasementKellerWardrobeItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es wurden nocht nicht genuegend Holzplanken verbaut!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            BasementWardrobeLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeMechanic(DbPlayer dbPlayer, int index)
        {
            if (BasementMechanicLevel == index) return false;

            if (BasementLevel == 2 && BasementMechanicLevel == 0)
            {
                if (BasementMachanicItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es wurden nocht nicht genuegend Eisenbarren verbaut!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            BasementMechanicLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeKellerGrundraum(DbPlayer dbPlayer, int index)
        {
            if (BasementLevel == index) return false;

            if (BasementLevel == 0)
            {
                if (BasementCleanUp != 0)
                {
                    dbPlayer.SendNewNotification("Es wurde noch nicht vollstaendig aufgeraeumt!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }
            else if (BasementLevel == 1)
            {
                if (BasementElectronicItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es wurden nocht nicht genuegend Elektronikteile verbaut!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            BasementLevel = index;
            CreateMarker();

            if (BasementLevel == 2)
            {
                ActivateVehicleJumppoint();
            }

            return true;
        }

        private bool UpgradeGrundraum(DbPlayer dbPlayer, int index)
        {
            if (MainFloor == index) return false;

            if (MainFloor == 1)
            {
                if (MainFloorItem1ItemCount != 0 || MainFloorItem2ItemCount != 0 || MainFloorItem3ItemCount != 0 || MainFloorItem4ItemCount != 0 || MainFloorItem5ItemCount != 0)
                {
                    dbPlayer.SendNewNotification("Es fehlen noch Materialien zum Ausbau!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            if (MainFloor >= 1 && DoorObject != null)
            {
                DoorObject.Delete();
            }

            DeleteMarker();
            MainFloor = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeSpiegel(DbPlayer dbPlayer, int index)
        {
            if (MainFloorMirrorLevel == index) return false;

            if (MainFloor >= 1 && MainFloorMirrorLevel == 0)
            {
                if (MainFloorMirrorItemCount != 0)
                {
                    dbPlayer.SendNewNotification($"Es fehlt noch Glas ({MainFloorMirrorItemCount} Stk.) zum Ausbau!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            MainFloorMirrorLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeInneneinrichtung(DbPlayer dbPlayer, int index)
        {
            if (MainFloorInteriorLevel == index) return false;

            if (MainFloor >= 1 && MainFloorInteriorLevel == 0)
            {
                if (MainFloorInteriorItmeCount != 0)
                {
                    dbPlayer.SendNewNotification($"Es fehlt noch Glas ({MainFloorInteriorItmeCount} Stk.) zum Ausbau!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            MainFloorInteriorLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeSpielautomaten(DbPlayer dbPlayer, int index)
        {
            if (MainFloorSlotMachineLevel == index) return false;

            if (MainFloor >= 1 && MainFloorSlotMachineLevel == 0)
            {
                if (MainFloorSlotMachineItemCount != 0)
                {
                    dbPlayer.SendNewNotification($"Es fehlt noch Glas ({MainFloorSlotMachineItemCount} Stk.) zum Ausbau!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }

            DeleteMarker();
            MainFloorSlotMachineLevel = index;
            CreateMarker();
            return true;
        }

        private bool UpgradeEinrichtungsstyle(DbPlayer dbPlayer, int index)
        {
            int itemAmount = dbPlayer.Container.GetItemAmount(PlanningModule.Instance.EinrichtungsstyleItem);

            if (itemAmount < PlanningModule.Instance.EinrichtungsstyleAmount)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Farbe dabei, um den Einrichtungsstyle zu ändern! {PlanningModule.Instance.EinrichtungsstyleAmount} benoetigt!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            dbPlayer.Container.RemoveItem(PlanningModule.Instance.EinrichtungsstyleItem, PlanningModule.Instance.EinrichtungsstyleAmount);
            MainFloorStyle = index;

            return true;
        }

        private void ActivatePlanningroomJumppoint()
        {
            JumpPoint jumpPoint = JumpPointModule.Instance.Get(JumpPointId);

            if (jumpPoint != null)
            {
                jumpPoint.Disabled = false;
                jumpPoint.HideInfos = false;
            }
        }

        private void ActivateVehicleJumppoint()
        {
            JumpPoint jumpPoint = JumpPointModule.Instance.Get(JumpPointVehicleId);

            if (jumpPoint != null)
            {
                jumpPoint.Disabled = false;
                jumpPoint.HideInfos = false;
            }
        }

        public void LoadInterior(DbPlayer dbPlayer)
        {
            int einrichtung = MainFloorStyle;
            int spiegel = MainFloorMirrorLevel;

            if (MainFloor == 1)
            {
                einrichtung = 0;
                spiegel = 0;
            }

            dbPlayer.Player.TriggerEvent("loadplanningroom", MainFloor, spiegel, einrichtung, MainFloorInteriorLevel, MainFloorSlotMachineLevel, NAPI.Util.ToJson(rewardIds), BasementLevel, BasementMechanicLevel, BasementHackerLevel, BasementWeaponsLevel, BasementWardrobeLevel, CasinoPlanLevel, CasinoPropDoor);
        }

        public void UnloadInterior(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("unloadplanningroom");
        }

        public void CreateMarker()
        {
            if (Bought && MainFloor == 0)
            {
                PlanningModule.Instance.PlanningRoomPickupTrashLocations.ForEach(location =>
                {
                    markers.Add(Markers.Create(27, location.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true));
                    textLabels.Add(NAPI.TextLabel.CreateTextLabel("Muellsaecke", location.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId));
                });
            }
            else if (MainFloor == 1)
            {
                if (GrundraumWoodMarker == null && GrundraumIronMarker == null && GrundraumCementMarker == null && GrundraumPlasticMarker == null && GrundraumGlasMarker == null && GrundraumWoodLabel == null && GrundraumIronLabel == null && GrundraumCementLabel == null && GrundraumPlasticLabel == null && GrundraumGlasLabel == null)
                {
                    GrundraumWoodMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomGrundraumWoodLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                    GrundraumIronMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomGrundraumIronLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                    GrundraumCementMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomGrundraumCementLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                    GrundraumPlasticMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomGrundraumPlasticLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                    GrundraumGlasMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomGrundraumGlasLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                    GrundraumWoodLabel = NAPI.TextLabel.CreateTextLabel("Holzplanken", PlanningModule.Instance.PlanningRoomGrundraumWoodLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    GrundraumIronLabel = NAPI.TextLabel.CreateTextLabel("Eisenbarren", PlanningModule.Instance.PlanningRoomGrundraumIronLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    GrundraumCementLabel = NAPI.TextLabel.CreateTextLabel("Zement", PlanningModule.Instance.PlanningRoomGrundraumCementLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    GrundraumPlasticLabel = NAPI.TextLabel.CreateTextLabel("Plastik", PlanningModule.Instance.PlanningRoomGrundraumPlasticLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    GrundraumGlasLabel = NAPI.TextLabel.CreateTextLabel("Glas", PlanningModule.Instance.PlanningRoomGrundraumGlasLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                }
            }
            else if (MainFloor > 1)
            {
                if (MainFloorMirrorLevel == 0)
                {
                    if (SpiegelGlasMarker == null && SpiegelGlasLabel == null)
                    {
                        SpiegelGlasMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomSpiegelLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                        SpiegelGlasLabel = NAPI.TextLabel.CreateTextLabel("Spiegelupgrade: Glas", PlanningModule.Instance.PlanningRoomSpiegelLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    }
                }

                if (MainFloorInteriorLevel == 0)
                {
                    if (InneneinrichtungGlasMarker == null && InneneinrichtungGlasLabel == null)
                    {
                        InneneinrichtungGlasMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomInneneinrichtungLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                        InneneinrichtungGlasLabel = NAPI.TextLabel.CreateTextLabel("Inneneinrichtungsupgrade: Holzplanken", PlanningModule.Instance.PlanningRoomInneneinrichtungLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    }
                }

                if (MainFloorSlotMachineLevel == 0)
                {
                    if (SpielautomatenGlasMarker == null && SpielautomatenGlasLabel == null)
                    {
                        SpielautomatenGlasMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomSpielautomatenLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                        SpielautomatenGlasLabel = NAPI.TextLabel.CreateTextLabel("Spielautomatenupgrade: Spielautomat Baukaesten", PlanningModule.Instance.PlanningRoomSpielautomatenLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    }
                }

                if (BasementLevel == 0)
                {
                    if (markers.Count() == 0 && textLabels.Count() == 0)
                    {
                        PlanningModule.Instance.PlanningRoomCleanupKellerLocations.ForEach(location =>
                        {
                            markers.Add(Markers.Create(27, location.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true));
                            textLabels.Add(NAPI.TextLabel.CreateTextLabel("Muellsaecke", location.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId));
                        });
                    }
                }
                if (BasementLevel == 1)
                {
                    if (KellerElectronicMarker == null && KellerElectronicLabel == null)
                    {
                        KellerElectronicMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomKellerElectronicLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                        KellerElectronicLabel = NAPI.TextLabel.CreateTextLabel("Elektronik", PlanningModule.Instance.PlanningRoomKellerElectronicLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                    }
                }
                if (BasementLevel > 1)
                {
                    if (BasementMarker == null)
                    {
                        NAPI.Task.Run(() =>
                        {
                            BasementMarker = Markers.Create(0, PlanningModule.Instance.PlanningRoomKellerLaptopLocation, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 255, 0, TeamId, true);
                        });
                    }

                    if (BasementMechanicLevel == 0)
                    {
                        if (KellerMechanicMarker == null && KellerMechanicLabel == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerMechanicMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomKellerMechanicUpdateLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                                KellerMechanicLabel = NAPI.TextLabel.CreateTextLabel("Mechanikerupgrade: Eisenbarren", PlanningModule.Instance.PlanningRoomKellerMechanicUpdateLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                            });
                        }
                    }

                    if (BasementHackerLevel == 0)
                    {
                        if (KellerHackerMarker == null && KellerHackerLabel == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerHackerMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomKellerHackerUpdateLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                                KellerHackerLabel = NAPI.TextLabel.CreateTextLabel("Hackerupgrade: Elektronik", PlanningModule.Instance.PlanningRoomKellerHackerUpdateLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                            });
                        }
                    }

                    if (BasementWeaponsLevel == 0)
                    {
                        if (KellerWeaponMarker == null && KellerWeaponLabel == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerWeaponMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomKellerWeaponUpdateLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                                KellerWeaponLabel = NAPI.TextLabel.CreateTextLabel("Waffenupgrade: Eisenbarren", PlanningModule.Instance.PlanningRoomKellerWeaponUpdateLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                            });
                        }
                    }

                    if (BasementWardrobeLevel == 0)
                    {
                        if (KellerWardrobeMarker == null && KellerWardrobeLabel == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerWardrobeMarker = Markers.Create(27, PlanningModule.Instance.PlanningRoomKellerWardrobeUpdateLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, TeamId, true);
                                KellerWardrobeLabel = NAPI.TextLabel.CreateTextLabel("Umkleideupgrade: Holzplanken", PlanningModule.Instance.PlanningRoomKellerWardrobeUpdateLocation.Add(new Vector3(0, 0, -0.5f)), 5.0f, 1.0f, 0, new Color(255, 0, 0, 255), false, TeamId);
                            });
                        }
                    }

                    // Mechanic done
                    if (BasementMechanicLevel == 1)
                    {
                        if (KellerMechanicFinalMarker == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerMechanicFinalMarker = Markers.Create(1, PlanningModule.Instance.PlanningroomTuningLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                                KellerMechanicGarageMarker = Markers.Create(1, PlanningModule.Instance.PlanningroomGarageLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                            });
                        }
                    }

                    // Wardrobe done
                    if (BasementWardrobeLevel == 1)
                    {
                        if (KellerWadrobeFinalMarker == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerWadrobeFinalMarker = Markers.Create(1, PlanningModule.Instance.PlanningroomWadrobeLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                            });
                        }
                    }

                    // Weapons done
                    if (BasementWeaponsLevel == 1)
                    {
                        if (KellerWeaponFinalMarker == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerWeaponFinalMarker = Markers.Create(1, PlanningModule.Instance.PlanningRoomWeaponLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                            });
                        }
                    }

                    // Final markers everything unlocked
                    if (BasementMechanicLevel == 1 && BasementHackerLevel == 1 && BasementWeaponsLevel == 1 && BasementWardrobeLevel == 1)
                    {
                        if (KellerPreQuestMarker == null && KellerHeistQuestMarker == null)
                        {
                            NAPI.Task.Run(() =>
                            {
                                KellerPreQuestMarker = Markers.Create(1, PlanningModule.Instance.PlanningroomPreQuestLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                                KellerHeistQuestMarker = Markers.Create(1, PlanningModule.Instance.PlanningroomHeistQuestLocation.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 50, 255, 255, 255, TeamId, true);
                            });
                        }
                    }
                }
            }
        }

        public void DeleteMarker()
        {
            if (Bought && MainFloor == 0)
            {
                markers.ForEach(marker => marker.Delete());
                markers.Clear();
                textLabels.ForEach(label => label.Delete());
                textLabels.Clear();
            }
            else if (MainFloor == 1)
            {
                NAPI.Task.Run(() =>
                {
                    GrundraumWoodMarker.Delete();
                    GrundraumIronMarker.Delete();
                    GrundraumCementMarker.Delete();
                    GrundraumPlasticMarker.Delete();
                    GrundraumGlasMarker.Delete();
                    GrundraumWoodLabel.Delete();
                    GrundraumIronLabel.Delete();
                    GrundraumCementLabel.Delete();
                    GrundraumPlasticLabel.Delete();
                    GrundraumGlasLabel.Delete();

                    GrundraumWoodMarker = null;
                    GrundraumIronMarker = null;
                    GrundraumCementMarker = null;
                    GrundraumPlasticMarker = null;
                    GrundraumGlasMarker = null;
                    GrundraumWoodLabel = null;
                    GrundraumIronLabel = null;
                    GrundraumCementLabel = null;
                    GrundraumPlasticLabel = null;
                    GrundraumGlasLabel = null;
                });
            }
            else if (MainFloor > 1)
            {
                if (MainFloorMirrorLevel == 0 && MainFloorMirrorItemCount == 0)
                {
                    SpiegelGlasMarker.Delete();
                    SpiegelGlasLabel.Delete();

                    SpiegelGlasMarker = null;
                    SpiegelGlasLabel = null;
                }

                if (MainFloorInteriorLevel == 0 && MainFloorInteriorItmeCount == 0)
                {
                    InneneinrichtungGlasMarker.Delete();
                    InneneinrichtungGlasLabel.Delete();

                    InneneinrichtungGlasMarker = null;
                    InneneinrichtungGlasLabel = null;
                }

                if (MainFloorSlotMachineLevel == 0 && MainFloorSlotMachineItemCount == 0)
                {
                    SpielautomatenGlasMarker.Delete();
                    SpielautomatenGlasLabel.Delete();

                    SpielautomatenGlasMarker = null;
                    SpielautomatenGlasLabel = null;
                }

                if (BasementLevel == 0)
                {
                    markers.ForEach(marker => marker.Delete());
                    markers.Clear();
                    textLabels.ForEach(label => label.Delete());
                    textLabels.Clear();
                }

                if (BasementLevel == 1)
                {
                    NAPI.Task.Run(() =>
                    {
                        if (KellerElectronicMarker != null && KellerElectronicLabel != null)
                        {
                            KellerElectronicMarker.Delete();
                            KellerElectronicLabel.Delete();

                            KellerElectronicMarker = null;
                            KellerElectronicLabel = null;
                        }
                    });
                }

                if (BasementLevel > 1)
                {
                    if (BasementMechanicLevel == 0 && BasementMachanicItemCount == 0)
                    {
                        if (KellerMechanicMarker != null)
                            KellerMechanicMarker.Delete();
                        if (KellerMechanicLabel != null)
                            KellerMechanicLabel.Delete();

                        KellerMechanicMarker = null;
                        KellerMechanicLabel = null;
                    }
                    if (BasementHackerLevel == 0 && BasementHackerItemCount == 0)
                    {
                        if (KellerHackerMarker != null)
                            KellerHackerMarker.Delete();
                        if (KellerHackerLabel != null)
                            KellerHackerLabel.Delete();

                        KellerHackerMarker = null;
                        KellerHackerLabel = null;
                    }
                    if (BasementWeaponsLevel == 0 && BasementWeaponsItemCount == 0)
                    {
                        if (KellerWeaponMarker != null)
                            KellerWeaponMarker.Delete();
                        if (KellerWeaponLabel != null)
                            KellerWeaponLabel.Delete();

                        KellerWeaponMarker = null;
                        KellerWeaponLabel = null;
                    }
                    if (BasementWardrobeLevel == 0 && BasementKellerWardrobeItemCount == 0)
                    {
                        if (KellerWardrobeMarker != null)
                            KellerWardrobeMarker.Delete();
                        if (KellerWardrobeLabel != null)
                            KellerWardrobeLabel.Delete();

                        KellerWardrobeMarker = null;
                        KellerWardrobeLabel = null;
                    }
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
