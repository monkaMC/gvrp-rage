using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Spawners;

namespace GVRP.Module.Players
{
    public sealed class PlayerNotifications
    {
        public class Notification
        {
            public int Id { get; set; }
            public ColShape Shape { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
            public int Duration { get; set; }
        }

        public static PlayerNotifications Instance { get; } = new PlayerNotifications();

        private readonly Dictionary<int, Notification> notifications;

        private static int _count;

        private PlayerNotifications()
        {
            notifications = new Dictionary<int, Notification>();
        }

        public Notification Add(Vector3 position, string title, string text, int duration = 5000)
        {
            var notification = new Notification
            {
                Id = _count++,
                Shape = ColShapes.Create(position, 3.0f),
                Title = title,
                Text = text,
                Duration = duration
            };
            notification.Shape.SetData("notificationId", notification.Id);
            notifications.Add(notification.Id, notification);
            return notification;
        }

        public Notification GetById(int id)
        {
            return notifications.TryGetValue(id, out var notification) ? notification : null;
        }
        
        public void Remove(int id)
        {
            notifications.Remove(id);
        }
    }
}