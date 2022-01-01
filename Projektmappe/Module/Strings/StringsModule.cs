using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;

namespace GVRP.Module.Strings
{
    public sealed class StringsModule : Module<StringsModule>
    {
        private const string Lang = "german"; //Todo: read from config

        private readonly Dictionary<string, string> strings;

        public StringsModule()
        {
            strings = new Dictionary<string, string>();
        }

        protected override bool OnLoad()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT `key`, `{Lang}` FROM `languages`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return true;
                    while (reader.Read())
                    {
                        if (strings.ContainsKey(reader.GetString(0)))
                        {
                            Console.WriteLine(@"ERROR DUPLICATE KEY FOR " + reader.GetString(0));
                        }
                        else
                        {
                            strings.Add(reader.GetString(0), reader.GetString(1));
                        }
                    }
                }
            }
            return true;
        }

        public string this[string key] => !strings.TryGetValue(key, out var value) ? $"NOT FOUND {key}" : value;
    }
}