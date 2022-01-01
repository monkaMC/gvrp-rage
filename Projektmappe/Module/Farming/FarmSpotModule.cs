using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Gangwar;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Jobs;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Farming
{
    public sealed class FarmSpotModule : SqlModule<FarmSpotModule, FarmSpot, uint>
    {
        public static Random Rnd = new Random();
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemsModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `farms`;";
        }

        public FarmPosition GetByPosition(Vector3 position)
        {
            var l_Positions = FarmPositionModule.Instance.GetAll();

            foreach (var l_Position in l_Positions)
            {
                Vector3 l_Vector = l_Position.Value.Position;
                var l_Range = l_Position.Value.range;

                if (position.DistanceTo(l_Vector) > l_Range)
                    continue;

                return l_Position.Value;
            }

            return null;
        }

        public bool PlayerFarmSpot(DbPlayer dbPlayer)
        {
            var xFarmPosition = GetByPosition(dbPlayer.Player.Position);
            if (xFarmPosition == null) return false;

            var xFarm = FarmSpotModule.Instance.Get(xFarmPosition.FarmSpotId);

            if (xFarm == null) return false;

            if (!dbPlayer.CanInteract(true) || dbPlayer.Player.IsInVehicle) return false;

            if (xFarm.RequiredLevel > 0 && dbPlayer.Level < xFarm.RequiredLevel)
            {
                dbPlayer.SendNewNotification("Das geht erst ab Level " + xFarm.RequiredLevel);
                return false;
            }

            if (xFarm.RequiredItemId != 0)
            {
                if (dbPlayer.Container.GetItemAmount(xFarm.RequiredItemId) < 1)
                {
                    dbPlayer.SendNewNotification(
                  "Zum Farmen von " + ItemModelModule.Instance.Get(xFarm.ItemId).Name +
                        " benötigen Sie ein/en " + ItemModelModule.Instance.Get(xFarm.RequiredItemId).Name);
                    return false;
                }
                double breakRoll = Rnd.NextDouble() * 100;
                if (breakRoll < xFarm.RequiredItemChanceToBreak)
                {
                    dbPlayer.Container.RemoveItem(xFarm.RequiredItemId, 1);
                    dbPlayer.SendNewNotification("Dein Werkzeug ist kaputt gegangen!");
                }
                if (dbPlayer.Container.GetItemAmount(xFarm.RequiredItemId) == 0)
                {
                    dbPlayer.SendNewNotification("Kein Werkzeug vorhanden!", duration: 20000);
                    dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                    dbPlayer.ResetData("pressedEOnFarm");
                    if (FarmingModule.FarmingList.Contains(dbPlayer)) FarmingModule.FarmingList.Remove(dbPlayer);
                    return false;
                }
            }

            var amount = Rnd.Next(xFarm.MinResultAmount, xFarm.MaxResultAmount);
            if (!dbPlayer.Container.CanInventoryItemAdded(xFarm.ItemId, amount))
            {
                dbPlayer.SendNewNotification("Dein Inventar ist voll!", duration: 20000);
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                dbPlayer.SendNewNotification("Farming beendet!");
                dbPlayer.ResetData("pressedEOnFarm");
                if (FarmingModule.FarmingList.Contains(dbPlayer)) FarmingModule.FarmingList.Remove(dbPlayer);
                return false;
            }
            
            switch (xFarm.SpecialType)
            {
                case FarmType.Pickup:
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");
                    break;
                case FarmType.Drill:
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");
                    break;
                case FarmType.Hammer:
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");
                    break;
            }

            if (dbPlayer.HasCustomDrugBuff()) amount++;
            if (xFarm.Id == 1)
            {
                dbPlayer.SendNewNotification("Du hast " + amount + " " + xFarm.RessourceName + " gesammelt!");
            } else
            {
                dbPlayer.SendNewNotification("Du hast " + amount + " " + xFarm.RessourceName + " gesammelt!");

            }
            dbPlayer.Container.AddItem(xFarm.ItemId, amount);

            if (FarmingModule.Instance.FarmAmount.ContainsKey(xFarm))
            {
                FarmingModule.Instance.FarmAmount[xFarm] += amount;
            }
            else
            {
                FarmingModule.Instance.FarmAmount[xFarm] = amount;
            }

            return true;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle) return false;
            if (dbPlayer.Player.Dimension == GangwarModule.Instance.DefaultDimension) return false;
            var xFarm = GetByPosition(dbPlayer.Player.Position);
            if (xFarm == null) return false;

            if (dbPlayer.HasData("pressedEOnFarm") && dbPlayer.GetData("pressedEOnFarm"))
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                dbPlayer.SendNewNotification("Farming beendet!");
                dbPlayer.ResetData("pressedEOnFarm");
                if (FarmingModule.FarmingList.Contains(dbPlayer)) FarmingModule.FarmingList.Remove(dbPlayer);
                return true;
            }
            else
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SendNewNotification("Farming gestartet!");
                dbPlayer.SetData("pressedEOnFarm", true);
                if (!FarmingModule.FarmingList.Contains(dbPlayer)) FarmingModule.FarmingList.Add(dbPlayer);
                return true;
            }
        }
    }
}