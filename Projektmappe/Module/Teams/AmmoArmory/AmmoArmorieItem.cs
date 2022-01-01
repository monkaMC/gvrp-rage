using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Teams.AmmoArmory
{
    public class AmmoArmorieItem : Loadable<uint>
    {
        public UInt32 Id { get; set; }
        public UInt32 AmmoArmorieId { get; set; }
        public UInt32 ItemId { get; set; }
        public int RequiredPackets { get; set; }

        public int TeamPrice { get; set; }

        public AmmoArmorieItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            AmmoArmorieId = reader.GetUInt32("ammoarmorie_id");
            ItemId = reader.GetUInt32("item_id");
            RequiredPackets = reader.GetInt32("required_packets");
            TeamPrice = reader.GetInt32("team_price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void ChangeTeamPrice(int newprice)
        {
            TeamPrice = newprice;
            MySQLHandler.ExecuteAsync($"UPDATE team_ammoarmories_items SET team_price = '{TeamPrice}' WHERE id = '{Id}'");
        }

        public int GetRequiredPacketsForTeam(DbTeam dbTeam)
        {
            if (dbTeam.TeamMetaData.Respect >= 6000)
            {
                return RequiredPackets - 6;
            }
            else if (dbTeam.TeamMetaData.Respect >= 5000)
            {
                return RequiredPackets - 5;
            }
            else if (dbTeam.TeamMetaData.Respect >= 4000)
            {
                return RequiredPackets - 4;
            }
            else if (dbTeam.TeamMetaData.Respect >= 3000)
            {
                return RequiredPackets - 3;
            }
            else if (dbTeam.TeamMetaData.Respect >= 2000)
            {
                return RequiredPackets - 2;
            }
            else if (dbTeam.TeamMetaData.Respect >= 1000)
            {
                return RequiredPackets - 1;
            }
            else return RequiredPackets;
        }
    }
}
