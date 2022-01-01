using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Sync;

namespace GVRP
{
    public static class MySQLHandler
    {
        public static void Execute(string query)
        {
            if (query == "") return;
            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void ExecuteAsync(string query, bool inventoryquery = false)
        {
            if (query == "") return;

            MySqlSyncThread.Instance.Add(query, inventoryquery);
        }

        public static async void InsertAsync(string tableName, params object[] data)
        {
            if (data.Length == 0) return;

            string columns = string.Join(",", data.Where((value, index) => index % 2 == 0));
            string values = string.Join(",", data.Where((value, index) => index % 2 == 1));

            string query = $"INSERT INTO {tableName} ({columns}) VALUES ({values})";

            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = query;
                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static async void UpdateAsync(string tableName, string condition, params object[] data)
        {
            if (data.Length == 0) return;

            int i = 0;
            string str = "";

            foreach (var item in data)
            {
                str += item;
                if (i < data.Length - 1)
                {
                    if (i % 2 == 0)
                        str += " = ";
                    else
                        str += ", ";
                }

                i++;
            }

            string query = $"UPDATE {tableName} SET {str} WHERE {condition}";

            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = query;
                    await cmd.ExecuteNonQueryAsync();
                    await conn.CloseAsync();
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public static void QueryFailed(Task task)
        {
            Exception ex = task.Exception;
            if (ex != null) Logger.Crash(ex);
        }

        public static void ExecuteForum(string query)
        {
            Task.Factory.StartNew(() =>
            {
                try
                {

                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            });
        }
    }
}