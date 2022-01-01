using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Freiberuf.Mower
{
    public sealed class MeertraeubelModul : Module<MeertraeubelModul>
    {
        public static Vector3 MeertraeubelDryPoint = new Vector3();
        public static uint FishingNetzId = 519;
        public static uint NassMeetr = 344;
        public static uint TrockenMeetr = 518;

        public static List<DbPlayer> FishingPlayers = new List<DbPlayer>();

        public static int MaxWorkAmountMeerTr = 45;
        public static int MaxWorkAmountCocain = 30;

        public override bool Load(bool reload = false)
        {
            FishingPlayers = new List<DbPlayer>();

            return base.Load(reload);
        }

        public void UpdateMeertraubelContainer()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (House house in HouseModule.Instance.GetAll().Values.ToList().Where(h => h.Keller == 2 && h.LaborContainer != null && !h.LaborContainer.IsEmpty()))
                {
                    // Check Meetr
                    if (house.LaborContainer.GetItemAmount(NassMeetr) > 0)
                    {
                        if (house.LaborContainer.CanInventoryItemAdded(TrockenMeetr))
                        {
                            var l_Amount = house.LaborContainer.GetItemAmount(NassMeetr);
                            if (l_Amount > MaxWorkAmountMeerTr)
                                l_Amount = MaxWorkAmountMeerTr;

                            house.LaborContainer.RemoveItem(NassMeetr, l_Amount, true);
                            house.LaborContainer.AddItem(TrockenMeetr, l_Amount, new Dictionary<string, dynamic>(), -1, true);
                            house.LaborContainer.SaveAll();
                        }
                    }
                }
            }));
        }

        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            if (vehicle == null || !vehicle.GetModel().Equals(VehicleHash.Tug)) return;

            SxVehicle sxVeh = vehicle.GetVehicle();
            if (sxVeh == null || !sxVeh.IsValid() || (!sxVeh.IsPlayerVehicle() && sxVeh.IsTeamVehicle()) || seat != -1)
            {
                return;
            }

            if(!FishingPlayers.ToList().Contains(dbPlayer)) FishingPlayers.Add(dbPlayer);
        }

        public override void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
            if (vehicle == null || !vehicle.GetModel().Equals(VehicleHash.Tug)) return;
            if (FishingPlayers.ToList().Contains(dbPlayer)) FishingPlayers.Remove(dbPlayer);
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (FishingPlayers.ToList().Contains(dbPlayer)) FishingPlayers.Remove(dbPlayer);
        }

        public override async void OnTwoMinutesUpdate()
        {
            foreach (DbPlayer iPlayer in FishingPlayers.ToList())
            {
                // Spieler in Fahrzeug....
                if (iPlayer == null || !iPlayer.IsValid()) return;

                if (iPlayer.Player.IsInVehicle)
                {
                    if (iPlayer.Player.VehicleSeat == -1 && iPlayer.Player.Vehicle.GetModel().Equals(VehicleHash.Tug))
                    {
                        SxVehicle sxVehicle = iPlayer.Player.Vehicle.GetVehicle();
                        if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.GetSpeed() > 8.0f && (sxVehicle.IsPlayerVehicle() || sxVehicle.IsTeamVehicle()))
                        {
                            if (iPlayer.HasData("lastTugPoint"))
                            {
                                if (iPlayer.GetData("lastTugPoint").DistanceTo(iPlayer.Player.Position) < 4.0f)
                                    continue; //Anti Kreisfahren
                            }
                            iPlayer.SetData("lastTugPoint", iPlayer.Player.Position);

                            //genarate random item
                            if (sxVehicle.Container.GetItemAmount(FishingNetzId) > 0) // Check Netz
                            {
                                for (int l_Itr = 0; l_Itr < 13; l_Itr++)
                                {
                                    ItemModel item = GenerateItem();
                                    if (sxVehicle.Container.CanInventoryItemAdded(item)) sxVehicle.Container.AddItem(item); // Add Item if can added
                                }
                            }
                        }
                    }
                }
            }
        }

        public ItemModel GenerateItem()
        {
            int random = new Random().Next(1, 80);
            uint itemId = 344;//Meertr.
            if (random == 6) itemId = 520; // Portmonie
            else if (random == 7) itemId = 521; 
            else if (random == 8) itemId = 522; 
            else if (random == 9) itemId = 514; 
            else if (random == 10 || random == 11) itemId = 523; 
            else if (random == 11) itemId = 517; // 
            else if (random == 12 || random == 13) itemId = 468; // 
            else if (random == 14 || random == 15) itemId = 168; // 
            else if (random == 16 || random == 17) itemId = 167; // 
            else if (random == 18 || random == 19) itemId = 166; // 
            else if (random == 20 || random == 21) itemId = 165; // 
            else if (random == 22 || random == 23) itemId = 164; // 
            else if (random == 24 || random == 25) itemId = 163; // 
            else if (random == 26 || random == 27) itemId = 162; // 
            else if (random == 28 || random == 29) itemId = 161; // 
            else if (random == 30 || random == 31) itemId = 160; // 
            else if (random == 32) itemId = 525;
            else if (random == 33 || random == 34) itemId = 516; // 
            else if (random == 33 || random == 34) itemId = 516; // 

            if (ItemModelModule.Instance.GetAll().ContainsKey(itemId)) return ItemModelModule.Instance.Get(itemId);
            else return ItemModelModule.Instance.Get(160); // Sardine
        }
    }
}