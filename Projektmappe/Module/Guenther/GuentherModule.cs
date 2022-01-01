using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players.Db;
using GVRP.Module.Players;
using GVRP.Module.Items;
using GVRP.Module.Menu;

namespace GVRP.Module.Guenther
{
    public sealed class GuentherModule : Module<GuentherModule>
    {
        NpcSpawner.Npc guenther;
        NpcSpawner.Npc security;
        NpcSpawner.Npc security2;
        public static int Id { get; set; }
        public static uint GuentherClubCardId { get; set; }
        public static int Random { get; set; }
        public static Vector3 Outside { get; set; }
        public static Vector3 Inside { get; set; }
        public static Vector3 Garage { get; set; }
        public static uint Dimension { get; set; }
        private static int counter;
        public static List<DbPlayer> DbPlayers { get; set; }

        //protected override bool OnLoad()
        //{
        //    counter = 0;
        //    Id = Utils.RandomNumber(1, 5);
        //    Random = Utils.RandomNumber(0, 7);
        //    DbPlayers = new List<DbPlayer>();
        //    GuentherClubCardId = 716;
        //    Dimension = 123;
        //    Outside = new Vector3(-1581.25f, -558.255f, 34.9532f);
        //    Inside = new Vector3(-1586.33f, -561.468f, 86.5004f);
        //    Vector3 position = new Vector3(0, 0, -100);
        //    float heading = 0.0f;
        //    PedHash skin = PedHash.Bankman01;
        //    switch (Id)
        //    {
        //        case 1:
        //            position = new Vector3(-866.368f, -221.926f, 39.6794f); //Versicherung
        //            heading = 112.224f;
        //            skin = PedHash.Bankman01;
        //            break;
        //        case 2:
        //            position = new Vector3(-1018.57f, -1363.81f, 5.55319f); //Fisch
        //            heading = 291.303f;
        //            skin = PedHash.JimmyBoston;
        //            break;
        //        case 3:
        //            position = new Vector3(121.232f, -3082.02f, 6.00888f); //Hafen
        //            heading = 262.39f;
        //            skin = PedHash.Floyd;
        //            break;
        //        case 4:
        //            position = new Vector3(-1345.63f, 58.5723f, 55.2456f); //Golf
        //            heading = 276.058f;
        //            skin = PedHash.Golfer01AMM;
        //            break;
        //        default:
        //            position = new Vector3(-354.491f, -54.2736f, 49.0463f); //Fleeca-Bank
        //            heading = 337.258f;
        //            skin = PedHash.Bankman;
        //            break;
        //    }
        //    MenuManager.Instance.AddBuilder(new Menu.Menus.Guenther.GuentherAusgangMenuBuilder());
        //    guenther = new NpcSpawner.Npc(skin, position, heading, 0);
        //    security = new NpcSpawner.Npc(PedHash.JewelSec01, new Vector3(-1582.61f, -559.247f, 34.9539f), 36.452f, 0);
        //    security2 = new NpcSpawner.Npc(PedHash.Highsec01SMM, new Vector3(-1580f, -557.4f, 34.9527f), 35.4735f, 0);
        //    return base.OnLoad();
        //}
        //public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        //{
        //    if (dbPlayer == null || !dbPlayer.IsValid()) return false;
        //    if (key != Key.E)
        //        return false;
        //    if (dbPlayer.Player.Position.DistanceTo(guenther.Position) < 2.0f)
        //    {
        //        if(dbPlayer.Container.GetItemAmount(GuentherClubCardId) < 1)
        //        {
        //            dbPlayer.SendNewNotification("Lass mich in Ruhe! Ich kenn dich nicht!");
        //            return false;
        //        }
        //        if (dbPlayer.Id % 7 != Random)
        //        {
        //            dbPlayer.SendNewNotification("Ich habe dir momentan nichts zu sagen. Komm später wieder.");
        //            return false;
        //        }
        //        int pass = Utils.GeneratePassword(dbPlayer);
        //        dbPlayer.SendNewNotification("Dein persönlicher Zugangscode lautet: " + pass);
        //        MySQLHandler.ExecuteAsync($"INSERT INTO `log_guentherclub` (`player_id`, `info`) VALUES ('{dbPlayer.Id}', 'Passwort gefunden: {dbPlayer.Player.Name} (Id: {dbPlayer.Id}), Passwort: {pass}');");
        //        dbPlayer.SendNewNotification("Der Club befindet sich in einem der Wolkenkratzer in Los Santos. Benutze einfach die Clubkarte am Eingang.");
        //        return true;
        //    }
        //    else if(dbPlayer.Player.Position.DistanceTo(Inside) < 3.0f)
        //    {
        //        MenuManager.Instance.Build(PlayerMenu.GuentherAusgangMenu, dbPlayer).Show(dbPlayer);
        //        return true;
        //    }
        //    return false;
        //}
        public void FahrstuhlTeleport(DbPlayer dbPlayer, Vector3 destination, float heading)
        {
            //if (dbPlayer == null || !dbPlayer.IsValid()) return;
            //dbPlayer.Player.TriggerEvent("freezePlayer", true);
            //dbPlayer.Player.TriggerEvent("unloadguenther");
            //dbPlayer.Player.SetPosition(destination);
            //dbPlayer.Player.SetRotation(heading);
            //dbPlayer.Player.TriggerEvent("freezePlayer", false);
            //if (DbPlayers.Contains(dbPlayer))
            //    DbPlayers.Remove(dbPlayer);
        }
        //public override void OnFiveMinuteUpdate()
        //{
        //    counter++;
        //    if(counter >= 14)
        //    {
        //        Random = Utils.RandomNumber(0, 7);
        //        counter = 0;
        //    }
        //}
    }
}
