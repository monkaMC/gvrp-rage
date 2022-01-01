using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.PlayerName;

/// <summary>
/// This function checks for any sleeping connections beyond a reasonable time and kills them.
/// Since .NET appears to have a bug with how pooling MySQL connections are handled and leaves
/// too many sleeping connections without closing them, we will kill them here.
/// </summary>
/// iMinSecondsToExpire - all connections sleeping more than this amount in seconds will be killed.
/// <returns>integer - number of connections killed</returns>
/// 
namespace GVRP
{
    public class DBLogging
    {
        public static void LogAdminAction(Client admin, string user, adminLogTypes type, string reason, int optime = 0,
            bool devmode = false)
        {
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case adminLogTypes.perm:
                    xtype = "permanent Ban";
                    break;
                case adminLogTypes.timeban:
                    xtype = "terminated Ban (" + optime + ")";
                    break;
                case adminLogTypes.kick:
                    xtype = "kick";
                    break;
                case adminLogTypes.warn:
                    xtype = "Verwarnung";
                    break;
                case adminLogTypes.log:
                    xtype = "Logging";
                    break;
                case adminLogTypes.whisper:
                    xtype = "Whisper";
                    break;
                case adminLogTypes.setitem:
                    xtype = "Setitem";
                    break;
                case adminLogTypes.coord:
                    xtype = "Coord";
                    break;
                case adminLogTypes.veh:
                    xtype = "Veh";
                    break;
                case adminLogTypes.arev:
                    xtype = "Arev";
                    break;
            }
            
            string query = "";

            // Special Whisperlog
            if (type == adminLogTypes.whisper)
            {
                query = string.Format(
                    "INSERT INTO `log_whisper` (`sender`, `player`, `message`) VALUES ('{0}', '{1}', '{2}')",
                    admin.Name, MySqlHelper.EscapeString(user), MySqlHelper.EscapeString(reason));
            }
            else
            {
                query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    admin.Name, MySqlHelper.EscapeString(user), (int)type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            }
            MySQLHandler.ExecuteAsync(query);
        }

        public static void LogAcpAdminAction(PlayerName admin, string user, adminLogTypes type, string reason)
        {
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case adminLogTypes.kick:
                    xtype = "acp-kick";
                    break;
                case adminLogTypes.whisper:
                    xtype = "acp-whisper";
                    break;
                case adminLogTypes.setmoney:
                    xtype = "acp-setmoney";
                    break;
            }

            string query = "";

            // Special Whisperlog
            query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    admin.Name, MySqlHelper.EscapeString(user), (int) type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            MySQLHandler.ExecuteAsync(query);
        }

        public static void LogAcpAdminAction(string admin, string user, adminLogTypes type, string reason)
        {
            string xtype = "undefined";

            // Getting Type
            switch (type)
            {
                case adminLogTypes.kick:
                    xtype = "acp-kick";
                    break;
                case adminLogTypes.whisper:
                    xtype = "acp-whisper";
                    break;
                case adminLogTypes.setmoney:
                    xtype = "acp-setmoney";
                    break;
            }

            string query = "";

            // Special Whisperlog
            query = string.Format(
                    "INSERT INTO `log_admin` (`admin`, `user`, `type`, `reason`) VALUES ('{0}', '{1}', '{2}', '{3}')",
                    admin, MySqlHelper.EscapeString(user), (int)type, MySqlHelper.EscapeString(xtype + ":: " + reason));
            MySQLHandler.ExecuteAsync(query);
        }
    }
}