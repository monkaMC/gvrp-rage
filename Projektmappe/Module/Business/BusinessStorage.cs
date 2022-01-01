using MySql.Data.MySqlClient;
using System;
using GVRP.Module.Configurations;

namespace GVRP.Module.Business
{
    public class BusinessStorage
    {
        public static BusinessStorage Instance { get; } = new BusinessStorage();

        private BusinessStorage()
        {
        }

        public void DeleteAllStorageKeys(uint storageId)
        {
            foreach (var biz in BusinessModule.Instance.GetAll().Values)
            {
                if (biz?.StorageKeys == null) continue;
                if (biz.StorageKeys.Contains(storageId))
                {
                    biz.StorageKeys.Remove(storageId);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `business_storages` WHERE `storage_id` = '{storageId}';");
        }
    }
    public static class BusinessStorageExtension
    {
        public static void DeleteStorageKey(this Business biz, int storageId)
        {
            MySQLHandler.ExecuteAsync(
                    $"DELETE FROM `business_storages` WHERE `business_id` = '{biz.Id}' AND `storage_id` = '{storageId}';");
        }

        public static void AddStorageKey(this Business biz, uint storageId)
        {
            if (storageId == 0) return;
            if (biz.StorageKeys.Contains(storageId)) return;
            biz.StorageKeys.Add(storageId);
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO `business_storages` (`business_id`, `storage_id`) VALUES ('{biz.Id}', '{storageId}');");
        }

        public static void LoadStorageKeys(this Business biz)
        {
            biz.StorageKeys.Clear();

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText =
                    $"SELECT storage_id FROM `business_storages` WHERE business_id = '{biz.Id}';";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                    {
                        while (keyReader.Read())
                        {
                            var keyId = keyReader.GetUInt32(0);
                            if (!biz.StorageKeys.Contains(keyId))
                            {
                                biz.StorageKeys.Add(keyId);
                            }
                        }
                    }
                }
                keyConn.Close();
            }
        }
    }
}