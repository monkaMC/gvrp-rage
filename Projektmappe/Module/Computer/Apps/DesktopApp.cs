using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps
{
    public class DesktopApp : App<Func<DbPlayer, List<ComputerAppClientObject>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "apps")] private List<ComputerAppClientObject> Apps { get; }

            public ShowEvent(DbPlayer dbPlayer, List<ComputerAppClientObject> computer) : base(dbPlayer)
            {
                Apps = Apps;
            }
        }

        public DesktopApp() : base("DesktopApp", "DesktopApp")
        {
        }

        public override Func<DbPlayer, List<ComputerAppClientObject>, bool> Show()
        {
            return (dbPlayer, apps) => OnShow(new ShowEvent(dbPlayer, apps));
        }

        [RemoteEvent]
        public void requestComputerApps(Client player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (player.IsReloading) return;
            if (!iPlayer.CanInteract()) return;

            List<ComputerAppClientObject> computerAppClientObjects = new List<ComputerAppClientObject>();

            foreach (KeyValuePair<uint, ComputerApp> kvp in ComputerAppModule.Instance.GetAll())
            {
                if (iPlayer.CanAccessComputerApp(kvp.Value) && kvp.Value.Type == ComputerTypes.Computer) computerAppClientObjects.Add(new ComputerAppClientObject(kvp.Value));
            }

            TriggerEvent(player, "responseComputerApps", NAPI.Util.ToJson(computerAppClientObjects));
          //  Console.WriteLine("" + NAPI.Util.ToJson(computerAppClientObjects));
        }
    }
}
