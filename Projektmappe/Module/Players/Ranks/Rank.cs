using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Players.Ranks
{
    public class Rank : Loadable<uint>
    {
        public uint Id { get; }

        public string Name { get; }

        public string Marker { get; }

        public HashSet<string> Commands { get; }

        public HashSet<string> Events { get; }
        
        public HashSet<string> Features { get; }

        public int Salary { get; }

        public Rank(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Marker = reader.GetString("marker");
            Commands = new HashSet<string>();
            Features = new HashSet<string>();
            Events = new HashSet<string>();
            var commandString = reader.GetString("commands");
            if (!string.IsNullOrEmpty(commandString))
            {
                foreach (var command in commandString.Split(','))
                {
                    Commands.Add(command.ToLower());
                }
            }
            
            var eventString = reader.GetString("events");
            if (!string.IsNullOrEmpty(eventString))
            {
                foreach (var eventName in eventString.Split(','))
                {
                    Events.Add(eventName.ToLower());
                }
            }

            var featureString = reader.GetString("features");
            if (!string.IsNullOrEmpty(featureString))
            {
                foreach (var feature in featureString.Split(','))
                {
                    Features.Add(feature);
                }
            }

            Salary = reader.GetInt32("salary");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public IEnumerable<string> GetFeatures()
        {
            return Features;
        }

        //Todo: replace with default contains implementation when commands are gone
        public bool CanAccessCommand(string command)
        {
            if (command.Contains(" "))
            {
                var split = command.Split(' ');
                if (split.Length > 0)
                {
                    command = split[0];
                }
            }

            return Commands.Contains(command.Replace("/", ""));
        }

        public bool CanAccessFeature(string feature)
        {
            return Features.Contains(feature);
        }
        
        public bool CanAccessEvent(string eventName)
        {
            return Events.Contains(eventName);
        }

        public string GetDisplayName()
        {
            return "[" + Name + "]";
        }
    }
}