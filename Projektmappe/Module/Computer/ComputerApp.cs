using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GVRP.Module.Computer
{
    public class ComputerApp : Loadable<uint>
    {
        public uint Id { get; }
        public string AppName { get; }
        public string DisplayName { get; }
        public string IconPath { get; }
        public HashSet<uint> Teams { get; }
        public int Rang { get; }
        public bool Duty { get; }
        public ComputerTypes Type { get; }

        public ComputerApp(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            AppName = reader.GetString("app_name");
            DisplayName = reader.GetString("display_name");
            IconPath = reader.GetString("icon_path");
            Rang = reader.GetInt32("rang");
            Duty = reader.GetInt32("duty") == 1;
            Type = (ComputerTypes)reader.GetInt32("type");

            var teamString = reader.GetString("teams");
            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    Teams.Add(teamId);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class ComputerAppClientObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "appName")]
        public string AppName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string DisplayName { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string IconPath { get; set; }

        public ComputerAppClientObject(ComputerApp computerApp)
        {
            Id = computerApp.Id;
            AppName = computerApp.AppName;
            DisplayName = computerApp.DisplayName;
            IconPath = computerApp.IconPath;
        }

        public ComputerAppClientObject(uint id, string appName, string displayName, string iconPath)
        {
            Id = id;
            AppName = appName;
            DisplayName = displayName;
            IconPath = iconPath;
        }
    }
}
