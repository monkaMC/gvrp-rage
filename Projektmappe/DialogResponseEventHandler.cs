using System;
using System.IO;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Chat;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;

using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Players.Events;

namespace GVRP
{
    public class DialogResponseEventHandler : Script
    {
        [RemoteEvent]
        public void DialogResponse(Client player, params object[] args)
        {
            if (args.Length == 0) return;
            var iPlayer = player.GetPlayer();
            if (iPlayer == null) return;
            // General Close Dialog
            if (Convert.ToString(args[0]) == "false")
            {
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                //player.Freeze(false);
                iPlayer.watchDialog = 0;
                player.TriggerEvent("deleteDialog");
                return;
            }

            var dialogid = iPlayer.watchDialog;
            var input = args[0];
            var input2 = "";
            if (args.Length > 1 && Convert.ToString(args[1]) != "")
            {
                input2 = Convert.ToString(args[1]);
            }

            if (dialogid == Dialogs.menu_login)
            {
                string response = Convert.ToString(input);

                if (response != "false")
                {
                    if (string.IsNullOrEmpty(response) || response == null)
                    {
                        iPlayer.SendNewNotification("Geben Sie ein Passwort ein!", title:"SERVER", notificationType:PlayerNotification.NotificationType.SERVER);
                        return;
                    }

                    if (iPlayer.AccountStatus != AccountStatus.Registered)
                    {
                        iPlayer.SendNewNotification("Sie sind bereits eingeloggt!", title: "SERVER", notificationType: PlayerNotification.NotificationType.SERVER);
                        iPlayer.CloseUserDialog(Dialogs.menu_login);
                        return;
                    }

                    var pass = HashThis.GetSha256Hash(iPlayer.Salt + response);
                    var pass2 = iPlayer.Password;
                    if (pass == pass2)
                    {
                        iPlayer.AccountStatus = AccountStatus.LoggedIn;

                        iPlayer.Player.ResetData("loginStatusCheck");

                        iPlayer.CloseUserDialog(Dialogs.menu_login);

                        player.SetSharedData("AC_Status", true);

                        iPlayer.Firstspawn = true;
                        PlayerSpawn.OnPlayerSpawn(player);

                        // send phone data
                        Phone.SetPlayerPhoneData(iPlayer);
                        return;
                    }
                    else
                    {
                        iPlayer.PassAttempts += 1;

                        if (iPlayer.PassAttempts >= 3)
                        {
                            iPlayer.Player.SendNotification("Sie haben ein falsches Passwort 3x eingegeben, Sicherheitskick.");
                            player.Kick("Falsches Passwort (3x)");
                            return;
                        }

                        string message = string.Format("Sie haben ein falsches Passwort eingegeben. Warnung [{0}/3]",
                            iPlayer.PassAttempts);
                        iPlayer.SendNewNotification(message, title: "SERVER", notificationType: PlayerNotification.NotificationType.SERVER);
                        return;
                    }
                }
            }
            else if (dialogid == Dialogs.menu_weapondealer_input)
            {
                if (!iPlayer.HasData("sWeaponBuild")) return;
                if (!int.TryParse(input.ToString(), out var amount)) return;
                if (amount > 0 && amount < 9999)
                {
                    uint itemid = iPlayer.GetData("sWeaponBuild");
                    //ItemData item = itemid);

                    int price = 500;//item.JobMats * 500;
                    if (!iPlayer.TakeMoney(price))
                    {
                        iPlayer.SendNewNotification(
                             MSG.Money.NotEnoughMoney(price));
                        return;
                    }
                    
                    iPlayer.SendNewNotification(
                  "Sie haben sich " + amount + " ");
                    iPlayer.PlayAnimation(AnimationScenarioType.Animation,
                        "amb@prop_human_movie_studio_light@base", "base");
                    iPlayer.CloseUserDialog(Dialogs.menu_weapondealer_input);
                }

                return;
            }
            else if (dialogid == Dialogs.menu_ad_input)
            {
                //Moved to Window
            }
            else if (dialogid == Dialogs.menu_givemoney_input)
            {
                if (!iPlayer.HasData("sInteraction")) return;
                if (!int.TryParse(input.ToString(), out var amount)) return;
                DbPlayer desPlayer = iPlayer.GetData("sInteraction");
                if (!desPlayer.IsValid()) return;
                if (desPlayer.Player.Position.DistanceTo(iPlayer.Player.Position) > 5.0f) return;
                iPlayer.GiveMoneyToPlayer(desPlayer, amount);
                iPlayer.ResetData("sInteraction");
                iPlayer.CloseUserDialog(Dialogs.menu_givemoney_input);
                return;
            }
            else if (dialogid == Dialogs.menu_shop_input)
            {
                if (!int.TryParse(input.ToString(), out var amount)) return;
                if (!iPlayer.TryData("sBuyItem", out uint itemid)) return;
                if (amount <= 0) return;
                iPlayer.CloseUserDialog(Dialogs.menu_shop_input);
                return;
            }
        }
        
        public static void SendChatMessageToAll(string command, bool sponly = false)
        {
            var players = Players.Instance.GetValidPlayers();
            foreach (var player in players)
            {
                if (!player.IsValid()) continue;
                player.SendNewNotification(command);
            }
        }
    }
}