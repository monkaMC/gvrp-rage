using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;

namespace GVRP.Module.Players.Phone.Apps
{
    //TODO: rename to PlayerApp because it is used in computer as well
    public class PhoneApp : Loadable<string>
    {
        [JsonProperty(PropertyName = "id")] public string Id { get; }
        [JsonProperty(PropertyName = "name")] public string Name { get; }
        [JsonProperty(PropertyName = "icon")] public string Icon { get; }

        public PhoneApp(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetString("id");
            Name = reader.GetString("name");
            Icon = reader.GetString("icon");
        }

        public PhoneApp(string id, string name, string icon) : base(null)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }

        public override string GetIdentifier()
        {
            return Id;
        }
    }
    
    public class HomeApp : SimpleApp
    {
        public HomeApp() : base("HomeApp")
        {
        }

        [RemoteEvent]
        public void requestApps(Client player)
        {

            var dbPlayer = player.GetPlayer();
            var teamstring = "";
            var business = "";

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.TeamId != 0)
            {
                teamstring = "{\"id\":\"TeamApp\",\"name\":\"Team\",\"icon\": \"TeamApp.png\"}, ";

            }

            if (dbPlayer.ActiveBusiness != null)
            {
                business = "{\"id\":\"BusinessApp\",\"name\":\"Business\",\"icon\": \"BusinessApp.png\"}, ";

            }
            TriggerEvent(player, "responseApps", "[" + teamstring + " " + business + " {\"id\":\"FunkApp\",\"name\":\"Funkgerät\",\"icon\": \"FunkApp.png\"}, {\"id\":\"MessengerApp\",\"name\":\"SMS\",\"icon\": \"MessengerApp.png\"}, {\"id\":\"ContactsApp\",\"name\":\"Kontakte\",\"icon\": \"ContactsApp.png\"}, {\"id\":\"TelefonApp\",\"name\":\"Telefon\",\"icon\": \"TelefonApp.png\"}, {\"id\":\"ServiceRequestApp\",\"name\":\"Services\",\"icon\": \"ServiceApp.png\"}, {\"id\":\"LifeInvaderApp\",\"name\":\"LifeInvader\",\"icon\": \"LifeinvaderApp.png\"}, {\"id\":\"GpsApp\",\"name\":\"GPS\",\"icon\": \"GpsApp.png\"}, {\"id\":\"ProfileApp\",\"name\":\"Profil\",\"icon\": \"ProfilApp.png\"}, {\"id\":\"SettingsApp\",\"name\":\"Settings\",\"icon\": \"SettingsApp.png\"}, {\"id\":\"CalculatorApp\",\"name\":\"Rechner\",\"icon\": \"CalculatorApp.png\"}, {\"id\":\"TaxiApp\",\"name\":\"Taxi\",\"icon\": \"TaxiApp.png\"}]");
        }
        [RemoteEvent]
        public void requestPhoneWallpaper(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            //TriggerEvent(player, "responsePhoneWallpaper", dbPlayer.wallpaper.File);
        }

    }
}