using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Asservatenkammer;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Heist.Planning;
using GVRP.Module.Houses;
using GVRP.Module.JobFactions.Mine;
using GVRP.Module.Laboratories;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Vehicles;
using GVRP.Module.Voice;

namespace GVRP.Module.Items
{
    public static class ContainerManager
    {
        // Default Player Slots
        public static int defaultSlots = 6;
        public static int defaultWeight = 25000;

        public static Dictionary<Vector3, Container> StaticContainers = new Dictionary<Vector3, Container>();

        public static Dictionary<uint, Container> LoadedPrisonLockerContainers = new Dictionary<uint, Container>();
        public static Dictionary<uint, Container> LoadedCopInvContainers = new Dictionary<uint, Container>();
        public static Dictionary<uint, Container> LoadedWeaponImportContainers = new Dictionary<uint, Container>();
        public static Dictionary<uint, Container> LoadedRefundContainers = new Dictionary<uint, Container>();

        public static void Load()
        {
            LoadedPrisonLockerContainers = new Dictionary<uint, Container>();
            LoadedCopInvContainers = new Dictionary<uint, Container>();
            LoadedWeaponImportContainers = new Dictionary<uint, Container>();
            LoadedRefundContainers = new Dictionary<uint, Container>();
        }

        public static void CheckStaticContainerInserting(Container container)
        {
            if (container.Type == ContainerTypes.PRISONLOCKER) ContainerManager.LoadedPrisonLockerContainers.Add(container.Id, container);
            if (container.Type == ContainerTypes.Copinv) ContainerManager.LoadedCopInvContainers.Add(container.Id, container);
            if (container.Type == ContainerTypes.WEAPON_IMPORT) ContainerManager.LoadedWeaponImportContainers.Add(container.Id, container);
            if (container.Type == ContainerTypes.REFUND) ContainerManager.LoadedRefundContainers.Add(container.Id, container);
        }

        public static int GetMaxSlots(ContainerTypes type)
        {
            switch (type)
            {
                case ContainerTypes.STORAGE:
                case ContainerTypes.SHELTER:
                case ContainerTypes.MINEBASESTORAGE:
                    return 63;
                case ContainerTypes.STATIC:
                    return 109;
                default:
                    return 48;
            }
        }

