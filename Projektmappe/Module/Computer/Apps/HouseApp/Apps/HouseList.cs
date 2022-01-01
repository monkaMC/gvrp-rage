using GTANetworkAPI;
using System.Linq;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Houses;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.HouseApp.Apps
{
    public class HouseList : SimpleApp
    {

        public HouseList() : base("HouseList")
        {

        }

        [RemoteEvent]
        public void requestTenants(Client client)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (dbPlayer.ownHouse[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt kein Haus.");
                return;
            }
            TriggerEvent(client, "responseTenants", NAPI.Util.ToJson(HouseAppFunctions.GetTenantsForHouseByPlayer(dbPlayer)));
        }


        [RemoteEvent]
        public void saverentprice(Client client, int price, int slotid)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (slotid == 0) return; // Gibts nicht

            if (dbPlayer.ownHouse[0] != 0)
            {
                if (price < 0 || price > 15000) return;

                HouseRent houseRent = HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.ownHouse[0] && hr.SlotId == slotid).FirstOrDefault();
                if (houseRent == null) return;

                houseRent.RentPrice = price;
                houseRent.Save();
                dbPlayer.SendNewNotification($"Sie haben den Mietpreis des Mietslots {houseRent.SlotId} auf ${price} geändert!");

                var findPlayer = Players.Players.Instance.FindPlayer(houseRent.PlayerId);
                
                if (findPlayer != null && findPlayer.IsValid())
                {
                    findPlayer.SendNewNotification($"Dein Mietvertrag wurde geändert, neuer Mietpreis ${price}!");
                }
            }
        }

        [RemoteEvent]
        public void unrentTenant(Client client, int slotid)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
                return;

            if (slotid == 0) return; // Gibts nicht

            if (dbPlayer.ownHouse[0] != 0)
            {

                HouseRent houseRent = HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.ownHouse[0] && hr.SlotId == slotid).FirstOrDefault();
                if (houseRent == null || houseRent.PlayerId == 0) return;

                var findPlayer = Players.Players.Instance.FindPlayer(houseRent.PlayerId);

                string PlayerName = PlayerNameModule.Instance.Get(houseRent.PlayerId).Name;

                if (findPlayer != null && findPlayer.IsValid())
                {
                    findPlayer.SendNewNotification($"Dein Mietvertrag für das Haus von {dbPlayer.Player.Name} wurde gekündigt.");
                }

                houseRent.Clear();

                dbPlayer.SendNewNotification($"Du hast den Mietvertrag von {PlayerName} gekündigt.");
            }
        }
    }
}
