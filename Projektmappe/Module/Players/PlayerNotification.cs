using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerNotification
    {
        public enum NotificationType
        {
            STANDARD = 0,
            SERVER = 1,
            ADMIN = 2,
            TEAM = 3,
            HOUSE = 4,
            SUCCESS = 5,
            ERROR = 6,
            BUSINESS = 7,
            NEWS = 8,
            OOC = 9,
            FRAKTION = 10,
            INFO = 11,
            DELIVERY = 12,
            FREIBERUF = 13,
            CASINO = 14,
            HIGH = 15
        }

        public static void SendNewNotification(this DbPlayer iPlayer, string text, NotificationType notificationType = NotificationType.STANDARD, string title = "", int duration = 5000)
        {
            if (iPlayer == null || !iPlayer.IsValid())
                return;

            string color = "";
            switch (notificationType)
            {
                case NotificationType.ERROR:
                case NotificationType.ADMIN:
                    color = "red";
                    break;
                case NotificationType.TEAM:
                    color = "Magenta";
                    break;
                case NotificationType.SERVER:
                    color = "yellow";
                    break;
                case NotificationType.HOUSE:
                    color = "RoyalBlue";
                    break;
                case NotificationType.SUCCESS:
                    color = "green";
                    break;
                case NotificationType.BUSINESS:
                    color = "orange";
                    break;
                case NotificationType.NEWS:
                    color = "yellow";
                    break;
                case NotificationType.OOC:
                    color = "DarkGreen";
                    break;
                case NotificationType.FRAKTION:
                    color = Utils.HexConverter(iPlayer.Team.RgbColor);
                    break;
                case NotificationType.INFO:
                    color = "DodgerBlue";
                    break;
                case NotificationType.DELIVERY:
                    color = "RoyalBlue";
                    break;
                case NotificationType.FREIBERUF:
                    color = "RoyalBlue";
                    break;
                case NotificationType.CASINO:
                    color = "LightGreen";
                    break;
                case NotificationType.HIGH:
                    color = "#e67b00";
                    break;
            }

            iPlayer.Player.TriggerEvent("sendPlayerNotification", text, duration, color, title);

        }

        public static void SendNotification(this DbPlayer iPlayer, PlayerNotifications.Notification notify)
        {
            iPlayer.SendNewNotification(notify.Text, title: notify.Title, notificationType: NotificationType.INFO);
        }
    }
}