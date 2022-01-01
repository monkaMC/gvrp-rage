using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Houses.Menu
{
    public class VoltageMenuBuilder : MenuBuilder
    {
        public VoltageMenuBuilder() : base(PlayerMenu.VoltageMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Stromkasten", "");

            menu.Add($"Schließen");
            menu.Add($"Messung durchführen");

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else 
                {

                    HousesVoltage housesVoltage = HousesVoltageModule.Instance.GetAll().Values.ToList().Where(hv => hv.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f).FirstOrDefault();
                    if (housesVoltage == null) return false;

                    if(index == 1)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 45000);

                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                            await Task.Delay(45000);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.ResetData("userCannotInterrupt");
                            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                            
                            if (housesVoltage.DetectedHouses.Count() > 0)
                            {
                                dbPlayer.SendNewNotification($"Folgende Häuser haben einen hohen Verbrauch aufgewiesen {string.Join(',', housesVoltage.DetectedHouses.ToList())}!");
                                housesVoltage.DetectedHouses.Clear();
                                return;
                            }
                            dbPlayer.SendNewNotification($"Es konnte kein auffälliger Verbrauch festgestellt werden!");
                        }));
                    }
                }

                return false;
            }
        }
    }
}
