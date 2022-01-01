using GTANetworkAPI;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teamfight;
using GVRP.Module.Teamfight.Windows;

namespace GVRP.Module.Teams.Shelter
{
    public class ShelterFightMenuBuilder : MenuBuilder
    {
        public ShelterFightMenuBuilder() : base(PlayerMenu.ShelterFightMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Fraktionskampf", "Fraktionskampf Verwaltung");

            menu.Add($"Schließen");
            menu.Add($"Kampfantrag stellen");
            menu.Add($"Kampfantrag beantworten");
            menu.Add($"Equipment erhalten");
            menu.Add($"Schutzweste kaufen (8000$)");
            menu.Add($"Verbandskästen kaufen (500$)");
            menu.Add($"Punktestand abfragen");
            menu.Add($"Aufgeben");

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                // Get shelter pos
                var teamShelter = TeamShelterModule.Instance.GetByTeam(iPlayer.TeamId);

                if (iPlayer.Player.Position.DistanceTo(teamShelter.MenuPosition) > 5.0f)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }

                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                // Start fight
                else if (index == 1)
                {
                    iPlayer.SendNewNotification("Aktuell Deaktiviert");
                    return false;

                    if (iPlayer.TeamRankPermission.Manage < 1) return false;

                    MenuManager.DismissCurrent(iPlayer);
                    ComponentManager.Get<TeamfightWindow>().Show()(iPlayer, iPlayer.TeamId, iPlayer.Team.Name, true);
                    return false;
                }
                // Answer fight
                else if (index == 2)
                {
                    if (iPlayer.TeamRankPermission.Manage < 1) return false;

                    Teamfight.Teamfight requestedFight = iPlayer.Team.RequestedTeamfight;

                    if (requestedFight == null)
                    {
                        iPlayer.SendNewNotification("Deine Fraktion hat aktuell keine Anfrage zu einem Fraktionskampf offen!");
                        return false;
                    }

                    ComponentManager.Get<TeamfightRequestWindow>().Show()(iPlayer, iPlayer.TeamId, requestedFight.Team_a_money, requestedFight.Team_a, TeamModule.Instance.Get(requestedFight.Team_a).Name, true);

                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                // equipment
                else if (index == 3)
                {
                    if (iPlayer.Team.IsInTeamfight())
                    {
                        iPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500, true, true);
                        iPlayer.GiveWeapon(WeaponHash.AssaultRifle, 600, true, true);
                        iPlayer.GiveWeapon(WeaponHash.CompactRifle, 600, true, true);
                        iPlayer.GiveWeapon(WeaponHash.Gusenberg, 600, true, true);
                        iPlayer.SetArmor(100);
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // schutzweste
                else if (index == 4)
                {
                    if (iPlayer.Team.IsInTeamfight())
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(655, 1))
                        {
                            iPlayer.SendNewNotification($"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                            return false;
                        }

                        if (!iPlayer.TakeBankMoney(8000))
                        {
                            iPlayer.SendNewNotification($"Dieses Item kostet 8000$ (Bank)!");
                            return false;
                        }

                        iPlayer.Container.AddItem(655, 1);
                        iPlayer.SendNewNotification($"Sie haben sich eine Schutzweste gekauft!");
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Verbandskasten
                else if (index == 5)
                {
                    if (iPlayer.Team.IsInTeamfight())
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(654, 1))
                        {
                            iPlayer.SendNewNotification($"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                            return false;
                        }

                        if (!iPlayer.TakeBankMoney(500))
                        {
                            iPlayer.SendNewNotification($"Dieses Item kostet 500$ (Bank)!");
                            return false;
                        }

                        iPlayer.Container.AddItem(654, 1);
                        iPlayer.SendNewNotification($"Sie haben sich ein Verbandskasten gekauft!");
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Punktestand abfragen
                else if (index == 6)
                {
                    if (iPlayer.Team.IsInTeamfight())
                    {
                        var teamFight = TeamfightModule.Instance.getOwnTeamFight(iPlayer.TeamId);
                        iPlayer.SendNewNotification($"{TeamModule.Instance.GetById((int)teamFight.Team_a).Name} {teamFight.Kills_team_a} : {TeamModule.Instance.GetById((int)teamFight.Team_b).Name} {teamFight.Kills_team_b}", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
                // Aufgeben
                else
                {
                    if (iPlayer.TeamRankPermission.Manage < 1) return false;

                    if (iPlayer.Team.IsInTeamfight())
                    {
                        var teamFight = TeamfightModule.Instance.getOwnTeamFight(iPlayer.TeamId);
                        TeamfightFunctions.surrenderTeamfight(teamFight, iPlayer.Team);
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Deine Fraktion befindet sich aktuell in keinem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }

                    return false;
                }
            }
        }
    }
}
