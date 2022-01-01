using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.ClientUI.Windows
{
    public abstract class Window<T> : Component
    {
        public class Event
        {
            [JsonIgnore] public DbPlayer DbPlayer { get; }

            public Event(DbPlayer dbPlayer)
            {
                DbPlayer = dbPlayer;
            }
        }

        public Window(string name) : base(name)
        {
        }

        public bool OnShow(Event @event)
        {
            string json;
            try
            {
                json = NAPI.Util.ToJson(@event);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
                json = null;
            }

            if (string.IsNullOrEmpty(json)) return false;

            if (Configuration.Instance.DevMode)
            {
                Logger.Print($"Open Window ${Name} with args ${json}");
            }

            Open(@event.DbPlayer.Player, json);
            return true;
        }

        public virtual void Open(Client player, string json)
        {
            player.TriggerEvent("openWindow", Name, json);
        }

        public virtual void Close(Client player)
        {
            player.TriggerEvent("closeWindow", Name);
        }

        public abstract T Show();
    }
}