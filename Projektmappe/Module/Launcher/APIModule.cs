using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using GVRP.Module.Helper;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Zone;

namespace GVRP.Module.Launcher
{
    public sealed class APIModule : Module<APIModule>
    {
        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            try
            {
                if (!Configurations.Configuration.Instance.DevMode)
                {
                    using (WebClient l_WebClient = new WebClient())
                    {
                        l_WebClient.Encoding = System.Text.Encoding.UTF8;
                        l_WebClient.Headers["Content-Type"] = "application/json";

                        ResetWhitelistData l_ResetData = new ResetWhitelistData(dbPlayer.ForumId.ToString());
                        string l_Response = l_WebClient.UploadString("https://launcher.gvmp.de:5002/player/whitelist/reset", JsonConvert.SerializeObject(l_ResetData));
                        ResetWhitelistDataAnswer l_Answer = JsonConvert.DeserializeObject<ResetWhitelistDataAnswer>(l_Response);
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        public bool ClearWhitelist()
        {
            using (WebClient l_Client = new WebClient())
            {
                l_Client.Encoding = System.Text.Encoding.UTF8;
                l_Client.Headers["Content-Type"] = "application/json";

                ClearWhitelistData l_Data = new ClearWhitelistData();
                var l_Response = l_Client.UploadString("https://launcher.gvmp.de:5002/player/whitelist/clear", JsonConvert.SerializeObject(l_Data));
                var l_ResponseData = JsonConvert.DeserializeObject<ClearWhitelistDataAnswer>(l_Response);

                if (!l_ResponseData.success)
                    return false;

                return true;
            }
        }

        public bool IsWhitelisted(uint p_ForumID)
        {
            using (WebClient l_Client = new WebClient())
            {
                try
                {
                    string l_Response = l_Client.DownloadString("https://launcher.gvmp.de:5002/player/whitelist/" + p_ForumID.ToString());
                    WhitelistDataAnswer l_Answer = JsonConvert.DeserializeObject<WhitelistDataAnswer>(l_Response);
                    return l_Answer.success;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }
    }
}
