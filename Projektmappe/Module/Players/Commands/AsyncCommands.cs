using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Commands;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using GVRP.Module.Robbery;
using static GVRP.Module.Chat.Chats;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Dealer;
using GVRP.Module.Clothes;

namespace GVRP.Module.Players.Commands
{
    public class AsyncCommands : Script
    {
        public static AsyncCommands Instance { get; } = new AsyncCommands();

        public async Task HandleGrab(Client player, string playerName)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (string.IsNullOrWhiteSpace(playerName))
            {
                dbPlayer.SendNewNotification( MSG.General.Usage("/grab", "[playerName]"));
                return;
            }

            var findPlayer = Players.Instance.FindPlayer(playerName);
            if (findPlayer == null || findPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20.0f)
            {
                dbPlayer.SendNewNotification(
                                                "Person nicht gefunden oder außerhalb der Reichweite!");
                return;
            }

            if (!dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                                                "Sie muessen sich in einem Fahrzeug befinden!");
                return;
            }

            if (findPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                                                "Die Person darf sich nicht in einem Auto befinden!");
                return;
            }

            if (findPlayer.IsCuffed || findPlayer.IsTied)
            {
                if (!VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(dbPlayer.Player.Vehicle.GetVehicle(), findPlayer))
                {
                    dbPlayer.SendNewNotification("Es sind keine freien Sitze mehr verfuegbar!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }
                if (findPlayer.IsCuffed)
                {
                    findPlayer.SetCuffed(true, true);
                }
                else
                {
                    findPlayer.SetTied(true, true);
                }
                dbPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " ins Fahrzeug gezogen.");
                findPlayer.SendNewNotification("Du wurdest ins Fahrzeug gezogen.");
            }
            else
            {
                dbPlayer.SendNewNotification("Du musst die Person erst fesseln.");
                return;
            }            
        }
        
        public async Task HandleTakeLic(Client p_Player, string p_Name)
        {
            
                var dbPlayer = p_Player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                {
                    dbPlayer.SendNewNotification( MSG.Error.NoPermissions());
                    return;
                }

                var findPlayer = Players.Instance.FindPlayer(p_Name);
                if (findPlayer == null || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
                {
                    dbPlayer.SendNewNotification(
                                                    "Spieler nicht gefunden oder außerhalb der Reichweite!");
                    return;
                }

                dbPlayer.SetData("takeLic", findPlayer);

                DialogMigrator.CreateMenu(p_Player, Dialogs.menu_takelic, "Lizenzen",
                    "Welche Lizenz wollen Sie " + findPlayer.GetName() + " entziehen...");
                if (findPlayer.Lic_Car[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Car, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Car, "");
                if (findPlayer.Lic_LKW[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Lkw, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Lkw, "");
                if (findPlayer.Lic_Bike[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Bike, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Bike, "");
                if (findPlayer.Lic_Boot[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Boot, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Boot, "");
                if (findPlayer.Lic_PlaneA[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneA, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneA, "");
                if (findPlayer.Lic_PlaneB[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneB, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.PlaneB, "");
                if (findPlayer.Lic_Biz[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Biz, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Biz, "");
                if (findPlayer.Lic_Gun[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Gun, "");
                else DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Gun, "");
                if (findPlayer.Lic_Transfer[0] > 0)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Transfer, "");
                else
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, Content.License.Transfer, "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_takelic, "Menu schließen", "");
                DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_takelic);
            
        }

        public async Task HandleGiveLic(Client p_Player, string p_Name)
        {
            
                var dbPlayer = p_Player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL || !dbPlayer.IsInDuty())
                {
                    dbPlayer.SendNewNotification( MSG.Error.NoPermissions());
                    return;
                }

                var findPlayer = Players.Instance.FindPlayer(p_Name);

                if (findPlayer == null || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
                {
                    dbPlayer.SendNewNotification(
                        
                        "Spieler nicht gefunden oder außerhalb der Reichweite!");
                    return;
                }

                if (findPlayer.IsHomeless())
                {
                    dbPlayer.SendNewNotification("Ohne Wohnsitz kann diese Person keine Lizenz erhalten!");
                    return;
                }

                if (dbPlayer.Player.Position.DistanceTo(new Vector3(-810.6085, -1347.864, 5.166561)) >= 20.0f)
                {
                    dbPlayer.SendNewNotification("Du musst an dem Fahrschulgebaeude sein um Scheine auszustellen!");
                    return;
                }

                dbPlayer.SetData("giveLic", findPlayer.Player);

                DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_givelicenses, "Lizenzen",
                    "Vergebe Lizenzen an " + findPlayer.GetName());
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.Car + " " + Price.License.Car + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.Lkw + " " + Price.License.Lkw + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.Bike + " " + Price.License.Bike + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.Boot + " " + Price.License.Boot + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.PlaneA + " " + Price.License.PlaneA + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.PlaneB + " " + Price.License.PlaneB + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses,
                    Content.License.Transfer + " " + Price.License.Transfer + "$", "");
                DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_givelicenses, "Menu schließen", "");
                DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_givelicenses);
            
        }

        public async Task HandleGiveMarryLic(Client p_Player, string p_Name)
        {

            var dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != (int)teams.TEAM_GOV || !dbPlayer.IsInDuty())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            if (dbPlayer.TeamRank < 9)
            {
                dbPlayer.SendNewNotification("Sie benötigen mind. Rang 9 um die Standesamt-Lizenz zu vergeben.", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            var findPlayer = Players.Instance.FindPlayer(p_Name);

            if (findPlayer == null || !findPlayer.IsValid() || findPlayer.Player.Position.DistanceTo(p_Player.Position) > 5.0f)
            {
                dbPlayer.SendNewNotification(
                    "Spieler nicht gefunden oder außerhalb der Reichweite!");
                return;
            }
            if (findPlayer.marryLic == 1)
            {
                dbPlayer.SendNewNotification(findPlayer.GetName() + " hat bereits die " + Content.License.marryLic);
            }
            else
            {                           
                MySQLHandler.ExecuteAsync($"UPDATE player SET marrylic = '1' WHERE id = '{findPlayer.Id}'");

                findPlayer.marryLic = 1;

                findPlayer.SendNewNotification("Sie haben die "+Content.License.marryLic+" erhalten!");
                dbPlayer.SendNewNotification("Sie haben "+findPlayer.GetName()+" die "+Content.License.marryLic+" gegeben.");

            }

        }

        public void HandleSupport(string p_Name, int p_ForumID, string p_Message)
        {
                Players.Instance.SendMessageToAuthorizedUsers("support", $"[{p_Name}({p_ForumID})]: {p_Message}", time:20000);
        }

        public async Task SendGovMessage(DbPlayer player, string mesage)
        {
            var id = player.Team.Id;
            var from = "LSPD Nachricht";

            if (id == (uint)teams.TEAM_GOV)
                from = "Regierungsnachricht";
            else if (id == (uint)teams.TEAM_FIB)
                from = "FIB Nachricht";
            else if (id == (uint)teams.TEAM_COUNTYPD)
                from = "Sheriff Department";
            else if (id == (uint)teams.TEAM_ARMY)
                from = "Army Nachricht";
            else if (id == (uint)teams.TEAM_DPOS)
                from = "DPOS Nachricht";
            else if (id == (uint)teams.TEAM_NEWS)
                from = "WEAZLE NEWS";

            await Chats.SendGlobalMessage($"{from}: {mesage}", COLOR.LIGHTBLUE, ICON.GOV);

            Players.Instance.SendMessageToAuthorizedUsers("GOV-Message", "von " + player.Player.Name);
            
        }

        public async Task SetHandMoney(Client player, string commandParams)
        {
            await Task.Run(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (!int.TryParse(command[1], out int amount)) return;

                var findPlayer = Players.Instance.FindPlayer(command[0]);

                var name = findPlayer != null ? findPlayer.GetName() : command[0];

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + player.Name + " hat das Geld von " + name + " um $" + amount + " veraendert.");

                //DBLogging.LogAdminAction(player, name, adminLogTypes.log, $"{amount}$ GivemoneyHand");

                if (findPlayer == null) return;

                if (amount > 0)
                {
                    if (findPlayer.GiveMoney(amount))
                    {

                    };

                    iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                            " auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator " + player.Name + " hat ihnen $" +
                                               amount + " auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (iPlayer.RankId < (int)adminlevel.Projektleitung)
                        Players.Instance.SendMessageToAuthorizedUsers("log",
                            "Admin " + player.Name + " hat " + findPlayer.GetName() + " $" + amount + " auf die Hand gegeben!");

                    //DBLogging.LogAdminAction(player, findPlayer.GetName(), adminLogTypes.log, $"{amount}$ GivemoneyHand");
                    return;
                }

                var success = findPlayer.TakeAnyMoney(Math.Abs(amount));
                string kontotyp = "";

                switch (success)
                {
                    case -1:
                        iPlayer.SendNewNotification($"Beim Entfernen des Geldes vom Spieler {findPlayer.GetName()} in Hoehe von {amount} ist ein Fehler aufgetreten. Pruefen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    case 0:
                        kontotyp = "Geldbörse";
                        break;
                    case 1:
                        kontotyp = "Bank";
                        break;
                    default:
                        iPlayer.SendNewNotification($"Beim Entfernen des Geldes vom Spieler {findPlayer.GetName()} in Hoehe von {amount} ist ein Fehler aufgetreten. Pruefen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                }

                iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount +
                                        " von " + kontotyp + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification("Administrator" + player.Name + " hat ihnen $" +
                                           amount + " aus der " + kontotyp + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + player.Name + " hat " + findPlayer.GetName() + " $" + amount + " aus der Geldbörse entfernt!");

                //DBLogging.LogAdminAction(player, findPlayer.GetName(), adminLogTypes.log, $"-{amount}$ GivemoneyHand");
            });
        }

        public async Task SetBlackMoney(Client player, string commandParams)
        {
            await Task.Run(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (!int.TryParse(command[1], out int amount)) return;

                var findPlayer = Players.Instance.FindPlayer(command[0]);

                var name = findPlayer != null ? findPlayer.GetName() : command[0];

                Players.Instance.SendMessageToAuthorizedUsers("log",
                    "Admin " + player.Name + " hat das Schwarzgeld von " + name + " um $" + amount + " veraendert.");

                //DBLogging.LogAdminAction(player, name, adminLogTypes.log, $"{amount}$ GiveBlackMoneyHand");

                if (findPlayer == null) return;

                if (amount > 0)
                {
                    if (findPlayer.GiveBlackMoney(amount))
                    {

                    };

                    iPlayer.SendNewNotification("Sie haben " + findPlayer.GetName() + " $" + amount + " Schwarzgeld auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator " + player.Name + " hat ihnen $" + amount + " Schwarzgeld auf die Hand gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (iPlayer.RankId < (int)adminlevel.Projektleitung)
                        Players.Instance.SendMessageToAuthorizedUsers("log", "Admin " + player.Name + " hat " + findPlayer.GetName() + " $" + amount + " Schwarzgeld auf die Hand gegeben!");

                    ////DBLogging.LogAdminAction(player, findPlayer.GetName(), adminLogTypes.log, $"{amount}$ GiveBlackMoneyHand");
                    return;
                }
                
            });
        }

        public async Task HandleFindüberfall(DbPlayer dbPlayer)
        {
            
                if (!dbPlayer.IsValid())
                    return;

                if (RobberyModule.Instance.GetActiveRobs().Count <= 0)
                {
                    dbPlayer.SendNewNotification( "Kein Raubueberfall gefunden.");
                    return;
                }

                DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_findrob, "Aktuelle Raube", "");
                foreach (Rob rob in RobberyModule.Instance.GetActiveRobs(true))
                {
                    if (rob.Id == RobberyModule.Juwelier)
                        DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_findrob, "Juwelier", "");
                    else
                        DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_findrob, "Shopraub", "");
                }

                DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_findrob);
        }

        public async Task HandleFindBlackW(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return;

            // Get hint location
            ExchangeLocation location = ExchangeLocationModule.Instance.getAlertedExchangeLocation();

            // Check if location is alerted
            if (location == null || !location.Alerted)
            {
                dbPlayer.SendNewNotification("Es ist kein Tipp zu einer Schwarzgeldwäsche eingegangen!");
                return;
            }

            // Set gps and inform user
            dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", location.Position.X, location.Position.Y);
            dbPlayer.SendNewNotification("Sie haben den Ort der Meldung im GPS markiert!");

            // Reset data
            location.Alerted = false;
        }

        public async Task HandleFindDealer(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsValid()) return;

            DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_dealerhint, "Aktuelle Informationen", "");
            DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_dealerhint, "Schließen", "");
            int index = 1;
            foreach (Dealer.Dealer dealer in DealerModule.Instance.GetAll().Values)
            {
                if (dealer.Alert)
                {
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_dealerhint, "Dealertipp " + index, "");
                    index++;
                }
            }
            DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_dealerhint);
        }

        public async Task HandleJail(DbPlayer dbPlayer)
        {
            
                if (!dbPlayer.IsValid())
                    return;

                Dictionary<string, int> l_JailInhabits = new Dictionary<string, int>();

                foreach (var l_Player in Players.Instance.GetValidPlayers())
                {
                    if (l_Player.jailtime[0] > 0)
                        l_JailInhabits.TryAdd(l_Player.GetName(), l_Player.jailtime[0]);
                }

                DialogMigrator.CreateMenu(dbPlayer.Player, Dialogs.menu_jailinhabits, "Insassen (Name - Verbleib. Monate)", "");
                foreach (var l_Pair in l_JailInhabits)
                    DialogMigrator.AddMenuItem(dbPlayer.Player, Dialogs.menu_jailinhabits, $"{l_Pair.Key} - {l_Pair.Value}", "");

                DialogMigrator.OpenUserMenu(dbPlayer, Dialogs.menu_jailinhabits);
                dbPlayer.SendNewNotification($"Es befinden sich insgesamt {l_JailInhabits.Count} Insassen im SG!");
            
        }
        
        public async Task HandleNews(string p_NewsMessage)
        {
            await Task.Run(() =>
            {
                foreach (var l_Player in Players.Instance.GetValidPlayers())
                {
                    if (!Main.newsActivated(l_Player.Player))
                        continue;

                    l_Player.SendNewNotification(Chats.MsgNews + p_NewsMessage);
                }
            });
        }
    }
}
