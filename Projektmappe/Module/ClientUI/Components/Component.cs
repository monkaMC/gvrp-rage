using GTANetworkAPI;

namespace GVRP.Module.ClientUI.Components
{
    public abstract class Component : Script
    {   
        public string Name { get; }

        public Component(string name)
        {
            Name = name;
            ComponentManager.Instance.Register(this);
        }

        public void TriggerEvent(Client player, string eventName, params object[] args)
        {
            if (player == null)
            {
                player.Kick("NEIN!");
                return;

            }
            var eventArgs = new object[2 + args.Length];
            eventArgs[0] = Name;
            eventArgs[1] = eventName;

            for (var i = 0; i < args.Length; i++)
            {
                eventArgs[i + 2] = args[i];
            }

            player.TriggerEvent("componentServerEvent", eventArgs);
        }
    }
}