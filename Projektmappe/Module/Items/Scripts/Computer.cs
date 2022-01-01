using GTANetworkMethods;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Computer(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.IsValid()) return false;
            if ((iPlayer.IsInDuty() && iPlayer.Team.Id == (int)teams.TEAM_FIB))
            {
                Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAComputerMenu, iPlayer).Show(iPlayer);
                return true;
            }
            return false;
        }
    }
}