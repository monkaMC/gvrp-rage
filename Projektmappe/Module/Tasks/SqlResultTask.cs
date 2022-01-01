using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;

namespace GVRP.Module.Tasks
{
    public abstract class SqlResultTask : SynchronizedTask
    {
        public override void Execute()
        {
            using (var connection = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = GetQuery();
                    using (var reader = command.ExecuteReader())
                    {
                        OnFinished(reader);
                    } 
                }
            }
        }

        public abstract string GetQuery();

        public abstract void OnFinished(MySqlDataReader reader);
    }
}