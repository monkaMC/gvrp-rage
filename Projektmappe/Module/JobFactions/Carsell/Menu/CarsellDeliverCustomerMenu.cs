using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.JobFactions.Carsell;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Telefon.App;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.RegistrationOffice;

namespace GVRP.Module.Carsell.Menu
{
    public class CarsellDeliverCustomerMenuBuilder : MenuBuilder
    {
        public CarsellDeliverCustomerMenuBuilder() : base(PlayerMenu.CarsellDeliverCustomerMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "Bestellung abschliessen");
            l_Menu.Add($"Schließen");

            // Show finished orders from team
            foreach(DeliveryOrder deliveryOrder in JobCarsellFactionModule.Instance.DeliverableOrderList.ToList().Where(o => o.TeamId == p_DbPlayer.TeamId))
            {
                VehicleData vehData = VehicleDataModule.Instance.GetDataById(deliveryOrder.VehicleDataId);
                if (vehData == null) continue;

                l_Menu.Add($"{PlayerNameModule.Instance.Get(deliveryOrder.PlayerId).Name} - {(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)}");
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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }

                int idx = 1;

                // Show finished orders from team
                foreach (DeliveryOrder deliveryOrder in JobCarsellFactionModule.Instance.DeliverableOrderList.ToList().Where(o => o.TeamId == iPlayer.TeamId))
                {
                    if (idx == index)
                    {
                        VehicleData vehData = VehicleDataModule.Instance.GetDataById(deliveryOrder.VehicleDataId);
                        if (vehData == null) return false;

                        if(!iPlayer.Container.CanInventoryItemAdded(641))
                        {
                            iPlayer.SendNewNotification("Sie haben keinen Platz für einen Kaufvertrag!");
                            return false;
                        }

                        uint GarageId = JobCarsellFactionModule.GarageTeam1;

                        if (iPlayer.TeamId == (int)teams.TEAM_CARSELL2) GarageId = JobCarsellFactionModule.GarageTeam2;
                        if (iPlayer.TeamId == (int)teams.TEAM_CARSELL3) GarageId = JobCarsellFactionModule.GarageTeam3;

                        // INSERT VEHICLE
                        MySQLHandler.Execute($"INSERT INTO `vehicles` (`team_id`, `owner`, `color1`, `color2`, `tuning`, `inGarage`, `garage_id`, `model`, `vehiclehash`) " +
                            $"VALUES ('0', '{deliveryOrder.PlayerId}', '{deliveryOrder.Color1}', '{deliveryOrder.Color2}', '23:{deliveryOrder.Wheel}', '1', '{GarageId}', '{vehData.Id}', '{(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)}');");

                        string query = string.Format($"SELECT * FROM `vehicles` WHERE `owner` = '{deliveryOrder.PlayerId}' AND `model` LIKE '{vehData.Id}' ORDER BY id DESC LIMIT 1;");

                        uint id = 0;

                        using (var conn =
                            new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                        using (var cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            cmd.CommandText = @query;
                            using (var reader = cmd.ExecuteReader())
                            {
                                if (reader.HasRows)
                                {
                                    while (reader.Read())
                                    {
                                        id = reader.GetUInt32("id");
                                        break;
                                    }
                                }
                            }
                            conn.Close();
                        }

                        // Give Kaufvertrag zu Verkäufer
                        var info = $"Besitzer: {PlayerNameModule.Instance.Get(deliveryOrder.PlayerId).Name} Fahrzeug: {(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)} ({id}) am {DateTime.Now.ToString("dd.MM.yyyy HH:mm")}. VK von {iPlayer.GetName()}";

                        iPlayer.Container.AddItem(641, 1, new Dictionary<string, dynamic>() { { "Info", info }, { "vehicleId", id } });

                        iPlayer.SendNewNotification($"Sie haben {(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)} von {PlayerNameModule.Instance.Get(deliveryOrder.PlayerId).Name} für die Liefergarage freigegeben");

                        // Set Vehicle to Status 2
                        MySQLHandler.ExecuteAsync($"UPDATE `jobfaction_carsell_orders` SET status = '2' WHERE id = '{deliveryOrder.Id}';");

                        // Remove From List
                        JobCarsellFactionModule.Instance.DeliverableOrderList.Remove(deliveryOrder);
                        return true;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}
