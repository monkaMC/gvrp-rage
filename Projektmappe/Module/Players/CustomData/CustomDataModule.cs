using System;
using System.Collections.Generic;
using System.Text;
using MySql.Data.MySqlClient;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public sealed class CustomDataModule : Module<CustomDataModule>
    {
        protected override bool OnLoad()
        {
            return base.OnLoad();
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.CustomData = dbPlayer.LoadCustomData();
        }
    }
}
