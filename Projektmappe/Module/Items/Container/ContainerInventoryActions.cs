using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkMethods;
using GVRP.Handler;
using GVRP.Module.Asservatenkammer;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Items.Scripts;
using GVRP.Module.JobFactions.Mine;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.RemoteEvents;
using GVRP.Module.Vehicles;
using GVRP.Module.Workstation;

namespace GVRP.Module.Items
{
    public static class ContainerInventoryActions
    {
        public static DiscordHandler Discord = new DiscordHandler();
        public static async Task<bool> MoveItemToInventory(DbPlayer dbPlayer, ContainerMoveTypes containerMoveType, int sourceSlot, int destinationslot, int amount)
        {
            try { 
            // Player Validationchecks
            if (!dbPlayer.CanUseAction() || !dbPlayer.CanAccessRemoteEvent()) return false;
            
            // External Container
            Container eXternContainer = ItemsModule.Instance.findInventory(dbPlayer.Player);
            if (eXternContainer == null || eXternContainer.Locked) return false;

            ItemModel model = containerMoveType == ContainerMoveTypes.SelfInventory ? dbPlayer.Container.GetModelOnSlot(sourceSlot) : eXternContainer.GetModelOnSlot(sourceSlot);

            // Return on wrong amount, wrong model or not found external container
            if (amount <= 0 || model == null) return false;
            
            // From Self To Another
            if (containerMoveType == ContainerMoveTypes.SelfInventory)
            {
                if (amount > dbPlayer.Container.Slots[sourceSlot].Amount) return false;

                // Rucksack
                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null && !ItemScript.backpack(dbPlayer, model, true))
                {
                    dbPlayer.SendNewNotification( $"Der Rucksack konnte nicht entfernt werden!");
                    return false;
                } 
                
                //Prüfen ob gegenüber das Item in der Anzahl aufnehmen kann
                if (!eXternContainer.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Das Inventar reicht nicht aus!");
                    return false;
                }

                if (eXternContainer.IsItemRestrictedForContainer(model, dbPlayer))
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                    return false;
                }
                
                if(eXternContainer.Type == ContainerTypes.WORKSTATIONFUEL || eXternContainer.Type == ContainerTypes.WORKSTATIONINPUT)
                {
                    Workstation.Workstation workstation = dbPlayer.GetWorkstation();
                    if(workstation != null)
                    {
                        if(eXternContainer.Type == ContainerTypes.WORKSTATIONFUEL && workstation.FuelItemId != model.Id)
                        {
                            dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                            return false;
                        }
                        
                        if (eXternContainer.Type == ContainerTypes.WORKSTATIONINPUT)
                        {
                            if (workstation.SourceItemId != model.Id)
                            {
                                dbPlayer.SendNewNotification("Dieses Item koennen sie hier nicht einlagern!");
                                return false;
                            }
                            
                            if ((eXternContainer.GetItemAmount(model.Id) + amount) > workstation.LimitedSourceSize)
                            {
                                dbPlayer.SendNewNotification($"Sie können hier maximal {workstation.LimitedSourceSize} {model.Name} einlagern!");
                                return false;
                            }
                        }
                    }
                }

                if (eXternContainer.Type == ContainerTypes.REFUND)
                {
                    dbPlayer.SendNewNotification("In das Erstattungsinventar können keine Gegenstände eingelagert werden.");
                    return false;
                }
                
                if (eXternContainer.Type == ContainerTypes.STATIC && eXternContainer.Id == (uint)StaticContainerTypes.ASERLSPD)
                {
                    dbPlayer.SendNewNotification("Hier kannst du keine Gegenstände verstauen");
                    return false;
                }
                
                MoveItemToAnotherContainer(dbPlayer.Container, eXternContainer, sourceSlot, destinationslot, amount);

                // Drug Infections
                if (model.CanDrugInfect()) dbPlayer.IncreasePlayerDrugInfection();

                if (eXternContainer != null)
                {
                    if (ServerFeatures.IsActive("itemlog"))
                    {
                        Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), model.Id, -amount, "" + eXternContainer.Type, (int) eXternContainer.Id);
                    }
                }
                await dbPlayer.PlayInventoryInteractAnimation();

