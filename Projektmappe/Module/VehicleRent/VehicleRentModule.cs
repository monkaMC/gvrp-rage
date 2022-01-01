using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.VehicleRent
{
    public class VehicleRentModule : Module<VehicleRentModule>
    {
        public static List<PlayerVehicleRentKey> PlayerVehicleRentKeys = new List<PlayerVehicleRentKey>();

        public static uint VehicleRentItemId = 626;

        protected override bool OnLoad()
        { 
            PlayerVehicleRentKeys = new List<PlayerVehicleRentKey>();
            MenuManager.Instance.AddBuilder(new VehicleRentMenuBuilder());
            return true;
        }

        public override void OnFiveMinuteUpdate()
        {
            // Clear Up DB
            MySQLHandler.ExecuteAsync("DELETE FROM player_vehicle_rent WHERE `ending_date` < CURDATE()");

            // Clear Up List
            PlayerVehicleRentKeys.RemoveAll(k => k.EndingDate < DateTime.Now);
        }
    }

    public static class VehicleRentPlayerExtension
    {
        public static void AddPlayerVehicleKeyForPlayer(this DbPlayer owner, DbPlayer Renter, uint vehicleId, DateTime endDate)
        {
            try {
                if (owner == null || !owner.IsValid() || Renter == null || !Renter.IsValid())
                {
                    owner.SendNewNotification("Fehler!");
                    return;
                }
                // Valid Player hasnt key already
                //if (VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.PlayerId == Renter.Id && k.VehicleId == vehicleId).Count() > 0) return;

                VehicleRentModule.PlayerVehicleRentKeys.Add(new PlayerVehicleRentKey()
                {
                    OwnerId = owner.Id,
                    PlayerId = Renter.Id,
                    VehicleId = vehicleId,
                    BeginDate = DateTime.Now,
                    EndingDate = endDate
                });

                // Add to DB for after restart compatiblity ... :)
                MySQLHandler.ExecuteAsync($"INSERT INTO `player_vehicle_rent` (`owner_id`, `player_id`, `vehicle_id`, `begin_date`, `ending_date`) VALUES('{owner.Id}', '{Renter.Id}', '{vehicleId}', '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}', '{endDate.ToString("yyyy-MM-dd HH:mm:ss")}');");

            }
            catch(Exception e)
            {
                return;
            }
        }

        public static List<PlayerVehicleRentKey> GetPlayerVehicleRents(this DbPlayer dbPlayer)
        {
            List<PlayerVehicleRentKey> myRents = new List<PlayerVehicleRentKey>();

            myRents = VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(pr => pr.OwnerId == dbPlayer.Id).ToList();

            return myRents;
        }
    }

    public class PlayerVehicleRentKey
    {
        public uint OwnerId { get; set; }
        public uint PlayerId { get; set; }
        public uint VehicleId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndingDate { get; set; }

    }
}