using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Houses;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.HouseApp.Apps
{
    public class HouseEdit : SimpleApp
    {
        public HouseEdit() : base("HouseEdit") {}

        [RemoteEvent]
        public void requestHouseData(Client client)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.ownHouse[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt kein Haus.");
                return;
            }

            House house = HouseModule.Instance.GetByOwner(dbPlayer.Id);
            if (house == null)
                return;

            TriggerEvent(client, "responseHouseData", house.InventoryCash);
        }

        [RemoteEvent]
        public void withDrawHouseCash(Client client, int amount)
        {
            DbPlayer dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.ownHouse[0] == 0) return;

            House iHouse;

            if ((iHouse = HouseModule.Instance.GetThisHouseFromPos(dbPlayer.Player.Position)) == null || iHouse.Id != dbPlayer.ownHouse[0])
            {
                dbPlayer.SendNewNotification("Sie befinden sich nicht an Ihrem Haus!", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (amount > 0 && amount <= iHouse.InventoryCash)
            {
                iHouse.InventoryCash -= amount;
                dbPlayer.GiveMoney(amount);
                dbPlayer.SendNewNotification($"Sie haben { amount }$ aus Ihrer Hauskasse entnommen.", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.SUCCESS);
                iHouse.SaveHouseBank();
                dbPlayer.Save();
            }
            else
            {
                dbPlayer.SendNewNotification("Ungueltiger Betrag!", title: "Hauskasse", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
        }
    }
}
