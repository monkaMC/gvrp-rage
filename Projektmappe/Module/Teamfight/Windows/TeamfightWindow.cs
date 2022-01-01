using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Linq;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.Teamfight.Windows
{
    public class TeamfightWindow : Window<Func<DbPlayer, uint, string, bool, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "team")] private uint Team { get; }
            [JsonProperty(PropertyName = "teamName")] private string TeamName { get; }
            [JsonProperty(PropertyName = "attacker")] private bool Attacker { get; }

            public ShowEvent(DbPlayer dbPlayer, uint team, string teamName, bool attacker) : base(dbPlayer)
            {
                Team = team;
                TeamName = teamName;
                Attacker = attacker;
            }
        }

        public TeamfightWindow() : base("Teamfight")
        {
        }

        public override Func<DbPlayer, uint, string, bool, bool> Show()
        {
            return (player, team, teamName, attacker) => OnShow(new ShowEvent(player, team, teamName, attacker));
        }

        [RemoteEvent]
        public void requestTeamfight(Client player, uint id, int money)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;

                // Get enemy team
                Team enemy = TeamModule.Instance.GetById((int)id);

                // Own team check
                if (enemy.Id == dbPlayer.Team.Id)
                {
                    dbPlayer.SendNewNotification("Du kannst deinem eigenem Team keine Anfrage zu einem Fraktionskampf senden!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                // Money check
                if (money < 150000 || money > 3000000)
                {
                    dbPlayer.SendNewNotification("Die Summe des Schwarzgeldes darf nicht kleiner als 150000 und nicht größer als 1000000 sein!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                // Rank permission check
                if (dbPlayer.TeamId == 0 || dbPlayer.TeamRankPermission.Manage < 1)
                {
                    dbPlayer.SendNewNotification("Du besitzt nicht die notwendigen Rechte für dieses Vorhaben!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                // Assign Teamfight to team
                if (enemy.RequestedTeamfight != null)
                {
                    dbPlayer.SendNewNotification("Die Gegnerische Partei hat bereits eine Anfrage zu einem Fraktionskampf offen!");
                    return;
                }

                // Is in teamfight check
                if (dbPlayer.Team.IsInTeamfight())
                {
                    dbPlayer.SendNewNotification("Deine Fraktion befindet sich bereits in einem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                // Get own team
                var shelter = Teams.Shelter.TeamShelterModule.Instance.GetByTeam(dbPlayer.TeamId);

                // Schwarzgeld check in fraction inventory
                if (shelter.Container.GetItemAmount(640) < money)
                {
                    dbPlayer.SendNewNotification("Deine Fraktion hat nicht genügend Schwarzgeld im Fraktionslager gelagert!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (!enemy.IsInTeamfight())
                {
                    // Send notification to enemy team members with manage permission
                    if (enemy.Members.Where(c => c.Value.TeamRankPermission.Manage > 1).Count() > 0)
                    {
                        foreach (var findplayer in enemy.Members.Where(c => c.Value.TeamRankPermission.Manage > 1))
                        {
                            findplayer.Value.SendNewNotification("Es ist eine Anfrage zu einem Fraktionskampf eingegangen, bitte beantworte diese in deinem Fraktionslager!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Es sind keine gegnerischen Entscheidungsträger anwesend!", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }

                    enemy.RequestedTeamfight = new Teamfight((uint)TeamfightModule.Instance.GetAll().Count + 1, dbPlayer.TeamId, enemy.Id, 0, 0, money, money / 2, 0);

                    // Inform user that a request was sent
                    dbPlayer.SendNewNotification($"Die Anfrage wurde an die { enemy.Name } gesendet!", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
                else
                {
                    dbPlayer.SendNewNotification("Die gegnerische Fraktion befindet sich bereits in einem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }
            }));
        }
    }
}
