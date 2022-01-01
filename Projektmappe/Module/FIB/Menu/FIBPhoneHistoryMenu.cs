using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.ReversePhone;
using GVRP.Module.Telefon.App;
namespace GVRP.Module.FIB.Menu
{
    public class FIBPhoneHistoryMenu : MenuBuilder
    {
        public FIBPhoneHistoryMenu() : base (PlayerMenu.FIBPhoneHistoryMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("fib_phone_history"))
                return null;

            DbPlayer l_Target = Players.Players.Instance.FindPlayer(p_DbPlayer.GetData("fib_phone_history"));
            if (l_Target == null || !l_Target.IsValid())
                return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Telekommunikationsdaten");
            l_Menu.Add($"Schließen");

            var l_Histories = ReversePhoneModule.Instance.phoneHistory[l_Target.Id];

            foreach (var l_History in l_Histories.ToList())
            {
                l_Menu.Add($"[{l_History.Time.ToString()}] An: {l_History.Number.ToString()} ({(l_History.Dauer / 60).ToString()} min");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int p_Index, DbPlayer p_DbPlayer)
            {
                MenuManager.DismissCurrent(p_DbPlayer);
                return true;
            }
        }
    }
}
