using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Keys;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Storage;

namespace GVRP.Handler
{
    public class StorageKeyHandler
    {
        public static StorageKeyHandler Instance { get; } = new StorageKeyHandler();

        private StorageKeyHandler()
        {

        }

        private string tableName = "player_to_storage";

        public void AddStorageKey(DbPlayer iPlayer, StorageRoom storageRoom)
        {
            if (iPlayer.StorageKeys.Contains(storageRoom.Id)) return;
            iPlayer.StorageKeys.Add(storageRoom.Id);
            MySQLHandler.ExecuteAsync(
                    $"INSERT INTO `{tableName}` (`player_id`, `storage_id`) VALUES ('{iPlayer.Id}', '{storageRoom.Id}');");
        }

        public void DeleteStorageKey(DbPlayer iPlayer, StorageRoom storageRoom)
        {
            if (!iPlayer.StorageKeys.Contains(storageRoom.Id)) return;
            iPlayer.StorageKeys.Remove(storageRoom.Id);
            MySQLHandler.ExecuteAsync(
                $"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}' AND `player_id` = '{iPlayer.Id}';");
        }

        public void DeleteAllStorageKeys(StorageRoom storageRoom)
        {
            foreach (var iPlayer in Players.Instance.GetValidPlayers())
            {
                if (iPlayer?.StorageKeys == null) continue;
                if (iPlayer.StorageKeys.Contains(storageRoom.Id))
                {
                    iPlayer.StorageKeys.Remove(storageRoom.Id);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}';");
        }

        public void GetAllStorageKeys(StorageRoom storageRoom)
        {
            foreach (var iPlayer in Players.Instance.GetValidPlayers())
            {
                if (iPlayer?.StorageKeys == null) continue;
                if (iPlayer.StorageKeys.Contains(storageRoom.Id))
                {
                    iPlayer.StorageKeys.Remove(storageRoom.Id);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `{tableName}` WHERE `storage_id` = '{storageRoom.Id}';");
        }

        public async Task LoadStorageKeys(DbPlayer iPlayer)
        {
            
                using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var keyCmd = keyConn.CreateCommand())
                {
                    await keyConn.OpenAsync();
                    keyCmd.CommandText =
                        $"SELECT storage_id FROM `{tableName}` WHERE player_id = '{iPlayer.Id}';";
                    using (var keyReader = keyCmd.ExecuteReader())
                    {
                        if (keyReader.HasRows)
                        {
                            while (keyReader.Read())
                            {
                                var keyId = (uint)keyReader.GetInt32(0);
                                if (!iPlayer.StorageKeys.Contains(keyId))
                                {
                                    iPlayer.StorageKeys.Add(keyId);
                                }
                            }
                        }
                    }
                    await keyConn.CloseAsync();
                }
            
        }

        public List<VHKey> GetAllKeysPlayerHas(DbPlayer iPlayer)
        {
            List<VHKey> storages = new List<VHKey>();
            foreach (uint storage in iPlayer.StorageKeys)
            {
                storages.Add(new VHKey("" + storage, storage));
            }
            foreach (KeyValuePair<uint, StorageRoom> kvp in StorageRoomModule.Instance.GetAll().Where(st => st.Value.OwnerId == iPlayer.Id))
            {
                storages.Add(new VHKey("" + kvp.Key, kvp.Key));
            }
            return storages;
        }

        public List<VHKey> GetOwnStorageKey(DbPlayer iPlayer)
        {
            List<VHKey> storages = new List<VHKey>();
            foreach(KeyValuePair<uint, StorageRoom> kvp in StorageRoomModule.Instance.GetAll().Where(st => st.Value.OwnerId == iPlayer.Id))
            {
                storages.Add(new VHKey("" + kvp.Key, kvp.Key));
            }
            return storages;
        }

    }
}