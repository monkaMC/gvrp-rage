using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.VirtualGarages
{
    public class VirtualGarage : Loadable<uint>
    {
        public uint Id { get; }

        public Vector3 Position { get; set; }

        public string Name { get; set; }

        public HashSet<uint> Teams { get; set; }

        public HashSet<uint> Houses { get; set; }

        public bool ShowMarker { get; set; }

        public VirtualGarage(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Name = reader.GetString("name");

            ShowMarker = reader.GetInt32("show_marker") == 1;

            var teamstring = reader.GetString("teams");
            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamstring))
            {
                var splittedTeams = teamstring.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    Teams.Add(teamId);
                }
            }

            var housestring = reader.GetString("houses");
            Houses = new HashSet<uint>();
            if (!string.IsNullOrEmpty(housestring))
            {
                var splittedHouses = housestring.Split(',');
                foreach (var houseIdString in splittedHouses)
                {
                    if (!uint.TryParse(houseIdString, out var houseid)) continue;
                    Houses.Add(houseid);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
