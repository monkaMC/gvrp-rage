using System.Threading.Tasks;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Chat
{
    public sealed class Chats
    {
        public const string MsgVehicleInfo = "[KFZ]: ";
        public const string MsgVehicleShop = "[Auto-Haendler]: ";
        public const string MsgAdvert = "[Werbung]: ";
        public const string MsgNews = "[News]: ";
        public const string MsgServerCc = "[Fahrzeugchat]: ";
        public const string MsgLeistelle = "[Leitstelle]: ";
        public const string MsgHandy = "[Handy]: ";
        public const string MsgBusiness = "[Business]: ";

        public enum ICON
        {
            GLOB,
            DEV,
            GOV,
            WED,
            CASINO
        }
        public enum COLOR
        {
            WHITE,
            ORANGE,
            LIGHTBLUE,
            RED,
            CHARTREUSE,
            LIGHTGREEN
        }
        public static async Task SendGlobalMessage(string message, COLOR color, ICON icon, int duration = 10000)
        {
            await Task.Run(() =>
            {
                  foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
                  {
                      iPlayer.Player.TriggerEvent("sendGlobalNotification", message, duration, color.ToString().ToLower(), icon.ToString().ToLower());
                  }
            });
        }

        public static void sendProgressBar(DbPlayer iPlayer, int timeInMs)
        {
            iPlayer.Player.TriggerEvent("sendProgressbar", timeInMs);
        }
    }
}