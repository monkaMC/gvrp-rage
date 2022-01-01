using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Keys;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Handler
{
    public class VehicleKeyHandler
    {
        public static VehicleKeyHandler Instance { get; } = new VehicleKeyHandler();

        private VehicleKeyHandler()
        {
        }

        public void DeletePlayerKey(DbPlayer iPlayer, uint vehicleId)
        {
            if (!iPlayer.VehicleKeys.ContainsKey(vehicleId)) return;
            iPlayer.VehicleKeys.Remove(vehicleId);
            MySQLHandler.ExecuteAsync(
                $"DELETE FROM `player_to_vehicle` WHERE `playerID` = '{iPlayer.Id}' AND `vehicleID` = '{vehicleId}';");
        }

        public void DeleteAllVehicleKeys(uint vehicleId)
        {
            foreach (var iPlayer in Players.Instance.GetValidPlayers())
            {
                if (iPlayer?.VehicleKeys == null) continue;
                if (iPlayer.VehicleKeys.ContainsKey(vehicleId))
                {
                    iPlayer.VehicleKeys.Remove(vehicleId);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `player_to_vehicle` WHERE `vehicleID` = '{vehicleId}';");
        }

        public int GetVehicleKeyCount(uint vehicleId)
        {
            if (vehicleId == 0) return 0;
            int keyCount = 0;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM `player_to_vehicle` WHERE vehicleID = '{vehicleId}'";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            keyCount = reader.GetInt32(0);
                            break;
                        }
                    }
                }
            }

            return keyCount;
        }

        public void AddPlayerKey(DbPlayer iPlayer, uint vehicleId, string vehicleName)
        {
            if (vehicleId == 0) return;
            if (iPlayer.VehicleKeys.ContainsKey(vehicleId)) return;
            iPlayer.VehicleKeys.Add(vehicleId, vehicleName);
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO `player_to_vehicle` (`playerID`, `vehicleID`) VALUES ('{iPlayer.Id}', '{vehicleId}');");
        }

        public async Task LoadPlayerVehicleKeys(DbPlayer iPlayer)
        {
            
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText =
                        $"SELECT player_to_vehicle.vehicleID, vehicles.vehiclehash FROM player_to_vehicle INNER JOIN vehicles ON player_to_vehicle.vehicleID = vehicles.id WHERE playerID = '{iPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = keyReader.GetUInt32(0);
                                var keyName = keyReader.GetString(1);
                                if (!iPlayer.VehicleKeys.ContainsKey(keyId))
                                {
                                    iPlayer.VehicleKeys.Add(keyId, keyName);
                                }
                            }
                        }
                    }

                    keyCmd.CommandText = $"SELECT id, vehiclehash FROM `vehicles` WHERE owner = '{iPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = keyReader.GetUInt32(0);
                                var keyName = keyReader.GetString(1);
                                if (!iPlayer.OwnVehicles.ContainsKey(keyId))
                                {
                                    iPlayer.OwnVehicles.Add(keyId, keyName);
                                }
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer iPlayer)
        {
            List<VHKey> vehicles = new List<VHKey>();
            foreach (var item in iPlayer.VehicleKeys)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }
            foreach (var item in iPlayer.OwnVehicles)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }

            return vehicles;
        }

        public List<VHKey> GetOwnVehicleKeys(DbPlayer iPlayer)
        {
            List<VHKey> vehicles = new List<VHKey>();
            foreach (var item in iPlayer.OwnVehicles)
            {
                vehicles.Add(new VHKey(item.Value, item.Key));
            }

            return vehicles;
        }
    }
}