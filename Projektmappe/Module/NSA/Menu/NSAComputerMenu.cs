using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Vehicles.InteriorVehicles;

namespace GVRP.Module.NSA.Menu
{
    public class NSAComputerMenuBuilder : MenuBuilder
    {
        public NSAComputerMenuBuilder() : base(PlayerMenu.NSAComputerMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Hauptmenü");
            l_Menu.Add($"Schließen");
            l_Menu.Add($"IAA Transaction History");
            l_Menu.Add($"IAA Energie History");
            l_Menu.Add($"Aktive Ortung beenden");
            if (p_DbPlayer.IsNSA)
            {
                l_Menu.Add($"Rufnummer ändern");
                l_Menu.Add($"Observation hinzufügen");
                l_Menu.Add($"Observationen");
                l_Menu.Add($"Gespräch mithören/beenden");
                l_Menu.Add($"Laufende Anrufe");
                l_Menu.Add($"Nummer Nachverfolgung");
                l_Menu.Add($"Smartphone Cloning beenden");
                l_Menu.Add($"Aktive Peilsender");
                l_Menu.Add($"Keycard Nutzung (Tür)");
            }
            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }
                else if (index == 1)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSATransactionHistory, iPlayer).Show(iPlayer);
                    return false;
                }
                else if (index == 2)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAEnergyHistory, iPlayer).Show(iPlayer);
                    return false;
                }
                else if (index == 3)
                {
                    if (iPlayer.HasData("nsaOrtung"))
                    {
                        iPlayer.ResetData("nsaOrtung");
                        iPlayer.SendNewNotification("Ortung beendet!");
                        return true;
                    }
                    iPlayer.SendNewNotification("Keine aktive Ortung vorhanden!");
                    return true;
                }
                if (iPlayer.IsNSA)
                {
                    if (index == 4)
                    {
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Rufnummer ändern", Callback = "NSAChangePhoneNumber", Message = "Geben Sie eine Rufnummer ein:" });
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }
                    else if (index == 5)
                    {
                        /*if (iPlayer.RankId != 4 && iPlayer.RankId != 5 && iPlayer.RankId != 6)
                        {
                            iPlayer.SendNewNotification("Diese Funktion steht nur dem High-Team zur Verfügung.");
                            return true;
                        }*/ // Blödsinn bitte vorher mit dem der es erstellt hat auch abklären

                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Obersvation hinzufügen", Callback = "AddObservationPlayer", Message = "Geben Sie einen Namen ein:" });
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }
                    else if (index == 6)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAObservationsList, iPlayer).Show(iPlayer);
                        return false;
                    }
                    else if (index == 7)
                    {
                        if (iPlayer.HasData("nsa_activePhone"))
                        {
                            iPlayer.Player.TriggerEvent("setCallingPlayer", "");
                            iPlayer.ResetData("nsa_activePhone");
                            iPlayer.SendNewNotification("Mithören beendet!");
                            return true;
                        }

                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Handy abhören", Callback = "NSAAddPhoneHearing", Message = "Geben Sie eine Nummer ein:" });
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }
                    else if (index == 8)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSACallListMenu, iPlayer).Show(iPlayer);
                        return false;
                    }
                    else if (index == 9)
                    {
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Nummer Nachverfolgung", Callback = "NSACheckNumber", Message = "Geben Sie eine Nummer ein:" });
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }
                    else if (index == 10)
                    {
                        iPlayer.ResetData("nsa_smclone");
                        iPlayer.SendNewNotification($"Smartphone Clone beendet!");
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }
                    else if (index == 11)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAPeilsenderMenu, iPlayer).Show(iPlayer);
                        return false;
                    }
                    else if (index == 12)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSADoorUsedsMenu, iPlayer).Show(iPlayer);
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
