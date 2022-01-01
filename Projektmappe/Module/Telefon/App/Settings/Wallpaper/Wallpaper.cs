using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace GVRP.Module.Telefon.App.Settings.Wallpaper
{
    public class Wallpaper : Loadable<uint>
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }
        [JsonProperty(PropertyName = "file")]
        public string File { get;}
        [JsonIgnore]
        public bool isTeamOnly { get; }

        public Wallpaper(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            File = reader.GetString("file");
            isTeamOnly = reader.GetBoolean("isTeam");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
