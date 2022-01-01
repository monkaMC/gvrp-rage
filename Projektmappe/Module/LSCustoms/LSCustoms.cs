using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;

namespace GVRP.Module.LSCustoms
{
    public class LSCustoms : Loadable<uint>
    {
        public uint id { get; set; }
        public string name { get; set; }
        public uint type { get; set; }
        public Vector3 position { get; set; }

        public LSCustoms(MySqlDataReader reader) : base(reader)
        {
            id = reader.GetUInt32("id");
            name = reader.GetString("name");
            type = reader.GetUInt32("type");
            position = new Vector3(reader.GetFloat("x"), reader.GetFloat("y"), reader.GetFloat("z"));
        }

        public override uint GetIdentifier()
        {
            return id;
        }
    }

    public class LSCRechnung
    {
        public uint id { get; private set; }
        public uint tuner_id { get; private set; }
        public uint amount { get; private set; }
        public uint payerID { get; private set; }
        public uint vehicleID { get; private set; }

        public LSCRechnung(uint p_BillID, uint p_TunerID, uint p_Amount, uint p_PayerID, uint p_VehicleID)
        {
            id = p_BillID;
            tuner_id = p_TunerID;
            amount = p_Amount;
            payerID = p_PayerID;
            vehicleID = p_VehicleID;
        }

        public bool Save()
        {
            string l_SearchQuery = $"SELECT * FROM `lsc_bills` WHERE `bill_id`={id.ToString()};";

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = l_SearchQuery;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                        return false;
                }
                conn.Close();
            }

            string l_InsertQuery = $"INSERT INTO `lsc_bills` (`bill_id`, `tuner_id`, `amount`, `payerID`, `vehicleID`) VALUES " +
                $"({id.ToString()}, {tuner_id.ToString()}, {amount.ToString()}, {payerID.ToString()}, {vehicleID.ToString()});";

            MySQLHandler.ExecuteAsync(l_InsertQuery);
            return true;
        }
    }
}
