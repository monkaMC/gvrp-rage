using System;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;

namespace GVRP.Module
{
    public abstract class SqlSingleItemModule<T, TLoadable, TRequest, TId> : Module<T>
        where T : Module<T>
        where TLoadable : Loadable<TId>
    {
        protected virtual TLoadable RequestItem(TRequest request)
        {
            var query = OnItemRequest(request);
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return null;
                    while (reader.Read())
                    {
                        if (!(Activator.CreateInstance(typeof(TLoadable), reader) is TLoadable u)) continue;
                        return u;
                    }
                }
            }

            return null;
        }

        protected abstract string OnItemRequest(TRequest request);
    }
}