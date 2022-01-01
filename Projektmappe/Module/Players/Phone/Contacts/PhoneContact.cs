using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace GVRP.Module.Players.Phone.Contacts
{
    public class PhoneContact
    {
        [JsonIgnore]
        public uint Id { get; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "number")]
        public uint Number { get; set; }

        public PhoneContact(MySqlDataReader reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Number = reader.GetUInt32("number");
        }

        public PhoneContact(string name, uint number)
        {
            Name = name;
            Number = number;
        }
    }
}