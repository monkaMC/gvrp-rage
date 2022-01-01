using System;
using System.Linq;
using System.Reflection;
using GVRP.Module.Players.Db;
using GTANetworkAPI;
using GVRP.Module.Admin;
using GVRP.Module.ClientUI.Windows;

namespace GVRP.Module.Players.Windows
{
    public class DeathWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {
            }
        }

        public DeathWindow() : base("Death")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(new ShowEvent(player));
        }

        public void closeDeathWindowS(Client client)
        {
            TriggerEvent(client, "closeDeathScreen");
        }
    }
}