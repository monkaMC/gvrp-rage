using System;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Events
{
    public class Event : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public int ObjectGroup { get; }
        public bool IsActive { get; set; }

        public Event(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Name = reader.GetString(1);
            StartDate = reader.GetDateTime(2);
            EndDate = reader.GetDateTime(3);
            ObjectGroup = reader.GetInt32(4);

            UpdateActive();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        /**
         * Setzt Active wenn Datum größer als Start und kleiner als End Datum ist
         */
        public void UpdateActive()
        {
            IsActive = DateTime.Now >= EndDate && DateTime.Now <= StartDate;
        }
    }
}