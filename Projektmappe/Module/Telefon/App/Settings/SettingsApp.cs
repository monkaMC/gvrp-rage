using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Voice;

namespace GVRP.Module.Telefon.App
{
    public class SettingsApp : SimpleApp
    {
        public SettingsApp() : base("SettingsApp")
        {
        }

        [RemoteEvent]
        public void requestPhoneSettings(Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            TriggerEvent(player, "responsePhoneSettings", dbPlayer.phoneSetting.flugmodus, dbPlayer.phoneSetting.lautlos, dbPlayer.phoneSetting.blockCalls);
        }

        [RemoteEvent]
        public void savePhoneSettings(Client player, bool flugmodus, bool lautlos, bool blockCalls)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            dbPlayer.phoneSetting.flugmodus = flugmodus;
            dbPlayer.phoneSetting.lautlos = lautlos;
            dbPlayer.phoneSetting.blockCalls = blockCalls;

            if (flugmodus)
            {
                VoiceModule.Instance.ChangeFrequenz(dbPlayer, 0, true);
                VoiceModule.Instance.turnOffFunk(dbPlayer);
            }
        }
    }
}
