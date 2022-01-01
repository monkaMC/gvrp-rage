using MySql.Data.MySqlClient;

namespace GVRP.Module.Crime
{
    public class CrimeCategory : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int Order { get; set; }

        public CrimeCategory(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Order = reader.GetInt32("order");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}