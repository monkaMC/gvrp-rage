using System;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;

namespace GVRP.Module.Tasks
{
    public abstract class SqlTask : SynchronizedTask
    {
        public override void Execute()
        {
            OnFinished(ExecuteNonQuery(GetQuery()));
        }

        public abstract string GetQuery();

        public int ExecuteNonQuery(string query)
        {
            int result;
            using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            {
                connection.Open();
                try
                {
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = query;
                        result = command.ExecuteNonQuery();
                    }
                }
                catch (Exception e)
                {
                    Logger.Print($"{GetType()} erorr: {e}");
                    result = -1;
                }
            }

            return result;
        }

        public abstract void OnFinished(int result);
    }
}