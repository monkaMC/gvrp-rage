using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;

namespace GVRP.Module.PlayerLoginSecure
{
    public class PlayerLoginSecureModule : Module<PlayerLoginSecureModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            try
            {
              
            }
            catch(Exception e)
            {
                Logging.Logger.SaveToDbLog(e.ToString());
            }
        }
    }
}
