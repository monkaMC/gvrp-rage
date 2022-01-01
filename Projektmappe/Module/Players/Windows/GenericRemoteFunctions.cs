using System;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Configurations;
using GVRP.Module.Forum;
using GVRP.Module.Items;
using GVRP.Module.LifeInvader.App;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Staatskasse;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Permission;

namespace GVRP.Module.Players.Windows
{
    class GenericRemoteFunctions : Script
    {
        [RemoteEvent]
        public void kick(Client p_Player)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                return;

            //DBLogging.LogAdminAction(p_Player, p_Player.Name, adminLogTypes.kick, "AFK System", 0, Configuration.Instance.DevMode);
            l_DbPlayer.SendNewNotification("Anti AFK");
            l_DbPlayer.Kick("AFK-Check nicht bestanden.");
        }

        [RemoteEvent]
        public void openAnimationMenu(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null)
                return;
            if (p_Player.IsInVehicle || !l_DbPlayer.CanInteract()) return;

            MenuManager.Instance.Build(PlayerMenu.AnimationMenuOv, l_DbPlayer).Show(l_DbPlayer);
        }

        //Event: LifeInvader - Purchase of Ad
        [RemoteEvent]
        public async void LifeInvaderPurchaseAd(Client player, string ad)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            if (Main.adLastSend.AddSeconds(15) > DateTime.Now)
            {
                dbPlayer.SendNewNotification(
                    
                    "Aktuell kann keine Werbung gesendet werden, bitte warte kurz!");
                return;
            }
            if (dbPlayer.IsHomeless())
            {
                dbPlayer.SendNewNotification("Ohne Wohnsitz können Sie keine Werbung schalten!");
                return;
            }
            int newsprice = 0;
            if (ad.Length < 10 || ad.Length > 96)
            {
                dbPlayer.SendNewNotification(
                    
                    "Werbungen muessen zwischen 10 und 96 Zeichen lang sein!");
                return;
            }

            newsprice = ad.Length * 5;

            if (!dbPlayer.TakeMoney(newsprice))
            {
                dbPlayer.SendNewNotification(
                     MSG.Money.NotEnoughMoney(newsprice));
                return;
            }

            //ToDo: AddToLifeInvaderAdListWithTimeStamp
            ad = ad.Replace("\"", "");
            Main.adList.Add(new LifeInvaderApp.AdsFound(dbPlayer.Id, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}", $"{ad}"));

            Main.adList.Sort(delegate (LifeInvaderApp.AdsFound x, LifeInvaderApp.AdsFound y)
            {
                return y.DateTime.CompareTo(x.DateTime);
            });

            Players.Instance.SendMessageToAuthorizedUsers("log",
                "AD gesendet von " + dbPlayer.GetName() + "(" + dbPlayer.ForumId + ")");
            dbPlayer.SendNewNotification(
                "Werbung abgesendet! Kosten: $5 / Buchstabe (insgesamt: $" +
                newsprice + ")");
            await Main.sendNotificationToPlayersWhoCanReceive("Es gibt neue Werbung in der Lifeinvader App!", "Lifeinvader");
            Main.adLastSend = DateTime.Now;
            var adlog = ad.Replace("$", ":");
            Logging.Logger.LiveinvaderLog(dbPlayer.Id, dbPlayer.Player.Name, adlog);

        }

        //Event: TeamManageApp - addPlayerConfirmed after confirmation
        [RemoteEvent]
        public void addPlayerConfirmed(Client player, string invitingPersonName, string fraktion)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var editDbPlayer = Players.Instance.FindPlayer(invitingPersonName).Player.GetPlayer();
            var teamRankPermission = editDbPlayer.TeamRankPermission;
            if (teamRankPermission.Manage < 1)
            {
                dbPlayer.SendNewNotification("Die Person ist nicht berechtigt dich einzuladen!", title:"Fraktion", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
     //       if (dbPlayer.IsHomeless())
     //       {
     //           dbPlayer.SendNewNotification("Ohne einen Wohnsitz kannst du keiner Fraktion beitreten!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
     //           editDbPlayer.SendNewNotification($"{ dbPlayer.GetName()} hat keinen Wohnsitz und kann daher nicht der Fraktion beitreten!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
     //           return;
     //       }
            if (dbPlayer.TeamId != (uint) TeamList.Zivilist)
            {
                dbPlayer.SendNewNotification("Du bist bereits in einer Fraktion!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                editDbPlayer.SendNewNotification($"{dbPlayer.GetName()} ist bereits in einer Fraktion!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!string.Equals(editDbPlayer.Team.Name.ToLower(), fraktion.ToLower()))
            {
                dbPlayer.SendNewNotification($"{editDbPlayer.GetName()} hat dich in die falsche Fraktion eingeladen! :(", title: "Fraktion", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }
            
            dbPlayer.SetTeam(editDbPlayer.TeamId);
            dbPlayer.UpdateApps();
            dbPlayer.Team.AddMember(dbPlayer);
            dbPlayer.SynchronizeForum();

            // Rank Permissions & Rank
            dbPlayer.SetTeamRankPermission(false, 0, false, "");
            dbPlayer.TeamRank = 0;
            dbPlayer.fgehalt[0] = 0;

            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} ist jetzt ein Mitglied - {fraktion}!");
        }
        
    }
}