        public static Container LoadContainer(uint id, ContainerTypes type, int tempDefaultSlots = 0, int tempDefaultWeight = 0)
        {
            if (id == 0)
            {
                return CreateContainer(id, type, tempDefaultSlots, tempDefaultWeight);
            }

            // SpecialTypes
            if (type == ContainerTypes.PRISONLOCKER && LoadedPrisonLockerContainers.ContainsKey(id)) return LoadedPrisonLockerContainers[id];
            if (type == ContainerTypes.Copinv && LoadedCopInvContainers.ContainsKey(id)) return LoadedCopInvContainers[id];
            if (type == ContainerTypes.WEAPON_IMPORT && LoadedWeaponImportContainers.ContainsKey(id)) return LoadedWeaponImportContainers[id];
            if (type == ContainerTypes.REFUND && LoadedRefundContainers.ContainsKey(id)) return LoadedRefundContainers[id];

            string query = $"SELECT * FROM `{GetTableName(type)}` WHERE `id` = '{id}';";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return new Container(reader);
                        }
                    }
                    else
                    {
                        return CreateContainer(id, type, tempDefaultSlots, tempDefaultWeight);
                    }
                }
            }
            return null;
        }
        public static Container CreateContainer(uint id, ContainerTypes type, int tempDefaultSlots = 0, int tempDefaultWeight = 0)
        {
            if (tempDefaultSlots == 0 || tempDefaultWeight == 0)
            {
                tempDefaultSlots = ContainerMetaData.metaDataDic[type].DefaultSlots;
                tempDefaultWeight = ContainerMetaData.metaDataDic[type].DefaultWeight;
            }

            string query = $"INSERT INTO `{GetTableName(type)}` (`id`, `max_weight`, `max_slots`, `type`) VALUES ('{id}', '{tempDefaultWeight}', '{tempDefaultSlots}', '{(int)type}');";

            MySQLHandler.ExecuteAsync(query, true);

            return new Container(id, type, tempDefaultWeight, tempDefaultSlots, new Dictionary<int, Item>());
        }

        public static bool IsItemRestrictedToTakeOut(this Container container, ItemModel itemModel)
        {
            // Disable globaly
            if (container.Type == ContainerTypes.FUELSTATION || container.Type == ContainerTypes.NIGHTCLUB || container.Type == ContainerTypes.MINECONTAINERSCHMELZE)
            {
                return true;
            }

            // Nix aus Aser nehmen kappa
            if (container.Type == ContainerTypes.STATIC && container.Id == (int)StaticContainerTypes.ASERLSPD)
            {
                if(StaticContainerModule.Instance.Get((int)StaticContainerTypes.ASERLSPD).Locked) // Nur bei Locked kanns nicht entnommen werden (unlock bei Heist...)
                    return true;
            }
            return false;
        }
        
        public static bool IsItemRestrictedForContainer(this Container container, uint itemModelId, DbPlayer dbPlayer = null)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);
            if (container == null || itemModel == null) return false;

            return container.IsItemRestrictedForContainer(itemModel, dbPlayer);
        }

        public static bool IsItemRestrictedForContainer(this Container container, ItemModel item, DbPlayer dbPlayer = null)
        {
            // großes Geschenk oder Verbandskasten (Fraktion) oder Schutzweste (Fraktion)
            if (item.Id == 550 || item.Id == 654 || item.Id == 655) return true;
            
            // Nichts reinlegen GARNICHTS legen...
            if (container.Type == ContainerTypes.RAFFINERY || container.Type == ContainerTypes.TeamOrder || 
                container.Type == ContainerTypes.METHLABORATORYOUTPUT || 
                (container.Type == ContainerTypes.PRISONLOCKER && (dbPlayer.Player.Position.DistanceTo(Coordinates.PrisonLockerTakeOut) < 3.0f ||
                dbPlayer.Player.Position.DistanceTo(Coordinates.LSPDLockerPutOut) < 3.0f))
                ) return true;
            
            // Mining zeugs
            if (container.Type == ContainerTypes.MINECONTAINERALU || container.Type == ContainerTypes.MINECONTAINERBRONCE
                || container.Type == ContainerTypes.MINECONTAINERIRON || container.Type == ContainerTypes.MINECONTAINERZINK) return true;

            // Workstations
            if (container.Type == ContainerTypes.WORKSTATIONOUTPUT) return true;

            // Nichts in TeamOrders legen...
            if (container.Type == ContainerTypes.TeamOrder) return true;

            if (container.Type == ContainerTypes.MINECONTAINERSCHMELZCOAL)
            {
                if (item.Id != JobMineFactionModule.Instance.Coal) return true;
            }

            if (container.Type == ContainerTypes.MINECONTAINERSCHMELZE)
            {
                if (item.Id != JobMineFactionModule.Instance.AluErz && item.Id != JobMineFactionModule.Instance.IronErz &&
                    item.Id != JobMineFactionModule.Instance.ZinkKohle && item.Id != JobMineFactionModule.Instance.Copper) return true;
            }

            if (container.Type == ContainerTypes.NIGHTCLUB)
            {
                if (!item.Script.Contains("nightclubchest")) return true;
            }
            
            // Special Shelters Bratwa // Brigada
            if (container.Type == ContainerTypes.STATIC)
            {
                if (container.Id == 3 || container.Id == 4)
                {
                    //Eisenbarren,Aluminiumbarren,Bronzebarren,Plastik,Glas,Holz,Zement
                    if (item.Id != 300 && item.Id != 310 && item.Id != 312 && item.Id != 462 && item.Id != 464 && item.Id != 468 && item.Id != 466 && item.Id != 469 && item.Id != 470 && item.Id != 472 && item.Id != 473) return true;
                }
            }

            if (container.Type == ContainerTypes.METHLABORATORYINPUT)
                if (!MethlaboratoryModule.RessourceItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.METHLABORATORYOUTPUT)
                if (!MethlaboratoryModule.EndProductItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.METHLABORATORYFUEL)
                if (MethlaboratoryModule.FuelItemId != item.Id)
                    return true;
            if (container.Type == ContainerTypes.WEAPONLABORATORYINPUT)
                if (!WeaponlaboratoryModule.RessourceItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.CANNABISLABORATORYINPUT)
                if (!CannabislaboratoryModule.RessourceItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.WEAPONLABORATORYOUTPUT)
                if (!WeaponlaboratoryModule.RessourceItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.CANNABISLABORATORYOUTPUT)
                if (!CannabislaboratoryModule.RessourceItemIds.Contains(item.Id))
                    return true;
            if (container.Type == ContainerTypes.WEAPONLABORATORYFUEL)
                if (WeaponlaboratoryModule.FuelItemId != item.Id)
                    return true;
            if (container.Type == ContainerTypes.CANNABISLABORATORYFUEL)
                if (CannabislaboratoryModule.FuelItemId != item.Id)
                    return true;
            if (container.Type == ContainerTypes.BLACKMONEYCODES)
                if (SchwarzgeldModule.BankNotes != item.Id)
                    return true;
            if (container.Type == ContainerTypes.BLACKMONEYBATTERIE)
                if (SchwarzgeldModule.Batteries != item.Id)
                    return true;
            if (container.Type == ContainerTypes.BLACKMONEYINVENTORY)
                if (SchwarzgeldModule.SchwarzgeldId != item.Id)
                    return true;

            if (container.Type == ContainerTypes.STATIC && container.Id == (int)StaticContainerTypes.ASERLSPD)
            {
                if (item.Id != AsservatenkammerModule.AserBigWeaponId &&
                    item.Id != AsservatenkammerModule.AserAmmoId &&
                    item.Id != AsservatenkammerModule.AserDrugId &&
                    item.Id != AsservatenkammerModule.AserItemId &&
                    item.Id != AsservatenkammerModule.AserWeaponId) return true;
            }

            // Planningroom Wardrobe
            if (container.Type == ContainerTypes.PLANNINGROOMWARDROBE)
            {
                if (item.Id != PlanningModule.Instance.CasinoRequiredOutfitId) return true;
            }

            // Nur Benzin in Tankstellen
            if (container.Type == ContainerTypes.FUELSTATION)
            {
                if (item.Id != FuelStationModule.BenzinModelId) return true;
            }

            if (container.Type == ContainerTypes.LABOR_MEERTRAEUBEL)
            {
                if (item.Id != MeertraeubelModul.NassMeetr) return true;
            }

            if (container.Type == ContainerTypes.TEAMFIGHT) return true;

            if (container.Type == ContainerTypes.WEAPON_IMPORT) return true;

            if (container.Type == ContainerTypes.VEHICLE || container.Type == ContainerTypes.FVEHICLE)
            {
                var delVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(container.Id);
                if (delVeh == null || !delVeh.IsValid()) return true;

                //Wenn Item Rohöl oder Benzin dann nicht in die normalen Fahrzeuge legbar
                if (item.Id == FuelStationModule.RohoelModelId || item.Id == FuelStationModule.BenzinModelId)
                {
                    if (!VehicleHandler.Instance.isFuelCar(delVeh)) return true;
                }

                // Wenn in Items Fahrzeuge allowed dann prüfe
                if (item.AllowedVehicleModels.Count > 0)
                {
                    if (!item.AllowedVehicleModels.Contains(delVeh.Data.Id)) return true;
                }

                // Wenn in Fahrzeugen Items allowed dann prüfe
                if (delVeh.Data.AllowedItems.Count > 0)
                {
                    if (!delVeh.Data.AllowedItems.Contains(item.Id)) return true;
                }
            }
            
            return false;
        }

        public static void ShowFriskInventory(this Container container, DbPlayer dbPlayer, DbPlayer target, string optionalname = " ", int money = 0)
        {
            if (dbPlayer.IsACop() && dbPlayer.Duty)
            {
                dbPlayer.SetData("disableFriskInv", true);
                if (!String.IsNullOrWhiteSpace(optionalname) && target != null && target.IsValid())
                {
                    dbPlayer.SetData("friskInvUserName", optionalname);
                    dbPlayer.SetData("friskInvUserID", target.Id);
                }
            }
            else
            {
                dbPlayer.SetData("disableinv", true);
            }

            List<ClientContainerObject> containerList = new List<ClientContainerObject>();
            containerList.Add(container.ConvertForClient(1, "Inventar von " + optionalname?.Replace('_', ' '), money));
            ComponentManager.Get<InventoryWindow>().Show()(dbPlayer, containerList);
        }

        public static void ShowVehFriskInventory(this Container container, DbPlayer dbPlayer, string optionalname = " ")
        {
            if (dbPlayer.IsACop() && dbPlayer.Duty)
            {
                dbPlayer.SetData("disableFriskInv", true);
                if (!String.IsNullOrWhiteSpace(optionalname))
                {
                    dbPlayer.SetData("friskInvVeh", optionalname);
                }
            }
            else
            {
                dbPlayer.SetData("disableinv", true);
            }

            List<ClientContainerObject> containerList = new List<ClientContainerObject>();
            containerList.Add(container.ConvertForClient(1, "Inventar von " + optionalname?.Replace('_', ' ')));
            ComponentManager.Get<InventoryWindow>().Show()(dbPlayer, containerList);
        }

        public static void ShowHouseFriskInventory(this Container container, DbPlayer dbPlayer, uint dimension)
        {
            if (dbPlayer.IsACop() && dbPlayer.Duty)
            {
                dbPlayer.SetData("disableFriskInv", true);
                House house;
                if (dbPlayer.DimensionType[0] == DimensionType.House && dbPlayer.HasData("inHouse") && (house = HouseModule.Instance.Get(dbPlayer.GetData("inHouse"))) != null)
                {
                    dbPlayer.SetData("friskInvHouse", dbPlayer.Player.Dimension);
                }
                else
                {
                    dbPlayer.SetData("disableinv", true);
                    return;
                }
            }
            else
            {
                dbPlayer.SetData("disableinv", true);
            }

            List<ClientContainerObject> containerList = new List<ClientContainerObject>();
            containerList.Add(container.ConvertForClient(1, "Haus Inventar"));
            ComponentManager.Get<InventoryWindow>().Show()(dbPlayer, containerList);
        }

        public static ClientContainerObject ConvertForClient(this Container container, int sendId, string optionalname = "", int money = 0, int blackmoney = 0)
        {
            if (container == null) return null;
            ClientContainerObject clientContainerObject = new ClientContainerObject(container.MaxWeight, container.MaxSlots);

            clientContainerObject.Slots = new List<ClientContainerSlotObject>();

            if (container == null || container.Slots == null) return null;

            try
            {
                foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
                {
                    if (kvp.Value != null && kvp.Value.Amount > 0)
                    {
                        Item firstItem = kvp.Value;
                        if (firstItem == null) continue;
                        clientContainerObject.Slots.Add(new ClientContainerSlotObject((int)firstItem.Id, kvp.Key, firstItem.Model, firstItem.Data, kvp.Value.Amount));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            clientContainerObject.Id = sendId;
            clientContainerObject.Name = optionalname != "" ? optionalname : ContainerMetaData.metaDataDic[container.Type].InventoryName;
            clientContainerObject.Money = money;
            clientContainerObject.Blackmoney = blackmoney;

            return clientContainerObject;
        }

        public static int GetInventoryUsedSpace(this Container container)
        {
            if (container == null) return 0;
            int used = 0;

            foreach (KeyValuePair<int, Item> items in container.Slots.ToList())
            {
                if (items.Value.Id == 0 || items.Value.Amount == 0) continue;
                used += items.Value.Model.Weight * items.Value.Amount;
            }
            return used;
        }

        public static int GetUsedSlots(this Container container)
        {
            int usedSlots = 0;

            foreach (Item items in container.Slots.Values.ToList())
            {
                if (items.Amount != 0) usedSlots += 1;
            }
            return usedSlots;
        }

        public static bool RemoveIllegalItems(this Container container)
        {
            List<int> touchedSlots = new List<int>();
            foreach (KeyValuePair<int, Item> items in container.Slots.ToList())
            {
                if (items.Value.Model.Illegal)
                {
                    container.SetSlotClear(items.Key);
                    if (!touchedSlots.Contains(items.Key)) touchedSlots.Add(items.Key);
                }
            }

            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);

            return true;
        }

        public static bool ClearInventory(this Container container)
        {
            List<int> touchedSlots = new List<int>();

            for (int i = 0; i < GetMaxSlots(container.Type); i++)
            {
                container.SetSlotClear(i);
                if (!touchedSlots.Contains(i)) touchedSlots.Add(i);
            }

            if (container.Type == ContainerTypes.PLAYER)
            {
                container.MaxSlots = 6;
                container.MaxWeight = 25000;
            }

            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);

            container.SaveMetaData();
            return true;
        }

        public static int GetInventoryFreeSlots(this Container container)
        {
            int slots = 0;
            foreach(KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value != null && kvp.Value.Id == 0) slots++;
            }
            return slots;
        }

        public static int GetInventoryFreeSpace(this Container container)
        {
            return container.MaxWeight - container.GetInventoryUsedSpace();
        }

        public static bool CanInventoryItemAdded(this Container container, uint itemId, int amount = 1)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (container == null || itemModel == null) return false;

            return CanInventoryItemAdded(container, itemModel, amount);
        }

        public static bool CanInventoryItemAdded(this Container container, ItemModel itemModel, int amount = 1)
        {
            if (container == null || itemModel == null) return false;
            if (amount <= 0) return false;
            int slotCount = 0;
            int tmpamountsize = amount;
            // Check Slots (Weight is checked in return)
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (slotCount == container.MaxSlots) break;
                slotCount++;
                if (kvp.Value.Id == 0) tmpamountsize -= itemModel.MaximumStacksize;
                else if (kvp.Value.Id == itemModel.Id && kvp.Value.Amount + amount <= kvp.Value.Model.MaximumStacksize) tmpamountsize -= (kvp.Value.Model.MaximumStacksize - kvp.Value.Amount);

                if (tmpamountsize <= 0) break;
            }

            return itemModel.Weight * amount <= container.GetInventoryFreeSpace() && tmpamountsize <= 0;
        }

        public static bool CanInventoryWeaponAndAmmoAdded(this Container container, ItemModel weapon, ItemModel mags, int amount = 1)
        {
            if (container == null || mags == null || weapon == null) return false;
            if (amount <= 0) return false;
            bool weaponHasSpace = false;
            int tmpamountsize = amount;
            int slotCount = 0;
            // Check Slots (Weight is checked in return)
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (slotCount == container.MaxSlots) break;
                slotCount++;
                if (!weaponHasSpace && kvp.Value.Id == 0)
                {
                    weaponHasSpace = true;
                    continue;
                }

                if (kvp.Value.Id == 0) tmpamountsize -= mags.MaximumStacksize;
                else if (kvp.Value.Id == mags.Id && kvp.Value.Amount + amount <= kvp.Value.Model.MaximumStacksize) tmpamountsize -= (kvp.Value.Model.MaximumStacksize - kvp.Value.Amount);

                if (tmpamountsize <= 0 && weaponHasSpace) break;
            }



            return (mags.Weight * amount + weapon.Weight) <= container.GetInventoryFreeSpace() && tmpamountsize <= 0 && weaponHasSpace;
        }

        public static int GetMaxItemAddedAmount(this Container container, uint itemModelId)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);

            return container.GetMaxItemAddedAmount(itemModel);
        }

        public static int GetMaxItemAddedAmount(this Container container, ItemModel itemModel)
        {
            int slotAmount = 0;
            int weightAmount = 0;

            weightAmount = container.GetInventoryFreeSpace() / itemModel.Weight;

            int freeSlots = container.Slots.Where(cs => cs.Value == null || cs.Value.Id == 0).Count();
            slotAmount = freeSlots * itemModel.MaximumStacksize;

            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Id == itemModel.Id)
                {
                    slotAmount += itemModel.MaximumStacksize - kvp.Value.Amount;
                }
            }

            return slotAmount <= weightAmount ? slotAmount : weightAmount;
        }

        public static bool IsEmpty(this Container container)
        {
            return container.Slots.Values.ToList().Where(sl => sl.Id > 0).Count() <= 0;
        }

        public static bool AddItem(this Container container, uint itemId, int amount = 1, Dictionary<string, dynamic> data = null, int slot = -1, bool disablesave = false)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (itemModel == null) return false;

            return AddContainerItem(container, itemModel, amount, data, slot, disablesave);
        }

        public static bool AddItem(this Container container, ItemModel ItemModel, int amount = 1, Dictionary<string, dynamic> data = null, int slot = -1, bool disablesave = false)
        {
            if (ItemModel == null) return false;

            return AddContainerItem(container, ItemModel, amount, data, slot, disablesave);
        }

        public static DbPlayer GetPlayerByContainer(this Container container)
        {
            DbPlayer dbPlayer = Players.Players.Instance.GetValidPlayers().Find(p => p.Container.Id == container.Id);
            if (dbPlayer == null || !dbPlayer.IsValid()) return null;
            else return dbPlayer;
        }

        private static bool AddContainerItem(this Container container, ItemModel model, int amount, Dictionary<string, dynamic> data = null, int slot = -1, bool disablesave = false)
        {
            if (container == null) return false;
            if (model == null) return false;
            if (amount == 0) return false;

            // Workaround FUNK
            if (model.ItemModelType == ItemModelTypes.Radio)
            {
                if (container.Type == ContainerTypes.PLAYER)
                {
                    DbPlayer iPlayer = container.GetPlayerByContainer();
                    if (iPlayer != null && iPlayer.IsValid())
                    {
                        data = new Dictionary<string, dynamic> { { "Fq", (double)0.0 }, { "Volume", 5 } };
                    }
                }
            }

            // Nightclub transferItem
            if (container.Type == ContainerTypes.NIGHTCLUB && model.Script.ToLower().StartsWith("nightclubchest_"))
            {
                if (UInt32.TryParse(model.Script.Split('_')[1], out uint modelId))
                {
                    model = ItemModelModule.Instance.Get(modelId);
                    amount = amount * 5;
                }
            }

            List<int> touchedSlots = new List<int>();

            int leftamount = amount;

            try
            {
                while(leftamount > 0)
                {
                    if (slot == -1)
                    {
                        slot = container.GetSlotOfSimilairSingleItemsToStack(model.Id);


                        if (slot >= 0 && container.Slots[slot].Id == model.Id) // Slot zum stacken gefunden && Item is item
                        {
                            // Wenn Stacksize erreicht dann irgendwo...
                            if (container.Slots[slot].Amount >= container.Slots[slot].Model.MaximumStacksize)
                            {
                                AddContainerItem(container, model, leftamount, data);
                                break;
                            }
                            else
                            {
                                int ammountCanAdded = container.Slots[slot].Model.MaximumStacksize - container.Slots[slot].Amount; // Stacksize - actual = left

                                if (ammountCanAdded >= leftamount)
                                {
                                    container.Slots[slot].Amount += leftamount;
                                    leftamount = 0;
                                    if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                                    break;
                                }
                                else
                                {
                                    container.Slots[slot].Amount += ammountCanAdded;
                                    leftamount -= ammountCanAdded;
                                }
                            }
                        }
                        else
                        {
                            slot = container.GetNextFreeSlot();
                            if (slot == -1) return false;

                            if(leftamount > model.MaximumStacksize)
                            {
                                container.Slots[slot] = data == null ? new Item(model.Id, model.MaximumStacksize, model.MaximumStacksize, new Dictionary<string, dynamic>()) : new Item(model.Id, model.MaximumStacksize, model.MaximumStacksize, data);
                                leftamount -= model.MaximumStacksize;
                            }
                            else
                            {
                                container.Slots[slot] = data == null ? new Item(model.Id, model.MaximumStacksize, leftamount, new Dictionary<string, dynamic>()) : new Item(model.Id, model.MaximumStacksize, leftamount, data);
                                leftamount = 0;
                            }
                        }
                    }
                    else if (slot >= 0)
                    {
                        // Wenn was auf dem Slot ist...
                        if (container.Slots[slot] != null && container.Slots[slot].Id != 0)
                        {
                            // Wenn Stacksize erreicht oder item model not compare dann irgendwo...
                            if (container.Slots[slot].Amount >= container.Slots[slot].Model.MaximumStacksize || container.Slots[slot].Model.Id != model.Id)
                            {
                                AddContainerItem(container, model, leftamount, data);
                                break;
                            }
                            else
                            {
                                int ammountCanAdded = container.Slots[slot].Model.MaximumStacksize - container.Slots[slot].Amount; // Stacksize - actual = left

                                if (ammountCanAdded >= leftamount)
                                {
                                    container.Slots[slot].Amount += leftamount;
                                    leftamount = 0;
                                    if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                                    break;
                                }
                                else
                                {
                                    container.Slots[slot].Amount += ammountCanAdded;
                                    leftamount -= ammountCanAdded;
                                }
                            }
                        }
                        else
                        {
                            if (leftamount > model.MaximumStacksize)
                            {
                                container.Slots[slot] = data == null ? new Item(model.Id, model.MaximumStacksize, model.MaximumStacksize, new Dictionary<string, dynamic>()) : new Item(model.Id, model.MaximumStacksize, model.MaximumStacksize, data);
                                leftamount -= model.MaximumStacksize;
                            }
                            else
                            {
                                container.Slots[slot] = data == null ? new Item(model.Id, model.MaximumStacksize, leftamount, new Dictionary<string, dynamic>()) : new Item(model.Id, model.MaximumStacksize, leftamount, data);
                                leftamount = 0;
                            }
                        }
                    }

                    if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
            
            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "" && !disablesave)
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);

            // Workaround FUNK
            if (model.ItemModelType == ItemModelTypes.Radio)
            {
                if (container.Type == ContainerTypes.PLAYER)
                {
                    DbPlayer iPlayer = container.GetPlayerByContainer();
                    if (iPlayer != null && iPlayer.IsValid())
                    {
                        VoiceModule.Instance.ChangeFrequenz(iPlayer, 0f);
                        iPlayer.UpdateApps();
                    }
                }
            }
            return true;
        }

        public static void ChangeSlots(this Container container, int newSlots)
        {
            container.MaxSlots = newSlots;
            container.SaveMaxSlots();
        }

        public static void ChangeWeight(this Container container, int newWeight)
        {
            container.MaxWeight = newWeight;
            container.SaveMaxWeight();
        }

        public static bool EditFirstItemData(this Container container, ItemModel model, string key, dynamic data)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Model == model)
                {
                    kvp.Value.Data[key] = data;
                    return true;
                }
            }

            return false;
        }

        public static int GetFreeOrSimilairSlot(this Container container, ItemModel model, int amount)
        {
            if (amount <= 0 || model == null || container == null) return 0;
            if (container.GetSlotOfSimilairItemsToStackAmount(model, amount) != -1)
            {
                return container.GetSlotOfSimilairItemsToStackAmount(model, amount);
            }
            return container.GetNextFreeSlot();
        }

        public static void RemoveItem(this Container container, uint itemId, int amount = 1, bool disablesave = false)
        {
            List<int> affectedSlots = new List<int>();
            if (amount <= 0 || itemId == 0 || container == null) return;

            ItemModel model = ItemModelModule.Instance.Get(itemId);

            RemoveItem(container, model, amount, disablesave);
            return;
        }

        public static void RemoveItem(this Container container, ItemModel model, int amount = 1, bool disablesave = false)
        {
            if (amount <= 0 || model == null || container == null) return;

            int tmpAmount = amount;

            List<int> touchedSlots = new List<int>();
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value != null && kvp.Value.Id == model.Id)
                {
                    if (kvp.Value.Amount <= tmpAmount)
                    {
                        tmpAmount -= kvp.Value.Amount;
                        container.SetSlotClear(kvp.Key); // Wenn restbetrag größer als aktuell..
                        if (!touchedSlots.Contains(kvp.Key)) touchedSlots.Add(kvp.Key);
                    }
                    else if (tmpAmount > 0)
                    {
                        kvp.Value.Amount -= tmpAmount; // Restbetrag abziehen
                        tmpAmount = 0; // finish
                        if (!touchedSlots.Contains(kvp.Key)) touchedSlots.Add(kvp.Key);
                        break;
                    }
                    else break;
                }
            }


            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "" && !disablesave)
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
        }

        public static void CheckFunkDisabling(this Container container)
        {
            if (container.Type == ContainerTypes.PLAYER)
            {
                DbPlayer iPlayer = container.GetPlayerByContainer();
                if (iPlayer != null)
                {
                    if (!VoiceModule.Instance.hasPlayerRadio(iPlayer)) Voice.VoiceModule.Instance.turnOffFunk(iPlayer);
                    return;
                }
            }
        }

        public static void RemoveItemSlotFirst(this Container container, ItemModel model, int slot, int amount = 1)
        {
            if (container == null || model == null) return;
            if (amount <= 0) return;

            if (container.Slots[slot] != null && container.Slots[slot].Model == model)
            {
                if (container.Slots[slot].Amount <= amount) // Wenn mehr als dieser Slot,, dann 0 und abziehen von max
                {
                    amount -= container.Slots[slot].Amount;
                    container.SetSlotClear(slot);
                    if (amount > 0) RemoveItem(container, model, amount); // ziehe das andere dann iwo per removeitem raus
                }
                else
                {
                    container.Slots[slot].Amount -= amount;
                }
            }

            string l_ContainerSaveQuery = "";
            l_ContainerSaveQuery += container.GetSlotSaveQuery(slot);

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
            return;
        }

        public static void RemoveFromSlot(this Container container, int slot, int amount = 1)
        {
            Item l_Item = container.Slots[slot];
            l_Item.Amount = container.Slots[slot].Amount - amount;
            if (l_Item.Amount <= 0)
            {
                l_Item = new Item(0, 0, 0, new Dictionary<string, dynamic>());
            }

            container.Slots[slot] = l_Item;

            string l_ContainerSaveQuery = "";
            l_ContainerSaveQuery += container.GetSlotSaveQuery(slot);

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
        }

        public static void RemoveAllFromSlot(this Container container, int slot)
        {
            container.Slots[slot] = new Item(0, 0, 0, new Dictionary<string, dynamic>());

            string l_ContainerSaveQuery = "";
            l_ContainerSaveQuery += container.GetSlotSaveQuery(slot);

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
        }

        public static bool RemoveItemAll(this Container container, uint itemId)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            return container.RemoveItemAll(itemModel);
        }

        public static bool RemoveItemAll(this Container container, ItemModel itemModel)
        {
            if (itemModel == null) return false;
            List<int> touchedSlots = new List<int>();
            foreach (KeyValuePair<int, Item> listContainerSavedItem in container.Slots.ToList())
            {
                if (listContainerSavedItem.Value.Model == itemModel)
                {
                    container.SetSlotClear(listContainerSavedItem.Key);
                    touchedSlots.Add(listContainerSavedItem.Key);
                }
            }

            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);

            return true;
        }

        public static int GetItemAmount(this Container container, uint itemId)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (container == null || itemModel == null) return 0;
            return GetItemAmount(container, itemModel);
        }

        public static int GetItemAmount(this Container container, ItemModel model)
        {
            int amount = 0;

            if (container == null || model == null)
            {
                return 0;
            }

            try
            {
                foreach (KeyValuePair<int, Item> listContainerSavedItem in container.Slots.ToList())
                {
                    if (listContainerSavedItem.Value.Model == null) continue;
                    ItemModel l_Model = listContainerSavedItem.Value.Model;
                    if (l_Model != model) continue;
                    amount += listContainerSavedItem.Value.Amount;
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
                return 0;
            }

            return amount;
        }

        public static void SaveAll(this Container container)
        {
            string l_ContainerSaveQuery = "";
            for (int i = 0; i < container.MaxSlots; i++)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(i);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
        }

        public static Item GetItemById(this Container container, int id)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Id == id)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public static List<Item> GetItemsByDataKey(this Container container, string key)
        {
            List<Item> items = new List<Item>();

            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Data != null && kvp.Value.Data.ContainsKey(key))
                {
                    items.Add(kvp.Value);
                }
            }
            return items;
        }

        public static Item GetFirstItemByModel(this Container container, ItemModel model)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Model == model)
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public static Item GetItemOnSlot(this Container container, int slot)
        {
            if (slot < 0 || slot > 47 || !container.Slots.ContainsKey(slot)) return null;
            return container.Slots[slot];
        }

        private static int GetNextFreeSlot(this Container container)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Key >= container.MaxSlots) return -1;
                if (kvp.Value.Id == 0) return kvp.Key;
            }
            return -1;
        }

        internal static int GetSlotOfSimilairSingleItemsToStack(this Container container, uint itemId)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (container == null || itemModel == null) return -1;

            return GetSlotOfSimilairItemsToStackAmount(container, itemModel, 1);
        }

        internal static int GetSlotOfSimilairSingleItemsToStack(this Container container, ItemModel model)
        {
            if (container == null || model == null) return -1;

            return GetSlotOfSimilairItemsToStackAmount(container, model, 1);
        }

        internal static int GetAmountOfItemsOnSlot(this Container container, int slot)
        {
            if (container == null || slot == -1) return -1;

            return container.Slots[slot].Amount;
        }

        internal static int GetSlotOfSimilairItemsToStackAmount(this Container container, ItemModel model, int amount)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Id == model.Id && kvp.Value.Amount + amount <= kvp.Value.Model.MaximumStacksize) return kvp.Key;
            }
            return -1;
        }

        internal static int GetSlotOfSimilairSingleItems(this Container container, uint itemId)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemId);
            if (container == null || itemModel == null) return -1;

            return GetSlotOfSimilairItems(container, itemModel);
        }

        internal static int GetSlotOfSimilairSingleItems(this Container container, ItemModel model)
        {
            if (container == null || model == null) return -1;

            return GetSlotOfSimilairItems(container, model);
        }

        internal static int GetSlotOfSimilairItems(this Container container, ItemModel model)
        {
            foreach (KeyValuePair<int, Item> kvp in container.Slots.ToList())
            {
                if (kvp.Value.Id == model.Id && kvp.Value.Amount >= 1) return kvp.Key;
            }
            return -1;
        }

        private static bool CheckSimilairItemsOnSlotAndFits(this Container container, int slot, uint itemId, int amount = 1)
        {
            if (amount == 0) return false;
            return container.Slots[slot].Id == itemId && container.Slots[slot].Amount + amount <= container.Slots[slot].Model.MaximumStacksize;
        }

        internal static bool AddItemData(this Container container, Item item, string key, dynamic value)
        {
            var status = item.Data.TryAdd(key, value);
            if (status)
            {
                container.SaveAll();
            }
            return status;
        }

        public static bool ResortInventory(this Container container)
        {
            List<Int32> touchedSlots = new List<int>();
            var nextFreeSlot = GetNextFreeSlot(container);
            var x = container.Slots.Last(y => y.Value.Amount != 0);

            while (nextFreeSlot <= x.Key && nextFreeSlot != -1)
            {
                container.Slots[nextFreeSlot] = container.Slots[x.Key];
                container.SetSlotClear(x.Key);

                // call to save
                touchedSlots.Add(x.Key);
                touchedSlots.Add(nextFreeSlot);

                nextFreeSlot = GetNextFreeSlot(container);
                x = container.Slots.Last(y => y.Value.Amount != 0);
            }

            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots)
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery, true);
            return true;
        }

        private static void SetSlotClear(this Container container, int slot)
        {
            container.Slots[slot] = new Item(0, 0, 0, new Dictionary<string, dynamic>());
        }

        private static bool CheckSimilairItemsOnSlot(this Container container, int slot, ItemModel model)
        {
            return container.Slots[slot].Id == model.Id && container.Slots[slot].Amount >= 0;
        }

        public static ItemModel GetModelOnSlot(this Container container, int slot)
        {
            if (slot < 0 || !container.Slots.ContainsKey(slot)) return null;
            return container.Slots[slot]?.Model;
        }

        public static string GetTableName(this Container container)
        {
            return GetTableName(container.Type);
        }

        public static void LoadIntoVehicle(this Container container, SxVehicle sxVehicle, uint itemModelId, int amount = -1)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);
            container.LoadIntoVehicle(sxVehicle, itemModel, amount);
            return;
        }

        public static void LoadIntoVehicle(this Container container, SxVehicle sxVehicle, ItemModel itemModel, int amount = -1)
        {
            if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.Container != null)
            {
                if (sxVehicle.Container.IsItemRestrictedForContainer(itemModel))
                {
                    return;
                }

                int maxAmount = sxVehicle.Container.GetMaxItemAddedAmount(itemModel);

                if (amount > maxAmount || amount == -1) amount = maxAmount;

                if (amount > 0) container.RemoveItem(itemModel, amount);
                else container.RemoveItemAll(itemModel);

                sxVehicle.Container.AddContainerItem(itemModel, amount);
            }
        }

        public static void MoveIntoAnother(this Container container, Container externalContainer, uint itemModelId, int amount = -1)
        {
            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);
            container.MoveIntoAnother(externalContainer, itemModel, amount);
        }

        public static void MoveIntoAnother(this Container container, Container externalContainer, ItemModel itemModel, int amount = -1)
        {
            if (externalContainer != null)
            {
                if (externalContainer.IsItemRestrictedForContainer(itemModel))
                {
                    return;
                }

                int maxAmount = externalContainer.GetMaxItemAddedAmount(itemModel);

                if (amount > maxAmount || amount == -1) amount = maxAmount;

                if (amount > 0) container.RemoveItem(itemModel, amount);
                else container.RemoveItemAll(itemModel);

                externalContainer.AddContainerItem(itemModel, amount);
            }
        }

        public static string GetTableName(ContainerTypes type)
        {
            return ContainerMetaData.metaDataDic[type].DatabaseTable;
        }
    }

    public class ContainerManagerHandler
    {
        public event EventHandler ThresholdReached;

        protected virtual void OnThresholdReached(EventArgs e)
        {
            EventHandler handler = ThresholdReached;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
