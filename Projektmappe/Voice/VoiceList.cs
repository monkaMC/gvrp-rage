using System.Collections.Generic;
using System.Linq;
using System;
using GTANetworkAPI;
using GVRP.Module.Players.Db;
using GVRP.Module.Configurations;

namespace GVRP
{
    public sealed class VoiceListHandler
    {
        public static VoiceListHandler Instance { get; } = new VoiceListHandler();
        
        public Dictionary<int, DbPlayer> voiceHashList { get; set; }

        // Voice Ranges
        public const float VoiceRangeNormal = 12.0f;
        public const float VoiceRangeWhisper = 5.0f;
        public const float VoiceRangeShout = 45.0f;

        public readonly Dictionary<uint, string> PlayerVoiceList;
        
        public static readonly Random Random = new Random();
        
        private VoiceListHandler()
        {
            PlayerVoiceList = new Dictionary<uint, string>();
            voiceHashList = new Dictionary<int, DbPlayer>();
        }
        
        public static void AddToDeath(DbPlayer iPlayer)
        {
            if (!iPlayer.Player.HasSharedData("isInjured"))
            {
                iPlayer.Player.SetSharedData("isInjured", true);
            }
        }

        public static void RemoveFromDeath(DbPlayer iPlayer)
        {
            if (iPlayer.Player.HasSharedData("isInjured"))
            {
                iPlayer.Player.ResetSharedData("isInjured");
            }
        }
        
        public void InitPlayerVoice(DbPlayer iPlayer)
        {
            iPlayer.Player.TriggerEvent("setVoiceData", 1, "Ingame", "nigga111");
            if (!PlayerVoiceList.ContainsKey(iPlayer.Id))
            {
                int i = 0;
                while (voiceHashList.ContainsKey(i)) i++;

                voiceHashList.Add(i, iPlayer);


                    iPlayer.VoiceHash = "" + ((i < 100) ? "00" + i : "" + i);
                    PlayerVoiceList.Add(iPlayer.Id, "" + i);




            }
            iPlayer.Player.SetSharedData("voiceHash", iPlayer.VoiceHash);
        }

        private string GeneratePlayerHash()
        {
            var hash = RandomString(10);
            while(PlayerVoiceList.ContainsValue(hash))
            {
                hash = RandomString(10);
            }
            return hash;
        }

        private static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        public static void SetCaller(DbPlayer iPlayer, string callername)
        {
            iPlayer.Player.TriggerEvent("setCallingPlayer", callername);
        }

        // 0 aus, 1 an
        public static void SetPlayerRadio(Client player, int status)
        {
            player.TriggerEvent("setRadio", status);
        }
    }
}
