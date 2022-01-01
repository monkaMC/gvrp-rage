using MySql.Data.MySqlClient;

namespace GVRP.Module
{
    public abstract class Loadable<T> : Identifiable<T>
    {
        public Loadable(MySqlDataReader reader)
        {
        }

        public Loadable()
        {

        }

        public abstract T GetIdentifier();
    }
}