using GTANetworkAPI;
using System;
using System.Globalization;
using System.Threading.Tasks;
using GVRP.Module.Logging;
using GVRP.Module.Players;

namespace GVRP.Module.Voice
{
    public class VoiceEventHandler : Script
    {
        [RemoteEvent]
        public static void ChangeVoicRange(Client client, uint garageId, string state)
        {
           client.SetSharedData("VOICE_RANGE", state);
        }

        [RemoteEvent]
        public static void requestVoiceSettings(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            client.TriggerEvent("responseVoiceSettings", NAPI.Util.ToJson(new VoiceSettings(VoiceModule.Instance.getPlayerFrequenz(dbPlayer), (int)dbPlayer.funkStatus)));
        }

        [RemoteEvent]
        public static void changeFrequenz(Client client, string frequenz)
        {
            if (frequenz == "" || frequenz.Length < 1 || frequenz.Length > 9) return;

            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            // Check Cuff Die Death
            if (!dbPlayer.CanInteract()) return;

            if (!double.TryParse(frequenz, NumberStyles.Any, CultureInfo.InvariantCulture, out double fq)) return;

            VoiceModule.Instance.ChangeFrequenz(dbPlayer, fq);
        }

        [RemoteEvent]
        public static void changeSettings(Client client, int state)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;
            dbPlayer.funkStatus = (FunkStatus)state;
            VoiceModule.Instance.refreshFQVoiceForPlayerFrequenz(dbPlayer);

            switch (dbPlayer.funkStatus)
            {
                case FunkStatus.Active:
                    if (!client.IsInVehicle && !dbPlayer.IsCuffed && !dbPlayer.IsTied)
                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "random@arrests", "generic_radio_chatter");

                    break;
                case FunkStatus.Hearing:
                case FunkStatus.Deactive:
                    if (client.IsInVehicle)
                        break;

                    dbPlayer.Player.StopAnimation();
                    break;
                default:
                    break;
            }

            client.TriggerEvent("updateVoiceState", state);
        }
                
        public static void Connect(Client player, string characterName)
        {
            player.TriggerEvent("ConnectTeamspeak");
        }
    }
}