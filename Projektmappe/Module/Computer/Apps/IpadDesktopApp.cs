using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps
{
    public class IpadDesktopApp : App<Func<DbPlayer, List<ComputerAppClientObject>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "apps")] private List<ComputerAppClientObject> Apps { get; }

            public ShowEvent(DbPlayer dbPlayer, List<ComputerAppClientObject> computer) : base(dbPlayer)
            {
                Apps = Apps;
            }
        }

        public IpadDesktopApp() : base("IpadDesktopApp", "IpadDesktopApp") {}

        public override Func<DbPlayer, List<ComputerAppClientObject>, bool> Show()
        {
            return (dbPlayer, apps) => OnShow(new ShowEvent(dbPlayer, apps));
        }

        [RemoteEvent]
        public void requestIpadApp(Client player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (player.IsReloading) return;

            List<ComputerAppClientObject> computerAppClientObjects = new List<ComputerAppClientObject>();

            foreach (KeyValuePair<uint, ComputerApp> kvp in ComputerAppModule.Instance.GetAll())
            {
                if (iPlayer.CanAccessComputerApp(kvp.Value) && kvp.Value.Type == ComputerTypes.AdminTablet) computerAppClientObjects.Add(new ComputerAppClientObject(kvp.Value));
            }
            TriggerEvent(player, "responseIpadApps", "[{\"id\":\"1\",\"appName\":\"SupportOverviewApp\", \"name\":\"Support\",\"icon\": \"204316.svg\"}, {\"id\":\"2\",\"appName\":\"SupportVehicleApp\",\"name\":\"Fahrzeugsupport\",\"icon\": \"204316.svg\"}]");
        }
    }
}
