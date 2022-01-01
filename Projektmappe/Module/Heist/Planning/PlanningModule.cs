using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Menu.Menus.Heists.Planningroom;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Teams;

namespace GVRP.Module.Heist.Planning
{
    public class PlanningModule : SqlModule<PlanningModule, PlanningRoom, uint>
    {
        public Vector3 PlanningRoomSellNpcLocation = new Vector3(-142.817, -954.701, 114.136);
        public Vector3 ElectroExchangeLocation = new Vector3(756.185, -557.933, 33.6427);
        public Vector3 TrashworkerLocation = new Vector3(-529.966, -1708.55, 19.3229);
        public Vector3 PlanningroomTuningLocation = new Vector3(2687.17, -355.593, -55.1867);
        public Vector3 PlanningroomGarageLocation = new Vector3(2714.42, -384.578, -55.3049);
        public Vector3 PlanningroomPreQuestLocation = new Vector3(2712.94, -371.656, -54.7809);
        public Vector3 PlanningroomHeistQuestLocation = new Vector3(2715.45, -369.56, -54.7809);
        public Vector3 PlanningroomWadrobeLocation = new Vector3(2722.4, -370.702, -55.3809);
        public Vector3 PlanningRoomWeaponLocation = new Vector3(2713.84, -354.551, -55.1867);

        public Vector3 PlanningRoomLaptopPosition = new Vector3(2726.65f, -377.323f, -47.393f);
        public List<Vector3> PlanningRoomPickupTrashLocations = new List<Vector3>
        {
            new Vector3(2733.03, -383.504, -48.993),
            new Vector3(2725.06, -380.709, -48.9742),
            new Vector3(2731.33, -375.105, -48.9929),
        };
        public List<Vector3> PlanningRoomCleanupKellerLocations = new List<Vector3>
        {
            new Vector3(2712.86, -362.549, -55.1867),
            new Vector3(2697.19, -354.979, -55.1867),
            new Vector3(2689.64, -355.624, -55.1867),
            new Vector3(2688.06, -367.68, -54.7326),
        };

        public Vector3 PlanningRoomGrundraumWoodLocation = new Vector3(2726.34, -386.466, -48.993);
        public Vector3 PlanningRoomGrundraumIronLocation = new Vector3(2733.89, -385.291, -48.993);
        public Vector3 PlanningRoomGrundraumCementLocation = new Vector3(2732.6, -372.028, -48.993);
        public Vector3 PlanningRoomGrundraumPlasticLocation = new Vector3(2733.43, -390.971, -48.395);
        public Vector3 PlanningRoomGrundraumGlasLocation = new Vector3(2724.35, -377.463, -47.3999);
        public Vector3 PlanningRoomSpiegelLocation = new Vector3(2723.22, -380.869, -48.993);
        public Vector3 PlanningRoomInneneinrichtungLocation = new Vector3(2739.5, -381.309, -48.3951);
        public Vector3 PlanningRoomSpielautomatenLocation = new Vector3(2730.32, -383.426, -48.993);
        public Vector3 PlanningRoomKellerElectronicLocation = new Vector3(2719.32, -362.743, -53.5868);
        public Vector3 PlanningRoomKellerLaptopLocation = new Vector3(2719.32, -362.743, -53.5868);
        public Vector3 PlanningRoomKellerMechanicUpdateLocation = new Vector3(2689.27, -354.174, -55.1867);
        public Vector3 PlanningRoomKellerHackerUpdateLocation = new Vector3(2712.83, -354.098, -55.1867);
        public Vector3 PlanningRoomKellerWeaponUpdateLocation = new Vector3(2696.74, -354.117, -55.1867);
        public Vector3 PlanningRoomKellerWardrobeUpdateLocation = new Vector3(2704.66, -354.001, -55.1867);
        public Vector3 PlanningRoomCraftingLocation = new Vector3(1109.7, -2008.0, 31.05);

        public int TrashMaximumCarryAmount = 10;
        public int EinrichtungsstyleAmount = 20;

        public int TimeToPickupTrash = 60000;
        public int TimeToPickupKellerTrash = 60000;
        public int TimeToGrundraumWood = 72000;
        public int TimeToGrundraumIron = 192000;
        public int TimeToGrundraumCement = 96000;
        public int TimeToGrundraumPlastic = 192000;
        public int TimeToGrundraumGlas = 192000;
        public int TimeToSpiegelGlas = 216000;
        public int TimeToInneneinrichtungGlas = 144000;
        public int TimeToSpielautomatenGlas = 480000;
        public int TimeToKellerElectronic = 216000;

        public int TimeToKellerMechanic = 144000;
        public int TimeToKellerHacker = 108000;
        public int TimeToKellerWeapons = 144000;
        public int TimeToKellerWardrobe = 48000;
        public int TimeToRecycleTrash = 60000;

        public uint Trash = 940;
        public uint GrundraumWoodItem = 310;
        public uint GrundraumIronItem = 300;
        public uint GrundraumCementItem = 312;
        public uint GrundraumPlasticItem = 468;
        public uint GrundraumGlasItem = 466;
        public uint SpiegelGlasItem = 466;
        public uint InneneinrichtungGlasItem = 310;
        public uint SpielautomatenGlasItem = 967;
        public uint EinrichtungsstyleItem = 941;
        public uint KellerElectronicItem = 968;
        public uint KellerMechanicItem = 300;
        public uint KellerHackerItem = 968;
        public uint KellerWeaponItem = 300;
        public uint KellerWardrobeItem = 310;

