using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Business.Raffinery;
using GVRP.Module.Chat;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Farming
{
    public sealed class FarmProcessModule : SqlModule<FarmProcessModule, FarmProcess, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] {typeof(FarmSpotModule)};
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `farm_processes`;";
        }
        
        protected override bool OnLoad()
        {
            if (GetAll() != null)
            {
                foreach (var farmProcess in GetAll())
                {
                    farmProcess.Value.Ped?.Delete();
                }
            }
            MenuManager.Instance.AddBuilder(new FarmProcessMenuBuilder());
            return base.OnLoad();
        }

        public FarmProcess GetByPosition(Vector3 position)
        {
            return GetAll().FirstOrDefault(farmProcess =>
                farmProcess.Value.NpcPosition.DistanceTo(position) <= 5).Value;
        }

        public void FarmProcessAction(FarmProcess farmProcess, DbPlayer dbPlayer, Container UsingContainer, SxVehicle sxVehicle = null)
        {
            Dictionary<ItemModel, int> required = farmProcess.RequiredItems;
            Dictionary<ItemModel, int> canDo = new Dictionary<ItemModel, int>();

            if (!dbPlayer.CanInteract()) return; // already verarbeiten...

            //Geh jedes Item was benötigt wird durch
                foreach (var item in required)
            {
                //Rechte die maximal mögliche anzahl der herstellung
                decimal maxProcess = Math.Floor((decimal)UsingContainer.GetItemAmount(item.Key.Id) / (decimal)item.Value);
                canDo.Add(item.Key, (int)maxProcess);
            }

            //Kleinste Eintrag welche hergestellt werden kann
            var min = canDo.FirstOrDefault(x => x.Value == canDo.Values.Min());

            //Wenn kleinster eintrag = 0 ist, dann fehlen Items um herstellen zu können
            if (min.Value == 0)
            {
                dbPlayer.SendNewNotification($"Sie benoetigen mindestens {required.First().Value} {min.Key.Name}!");
                return;
            }

            int requiredItemsWeight = 0;

            foreach (KeyValuePair<ItemModel, int> kvp in farmProcess.RequiredItems)
            {
                requiredItemsWeight += (kvp.Key.Weight * kvp.Value * min.Value); // Pro Item * gewicht des Items * minimal Menge die hergestellt wird
            }

            if ((UsingContainer.GetInventoryFreeSpace() + requiredItemsWeight) < (ItemModelModule.Instance.Get(farmProcess.RewardItemId).Weight * farmProcess.RewardItemAmount * min.Value))
            {
                dbPlayer.SendNewNotification("Dein Inventar ist voll!");
                return;
            }

            int farmingtime = (farmProcess.RequiredTime * min.Value);
            if (dbPlayer.HasCustomDrugBuff()) farmingtime = Convert.ToInt32(farmingtime  * 0.3);

            Chats.sendProgressBar(dbPlayer, farmingtime);

            Task.Run(async () =>
            {
                if(sxVehicle != null)
                {
                    sxVehicle.CanInteract = false;
                }
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);
                await Task.Delay(1000 + farmingtime);
                dbPlayer.SetData("userCannotInterrupt", false);
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.ResetData("userCannotInterrupt");

                if (sxVehicle != null)
                {
                    sxVehicle.CanInteract = true;
                }

                //Wenn kleinster eintrag = 0 ist, dann fehlen Items um herstellen zu können
                if (min.Value == 0)
                {
                    dbPlayer.SendNewNotification($"Sie benoetigen mindestens {required.First().Value} {min.Key.Name}!");
                    return;
                }
                dbPlayer.SendNewNotification("Du hast " + (farmProcess.RewardItemAmount * min.Value) + " " + ItemModelModule.Instance.Get(farmProcess.RewardItemId).Name + " hergestellt!");
                foreach (KeyValuePair<ItemModel, int> kvp in farmProcess.RequiredItems)
                {
                    //remove item from            PLAYER        ITEM_ID     MIN Herstellungs amount
                    UsingContainer.RemoveItem(kvp.Key, kvp.Value * min.Value);
                }
                UsingContainer.AddItem(farmProcess.RewardItemId, farmProcess.RewardItemAmount * min.Value);

                if (FarmingModule.Instance.ProcessAmount.ContainsKey(farmProcess))
                {
                    FarmingModule.Instance.ProcessAmount[farmProcess] += farmProcess.RewardItemAmount * min.Value;
                }
                else
                {
                    FarmingModule.Instance.ProcessAmount[farmProcess] = farmProcess.RewardItemAmount * min.Value;
                }
                
            });
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;
            if (dbPlayer.Player.Dead) return false;
            if (dbPlayer.IsSwimmingOrDivingDoNotUse) return false;


            // Farm NPC Process
            var farmProcess = GetByPosition(dbPlayer.Player.Position);
            if (farmProcess == null) return false;

            if (dbPlayer.Player.IsInVehicle) return false;
            
            Container UsingContainer = dbPlayer.Container;

            if (!dbPlayer.CanInteract())
            {
                dbPlayer.SendNewNotification("Du verarbeitest noch!");
                return false;
            }

            if(farmProcess.UseFromVehicle)
            {
                MenuManager.Instance.Build(PlayerMenu.FarmProcessMenu, dbPlayer).Show(dbPlayer);
            }
            else FarmProcessAction(farmProcess, dbPlayer, UsingContainer);
            return true;
        }
    }
}