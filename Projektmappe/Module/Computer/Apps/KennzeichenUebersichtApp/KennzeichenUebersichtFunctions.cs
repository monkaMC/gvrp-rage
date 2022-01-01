using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using MySql.Data.MySqlClient;
using GVRP.Module.Computer.Apps.FahrzeuguebersichtApp;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;
using static GVRP.Module.Computer.Apps.KennzeichenUebersichtApp.Apps.KennzeichenUebersichtApp;

namespace GVRP.Module.Computer.Apps.KennzeichenUebersichtApp
{
    public class KennzeichenUebersichtFunctions
    {

        public static List<PlateHistory> GetVehicleInfoByPlateOrId(DbPlayer dbPlayer, SearchType searchType, String information)
        {
            List<PlateHistory> overviewVehicles = new List<PlateHistory>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                switch (searchType)
                {

                    case SearchType.PLATE:
                        overviewVehicles = GetOverviewVehiclesFromDb(cmd, "SELECT p1.name AS name1, p2.name AS name2, vehicle_id, plate, status, timestamp FROM vehicle_registrations INNER JOIN player AS p1 ON p1.id = owner_id INNER JOIN player AS p2 ON p2.id = officer_id WHERE plate LIKE @value ORDER BY vehicle_registrations.timestamp DESC LIMIT 25", information, searchType);
                        overviewVehicles.AddRange(GetOverviewVehiclesFromDb(cmd, "SELECT team.name_short as name, vehicle_id, plate, status, timestamp FROM vehicle_registrations INNER JOIN team ON team.id = owner_id*-1 INNER JOIN player ON player.id = officer_id WHERE plate LIKE @value AND owner_id < 0 ORDER BY vehicle_registrations.timestamp DESC LIMIT 25", information, searchType));

                        break;
                    case SearchType.VEHICLEID:
                        overviewVehicles = GetOverviewVehiclesFromDb(cmd, "SELECT p1.name AS name1, p2.name AS name2, vehicle_id, plate, status, timestamp FROM vehicle_registrations INNER JOIN player AS p1 ON p1.id = owner_id INNER JOIN player AS p2 ON p2.id = officer_id WHERE vehicle_id = @value ORDER BY vehicle_registrations.timestamp DESC LIMIT 25", information, searchType);
                        overviewVehicles.AddRange(GetOverviewVehiclesFromDb(cmd, "SELECT team.name_short as name, vehicle_id, plate, status, timestamp FROM vehicle_registrations INNER JOIN team ON team.id = owner_id*-1 WHERE vehicle_id = @value AND owner_id < 0 ORDER BY vehicle_registrations.timestamp DESC LIMIT 25", information, searchType));
                        break;
                }
            }

            return overviewVehicles;
        }


        private static List<PlateHistory> GetOverviewVehiclesFromDb(MySqlCommand cmd, string statement, String information, SearchType type)
        {
            cmd.CommandText = statement;
            switch (type)
            {
                case SearchType.PLATE:
                    cmd.Parameters.AddWithValue("@value", $"%{information}%");
                    break;
                case SearchType.VEHICLEID:
                    cmd.Parameters.AddWithValue("@value", Int32.Parse(information));
                    break;
            }
            cmd.Prepare();
            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                List<PlateHistory> ownVehicles = new List<PlateHistory>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PlateHistory vehicle = new PlateHistory
                        {
                            Id = reader.GetUInt32("vehicle_id"),
                            Owner = reader.GetString("name1"),
                            Officer = reader.GetString("name2"),
                            Plate = reader.GetString("plate"),
                            Status = reader.GetInt32("status") == 1,
                            TimeStamp = reader.GetMySqlDateTime("timestamp").ToString()
                        };

                        ownVehicles.Add(vehicle);
                    }

                }
                cmd.Parameters.Clear();
                reader.Close();
                return ownVehicles;
            }
        }
    }

}
