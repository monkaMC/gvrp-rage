using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace GVRP.Module.Node
{
    public static class Request
    {
        private static readonly HttpClient client = new HttpClient();
        private static readonly string url = Configurations.Configuration.Instance.DevMode ? "http://localhost:3007/" : "http://localhost:3000/";

        public static void Call(string name, string args)
        {
            var call = new Dictionary<string, string>
                {
                    { "name", name },
                    { "args", args }
                };

            Post("call", new FormUrlEncodedContent(call));
        }

        public static async void CallEntity(string entity,  string name, string args)
        {
            
                var call = new Dictionary<string, string>
                {
                    { "entity", entity },
                    { "name", name },
                    { "args", args }
                };

                Post("call-entity", new FormUrlEncodedContent(call));
        }

        public static void SetEntity(string entity, string name, string args)
        {
            var call = new Dictionary<string, string>
                {
                    { "entity", entity },
                    { "name", name },
                    { "args", args }
                };

            Post("set-entity", new FormUrlEncodedContent(call));
        }

        public static void TriggerInStreamingRange(string playerid, string arg)
        {
            var args = new Dictionary<string, string>
                {
                    { "playerid", playerid },
                    { "value", arg }
                };

            Post("streamingrange", new FormUrlEncodedContent(args));
        }

        public static void SetPlayerProps(string playerid, string prop, string drawable, string texture)
        {
            var args = new Dictionary<string, string>
                {
                    { "playerid", playerid },
                    { "prop", prop },
                    { "drawable", drawable },
                    { "texture", texture }
                };

            Post("setprop", new FormUrlEncodedContent(args));
        }

        async private static void Post(string call, FormUrlEncodedContent content)
        {
            try
            {
                await client.PostAsync(url + call, content);
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }
    }
}
