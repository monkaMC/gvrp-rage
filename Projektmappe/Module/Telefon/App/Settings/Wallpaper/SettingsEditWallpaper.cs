using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Telefon.App.Settings.Wallpaper;

namespace GVRP.Module.Telefon.App.Settings
{
    public class SettingsEditWallpaper : SimpleApp
    {
        public SettingsEditWallpaper() : base("SettingsEditWallpaperApp") { }

        [RemoteEvent]
        public void requestWallpaperList(Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            TriggerEvent(player, "responseWallpaperList", WallpaperModule.Instance.getJsonWallpapersForPlayer(dbPlayer));

        }

        [RemoteEvent]
        public void saveWallpaper(Client player, int wallpaperId)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            dbPlayer.wallpaper = WallpaperModule.Instance.Get((uint)wallpaperId);
            dbPlayer.SaveWallpaper();
        }
        
    }

}
