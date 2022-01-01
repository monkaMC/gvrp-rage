using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Blitzer;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool ChestUnpack(DbPlayer iPlayer, ItemModel ItemData)
        {
            string itemScript = ItemData.Script;

            if(!uint.TryParse(itemScript.Split('_')[1], out uint itemModelId))
            {
                return false;
            }

            if(!int.TryParse(itemScript.Split('_')[2], out int itemAmount))
            {
                return false;
            }

            if (itemModelId == 40) // Schutzweste
            {
                switch (iPlayer.TeamId)
                {
                    case (int)teams.TEAM_FIB:
                        itemModelId = 712;
                        break;
                    case (int)teams.TEAM_ARMY:
                        itemModelId = 722;
                        break;
                    case (int)teams.TEAM_POLICE:
                        itemModelId = 697;
                        break;
                    case (int)teams.TEAM_COUNTYPD:
                        itemModelId = 697;
                        break;
                    default:
                        break;
                }
            }

            ItemModel itemModel = ItemModelModule.Instance.Get(itemModelId);
            int addedWeight = itemModel.Weight * itemAmount;

            if((iPlayer.Container.GetInventoryFreeSpace() + ItemData.Weight) < addedWeight)
            {
                iPlayer.SendNewNotification("So viel kannst du nicht tragen!");
                return false;
            }

            iPlayer.Container.AddItem(itemModel, itemAmount);
            return true;
        }
    }
}