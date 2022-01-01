using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Assets.Beard;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Barber.Windows;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.Configurations;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.VehicleTax
{
    public sealed class VehicleTaxModule : Module<VehicleTaxModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.VehicleTaxSum = reader.GetInt32("tax_sum");
        }

        public override void OnFiveMinuteUpdate()
        {

        }

        public static int GetPlayerVehicleTaxesForGarages(DbPlayer iPlayer)
        {
            int tax = 0;

            string query = $"SELECT * FROM `vehicles` WHERE `owner` = '{iPlayer.Id}' AND `inGarage` = '1' AND `registered` = '1';";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
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
                            var modelId = reader.GetUInt32("model");
                            var data = VehicleDataModule.Instance.GetDataById(modelId);
                            if (data == null) continue;
                            tax = tax + data.Tax;
                        }
                    }
                }
            }
            return tax;
        }
    }
}