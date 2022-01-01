using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Telefon.App.Settings.Wallpaper
{
    public class WallpaperModule : SqlModule<WallpaperModule, Wallpaper, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `phone_wallpaper`;";
        }

        protected override void OnItemLoaded(Wallpaper wallpaper)
        {
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.wallpaper = Instance.Get(reader.GetUInt32("wallpaperId"));
        }

        public String getJsonWallpapersForPlayer(DbPlayer dbPlayer)
        {
            bool staffMember = dbPlayer.Rank.Id == 0 ? false : true;

            List<Wallpaper> liste = new List<Wallpaper>();

            foreach (var item in this.GetAll().Values)
            {
                if ((item.isTeamOnly && staffMember) || !item.isTeamOnly) liste.Add(item);
            }
            return JsonConvert.SerializeObject(liste);
        }



    }
}
