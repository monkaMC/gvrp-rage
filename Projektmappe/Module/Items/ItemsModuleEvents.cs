using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Items.Scripts;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Configurations;
using GVRP.Module.DropItem;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.RemoteEvents;
using GVRP.Module.Players.Phone;
using GVRP.Module.Voice;
using GVRP.Module.Guenther;
using GVRP.Module.Asservatenkammer;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items
{
    public class ItemsModuleEvents : Script
    {
        public static void RequestInventory(Client player)
        {
            try
            {
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid())
                {
                    return;
                }
                if (!Configuration.Instance.InventoryActivated)
                {
                    dbPlayer.SendNewNotification("Das Inventarsystem ist aus Testgruenden deaktiviert.");
                    dbPlayer.SendNewNotification("Es ist in paar Minuten wieder erreichbar!");
                    return;
                }

                if (dbPlayer.HasData("container_refund"))
                {
                    dbPlayer.ResetData("container_refund");
                }

                List<ClientContainerObject> containerList = new List<ClientContainerObject>
                {
                    dbPlayer.Container.ConvertForClient(1, "", dbPlayer.money[0], dbPlayer.blackmoney[0])
                };

                // Find Now The Inventory
                string clientsending = "[";
                clientsending += NAPI.Util.ToJson(dbPlayer.Container.ConvertForClient(1));

                Container externContainer = ItemsModule.Instance.findInventory(player);
                if (externContainer != null && !externContainer.Locked)
                {
                    containerList.Add(ItemsModule.Instance.findInventory(player).ConvertForClient(2));
                    clientsending += "," + NAPI.Util.ToJson(ItemsModule.Instance.findInventory(player).ConvertForClient(2));
                }

                clientsending += "]";

                dbPlayer.Player.TriggerEvent("responseInventory", clientsending);

                ComponentManager.Get<InventoryWindow>().Show()(dbPlayer, containerList);

                if (dbPlayer.IsACop() && dbPlayer.Duty)
                {
                    resetFriskInventoryFlags(dbPlayer);
                }
                else
                {
                    resetDisabledInventoryFlag(dbPlayer);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return;
            }
        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public void requestInventory(Client player)
        {


            RequestInventory(player);

        }

        public static void resetDisabledInventoryFlag(DbPlayer dbPlayer)
        {
            dbPlayer.ResetData("disableinv");
        }

        public static void resetFriskInventoryFlags(DbPlayer dbPlayer)
        {
            dbPlayer.ResetData("disableFriskInv");
            dbPlayer.ResetData("friskInvUserName");
            dbPlayer.ResetData("friskInvUserID");
            dbPlayer.ResetData("friskInvVeh");
            dbPlayer.ResetData("friskInvHouse");
        }

        public async void actualizeInventory(Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.HasData("cooldowninva"))
            {
                dbPlayer.SetData("cooldowninva", true);
                await Task.Delay(3000);
                dbPlayer.ResetData("cooldowninva");

            }
            if (dbPlayer.HasData("cooldowninva"))
            {
                dbPlayer.SendNewNotification("Du kannst dein Inventar nur 1 mal pro 3 Sekunden neuladen!", PlayerNotification.NotificationType.INFO, "", 2000);
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            List<ClientContainerObject> containerList = new List<ClientContainerObject>
            {
                dbPlayer.Container.ConvertForClient(1)
            };

            // Find Now The Inventory
            string clientsending = "[";
            clientsending += NAPI.Util.ToJson(dbPlayer.Container.ConvertForClient(1));

            if (ItemsModule.Instance.findInventory(player) != null)
            {
                containerList.Add(ItemsModule.Instance.findInventory(player).ConvertForClient(2));
                clientsending += "," + NAPI.Util.ToJson(ItemsModule.Instance.findInventory(player).ConvertForClient(2));
            }
            clientsending += "]";

            ContainerManager.CheckFunkDisabling(dbPlayer.Container);

            dbPlayer.Player.TriggerEvent("setInventoryItems", clientsending);
        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task dropInventoryItem(Client player, int slot, int amount)
        {

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.Player.IsInVehicle)
            {
                return;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return; // Show Inventory
            }
            
            if (slot < 0 || slot > 47)
            {
                return;
            }
            
            if (dbPlayer.HasData("disableFriskInv") && dbPlayer.GetData("disableFriskInv"))
            {
                if (!dbPlayer.HasData("friskInvUserName") && !dbPlayer.HasData("friskInvVeh") && !dbPlayer.HasData("friskInvHouse"))
                {
                    if (!dbPlayer.GetData("friskInvUserName") && !dbPlayer.GetData("friskInvVeh") && !dbPlayer.GetData("friskInvHouse"))
                    {
                        dbPlayer.SendNewNotification("Fehler beim Durchsuchen!");
                        return;
                    }
                }
                
                if (dbPlayer.HasData("friskInvVeh"))
                {
                    SxVehicle vehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
                    if (vehicle == null || dbPlayer.Player.Position.DistanceTo(vehicle.entity.Position) > 10.0f)
                    {
                        return;
                    }

                    if (dbPlayer.GetData("friskInvVeh") == vehicle.Data.Model)
                    {
                        ItemModel model = vehicle.Container.GetModelOnSlot(slot);
                        if (model == null)
                        {
                            return;
                        }

                        if (vehicle.Container == null || vehicle.Container.Slots[slot] == null || vehicle.Container.Slots[slot].Model == null)
                        {
                            return;
                        }


                        ItemModel BeschlagnahmtItem = ItemModelModule.Instance.GetById(AsservatenkammerModule.Instance.GetConvertionItemId(model.Id, model.Script.StartsWith("w_"), model.Script.StartsWith("ammo_")));

                        if (BeschlagnahmtItem == null) return;

                        if (!AsservatenkammerModule.Instance.IsAserItem(BeschlagnahmtItem.Id)) // Wenn kein AserItem (zb Handy etc) Einfach droppen
                        {
                            dbPlayer.SendNewNotification($"{vehicle.Container.Slots[slot].Amount} - {vehicle.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                        }
                        else
                        {
                            // Get Vehicle With Kofferraum open rofl
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehiclesFromTeamWithContainerOpen(dbPlayer.Player.Position, (int)dbPlayer.TeamId).FirstOrDefault();
                            if (sxVehicle == null || !sxVehicle.IsValid())
                            {
                                dbPlayer.SendNewNotification("Kein Fahrzeug zur Beschlagnahmung in der Nähe! (Kofferraum muss offen sein!)");
                                return;
                            }

                            if (!sxVehicle.Container.CanInventoryItemAdded(BeschlagnahmtItem, vehicle.Container.Slots[slot].Amount))
                            {
                                dbPlayer.SendNewNotification("Sie haben nicht genug Platz für eine Beschlagnahmung!");
                                return;
                            }

                            sxVehicle.Container.AddItem(BeschlagnahmtItem, vehicle.Container.Slots[slot].Amount);
                            dbPlayer.SendNewNotification($"{vehicle.Container.Slots[slot].Amount} - {vehicle.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                        }

                        vehicle.Container.RemoveFromSlot(slot, amount);
                        return;
                    }
                }

                if (dbPlayer.HasData("friskInvHouse"))
                {
                    if (dbPlayer.GetData("friskInvHouse") == dbPlayer.Player.Dimension && dbPlayer.DimensionType[0] == DimensionType.House && dbPlayer.HasData("inHouse"))
                    {
                        House house;
                        if ((house = HouseModule.Instance.Get(dbPlayer.GetData("inHouse"))) != null)
                        {
                            ItemModel model = house.Container.GetModelOnSlot(slot);
                            if (model == null)
                            {
                                return;
                            }

                            if (house.Container == null || house.Container.Slots[slot] == null || house.Container.Slots[slot].Model == null)
                            {
                                return;
                            }

                            dbPlayer.SendNewNotification($"{house.Container.Slots[slot].Amount} - {house.Container.Slots[slot].Model.Name} wurden entfernt.");

                            house.Container.RemoveFromSlot(slot, amount);
                            return;
                        }
                    }
                }

                try
                {
                    if (dbPlayer.HasData("friskInvUserID"))
                    {
                        DbPlayer findPlayer = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("friskInvUserID"));
                        if (findPlayer == null || !findPlayer.IsValid())
                        {
                            dbPlayer.SendNewNotification("Buerger nicht gefunden!");
                            return;
                        }

                        if (findPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
                        {
                            dbPlayer.SendNewNotification("Buerger zu weit entfernt!");
                            return;
                        }

                        if (!findPlayer.IsCuffed && !findPlayer.IsTied && !findPlayer.isInjured())
                        {
                            dbPlayer.SendNewNotification("Buerger nicht gefesselt / bewusstlos!");
                            return;
                        }

                        if (findPlayer.Container == null || findPlayer.Container.Slots[slot] == null || findPlayer.Container.Slots[slot].Model == null)
                        {
                            return;
                        }

                        ItemModel model = findPlayer.Container.GetModelOnSlot(slot);
                        if (model == null)
                        {
                            return;
                        }

                        if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                        {
                            bool suxxess = ItemScript.backpack(findPlayer, model, true);

                            string message = suxxess ? "" : "nicht ";

                            dbPlayer.SendNewNotification($"Der Rucksack wurde {message}entfernt!");
                            findPlayer.SendNewNotification($"Der Beamte hat den Rucksack {message}entfernt!");

                            if (!suxxess)
                            {
                                return;
                            }
                            else
                            {
                                findPlayer.Container.RemoveFromSlot(slot, amount);
                                actualizeInventory(findPlayer.Player);
                                return;
                            }
                        }

                        // Falls Zivis iwann frisk entfernen können
                        if (dbPlayer.IsACop() && dbPlayer.IsInDuty())
                        {
                            ItemModel BeschlagnahmtItem = ItemModelModule.Instance.GetById(AsservatenkammerModule.Instance.GetConvertionItemId(model.Id, model.Script.StartsWith("w_"), model.Script.StartsWith("ammo_")));

                            if (BeschlagnahmtItem == null) return;

                            if (!AsservatenkammerModule.Instance.IsAserItem(BeschlagnahmtItem.Id)) // Wenn kein AserItem (zb Handy etc) Einfach droppen
                            {
                                dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                            }
                            else
                            {
                                if(findPlayer.IsACop())
                                {
                                    dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                    findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                    findPlayer.Container.RemoveFromSlot(slot, amount);
                                }

                                // Get Vehicle With Kofferraum open rofl
                                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehiclesFromTeamWithContainerOpen(dbPlayer.Player.Position, (int)dbPlayer.TeamId).FirstOrDefault();
                                if (sxVehicle == null || !sxVehicle.IsValid())
                                {
                                    dbPlayer.SendNewNotification("Kein Fahrzeug zur Beschlagnahmung in der Nähe! (Kofferraum muss offen sein!)");
                                    return;
                                }

                                if (!sxVehicle.Container.CanInventoryItemAdded(BeschlagnahmtItem, findPlayer.Container.Slots[slot].Amount))
                                {
                                    dbPlayer.SendNewNotification("Sie haben nicht genug Platz für eine Beschlagnahmung!");
                                    return;
                                }
                                
                                sxVehicle.Container.AddItem(BeschlagnahmtItem, findPlayer.Container.Slots[slot].Amount);
                                dbPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");
                                findPlayer.SendNewNotification($"{findPlayer.Container.Slots[slot].Amount} - {findPlayer.Container.Slots[slot].Model.Name} wurden beschlagnahmt.");

                                // FIB
                                if (dbPlayer.TeamId == (int)teams.TEAM_FIB)
                                {
                                    if (findPlayer.Container.Slots[slot].Data != null && findPlayer.Container.Slots[slot].Data.ContainsKey("fingerprint"))
                                    {
                                        dbPlayer.SendNewNotification($"Es konnte ein Fingerabdruck auf dieser Waffe festgestellt werden! (ID: {findPlayer.Container.Slots[slot].Data["fingerprint"]})");
                                    }
                                }
                            }
                        }

                        findPlayer.Container.RemoveFromSlot(slot, amount);

                        // Komisches zeug für funk und apps
                //        if (findPlayer.Container.GetItemById(174).Amount < 1)
                  //      {
                       //     PhoneCall.CancelPhoneCall(findPlayer);
                      //      VoiceModule.Instance.turnOffFunk(findPlayer);
                    //    }
                        findPlayer.UpdateApps();
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }
            else
            {
                ItemModel model = dbPlayer.Container.GetModelOnSlot(slot);
                if (model == null) return;

                try
                {
                    // Check Cuff Die Death
                    if (!dbPlayer.CanInteract())
                    {
                        return;
                    }

                    if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                    {
                        if (!ItemScript.backpack(dbPlayer, model, true))
                        {
                            return;
                        }
                    }

                    if (model.CanDrugInfect())
                    {
                        dbPlayer.IncreasePlayerDrugInfection();
                    }

                    Dictionary<ItemModel, int> droppedItems = new Dictionary<ItemModel, int>();
                    droppedItems.Add(dbPlayer.Container.GetModelOnSlot(slot), amount);
                    DropItemModule.Instance.DropInventoryItem(dbPlayer, droppedItems);
                    dbPlayer.Container.RemoveFromSlot(slot, amount);

                    actualizeInventory(player);

                    await dbPlayer.PlayInventoryInteractAnimation();
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task giveInventoryItem(Client player, int slot, int amount)
        {

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return; // Show Inventory
            }

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.HasData("giveitem"))
            {
                return;
            }

            

            DbPlayer destinationPlayer = Players.Players.Instance.FindPlayerById(dbPlayer.GetData("giveitem"));
            dbPlayer.ResetData("giveitem");
            if (destinationPlayer == null || !destinationPlayer.IsValid())
            {
                dbPlayer.SendNewNotification("Niemand in der Nähe!");
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (destinationPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
            {
                dbPlayer.SendNewNotification("Zu weit entfernt!");
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (slot < 0 || slot > 47)
            {
                return;
            }

            Item item = dbPlayer.Container.GetItemOnSlot(slot);
            ItemModel model = item.Model;

            try
            {
                // Check Cuff Die Death
                if (!dbPlayer.CanInteract())
                {
                    return;
                }

                if (item.Id == 550)
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen sie nicht weitergeben.");
                    return;
                }

                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null)
                {
                    return;
                }

                if (model.CanDrugInfect())
                {
                    dbPlayer.IncreasePlayerDrugInfection();
                }

                if (!destinationPlayer.Container.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Kein Platz");
                    ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                    return;
                }

                if (model.Id == 174)
                {
                    PhoneCall.CancelPhoneCall(dbPlayer);
                    VoiceModule.Instance.turnOffFunk(dbPlayer);
                }

                dbPlayer.Container.RemoveFromSlot(slot, amount);
                destinationPlayer.Container.AddItem(model, amount, item.Data);
                actualizeInventory(player);
                actualizeInventory(destinationPlayer.Player);
                dbPlayer.SendNewNotification($"Du hast {amount} {model.Name} gegeben!");
                destinationPlayer.SendNewNotification($"Du hast {amount} {model.Name} bekommen!");

                await dbPlayer.PlayInventoryInteractAnimation();
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void moveItemToInventory(Client player, int sourceSlot, int destinationSlot, int inventoryType, int amount)
        {

            ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!dbPlayer.HasData("cooldowninve"))
            {
                dbPlayer.SetData("cooldowninve", true);
                await Task.Delay(1000);

                dbPlayer.ResetData("cooldowninve");

            }
            if (dbPlayer.HasData("cooldowninve"))
            {
                dbPlayer.SendNewNotification("Du kannst maximal 1 Item pro Sekunde verschieben!", PlayerNotification.NotificationType.INFO, "", 2000);
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (!await ContainerInventoryActions.MoveItemToInventory(dbPlayer, containerMoveType, sourceSlot, destinationSlot, amount))
            {
                // Close Inventory on Client
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
            }
            return;

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void fillInventorySlots(Client player, int inventoryType)
        {

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

        
            if (!dbPlayer.HasData("cooldowninvz"))
            {
                dbPlayer.SetData("cooldowninvz", true);
                await Task.Delay(3000);
        dbPlayer.ResetData("cooldowninvz");

            }
            if (dbPlayer.HasData("cooldowninvz"))
            {
                dbPlayer.SendNewNotification("Du kannst maximal 1 mal das Inventar refreshen pro 3 Sekunden!", PlayerNotification.NotificationType.INFO, "", 2000);
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }


ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

            // External Container
            Container eXternContainer = ItemsModule.Instance.findInventory(player);

            if (containerMoveType == ContainerMoveTypes.ExternInventory)
            {
                if (eXternContainer == null)
                {
                    return;
                }
            }

            ContainerInventoryActions.FillInventorySlots(containerMoveType == ContainerMoveTypes.ExternInventory ? eXternContainer : dbPlayer.Container);
            return;

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async void moveItemInInventory(Client player, int sourceSlot, int destinationSlot, int inventoryType, int amount)
        {

            ContainerMoveTypes containerMoveType = (ContainerMoveTypes)inventoryType;

            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.HasData("cooldowninv"))
            {
                dbPlayer.SetData("cooldowninv", true);
                await Task.Delay(1000);
                dbPlayer.ResetData("cooldowninv");

            }
            if (dbPlayer.HasData("cooldowninv"))
            {
                dbPlayer.SendNewNotification("Du kannst maximal 1 Item pro Sekunde verschieben!", PlayerNotification.NotificationType.INFO, "", 2000);
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return;
            }

            if (!ContainerInventoryActions.moveItemInInventory(dbPlayer, containerMoveType, sourceSlot, destinationSlot, amount))
            {
                // Close Inventory on Client
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
            }
            return;

        }

        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false, AllowedDeath = false)]
        [RemoteEvent]
        public async Task<bool> useInventoryItem(Client client, int slot)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return false;
            }

            bool usedSuccessfully = false;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract())
            {
                return false;
            }

            if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv"))
            {
                return false; // Show Inventory
            }

            if (dbPlayer.HasData("disableFriskInv") && dbPlayer.GetData("disableFriskInv"))
            {
                return false; // Show Inventory
            }

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return false;
            }
            
            if (slot < 0 || slot > 47)
            {
                return false;
            }

            ItemModel model = dbPlayer.Container.GetModelOnSlot(slot);
            Item item = dbPlayer.Container.GetItemOnSlot(slot);

            if (model != null)
            {
                if (dbPlayer.isInjured())
                {
                    dbPlayer.SendNewNotification("Sie koennen diese Funktion derzeit nicht benutzen.");
                    return false;
                }

                if(model.RestrictedToTeams.Count > 0)
                {
                    if (!model.RestrictedToTeams.Contains(dbPlayer.Team.Id)) return false;
                }

                // First Startswith Script
                if (model.Script.ToLower().StartsWith("w_"))
                {
                    usedSuccessfully = ItemScript.WeaponUnpack(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("bw_"))
                {
                    Console.WriteLine("Test " + model + ", " + item);
                    usedSuccessfully = ItemScript.WeaponUnpackCop(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("zw_"))
                {
                    usedSuccessfully = await ItemScript.ZerlegteWaffeUnpack(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("ammo_"))
                {
                    usedSuccessfully = await ItemScript.EquipAmo(dbPlayer, model, item.Amount);
                }
                else if (model.Script.ToLower().StartsWith("bammo_"))
                {
                    usedSuccessfully = await ItemScript.EquipAmoCop(dbPlayer, model, item.Amount);
                }
                else if (model.Script.ToLower().StartsWith("r_map"))
                {
                    usedSuccessfully = ItemScript.RessourceMap(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("tune_"))
                {
                    usedSuccessfully = ItemScript.TuningParts(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("bp_"))
                {
                    usedSuccessfully = ItemScript.backpack(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("alk_"))
                {
                    usedSuccessfully = await ItemScript.Alk(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("chest_"))
                {
                    usedSuccessfully = ItemScript.ChestUnpack(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("pkfzbrief_"))
                {
                    usedSuccessfully = ItemScript.PresentKFZBrief(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("pmoney_"))
                {
                    usedSuccessfully = ItemScript.PresentMoney(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("registervehicle_"))
                {
                    usedSuccessfully = ItemScript.VehicleRegister(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("unregistervehicle_"))
                {
                    usedSuccessfully = ItemScript.VehicleUnregister(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("divebottle_"))
                {
                    usedSuccessfully = ItemScript.Divebottle(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("dice_"))
                {
                    usedSuccessfully = await ItemScript.Dice(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("geschenk"))
                {
                    usedSuccessfully = ItemScript.Geschenk(dbPlayer);
                }
                else if (model.Script.ToLower().StartsWith("armortype_"))
                {
                    usedSuccessfully = await ItemScript.Armortype(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("vehicle_claw_"))
                {
                    usedSuccessfully = ItemScript.VehicleClaw(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("outfit_"))
                {
                    usedSuccessfully = await ItemScript.Outfit(dbPlayer, model, item);
                }
                else if (model.Script.ToLower().StartsWith("itemtocloth_"))
                {
                    usedSuccessfully = ItemScript.ItemToCloth(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("itemtoprop_"))
                {
                    usedSuccessfully = ItemScript.ItemToProp(dbPlayer, model);
                }
                else if (model.Script.ToLower().StartsWith("autohaus_"))
                {
                    usedSuccessfully = ItemScript.UseCarsell(dbPlayer, model, item);
                }
                else
                {
                    // Do Scripts
                    switch (model.Script)
                    {
                        case "wkiste":
                            usedSuccessfully = ItemScript.WeaponChest(dbPlayer, model);
                            break;
                        case "present":
                            usedSuccessfully = ItemScript.PresentScript(dbPlayer, model);
                            break;
                        case "methcook":
                            usedSuccessfully = ItemScript.MethCook(dbPlayer, model);
                            break;
                        case "repair":
                            usedSuccessfully = await ItemScript.VehicleRepair(dbPlayer, model);
                            break;
                        case "geldboerse":
                            usedSuccessfully = ItemScript.Geldboerse(dbPlayer, model);
                            break;
                        case "brecheisen":
                            usedSuccessfully = await ItemScript.Brecheisen(dbPlayer, model);
                            break;
                        case "schweissgeraet":
                            usedSuccessfully = await ItemScript.Schweissgereat(dbPlayer, model);
                            break;
                        case "barriere_arrow":
                            usedSuccessfully = await ItemScript.BarriereArrow(dbPlayer, model);
                            break;
                        case "stinger":
                            usedSuccessfully = await ItemScript.Stringer(dbPlayer, model);
                            break;
                        case "barriere2":
                            usedSuccessfully = await ItemScript.barriere2(dbPlayer, model);
                            break;
                        case "barriere":
                            usedSuccessfully = await ItemScript.barriere(dbPlayer, model);
                            break;
                        case "light":
                            usedSuccessfully = await ItemScript.light(dbPlayer, model);
                            break;
                        case "wkasten":
                            usedSuccessfully = await ItemScript.wkasten(dbPlayer, model);
                            break;
                        case "uncuff":
                            usedSuccessfully = await ItemScript.uncuff(dbPlayer, model);
                            break;
                        case "joint":
                            usedSuccessfully = await ItemScript.joint(dbPlayer, model);
                            break;
                        case "paper":
                            usedSuccessfully = ItemScript.paper(dbPlayer, model);
                            break;
                        case "armor":
                            usedSuccessfully = await ItemScript.Armor(dbPlayer, model);
                            break;
                        case "underarmour":
                            usedSuccessfully = await ItemScript.UnderArmor(dbPlayer, model);
                            break;
                        case "barmor":
                            usedSuccessfully = await ItemScript.BArmor(dbPlayer, model);
                            break;
                        case "medikit":
                            usedSuccessfully = await ItemScript.medikit(dbPlayer, model);
                            break;
                        case "fuel":
                            usedSuccessfully = ItemScript.fuel(dbPlayer, model);
                            break;
                        case "hookie":
                            usedSuccessfully = await ItemScript.hookie(dbPlayer, model);
                            break;
                        case "telefonguthaben":
                            usedSuccessfully = ItemScript.telefonguthaben(dbPlayer, model);
                            break;
                        case "carusorosso":
                            usedSuccessfully = await ItemScript.carusorosso(dbPlayer, model);
                            break;
                        case "blitzer70":
                            usedSuccessfully = await ItemScript.Blitzer70(dbPlayer, model);
                            break;
                        case "blitzer100":
                            usedSuccessfully = await ItemScript.Blitzer120(dbPlayer, model);
                            break;
                        case "b_koffer":
                            usedSuccessfully = ItemScript.Bargeldkoffer(dbPlayer, model, item);
                            break;
                        case "food":
                            usedSuccessfully = await ItemScript.Food(dbPlayer, model);
                            break;
                        case "drink":
                            usedSuccessfully = await ItemScript.Drink(dbPlayer, model);
                            break;
                        case "gps_tracker":
                            usedSuccessfully = ItemScript.GpsTracker(dbPlayer, model);
                            break;
                        case "plate_empty":
                            usedSuccessfully = await ItemScript.VehiclePlate(dbPlayer, item);
                            break;
                        case "vehiclerent":
                            usedSuccessfully = ItemScript.vehiclerent(dbPlayer, model);
                            break;
                        case "vehiclerentview":
                            usedSuccessfully = ItemScript.vehiclerentview(dbPlayer, model, item);
                            break;
                        case "vehicle_contract":
                            usedSuccessfully = ItemScript.VehicleContract(dbPlayer, item);
                            break;
                        case "marriage_certificate":
                            usedSuccessfully = ItemScript.Eheurkunde(dbPlayer, item);
                            break;
                        case "plate_used":
                            usedSuccessfully = await ItemScript.VehiclePlateUsed(dbPlayer, item);
                            break;
                        case "digging":
                            usedSuccessfully = ItemScript.Digging(dbPlayer);
                            break;
                        case "vehicle_note":
                            usedSuccessfully = await ItemScript.VehicleNote(dbPlayer, item);
                            break;
                        case "FMedikit":
                            usedSuccessfully = await ItemScript.FMedikit(dbPlayer, model);
                            break;
                        case "FArmor":
                            usedSuccessfully = await ItemScript.FArmor(dbPlayer, model);
                            break;
                        case "gaspberry":
                            usedSuccessfully = ItemScript.Gaspberry(dbPlayer, model);
                            break;
                        case "KisteMethamphetamin":
                            usedSuccessfully = await ItemScript.KisteMethamphetamin(dbPlayer, model);
                            break;
                        case "zentrifuge":
                            usedSuccessfully = await ItemScript.Zentrifuge(dbPlayer, model);
                            break;
                        case "guentherclub":
                            usedSuccessfully = await ItemScript.GuentherClub(dbPlayer);
                            break;
                        case "remove_vehicle_claw":
                            usedSuccessfully = await ItemScript.RemoveVehicleClaw(dbPlayer, model);
                            break;
                        case "alarm_system":
                            usedSuccessfully = ItemScript.AlarmSystem(dbPlayer, model);
                            break;
                        case "JohannsDietrich":
                            usedSuccessfully = await ItemScript.JohannsDietrich(dbPlayer, model);
                            break;
                        case "detectdrugsair":
                            usedSuccessfully = await ItemScript.DrugtestAir(dbPlayer, model);
                            break;
                        case "batterieram":
                            usedSuccessfully = await ItemScript.BatterieRam(dbPlayer, model);
                            break;
                        case "checkenergy":
                            usedSuccessfully = await ItemScript.VoltageTest(dbPlayer, model);
                            break;
                        case "bl_unpack":
                            usedSuccessfully = await ItemScript.BlackMoneyPack(dbPlayer, model, item, slot);
                            break;
                        case "jailtunnel":
                            if (Configurations.Configuration.Instance.JailescapeEnabled) usedSuccessfully = await ItemScript.JailTunnel(dbPlayer, model);
                            break;
                        case "computer":
                            usedSuccessfully = ItemScript.Computer(dbPlayer, model);
                            break;
                        case "sbenabletunnel":
                            usedSuccessfully = ItemScript.StaatsbankBlueprint(dbPlayer, model);
                            break;
                        case "sbdrill":
                            usedSuccessfully = await ItemScript.StaatsbankDrill(dbPlayer, model);
                            break;
                        case "sbhacking":
                            usedSuccessfully = await ItemScript.Hackingtool(dbPlayer, model);
                            break;
                        case "christmas":
                            usedSuccessfully = ItemScript.Present(dbPlayer, model);
                            break;
                        case "houserent":
                            usedSuccessfully = ItemScript.Houserent(dbPlayer, model);
                            break;
                        case "customdrug_meth":
                            usedSuccessfully = await ItemScript.CustomDrugMeth(dbPlayer, model);
                            break;
                        case "customdrug_weed":
                            usedSuccessfully = await ItemScript.CustomDrugWeed(dbPlayer, model);
                            break;
                        case "customdrug_jeff":
                            usedSuccessfully = await ItemScript.CustomDrugJeff(dbPlayer, model);
                            break;
                        case "customdrug_teflon":
                            usedSuccessfully = await ItemScript.CustomDrugTeflon(dbPlayer, model);
                            break;
                    }
                }

                // Remove Item On Use
                if (model.RemoveOnUse && usedSuccessfully)
                {
                    if (!model.Script.StartsWith("ammo_") && !model.Script.StartsWith("bammo_"))
                    {
                        dbPlayer.Container.RemoveItemSlotFirst(model, slot);
                    }

                    //Log if used
                    ItemModelModule.Instance.LogItem((int)model.Id, (int)dbPlayer.TeamId, 1);
                }
                ComponentManager.Get<InventoryWindow>().Close(dbPlayer.Player);
                return true;
            }
            return false;
        }

        [RemoteEvent]
        public void VehicleClawEvent(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!Regex.IsMatch(returnString, @"^[a-zA-Z0-9\s]+$"))
            {
                dbPlayer.SendNewNotification("Nur Buchstaben, Zahlen und Leerzeichen sind erlaubt");
                return;
            }
            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if (sxVehicle == null)
            {
                return;
            }
            ItemScript.DoVehicleClaw(dbPlayer, sxVehicle, dbPlayer.Team.Id == 1 && dbPlayer.IsInDuty(), returnString);


        }
        
        [RemoteEvent]
        public void CheckGuentherPassword(Client player, string returnString)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (!Regex.IsMatch(returnString, @"^[0-9]*$"))
            {
                dbPlayer.SendNewNotification("Nur Zahlen sind erlaubt");
                return;
            }
            if (!int.TryParse(returnString, out int userPass)) return;

            int password = Utils.GeneratePassword(dbPlayer);
            if(password != userPass)
            {
                dbPlayer.SendNewNotification("Falsches Passwort. Kein Zutritt für Dich!");
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.Player.TriggerEvent("loadguenther");
                dbPlayer.Player.SetPosition(GuentherModule.Inside);
                dbPlayer.Player.SetRotation(214.797f);
                await Task.Delay(1000);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                GuentherModule.DbPlayers.Add(dbPlayer);
                dbPlayer.Container.RemoveItem(GuentherModule.GuentherClubCardId, 1);
            }));
        }
    }
}
