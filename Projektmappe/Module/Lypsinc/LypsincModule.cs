using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Lypsinc
{
    class LypsincModule : Module<LypsincModule>
    {
        private static string TeamspeakQueryAddress { get; set; }
        private static short TeamspeakQueryPort { get; set; }
        private static string TeamspeakPort { get; set; }
        private static string TeamspeakLogin { get; set; }
        private static string TeamspeakPassword { get; set; }
        private static string TeamspeakChannel { get; set; }

        public override bool Load(bool reload = false)
        {
            if (Configurations.Configuration.Instance.DevMode) return false;
            TeamspeakQueryAddress = Configurations.Configuration.Instance.TeamspeakQueryAddress;

            short port = 0;
            if (short.TryParse(Configurations.Configuration.Instance.TeamspeakQueryPort, out port))
            {
                TeamspeakQueryPort = port;
            }
            else
            {
                Logging.Logger.Debug("Failed Convert Port");
                return false;
            }

            TeamspeakPort = Configurations.Configuration.Instance.TeamspeakPort;
            TeamspeakLogin = Configurations.Configuration.Instance.TeamspeakLogin;
            TeamspeakPassword = Configurations.Configuration.Instance.TeamspeakPassword;
            TeamspeakChannel = Configurations.Configuration.Instance.VoiceChannel;
            

            /*Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    CheckForLogins().Wait();
                    Thread.Sleep(2000);
                }
            }, TaskCreationOptions.LongRunning);*/

            return true;
        }


        public string ReplaceStr(string str)
        {
            str = str.Replace("\\\\", "\\");
            str = str.Replace("\\/", "/");
            str = str.Replace("\\s", " ");
            str = str.Replace("\\p", "|");
            str = str.Replace("\\a", "\a");
            str = str.Replace("\\b", "\b");
            str = str.Replace("\\f", "\f");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\r", "\r");
            str = str.Replace("\\t", "\t");
            str = str.Replace("\\v", "\v");
            return str;
        }
    }
}
