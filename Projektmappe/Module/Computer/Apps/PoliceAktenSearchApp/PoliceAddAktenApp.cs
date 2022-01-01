using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime;
using GVRP.Module.Crime.PoliceAkten;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceAddAktenApp : SimpleApp
    {
        public PoliceAddAktenApp() : base("PoliceAddAktenApp")
        {
        }

        [RemoteEvent]
        public async void savePersonAkte(Client player, string playername, string json)
        {
            
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                // GetValid Player by searchname
                DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(playername);
                if (foundPlayer == null || !foundPlayer.IsValid()) return;

                ResponseAkteJson responseAkteJson = null;
                try
                {
                    responseAkteJson = JsonConvert.DeserializeObject<ResponseAkteJson>(json);
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                    return;
                }
                if (responseAkteJson == null) return;

                if (responseAkteJson.AktenId != 0) // edit existing
                {
                    if (!dbPlayer.CanAktenEdit())
                    {
                        dbPlayer.SendNewNotification("Keine Berechtigung!");
                        return;
                    }
                    PoliceAktenModule.Instance.SaveServerAkte(foundPlayer, responseAkteJson);
                    dbPlayer.SendNewNotification("Akte gespeichert.");
                }
                else // create new
                {
                    if (!dbPlayer.CanAktenCreate())
                    {
                        dbPlayer.SendNewNotification("Keine Berechtigung!");
                        return;
                    }
                    PoliceAktenModule.Instance.AddServerAkte(foundPlayer, responseAkteJson);
                    dbPlayer.SendNewNotification("Akte angelegt.");
                }
            
        }

        [RemoteEvent]
        public async void deletePersonAkte(Client player, int aktenId)
        {
            
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (aktenId != 0)
                {
                    if (!dbPlayer.CanAktenDelete())
                    {
                        dbPlayer.SendNewNotification("Keine Berechtigung!");
                        return;
                    }

                    PoliceAktenModule.Instance.DeleteServerAkte((uint)aktenId);
                    dbPlayer.SendNewNotification("Akte gelöscht.");
                }
            
        }

        public class ResponseAkteJson
        {
            [JsonProperty(PropertyName = "number")]
            public uint AktenId { get; set; }

            [JsonProperty(PropertyName = "title")]
            public string Title { get; set; }

            [JsonProperty(PropertyName = "text")]
            public string Text { get; set; }

            [JsonProperty(PropertyName = "created")]
            public DateTime Created { get; set; }

            [JsonProperty(PropertyName = "closed")]
            public DateTime Closed { get; set; }

            [JsonProperty(PropertyName = "officer")]
            public string Officer { get; set; }

            [JsonProperty(PropertyName = "open")]
            public bool Open { get; set; }
        }
    }
}
