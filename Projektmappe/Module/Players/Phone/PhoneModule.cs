using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;

using GVRP.Module.Players.Db;

using GVRP.Module.Players.Phone.Apps;
using GVRP.Module.Players.Phone.Contacts;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Tasks;

namespace GVRP.Module.Players.Phone
{
    public class PhoneModule : Module<PhoneModule>
    {
        public static Dictionary<int, int> ActivePhoneCalls = new Dictionary<int, int>();

        protected override bool OnLoad()
        {
            ActivePhoneCalls = new Dictionary<int, int>();
            return base.OnLoad();
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.PhoneContacts = new PhoneContacts(dbPlayer.Id);
            dbPlayer.PhoneApps = new PhoneApps(dbPlayer);
        }
        
    }
}