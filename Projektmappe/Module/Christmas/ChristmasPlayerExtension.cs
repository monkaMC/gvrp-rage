using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Christmas
{
    public static class ChristmasPlayerExtension
    {
        public static void SaveChristmasState(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `xmasLast` = '" + dbPlayer.xmasLast.Date.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id = '" + dbPlayer.Id + "';");
        }
    }
}
