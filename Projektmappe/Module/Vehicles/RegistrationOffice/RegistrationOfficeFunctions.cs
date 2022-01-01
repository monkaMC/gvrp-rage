using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Vehicles.RegistrationOffice
{
    public class RegistrationOfficeFunctions
    {
        //Fester Preis für anmeldung

        public static int REGISTRATION_COST_NORMAL = 5000;
        public static int REGISTRATION_COST_WISH = 30000;

        public static int RegistrationRadius = 20;
        
        //Fahrzeug Registrierung

        public static bool registerVehicle(SxVehicle sxVehicle, DbPlayer owner, DbPlayer worker, String plate, bool wish)
        {
            if (worker == null || !worker.IsValid())
            {
                return false;
            }

            if (sxVehicle == null || !sxVehicle.IsValid())
            {
                return false;
            }
            //check if owner and person who is from dpos isnt offline or shit
            if (owner == null || worker == null || !owner.IsValid() || !worker.IsValid()) return false;

            //calculate costs for plate 
            int costs = wish == true ? REGISTRATION_COST_WISH : REGISTRATION_COST_NORMAL;
            if (owner.bank_money[0] < costs)
            {
                worker.SendNewNotification($"Das Konto des Kunden ist nicht gedeckt. {costs}$");
                owner.SendNewNotification("Ihr Konto ist nicht gedeckt... Sie benoetigen $" + costs);
                return false;
            }
            Logger.Print("dpos800");
            string vehiclePlate = GetRegisteredPlateByVehicleId(sxVehicle.databaseId, sxVehicle.teamid == 0 ? true : false);
            if (vehiclePlate.Equals(""))
            {
                Logger.Print("dpos700");

                //Plate has no entries -> is not registered

                Logger.Print("dpos500");
                //take money
                owner.TakeBankMoney(costs, "Fahrzeug angemeldet " + sxVehicle.databaseId);
                //update in database and log
                Logger.Print("dpos400+");
                sxVehicle.Registered = true;
                Logger.Print("dpos999900");
                UpdateVehicleRegistrationToDb(sxVehicle, owner, worker, plate, true);
                Logger.Print("dpos300");
                sxVehicle.plate = plate;
                Logger.Print("dpos88888KIDS0");
                sxVehicle.entity.NumberPlate = plate;
                Logger.Print("dposSOULCGHEATS");

                worker.GiveMoney(500);
                Logger.Print("dposMIYUDERHS");
                worker.SendNewNotification("Sie haben das Fahrzeug erfolgreich angemeldet.");
                owner.SendNewNotification("Ihr Fahrzeug wurde erfolgreich angemeldet.");
                return true;
            }
            else
            {
                //vehicle is already registered
                worker.SendNewNotification("Dieses Fahrzeug ist bereits mit einem Kennzeichen angemeldet. " + vehiclePlate);
            }

            return false;
        }

        public static bool IsPlateRegistered(String plate, bool privateCar)
        {
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM vehicle_registrations WHERE plate = '{plate}' AND owner_id {(privateCar ? ">0" : "<0")}  ORDER BY timestamp DESC LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetInt32("status") == 1)
                        {
                            conn.Close();
                            return true;
                        }
                    }
                    //Plate has no entries -> is not registered
                    conn.Close();
                    return false;
                }
            }
        }

        public static bool IsVehicleRegistered(uint vehicleId)
        {
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * FROM vehicle_registrations WHERE vehicle_id = " + vehicleId + " ORDER BY timestamp DESC LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.GetInt32("status") == 1)
                        {
                            conn.Close();
                            return true;
                        }
                    }
                    //Plate has no entries -> is not registered
                    conn.Close();
                    return false;
                }
            }
        }

        public static String GetRegisteredPlateByVehicleId(uint vehicleId, bool privateCar)
        {
            String plate = "";
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT plate FROM {(privateCar ? "vehicles" : "fvehicles")} WHERE id = " + vehicleId + " AND registered = 1 LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        plate = reader.GetString("plate");

                    }
                }
                conn.Close();

                return plate;
            }
        }
        public static string RandomString(int length)
        {
            Random random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }


        public static String GetRandomPlate(bool privateCar)
        {
            String plate = "";
                do
                {
                    plate = RandomString(8);
                    if (!IsPlateRegistered(plate, privateCar))break;
                } while (true);
            return plate;
        }

        public static void UpdateVehicleRegistrationToDb(SxVehicle sxVehicle, DbPlayer owner, DbPlayer officer, String plate, bool status)
        {
            if (owner == null || !owner.IsValid())
            {
                return;
            }

            if (sxVehicle == null || !sxVehicle.IsValid())
            {
                return;
            }
            String updateString = $"UPDATE vehicles SET Registered = {(status == true ? "1" : "0")}, plate = '{(status ? plate : "")}' WHERE id = {sxVehicle.databaseId}; INSERT INTO vehicle_registrations (owner_id, vehicle_id, officer_id, plate, status) VALUES({ owner.Id}, { sxVehicle.databaseId}, { officer.Id}, '{plate}', { (status == true ? 1 : 0)})"; ;
            if (sxVehicle.IsTeamVehicle())
            {
                updateString = $"UPDATE fvehicles SET Registered = {(status == true ? "1" : "0")}, plate = '{(status ? plate : "")}' WHERE id = {sxVehicle.databaseId}; INSERT INTO vehicle_registrations (owner_id, vehicle_id, officer_id, plate, status) VALUES({ (owner.Team.Id*-1)}, { sxVehicle.databaseId}, { officer.Id}, '{plate}', { (status == true ? 1 : 0)})";
            }


            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = updateString;
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void UpdateVehiclePlate(SxVehicle sxVehicle)
        {
            if (sxVehicle == null || !sxVehicle.IsValid())
            {
                return;
            }
            String tableName = "vehicles";
            if (sxVehicle.IsTeamVehicle())
            {
                tableName = "fvehicles";
            }


            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"UPDATE {tableName} SET plate = '{sxVehicle.plate}' WHERE id = {sxVehicle.databaseId}";
                cmd.ExecuteNonQuery();
                conn.Close();
            }
        }

        public static void GiveVehicleContract(DbPlayer owner, SxVehicle vehicle, string seller)
        {
            if (vehicle == null || !vehicle.IsValid())
            {
                return;
            }
            if (owner == null || !owner.IsValid())
            {
                return;
            }
            var info = $"Besitzer: {owner.Player.Name} Fahrzeug: {vehicle.GetName()} ({vehicle.databaseId}) am {DateTime.Now.ToString("dd.MM.yyyy HH:mm")} kauf durch {seller}";

            if(vehicle != null && vehicle.IsValid() && vehicle.Container != null)
                vehicle.Container.AddItem(641, 1, new Dictionary<string, dynamic>() { { "Info", info } });
            else
                owner.Container.AddItem(641, 1, new Dictionary<string, dynamic>() { { "Info", info } });
        }


        //Fahrzeug abmelden

        //Fahrzeug überprüfen







    }
}
