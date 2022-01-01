using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Vehicles;

namespace GVRP.Module.LSCustoms.Menu
{
    public class LSCVehicleSelectionMenuBuilder : MenuBuilder
    {
        public LSCVehicleSelectionMenuBuilder() : base(PlayerMenu.LSCVehicleListMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "LSC Fahrzeugauswahl");
            l_Menu.Add($"Schließen");

            Dictionary<uint, SxVehicle> l_NearVehicles = new Dictionary<uint, SxVehicle>();

            uint l_ID = 1;
            foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestVehicles(p_DbPlayer.Player.Position))
            {
                l_NearVehicles.Add(l_ID, sxVehicle);
                l_ID++;
            }

            p_DbPlayer.SetData("lsc_near_vehicles", l_NearVehicles);
            foreach(var l_Vehicle in l_NearVehicles.Values)
                l_Menu.Add($"{l_Vehicle.Data.Model} ({l_Vehicle.databaseId.ToString()})");

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
                iPlayer.SendNewNotification($"Auswahl: {index.ToString()}");

                if (index >= 1 && iPlayer.HasData("lsc_near_vehicles"))
                {
                    var l_Chosen = (uint)index;
                    Dictionary<uint, SxVehicle> l_Vehicles = iPlayer.GetData("lsc_near_vehicles");
                    if (!l_Vehicles.ContainsKey(l_Chosen) || l_Vehicles[l_Chosen] == null)
                    {
                        iPlayer.SendNewNotification($"Auswahl ist im Dictionary nicht vorhanden. Fahrzeug Menü ID: {l_Chosen.ToString()}");
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }

                    var l_Vehicle = l_Vehicles[l_Chosen];
                    if (l_Vehicle == null || !l_Vehicle.IsValid()) return false;

                    if (iPlayer.TeamRank < 10 && !l_Vehicle.InTuningProcess)
                    {
                        iPlayer.SendNewNotification("Fahrzeug ist nicht im Tuning Besitz!");
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    }

                    l_Vehicle.Repair();
                    iPlayer.SendNewNotification($"Fahrzeug ausgewählt: {l_Vehicle.Data.Model} ({l_Vehicle.databaseId.ToString()})");
                    iPlayer.SetData("tuneVeh", l_Vehicle.databaseId);

                    if (!iPlayer.HasData("lsc_type"))
                    {
                        iPlayer.SendNewNotification($"LSC-Typ nicht gesetzt!");
                        return false;
                    }

                    MenuManager.DismissCurrent(iPlayer);
                    LSCustoms l_LSC = LSCustomsModule.Instance.GetAll().Where(x => iPlayer.Player.Position.DistanceTo(x.Value.position) <= 3.0f).FirstOrDefault().Value;
                    if (l_LSC == null)
                    {
                        iPlayer.SendNewNotification("Du bist nicht in einer Tuningwerkstatt!");
                        return false;
                    }

                    var lsc_type = iPlayer.GetData("lsc_type");

                    if (lsc_type == 0)
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "SetCarColorLSC", Message = $"Geben Sie die Farben an. Aktuelle Farben: {l_Vehicle.color1.ToString()} {l_Vehicle.color1.ToString()}" });
                    else if (lsc_type == 1)
                        MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.LSCTuningMenu, iPlayer).Show(iPlayer);
                    else if (lsc_type == 2)
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Pearllack ändern", Callback = "SetCarPearlLSC", Message = $"Geben Sie eine Farbe an." });
                    else if (lsc_type == 3)
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Felgenfarbe ändern", Callback = "SetCarRimColorLSC", Message = $"Geben Sie eine Farbe an." });
                    else if (lsc_type == 4)
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Reifenrauchfarbe ändern", Callback = "SetCarTyreSmokeColorLSC", Message = $"Geben Sie drei Farben an." });

                    return false;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}
