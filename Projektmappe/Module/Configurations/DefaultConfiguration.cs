using System;
using System.Collections.Generic;

namespace GVRP.Module.Configurations
{
    public class DefaultConfiguration
    {
        public bool DevLog { get; }
        public bool Ptr { get; }
        public bool DevMode { get; }
        public string VoiceChannel { get; }
        public string VoiceChannelPassword { get; }
        public bool IsServerOpen { get; set; }
        public bool InventoryActivated { get; set; }
        public bool EKeyActivated { get; set; }
        public bool BlackMoneyEnabled { get; set; }
        public bool MeertraeubelEnabled { get; set; }
        public bool JailescapeEnabled { get; set; }
        public bool MethLabEnabled { get; set; }
        public bool JumpPointsEnabled { get; set; }
        public string mysql_pw { get; }
        public string mysql_user { get; }
        public string mysql_user_forum { get; }
        public string mysql_pw_forum { get; }
        public bool disableAPILogin { get; set; }
        public bool LipsyncActive { get; set; }
        public bool TuningActive { get; set; }

        public bool CanBridgeUsed { get; set; }
        public int MaxPlayers { get; set; }
        public bool IsUpdateModeOn { get; set; }

        public string TeamspeakQueryAddress { get; set; }
        public string TeamspeakQueryPort { get; set; }
        public string TeamspeakPort { get; set; }
        public string TeamspeakLogin { get; set; }
        public string TeamspeakPassword { get; set; }
        public bool PlayerSync { get; set; } = true;
        public bool VehicleSync { get; set; } = true;

        public float WeaponDamageMultipier { get; set; }
        public float MeeleDamageMultiplier { get; set; }
        public string RESET_API_KEY { get; set; }
        public string CLEAR_API_KEY { get; set; }
        
        public DefaultConfiguration(IReadOnlyDictionary<string, dynamic> data)
        {
            DevLog = bool.Parse(data["devlog"]);
            Ptr = bool.Parse(data["ptr"]);
            DevMode = bool.Parse(data["devmode"]);
            VoiceChannel = data["V_Channel"];
            VoiceChannelPassword = data["V_PW"];
            IsServerOpen = false;
            InventoryActivated = true;
            EKeyActivated = true;
            BlackMoneyEnabled = true;  //set to true later
            MethLabEnabled = true;  //set to true later
            MeertraeubelEnabled = true; //set to true later
            JailescapeEnabled = false;
            JumpPointsEnabled = true;
            mysql_pw = data["mysql_pw"];
            mysql_user = data["mysql_user"];
            mysql_user_forum = data["mysql_user_forum"];
            mysql_pw_forum = data["mysql_pw_forum"];

            TeamspeakQueryAddress = data["ts_query_address"];
            TeamspeakQueryPort = data["ts_query_port"];
            TeamspeakPort = data["ts_port"];
            TeamspeakLogin = data["ts_login"];
            TeamspeakPassword = data["ts_password"];
            CanBridgeUsed = false;
            MaxPlayers = data.ContainsKey("max_players") ? int.Parse(data["max_players"]) : 1000;

            // Damage Multipliers
            MeeleDamageMultiplier = 0.5f;
            WeaponDamageMultipier = 0.75f; // Rage default vermutlich so 0.3 iwas

            disableAPILogin = false;
            LipsyncActive = data.ContainsKey("lipsync") ? bool.Parse(data["lipsync"]) : false;
            TuningActive = true;
            IsUpdateModeOn = bool.Parse(data["updatemode"]);
            RESET_API_KEY = data.ContainsKey("reset_api_key") ? data["reset_api_key"] : "";
            CLEAR_API_KEY = data.ContainsKey("clear_api_key") ? data["clear_api_key"] : "";
        }
        
        public string GetMySqlConnection()
        {
           // Console.WriteLine("GetMySqlConnection" + "write");
            return Ptr
                ? "server='localhost'; uid='root'; pwd='root'; database='gvrp';max pool size=999;SslMode=none;"
                : "server='localhost'; uid='root'; pwd='root'; database='gvrp';max pool size=999;SslMode=none;";
            //   Console.WriteLine("connected bitch" + "write");
        }

        public string GetMySqlConnectionForum()
        {
            return

                "server='localhost'; uid='wcf'; pwd='?1bf4Y2a'; database='wcf1';max pool size=999;SslMode=none;";
                Console.WriteLine("sexwithniggas");
        }

        public override string ToString()
        {
            return $"Devlog: {DevLog}\n" +
                   $"Ptr: {Ptr}\n" +
                   $"DevMode: {DevMode}\n" +
                   $"VoiceChannel: {VoiceChannel}\n" +
                   $"VoiceChannelPassword: {VoiceChannelPassword}\n";
        }
    }
}