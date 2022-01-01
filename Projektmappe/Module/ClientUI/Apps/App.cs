using GTANetworkAPI;
using GVRP.Module.ClientUI.Windows;

namespace GVRP.Module.ClientUI.Apps
{
    public abstract class App<T> : Window<T>
    {
        private string Component { get; }

        public App(string name, string component) : base(name)
        {
            Component = component;
        }

        public override void Open(Client player, string json)
        {
            player.TriggerEvent("openApp", Component, Name, json);
        }
    }
}