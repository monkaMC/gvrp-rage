using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GVRP.Module.Injury
{
    public class InjuryCauseOfDeath : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public uint Hash { get; }
        public bool KilledByPlayer { get; }
        public HashSet<InjuryType> InjuryTypes { get; }

        public InjuryCauseOfDeath(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Hash = reader.GetUInt32("hash");
            KilledByPlayer = reader.GetInt32("killed_by_player") == 0 ? false : true;
            InjuryTypes = new HashSet<InjuryType>();
            if (!string.IsNullOrEmpty(reader.GetString("injury_types")))
            {
                foreach (var splittedInjury in reader.GetString("injury_types").Split(','))
                {
                    if (uint.TryParse(splittedInjury, out var injuryId)) InjuryTypes.Add(InjuryTypeModule.Instance[injuryId]);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}