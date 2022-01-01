using System.Linq;
using GVRP.Module.Chat;
using GVRP.Module.Meth;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool MethCook(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.DimensionType[0] != DimensionType.Camper)
            {
                iPlayer.SendNewNotification(
                     "Sie muessen in einem Wohnmobil sein!");
                return false;
            }

            if (!iPlayer.HasData("cooking"))
            {
                if (iPlayer.Container.GetItemAmount(13) >= 1)
                {
                    if (iPlayer.Container.GetItemAmount(15) <= 0 ||
                        iPlayer.Container.GetItemAmount(16) <= 0 ||
                        iPlayer.Container.GetItemAmount(14) <= 0)
                    {
                        iPlayer.SendNewNotification("Zum Meth kochen benötigen Sie Batterien, Ephedrin und Toilettenreiniger");
                        return false;
                    }

                    if(MethModule.CookingPlayers.ToList().Where(p => p != null && p.IsValid() && p.Player.Dimension == iPlayer.Player.Dimension).Count() >= 8)
                    {
                        iPlayer.SendNewNotification("Hier ist kein Platz mehr? Wo willst du deinen Kocher hinstellen, alla?");
                        return false;
                    }

                    iPlayer.SendNewNotification( "Sie kochen nun Meth");
                    iPlayer.SetData("cooking", true);
                    if (!MethModule.CookingPlayers.Contains(iPlayer)) MethModule.CookingPlayers.Add(iPlayer);
                    return true;
                }
            }
            else
            {
                iPlayer.ResetData("cooking");
                iPlayer.SendNewNotification( "Meth kochen beendet!");
                if (MethModule.CookingPlayers.Contains(iPlayer)) MethModule.CookingPlayers.Remove(iPlayer);
            }
            return false;
        }
    }
}