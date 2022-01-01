using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Ranks;

namespace GVRP.Module.ACP
{
    public sealed class ACPModule : Module<ACPModule>
    { 
        public override bool Load(bool reload = false)
        {
            return true;
        }


        enum ActionType
        {
            KICK=0,
            WHISPER=1,
            SETMONEY = 2,
        }

        public override async Task OnTenSecUpdateAsync()
        {
       
     

        }
    }
}