        public uint Gold = 487;
        public uint Bronce = 464;
        public uint Aluminium = 462;
        public uint Batterien = 15;
        public uint Alteisen = 601;

        public uint CasinoRequiredOutfitId = 1018;
        public int CasinoRequiredOutfitAmount = 6;
        public string CasinoRequiredVehicleModel = "529";

        public DateTime LastOutfitRob;

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_planningrooms`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }

        protected override void OnLoaded()
        {
            new Npc(Enum.TryParse("cs_lestercrest", true, out PedHash skin) ? skin : PedHash.LesterCrest, PlanningRoomSellNpcLocation, 32.2868f, 0);
            new Npc(Enum.TryParse("a_m_m_genfat_02", true, out PedHash vendor) ? vendor : PedHash.Genfat01AMM, ElectroExchangeLocation, 258.878f, 0);
            new Npc(Enum.TryParse("s_m_y_dockwork_01", true, out PedHash trash) ? trash : PedHash.Dockwork01SMM, TrashworkerLocation, 236.298f, 0);
            new Npc(Enum.TryParse("s_m_y_dockwork_01", true, out PedHash crafting) ? crafting : PedHash.Dockwork01SMM, PlanningRoomCraftingLocation, 52.817f, 0);

            MenuManager.Instance.AddBuilder(new PlanningroomPurchaseMenBuilder());
            MenuManager.Instance.AddBuilder(new PlanningroomUpgradeMenuBuilder());
            MenuManager.Instance.AddBuilder(new PlanningroomUpgradeSelectionMenuBuilder());
            MenuManager.Instance.AddBuilder(new PlanningroomKellerUpgradeMenu());
            MenuManager.Instance.AddBuilder(new ExchangeElectronicMenu());
            MenuManager.Instance.AddBuilder(new ExchangeTrashMenu());
            MenuManager.Instance.AddBuilder(new PlanningroomVehicleModifyMenuBuilder());
            MenuManager.Instance.AddBuilder(new PlanningroomVehicleTuningMenuBuilder());
            MenuManager.Instance.AddBuilder(new PlanningroomHeistMenu());
            MenuManager.Instance.AddBuilder(new PlanningroomPreQuestMenu());
            MenuManager.Instance.AddBuilder(new PlanningroomPreQuestSelectionMenu());
            MenuManager.Instance.AddBuilder(new PlanningroomCraftingMenu());

            if (Configurations.Configuration.Instance.DevMode)
            {
                TimeToPickupTrash = 5000;
                TimeToGrundraumWood = 5000;
                TimeToGrundraumIron = 5000;
                TimeToGrundraumCement = 5000;
                TimeToGrundraumPlastic = 5000;
                TimeToGrundraumGlas = 5000;
                TimeToSpiegelGlas = 5000;
                TimeToInneneinrichtungGlas = 5000;
                TimeToSpielautomatenGlas = 5000;
                TimeToPickupKellerTrash = 5000;
                TimeToKellerElectronic = 5000;
                TimeToKellerMechanic = 5000;
                TimeToKellerHacker = 5000;
                TimeToKellerWeapons = 5000;
                TimeToKellerWardrobe = 5000;
                TimeToRecycleTrash = 5000;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (!dbPlayer.IsAGangster()) return false;
            if (key != Key.E) return false;

            PlanningRoom room = Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);
            if (room == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(PlanningRoomCraftingLocation) < 2.0f)
            {
                if (room.CasinoPlanLevel == 1)
                {
                    return CraftingMenu(dbPlayer);
                }
                else
                {
                    dbPlayer.SendNewNotification($"Aktuell habe Ich noch keine Informationen für dich!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }
            else if (dbPlayer.Player.Position.DistanceTo(room.ContactPersonPosition) < 2.0f)
            {
                if (room.BasementMechanicLevel == 1 && room.BasementWardrobeLevel == 1 && room.BasementWeaponsLevel == 1 && room.BasementWeaponsLevel == 1)
                {
                    room.UpgradePlanningRoom(dbPlayer, 11, 1);
                    return true;
                }
                else
                {
                    dbPlayer.SendNewNotification($"Aktuell habe Ich noch keine Informationen für dich!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }
            }
            if (dbPlayer.Player.Position.DistanceTo(PlanningroomHeistQuestLocation) < 2.0f && room.BasementMechanicLevel == 1 && room.BasementWardrobeLevel == 1 && room.BasementWeaponsLevel == 1 && room.BasementWeaponsLevel == 1)
            {
                return HeistMenu(dbPlayer);
            }
            else if (dbPlayer.Player.Position.DistanceTo(PlanningroomPreQuestLocation) < 2.0f && room.BasementMechanicLevel == 1 && room.BasementWardrobeLevel == 1 && room.BasementWeaponsLevel == 1 && room.BasementWeaponsLevel == 1)
            {
                return PreQuestMenu(dbPlayer);
            }
            else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomWeaponLocation) < 2.0f && room.BasementWeaponsLevel == 1)
            {
                dbPlayer.SendNewNotification($"Aktuell habe Ich noch keine Informationen für dich!", PlayerNotification.NotificationType.ERROR);
                return false;
            }
            else if (dbPlayer.Player.Position.DistanceTo(PlanningroomTuningLocation) < 2.0f && room.BasementMechanicLevel == 1)
            {
                return PlanningroomVehicleTuningMenu(dbPlayer);
            }
            else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomSellNpcLocation) <= 2.0f)
            {
                return SellRoomNpc(dbPlayer, room);
            }
            else if (dbPlayer.Player.Position.DistanceTo(ElectroExchangeLocation) <= 2.0f)
            {
                return ExchangeElectronicMenu(dbPlayer);
            }
            else if (dbPlayer.Player.Position.DistanceTo(TrashworkerLocation) <= 2.0f)
            {
                if (room.Bought && (room.MainFloor == 0 && room.MainFloorCleanup != 0) || (room.BasementLevel == 0 && room.BasementCleanUp != 0))
                {
                    return ExchangeTrashMenu(dbPlayer);
                }
                else
                {
                    dbPlayer.SendNewNotification("Ich nehme aktuell keinen Muell entgegen!");
                    return false;
                }
            }
            else if (room.Bought)
            {
                bool actionDid = false;

                if (room.MainFloor == 0 && room.MainFloorCleanup != 0)
                {
                    actionDid = RaumAufräumen(dbPlayer, room);
                }
                else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomLaptopPosition) <= 2f && room.Bought && room.MainFloor >= 1)
                {
                    actionDid = RaumUpgradeComputer(dbPlayer, room);
                }
                else if (room.MainFloor == 1)
                {
                    actionDid = RaumAusbauen(dbPlayer, room);
                }

                if (!actionDid)
                {
                    if (room.BasementLevel == 0 && room.BasementCleanUp != 0)
                    {
                        actionDid = KellerAufräumen(dbPlayer, room);
                    }
                    else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerLaptopLocation) <= 2f && room.BasementLevel == 2)
                    {
                        actionDid = KellerUpgradeComputer(dbPlayer, room);
                    }
                }

                if (!actionDid && room.MainFloor > 1)
                {
                    actionDid = PlanningRoomAusbauen(dbPlayer, room);
                }
                

                return actionDid;
            }

            return false;
        }

        public bool ExchangeElectronicItem(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return false;
            if (dbPlayer.Player.IsInVehicle) return false;

            int itemCountGold = dbPlayer.Container.GetItemAmount(Gold);
            int itemCountBronce = dbPlayer.Container.GetItemAmount(Bronce);
            int itemCountAluminium = dbPlayer.Container.GetItemAmount(Aluminium);
            int itemCountBatterien = dbPlayer.Container.GetItemAmount(Batterien);
            int itemCountAlteisen = dbPlayer.Container.GetItemAmount(Alteisen);

            if (itemCountGold >= 1 && itemCountBronce >= 4 && itemCountAluminium >= 3 && itemCountBatterien >= 25 && itemCountAlteisen >= 8)
            {
                if (dbPlayer.Container.CanInventoryItemAdded(KellerElectronicItem, 1))
                {
                    dbPlayer.Container.RemoveItem(Gold, 1);
                    dbPlayer.Container.RemoveItem(Bronce, 4);
                    dbPlayer.Container.RemoveItem(Aluminium, 3);
                    dbPlayer.Container.RemoveItem(Batterien, 25);
                    dbPlayer.Container.RemoveItem(Alteisen, 8);

                    dbPlayer.Container.AddItem(KellerElectronicItem, 1);
                    dbPlayer.SendNewNotification("Materialien wurden erfolgreich umgewandelt!", PlayerNotification.NotificationType.INFO);
                    return true;
                }
                else
                {
                    dbPlayer.SendNewNotification("Nicht genügend Platz!", PlayerNotification.NotificationType.INFO);
                    return false;
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht die benoetigten Materialien dabei!");
                dbPlayer.SendNewNotification("Benoetigt: Goldbaren: 1, Bronze: 4, Aluminium: 3, Batterien: 25, Alteisen: 8");
                return false;
            }
        }

        private bool PlanningRoomAusbauen(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("inPlanningRoom") && room.TeamId == dbPlayer.TeamId)
            {
                if (room.MainFloorMirrorLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomSpiegelLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    RaumSpiegel(dbPlayer, room);
                }
                else if (room.MainFloorInteriorLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomInneneinrichtungLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    RaumInneneinrichtung(dbPlayer, room);
                }
                else if (room.MainFloorSlotMachineLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomSpielautomatenLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    RaumSpieleautomaten(dbPlayer, room);
                }
                else if (room.BasementLevel == 1 && dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerElectronicLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    KellerElectronic(dbPlayer, room);
                }
                else if (room.BasementLevel == 2 && room.BasementMechanicLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerMechanicUpdateLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    KellerMechanic(dbPlayer, room);
                }
                else if (room.BasementLevel == 2 && room.BasementHackerLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerHackerUpdateLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    KellerHacker(dbPlayer, room);
                }
                else if (room.BasementLevel == 2 && room.BasementWeaponsLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerWeaponUpdateLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    KellerWeapons(dbPlayer, room);
                }
                else if (room.BasementLevel == 2 && room.BasementWardrobeLevel == 0 && dbPlayer.Player.Position.DistanceTo(PlanningRoomKellerWardrobeUpdateLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                {
                    KellerWardrobe(dbPlayer, room);
                }
            }

            return false;
        }

        private bool ExchangeTrashMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.ExchangeTrashMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool ExchangeElectronicMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.ExchangeElectronicMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool CraftingMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.PlanningroomCraftingMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool HeistMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.PlanningroomHeistMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool PreQuestMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.PlanningroomPreQuestMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool PlanningroomVehicleTuningMenu(DbPlayer dbPlayer)
        {
            MenuManager.Instance.Build(PlayerMenu.PlanningroomVehicleModifyMenu, dbPlayer).Show(dbPlayer);
            return false;
        }

        private bool SellRoomNpc(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (room.Bought)
            {
                dbPlayer.SendNewNotification("Ich habe dir nichts mehr zu sagen!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }
            else
            {
                MenuManager.Instance.Build(PlayerMenu.PlanningroomPurchaseMenu, dbPlayer).Show(dbPlayer);
                MenuManager.Instance.Build(PlayerMenu.PlanningroomPurchaseMenu, dbPlayer).Show(dbPlayer);
                return false;
            }
        }

        private bool RaumAufräumen(DbPlayer dbPlayer, PlanningRoom room)
        {
            foreach (Vector3 position in PlanningRoomPickupTrashLocations)
            {
                if (dbPlayer.Player.Position.DistanceTo(position) <= 2f)
                {
                    if (dbPlayer.HasData("inPlanningRoom"))
                    {
                        if (dbPlayer.HasData("pressedEOnProcess")) return false;

                        if (!dbPlayer.Container.CanInventoryItemAdded(Trash))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genügend Platz!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        if (dbPlayer.Container.GetItemAmount(Trash) >= TrashMaximumCarryAmount)
                        {
                            dbPlayer.SendNewNotification("Du trägst bereits zu viel Müll bei dir, entsorge diesen zuerst!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", TrashworkerLocation.X, TrashworkerLocation.Y);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                            Chats.sendProgressBar(dbPlayer, TimeToPickupTrash);
                            await Task.Delay(TimeToPickupTrash);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");
                        }));

                        dbPlayer.Container.AddItem(Trash, 1);
                        return true;
                    }
                }
            }

            return false;
        }

        private bool RaumAusbauen(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("inPlanningRoom"))
            {
                if (room.TeamId == dbPlayer.TeamId)
                {
                    if (dbPlayer.Player.Position.DistanceTo(PlanningRoomGrundraumWoodLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                    {
                        if (room.MainFloorItem1ItemCount == 0)
                        {
                            dbPlayer.SendNewNotification("Es ist bereits genügend Holz vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        int amountNeeded = 1;

                        if (room.MainFloorItem1ItemCount < amountNeeded)
                        {
                            amountNeeded = room.MainFloorItem1ItemCount;
                        }

                        if (dbPlayer.Container.GetItemAmount(GrundraumWoodItem) < amountNeeded)
                        {
                            dbPlayer.SendNewNotification($"Du hast nicht genügend Holzplanken bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                            Chats.sendProgressBar(dbPlayer, TimeToGrundraumWood);
                            await Task.Delay(TimeToGrundraumWood);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");

                            if (room.MainFloorItem1ItemCount <= amountNeeded)
                            {
                                dbPlayer.Container.RemoveItem(GrundraumWoodItem, room.MainFloorItem1ItemCount);
                                dbPlayer.SendNewNotification("Es wird kein weiteres Holz benötigt!");
                                room.MainFloorItem1ItemCount = 0;
                            }
                            else
                            {
                                room.MainFloorItem1ItemCount -= amountNeeded;
                                dbPlayer.SendNewNotification($"{room.MainFloorItem1ItemCount} Holzplanken verbleibend");
                                dbPlayer.Container.RemoveItem(GrundraumWoodItem, amountNeeded);
                            }

                            room.SavePlanningRoom();
                        }));

                        return true;
                    }
                    else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomGrundraumIronLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                    {
                        if (room.MainFloorItem2ItemCount == 0)
                        {
                            dbPlayer.SendNewNotification("Es ist bereits genügend Eisen vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        int amountNeeded = 1;

                        if (room.MainFloorItem2ItemCount < amountNeeded)
                        {
                            amountNeeded = room.MainFloorItem2ItemCount;
                        }

                        if (dbPlayer.Container.GetItemAmount(GrundraumIronItem) < amountNeeded)
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genügend Eisenbarren (min. 1) bei dir!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                            Chats.sendProgressBar(dbPlayer, TimeToGrundraumIron);
                            await Task.Delay(TimeToGrundraumIron);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");

                            if (room.MainFloorItem2ItemCount <= amountNeeded)
                            {
                                dbPlayer.Container.RemoveItem(GrundraumIronItem, room.MainFloorItem2ItemCount);
                                dbPlayer.SendNewNotification("Es werden keine weiteren Eisenbarren benötigt!");
                                room.MainFloorItem2ItemCount = 0;
                            }
                            else
                            {
                                room.MainFloorItem2ItemCount -= amountNeeded;
                                dbPlayer.SendNewNotification($"{room.MainFloorItem2ItemCount} Eisenbarren verbleibend");
                                dbPlayer.Container.RemoveItem(GrundraumIronItem, amountNeeded);
                            }

                            room.SavePlanningRoom();
                        }));

                        return true;
                    }
                    else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomGrundraumCementLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                    {
                        if (room.MainFloorItem3ItemCount == 0)
                        {
                            dbPlayer.SendNewNotification("Es ist bereits genügend Zement vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        int amountNeeded = 1;

                        if (room.MainFloorItem3ItemCount < amountNeeded)
                        {
                            amountNeeded = room.MainFloorItem3ItemCount;
                        }

                        if (dbPlayer.Container.GetItemAmount(GrundraumCementItem) < amountNeeded)
                        {
                            dbPlayer.SendNewNotification($"Du hast nicht genügend Zement (min. {amountNeeded} Stück) bei dir!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_const_drill@male@drill@base", "base");

                            Chats.sendProgressBar(dbPlayer, TimeToGrundraumCement);
                            await Task.Delay(TimeToGrundraumCement);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");

                            if (room.MainFloorItem3ItemCount <= amountNeeded)
                            {
                                dbPlayer.Container.RemoveItem(GrundraumCementItem, room.MainFloorItem3ItemCount);
                                dbPlayer.SendNewNotification("Es wird kein weiteres Zement benötigt!");
                                room.MainFloorItem3ItemCount = 0;
                            }
                            else
                            {
                                room.MainFloorItem3ItemCount -= amountNeeded;
                                dbPlayer.SendNewNotification($"{room.MainFloorItem3ItemCount} Zement verbleibend");
                                dbPlayer.Container.RemoveItem(GrundraumCementItem, amountNeeded);
                            }

                            room.SavePlanningRoom();
                        }));

                        return true;
                    }
                    else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomGrundraumPlasticLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                    {
                        if (room.MainFloorItem4ItemCount == 0)
                        {
                            dbPlayer.SendNewNotification("Es ist bereits genügend Plastik vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        int amountNeeded = 1;

                        if (room.MainFloorItem4ItemCount < amountNeeded)
                        {
                            amountNeeded = room.MainFloorItem4ItemCount;
                        }

                        if (dbPlayer.Container.GetItemAmount(GrundraumPlasticItem) < amountNeeded)
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genügend Plastik (min. 1) bei dir!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                            Chats.sendProgressBar(dbPlayer, TimeToGrundraumPlastic);
                            await Task.Delay(TimeToGrundraumPlastic);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");

                            if (room.MainFloorItem4ItemCount <= amountNeeded)
                            {
                                dbPlayer.Container.RemoveItem(GrundraumPlasticItem, room.MainFloorItem4ItemCount);
                                dbPlayer.SendNewNotification("Es wird kein weiteres Plastik benötigt!");
                                room.MainFloorItem4ItemCount = 0;
                            }
                            else
                            {
                                room.MainFloorItem4ItemCount -= amountNeeded;
                                dbPlayer.SendNewNotification($"{room.MainFloorItem4ItemCount} Plastik verbleibend");
                                dbPlayer.Container.RemoveItem(GrundraumPlasticItem, amountNeeded);
                            }

                            room.SavePlanningRoom();
                        }));
                        return true;
                    }
                    else if (dbPlayer.Player.Position.DistanceTo(PlanningRoomGrundraumGlasLocation) < 2.0f && !dbPlayer.HasData("pressedEOnProcess"))
                    {
                        if (room.MainFloorItem5ItemCount == 0)
                        {
                            dbPlayer.SendNewNotification("Es ist bereits genügend Glas vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }
                        int amountNeeded = 1;

                        if (room.MainFloorItem5ItemCount < amountNeeded)
                        {
                            amountNeeded = room.MainFloorItem5ItemCount;
                        }

                        if (dbPlayer.Container.GetItemAmount(GrundraumGlasItem) < amountNeeded)
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genügend Glas (min. 1) bei dir!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");

                            Chats.sendProgressBar(dbPlayer, TimeToGrundraumGlas);
                            await Task.Delay(TimeToGrundraumGlas);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");

                            if (room.MainFloorItem5ItemCount <= amountNeeded)
                            {
                                dbPlayer.Container.RemoveItem(GrundraumGlasItem, room.MainFloorItem5ItemCount);
                                dbPlayer.SendNewNotification("Es wird kein weiteres Glas benötigt!");
                                room.MainFloorItem5ItemCount = 0;
                            }
                            else
                            {
                                room.MainFloorItem5ItemCount -= amountNeeded;
                                dbPlayer.SendNewNotification($"{room.MainFloorItem5ItemCount} Glas verbleibend");
                                dbPlayer.Container.RemoveItem(GrundraumGlasItem, amountNeeded);
                            }

                            room.SavePlanningRoom();
                        }));

                        return true;
                    }
                }
            }
            return false;
        }

        private bool RaumSpiegel(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.MainFloorMirrorItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es ist bereits genügend Glas vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.MainFloorMirrorItemCount < amountNeeded)
            {
                amountNeeded = room.MainFloorMirrorItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(SpiegelGlasItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Glas bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToSpiegelGlas);
                await Task.Delay(TimeToSpiegelGlas);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.MainFloorMirrorItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(SpiegelGlasItem, room.MainFloorMirrorItemCount);
                    dbPlayer.SendNewNotification("Spiegelupgrade: Es wird kein weiteres Glas benötigt!");
                    room.MainFloorMirrorItemCount = 0;
                }
                else
                {
                    room.MainFloorMirrorItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Spiegelupgrade: {room.MainFloorMirrorItemCount} Glas verbleibend");
                    dbPlayer.Container.RemoveItem(SpiegelGlasItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerWeapons(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.BasementWeaponsItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genügend Eisenbarren vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.BasementWeaponsItemCount < amountNeeded)
            {
                amountNeeded = room.BasementWeaponsItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(KellerWeaponItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast keine Eisenbarren bei dir! (min. {amountNeeded} benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToKellerWeapons);
                await Task.Delay(TimeToKellerWeapons);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.BasementWeaponsItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(KellerWeaponItem, room.BasementWeaponsItemCount);
                    dbPlayer.SendNewNotification("Waffenupgrade: Es werden keine weiteren Eisenbarren benötigt!");
                    room.BasementWeaponsItemCount = 0;
                }
                else
                {
                    room.BasementWeaponsItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Waffenupgrade: {room.BasementWeaponsItemCount} Eisenbarren verbleibend");
                    dbPlayer.Container.RemoveItem(KellerWeaponItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerWardrobe(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.BasementKellerWardrobeItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genügend Holzplanken vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.BasementKellerWardrobeItemCount < amountNeeded)
            {
                amountNeeded = room.BasementKellerWardrobeItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(KellerWardrobeItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Holzplanken bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToKellerWardrobe);
                await Task.Delay(TimeToKellerWardrobe);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.BasementKellerWardrobeItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(KellerWardrobeItem, room.BasementKellerWardrobeItemCount);
                    dbPlayer.SendNewNotification("Umkleideupgrade: Es werden keine weiteren Holzplanken benötigt!");
                    room.BasementKellerWardrobeItemCount = 0;
                }
                else
                {
                    room.BasementKellerWardrobeItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Umkleideupgrade: {room.BasementKellerWardrobeItemCount} Holzplanken verbleibend");
                    dbPlayer.Container.RemoveItem(KellerWardrobeItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerHacker(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.BasementHackerItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genügend Elektronikteile vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.BasementHackerItemCount < amountNeeded)
            {
                amountNeeded = room.BasementHackerItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(KellerHackerItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Elektronikteile bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToKellerHacker);
                await Task.Delay(TimeToKellerHacker);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.BasementHackerItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(KellerHackerItem, room.BasementHackerItemCount);
                    dbPlayer.SendNewNotification("Hackerupgrade: Es werden keine weiteren Elektronikteile benötigt!");
                    room.BasementHackerItemCount = 0;
                }
                else
                {
                    room.BasementHackerItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Hackerupgrade: {room.BasementHackerItemCount} Elektronikteile verbleibend");
                    dbPlayer.Container.RemoveItem(KellerHackerItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerMechanic(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.BasementMachanicItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genügend Eisenbarren vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.BasementMachanicItemCount < amountNeeded)
            {
                amountNeeded = room.BasementMachanicItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(KellerMechanicItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Eisenbarren bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToKellerMechanic);
                await Task.Delay(TimeToKellerMechanic);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.BasementMachanicItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(KellerMechanicItem, room.BasementMachanicItemCount);
                    dbPlayer.SendNewNotification("Mechanikerupgrade: Es werden keine weiteren Eisenbarren benötigt!");
                    room.BasementMachanicItemCount = 0;
                }
                else
                {
                    room.BasementMachanicItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Mechanikerupgrade: {room.BasementMachanicItemCount} Eisenbarren verbleibend");
                    dbPlayer.Container.RemoveItem(KellerMechanicItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool RaumInneneinrichtung(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.MainFloorInteriorItmeCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genuegend Holzplanken vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.MainFloorInteriorItmeCount < amountNeeded)
            {
                amountNeeded = room.MainFloorInteriorItmeCount;
            }

            if (dbPlayer.Container.GetItemAmount(InneneinrichtungGlasItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genuegend Holzplanken bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToInneneinrichtungGlas);
                await Task.Delay(TimeToInneneinrichtungGlas);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.MainFloorInteriorItmeCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(InneneinrichtungGlasItem, room.MainFloorInteriorItmeCount);
                    dbPlayer.SendNewNotification("Inneneinrichtungsupgrade: Keine weiteren Holzplanken benötigt!");
                    room.MainFloorInteriorItmeCount = 0;
                }
                else
                {
                    room.MainFloorInteriorItmeCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Inneneinrichtungsupgrade: {room.MainFloorInteriorItmeCount} Holzplanken verbleibend");
                    dbPlayer.Container.RemoveItem(InneneinrichtungGlasItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerElectronic(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.BasementElectronicItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es sind bereits genügend Elektronikteile vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.BasementElectronicItemCount < amountNeeded)
            {
                amountNeeded = room.BasementElectronicItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(KellerElectronicItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genügend Elektronikteile bei dir! (min. {amountNeeded} benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToKellerElectronic);
                await Task.Delay(TimeToKellerElectronic);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.BasementElectronicItemCount <= amountNeeded)
                {
                    room.BasementElectronicItemCount = 0;
                    room.UpgradePlanningRoom(dbPlayer, 6, 2);
                }
                else
                {
                    room.BasementElectronicItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Elektronikupgrade: {room.BasementElectronicItemCount} Elektronikteile verbleibend");
                    dbPlayer.Container.RemoveItem(KellerElectronicItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool RaumSpieleautomaten(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            if (room.MainFloorSlotMachineItemCount == 0)
            {
                dbPlayer.SendNewNotification("Es ist bereits genuegend Baukaesten vorhanden!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            int amountNeeded = 1;

            if (room.MainFloorSlotMachineItemCount < amountNeeded)
            {
                amountNeeded = room.MainFloorSlotMachineItemCount;
            }

            if (dbPlayer.Container.GetItemAmount(SpielautomatenGlasItem) < amountNeeded)
            {
                dbPlayer.SendNewNotification($"Du hast nicht genuegend Baukaesten bei dir! (min. {amountNeeded} Stück benoetigt)", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                return false;
            }

            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("pressedEOnProcess", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor1", "gang_chatting_base_c");

                Chats.sendProgressBar(dbPlayer, TimeToSpielautomatenGlas);
                await Task.Delay(TimeToSpielautomatenGlas);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("pressedEOnProcess");

                if (room.MainFloorSlotMachineItemCount <= amountNeeded)
                {
                    dbPlayer.Container.RemoveItem(SpielautomatenGlasItem, room.MainFloorSlotMachineItemCount);
                    dbPlayer.SendNewNotification("Spielautomatenupgrade: Keine weiteren Baukaesten benötigt!");
                    room.MainFloorSlotMachineItemCount = 0;
                }
                else
                {
                    room.MainFloorSlotMachineItemCount -= amountNeeded;
                    dbPlayer.SendNewNotification($"Spielautomatenupgrade: {room.MainFloorSlotMachineItemCount} Spielautomat Baukasten verbleibend");
                    dbPlayer.Container.RemoveItem(SpielautomatenGlasItem, amountNeeded);
                }

                room.SavePlanningRoom();
            }));

            return true;
        }

        private bool KellerUpgradeComputer(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("inPlanningRoom"))
            {
                if (room.TeamId == dbPlayer.TeamId)
                {
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomKellerUpgradeMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            return false;
        }

        private bool RaumUpgradeComputer(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("inPlanningRoom"))
            {
                if (room.TeamId == dbPlayer.TeamId)
                {
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomUpgradeMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            return false;
        }

        private bool KellerAufräumen(DbPlayer dbPlayer, PlanningRoom room)
        {
            if (dbPlayer.HasData("pressedEOnProcess")) return false;

            foreach (Vector3 position in PlanningRoomCleanupKellerLocations)
            {
                if (dbPlayer.Player.Position.DistanceTo(position) <= 2f)
                {
                    if (dbPlayer.HasData("inPlanningRoom"))
                    {
                        if (!dbPlayer.Container.CanInventoryItemAdded(Trash))
                        {
                            dbPlayer.SendNewNotification("Du hast nicht genügend Platz!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }

                        if (dbPlayer.Container.GetItemAmount(Trash) >= TrashMaximumCarryAmount)
                        {
                            dbPlayer.SendNewNotification("Du trägst bereits zu viel Müll bei dir, entsorge diesen draußen zuerst!", title: "Planungsraum", notificationType: PlayerNotification.NotificationType.ERROR);
                            dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", TrashworkerLocation.X, TrashworkerLocation.Y);
                            return false;
                        }

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("pressedEOnProcess", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                            Chats.sendProgressBar(dbPlayer, TimeToPickupKellerTrash);
                            await Task.Delay(TimeToPickupKellerTrash);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.Player.StopAnimation();
                            dbPlayer.ResetData("pressedEOnProcess");
                        }));

                        dbPlayer.Container.AddItem(Trash, 1);
                        return true;
                    }
                }
            }

            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.HasData("planningroomInteriorColshape")) return false;
            if (colShapeState == ColShapeState.Enter)
            {
                PlanningRoom planningRoom = GetPlanningRoomByDimension(dbPlayer.Player.Dimension);

                if (planningRoom == null) return false;
                planningRoom.LoadInterior(dbPlayer);

                if (planningRoom.PlayersInsideRoom.Count == 0)
                {
                    planningRoom.CreateMarker();
                }

                if(!planningRoom.PlayersInsideRoom.Contains(dbPlayer))
                {
                    planningRoom.PlayersInsideRoom.Add(dbPlayer);
                }

                dbPlayer.SetData("inPlanningRoom", planningRoom.TeamId);
                return true;
            }
            if (colShapeState == ColShapeState.Exit)
            {
                if (!dbPlayer.HasData("inPlanningRoom")) return false;
                PlanningRoom planningRoom = GetPlanningRoomByTeamId(dbPlayer.GetData("inPlanningRoom"));

                if (planningRoom == null) return false;
                planningRoom.UnloadInterior(dbPlayer);

                if (planningRoom.PlayersInsideRoom.Contains(dbPlayer))
                {
                    planningRoom.PlayersInsideRoom.Remove(dbPlayer);
                }

                if (planningRoom.PlayersInsideRoom.Count == 0)
                {
                    planningRoom.DeleteMarker();
                }

                dbPlayer.ResetData("inPlanningRoom");
                return true;
            }

            return false;
        }

        public void StartVehiclePreQuest(DbPlayer dbPlayer, int index)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            switch (index)
            {
                case 1:
                    GetCasinoVehicle(dbPlayer);
                    break;
            }
        }

        public void GetCasinoVehicle(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            PlanningRoom room = Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            if (room.CasinoPlanLevel == 1)
            {
                if (room.CasinoPropDoor == 0)
                {
                    dbPlayer.SendNewNotification($"Bevor wir weitermachen können, sollten wir einen Nachbau der Tresortür anfertigen!", PlayerNotification.NotificationType.ERROR);
                    return;
                }
                else
                {
                    if (room.CasinoPropDoor == 1 && room.PlanningroomWardrobeContainer.GetItemAmount(CasinoRequiredOutfitId) < CasinoRequiredOutfitAmount)
                    {
                        dbPlayer.SendNewNotification($"Bevor wir weitermachen können, sollten wir die Uniformen besorgen!", PlayerNotification.NotificationType.ERROR);
                        return;
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Bevor wir weitermachen können, sollten wir mit unserem Kontakt sprechen!", PlayerNotification.NotificationType.ERROR);
                        return;
                    }
                }
            }
            else
            {
                dbPlayer.SendNewNotification($"Es wurde noch kein Miniatur Bausatz gefunden!", PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public bool CanStart()
        {
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configurations.Configuration.Instance.DevMode) return true;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 40)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 20)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }

        public void StartOutfitPreQuest(DbPlayer dbPlayer, int index)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            DateTime actualDate = DateTime.Now;
            if (dbPlayer.Team.LastOutfitPreQuest.AddHours(48) < actualDate)
            {
                if (Configurations.Configuration.Instance.DevMode != true)
                {
                    if (!Instance.CanStart())
                    {
                        dbPlayer.SendNewNotification("Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 20 min vor und nach Restarts!)");
                        return;
                    }
                }

                if (TeamModule.Instance.DutyCops < 15 && !Configurations.Configuration.Instance.DevMode)
                {
                    dbPlayer.SendNewNotification("Es müssen mindestens 15 Beamte im Dienst sein!");
                    return;
                }

                switch (index)
                {
                    case 1:
                        GetCasinoOutfit(dbPlayer);
                        break;
                }

                dbPlayer.SaveLastOutfitPreQuest();
            }
            else
            {
                dbPlayer.SendNewNotification("Diese Pre Quest kann nur alle 48 Stunden gestartet werden!");
                return;
            }
        }

        public void GetCasinoOutfit(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            PlanningRoom room = Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            if (room.CasinoPlanLevel == 1)
            {
                if (room.CasinoPropDoor == 0)
                {
                    dbPlayer.SendNewNotification($"Bevor wir weitermachen können, sollten wir einen Nachbau der Tresortür anfertigen!", PlayerNotification.NotificationType.ERROR);
                    return;
                }
                else
                {
                    if (room.CasinoPropDoor == 1 && room.PlanningroomWardrobeContainer.GetItemAmount(CasinoRequiredOutfitId) < CasinoRequiredOutfitAmount)
                    {
                        dbPlayer.SendNewNotification($"Bevor wir weitermachen können, sollten wir {CasinoRequiredOutfitAmount} Polizei Uniformen besorgen.", PlayerNotification.NotificationType.INFO);
                        dbPlayer.SendNewNotification($"Diese Uniformen können wir im Mission Row, Sandy PD oder im Bolingbroke Penitentiary besorgen!", PlayerNotification.NotificationType.INFO);
                        dbPlayer.SendNewNotification($"Hierfür habt Ihr nun 1 Stunde Zeit!", PlayerNotification.NotificationType.INFO);

                        return;
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Nachdem wir nun die Uniformen besorgt haben, sollten wir mit den Fahrzeugen weitermachen!", PlayerNotification.NotificationType.INFO);
                        return;
                    }
                }
            }
            else
            {
                dbPlayer.SendNewNotification($"Es wurde noch kein Miniatur Bausatz gefunden!", PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public void StartExtraPreQuest(DbPlayer dbPlayer, int index)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            switch (index)
            {
                case 1:
                    GetCasinoExtra(dbPlayer);
                    break;
            }
        }

        public void GetCasinoExtra(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            PlanningRoom room = Instance.GetPlanningRoomByTeamId(dbPlayer.Team.Id);
            
            if (room.CasinoPropDoor == 0)
            {
                if (room.CasinoPlanLevel == 1)
                {
                    dbPlayer.SendNewNotification($"Lasst uns loslegen, fahre zur Schmiede um einen Nachbau der Tresortür anzufertigen!", PlayerNotification.NotificationType.ERROR);
                    dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", PlanningRoomCraftingLocation.X, PlanningRoomCraftingLocation.Y);

                    return;
                }
                else
                {
                    dbPlayer.SendNewNotification($"Es wurde noch kein Miniatur Bausatz gefunden!", PlayerNotification.NotificationType.ERROR);
                    return;
                }
            }
            else
            {
                dbPlayer.SendNewNotification($"Diese Aufgabe wurde bereits erledigt!", PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public void StartHeist(DbPlayer dbPlayer, int index)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            switch (index)
            {
                case 1:
                    RobCasino(dbPlayer);
                    break;
            }
        }

        public void RobCasino(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsAGangster()) return;
            if (dbPlayer.Player.IsInVehicle) return;

            PlanningRoom planningRoom = GetPlanningRoomByTeamId(dbPlayer.Team.Id);

            if (planningRoom.CasinoPlanLevel == 1 && planningRoom.PlanningroomWardrobeContainer.GetItemAmount(CasinoRequiredOutfitId) >= CasinoRequiredOutfitAmount && VehicleHandler.Instance.PlanningVehicleCheckByModel(dbPlayer.Team.Id, CasinoRequiredVehicleModel))
            {
                return;
            }
            else
            {
                dbPlayer.SendNewNotification($"Es wurden noch nicht alle Anforderungen erfüllt um diesen Heist zu starten!", PlayerNotification.NotificationType.ERROR);
                return;
            }
        }

        public PlanningRoom GetPlanningRoomByTeamId(uint teamId)
        {
            return Instance.GetAll().Values.Where(PlanningRoom => PlanningRoom.TeamId == teamId).FirstOrDefault();
        }

        public PlanningRoom GetPlanningRoomByDimension(uint dimension)
        {
            return Instance.GetAll().Values.Where(PlanningRoom => PlanningRoom.TeamId == dimension).FirstOrDefault();
        }

        // OnFiveMinuteUpdate
        public override void OnFiveMinuteUpdate()
        {
            if (LastOutfitRob != null && LastOutfitRob.AddMinutes(10) < DateTime.Now)
            {
                CloseContainers();
            }
        }

        public void CloseContainers()
        {
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR1);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR2);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR3);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR4);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR5);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR6);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR7);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR8);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR9);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.PLANNINGOUTFITMR10);
            if (StaticContainer != null)
            {
                StaticContainer.Container.ClearInventory();
                StaticContainer.Locked = true;
            }
        }
    }
}