                ContainerManager.CheckFunkDisabling(dbPlayer.Container);
                return true;

        }

            // From Another to Self
            if (containerMoveType == ContainerMoveTypes.ExternInventory)
            {
                if (amount > eXternContainer.Slots[sourceSlot].Amount) return false;

                // Teamshelter ohne rechte...
                if (eXternContainer.Type == ContainerTypes.SHELTER)
                {
                    if (!dbPlayer.TeamRankPermission.Inventory) return false;
                }

                //Prüfen ob gegenüber das Item in der Anzahl aufnehmen kann
                if (!dbPlayer.Container.CanInventoryItemAdded(model, amount))
                {
                    dbPlayer.SendNewNotification("Das Inventar reicht nicht aus!");
                    return false;
                }

                if (dbPlayer.Container.IsItemRestrictedForContainer(model)) // großes Geschenk
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht einlagern!");
                    return false;
                }

                if(AsservatenkammerModule.Instance.IsAserItem(model.Id))
                {
                    StaticContainer asserContainer = StaticContainerModule.Instance.Get(eXternContainer.Id);
                    if(asserContainer != null && asserContainer.Id == (uint)StaticContainerTypes.ASERLSPD)
                    {
                        if(asserContainer.Locked) // bei aufbruch offen deswegen abfrage hier...
                            return false;
                    }
                    else return false; // disablen wir erstmal bis andere Lösung
                }

                if (eXternContainer.IsItemRestrictedToTakeOut(model))
                {
                    dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                    return false;
                }

                if (eXternContainer.Type == ContainerTypes.MINEBASESTORAGE)
                {
                    if (!dbPlayer.TeamRankPermission.Inventory)
                    {
                        dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                        return false;
                    }
                }

                if(eXternContainer.Type == ContainerTypes.FVEHICLE)
                {
                    // Search for the Vehicle in Team Lists
                    SxVehicle sxvehicle = VehicleHandler.TeamVehicles[(uint)teams.TEAM_MINE1].ToList().Where(v => v.databaseId == eXternContainer.Id).FirstOrDefault();
                    if (sxvehicle == null || !sxvehicle.IsValid())
                        sxvehicle = VehicleHandler.TeamVehicles[(uint)teams.TEAM_MINE2].ToList().Where(v => v.databaseId == eXternContainer.Id).FirstOrDefault();

                    // Found? Player NOT mine1 or mine2
                    if (sxvehicle != null && sxvehicle.IsValid())
                    {
                        if(model.Id == JobMineFactionModule.Instance.AluBarren || model.Id == JobMineFactionModule.Instance.Batterien
                            || model.Id == JobMineFactionModule.Instance.BronceBarren || model.Id == JobMineFactionModule.Instance.IronBarren)
                        {
                            dbPlayer.SendNewNotification("Dieses Item koennen Sie hier nicht entnehmen!");
                            return false;
                        }
                    }
                }
                
                if (eXternContainer.Type == ContainerTypes.STATIC)
                {
                    var temp = StaticContainerModule.Instance.Get(eXternContainer.Id);
                    if (temp != null)
                    {
                        if (temp.TeamId != 0 && !dbPlayer.TeamRankPermission.Inventory)
                        {
                            dbPlayer.SendNewNotification("Du hast nicht die Befugniss hier Gegenstände rauszunehmen!");
                            return false;
                        }
                    }
                }


                MoveItemToAnotherContainer(eXternContainer, dbPlayer.Container, sourceSlot, destinationslot, amount);

                Console.WriteLine(dbPlayer + "MoveItemToAnotherContainer");
                    Discord.SendMessage(dbPlayer.GetName() + model.Id + amount + "MoveItemToAnotherContainer");

                    // Drug Infections
                    if (model.CanDrugInfect()) dbPlayer.IncreasePlayerDrugInfection();

                if (ServerFeatures.IsActive("itemlog"))
                {
                    Logger.SaveToItemLog(dbPlayer.Id, dbPlayer.GetName(), model.Id, amount, "" + eXternContainer.Type, (int) eXternContainer.Id);
                }

                await dbPlayer.PlayInventoryInteractAnimation();
                return true;
            }
            return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;
        }

        public static bool moveItemInInventory(DbPlayer dbPlayer, ContainerMoveTypes containerMoveType, int sourceSlot, int destinationslot, int amount)
        {

            try { 
            Console.WriteLine(dbPlayer + "moveItemInInventroy");
                Discord.SendMessage(dbPlayer.GetName() + amount + "moveItemInInventroy");
                // Player Validationchecks
                if (!dbPlayer.CanUseAction() || !dbPlayer.CanAccessRemoteEvent()) return false;

            // External Container
            Container eXternContainer = ItemsModule.Instance.findInventory(dbPlayer.Player);
            if (containerMoveType == ContainerMoveTypes.ExternInventory && (eXternContainer == null || eXternContainer.Locked)) return false;

            ItemModel model = containerMoveType == ContainerMoveTypes.SelfInventory ? dbPlayer.Container.GetModelOnSlot(sourceSlot) : eXternContainer.GetModelOnSlot(sourceSlot);

            // Return on wrong amount, wrong model or not found external container
            if (amount <= 0 || model == null) return false;

            // In Self Inventory
            if (containerMoveType == ContainerMoveTypes.SelfInventory)
            {
                if (amount > dbPlayer.Container.Slots[sourceSlot].Amount) return false;
                
                // Rucksack
                if (BackpackList.backpackList.Find(x => x.ItemModel == model) != null && !ItemScript.backpack(dbPlayer, model, true))
                {
                    dbPlayer.SendNewNotification( $"Der Rucksack konnte nicht verschoben werden!");
                    return false;
                }

                MoveItemToAnotherContainer(dbPlayer.Container, dbPlayer.Container, sourceSlot, destinationslot, amount);
                return true;
            }

            // From Another to Self
            if (containerMoveType == ContainerMoveTypes.ExternInventory)
            {
                if (eXternContainer.Type == ContainerTypes.STATIC && (eXternContainer.Id == 3 || eXternContainer.Id == 4))
                {
                    if (!dbPlayer.TeamRankPermission.Inventory) return false;
                }

                if (amount > eXternContainer.Slots[sourceSlot].Amount) return false;
                
                MoveItemToAnotherContainer(eXternContainer, eXternContainer, sourceSlot, destinationslot, amount);
                return true;
            }
            return false;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return false;

        }

        public static void FillInventorySlots(Container container)
        {
            try { 
            List<int> touchedSlots = new List<int>();
            Dictionary<uint, int> items = new Dictionary<uint, int>();

            foreach (KeyValuePair<int, Item> item in container.Slots.Where(i => i.Value != null && i.Value.Id != 0 && i.Value.Model.MaximumStacksize > 1).ToList())
            {
                if (!items.ContainsKey(item.Value.Id))
                {
                    items.Add(item.Value.Id, item.Value.Amount);
                }
                else
                {
                    items[item.Value.Id] += item.Value.Amount;
                }
                if (!touchedSlots.Contains(item.Key)) touchedSlots.Add(item.Key);
            }
            
            foreach (KeyValuePair<uint, int> kvp in items.ToList())
            {
                container.RemoveItem(kvp.Key, kvp.Value, true);
            }

            foreach (KeyValuePair<uint, int> kvp in items.ToList())
            {
                container.AddItem(kvp.Key, kvp.Value, new Dictionary<string, dynamic>(), -1, true);
            }
            
            // Add Keys for addings...
            foreach(KeyValuePair<int, Item> kvp in container.Slots.Where(i => i.Value != null && i.Value.Id != 0).ToList())
            {
                if (!touchedSlots.Contains(kvp.Key)) touchedSlots.Add(kvp.Key);
            }

            string l_ContainerSaveQuery = "";
            foreach (int l_Slot in touchedSlots.ToList())
            {
                l_ContainerSaveQuery += container.GetSlotSaveQuery(l_Slot);
            }

            if (l_ContainerSaveQuery != "")
                MySQLHandler.ExecuteAsync(l_ContainerSaveQuery);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void MoveItemToAnotherContainer(Container sourceContainer, Container externContainer, int sourceSlot, int destinationSlot, int amount)
        {
            externContainer.AddItem(sourceContainer.Slots[sourceSlot].Model, amount, sourceContainer.Slots[sourceSlot].Data, destinationSlot);
            sourceContainer.RemoveItemSlotFirst(sourceContainer.Slots[sourceSlot].Model, sourceSlot, amount);
        }
    }
}
