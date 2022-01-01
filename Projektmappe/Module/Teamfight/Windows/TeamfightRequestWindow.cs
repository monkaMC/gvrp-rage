using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.Teamfight.Windows
{
    public class TeamfightRequestWindow : Window<Func<DbPlayer, uint, int, uint, string, bool, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "team")] private uint Team { get; }
            [JsonProperty(PropertyName = "money")] private int Money { get; }
            [JsonProperty(PropertyName = "attackerTeam")] private uint AttackerTeam { get; }
            [JsonProperty(PropertyName = "teamName")] private string TeamName { get; }
            [JsonProperty(PropertyName = "attacker")] private bool Attacker { get; }

            public ShowEvent(DbPlayer dbPlayer, uint team, int money, uint attackerTeam, string teamName, bool attacker) : base(dbPlayer)
            {
                Team = team;
                Money = money;
                AttackerTeam = attackerTeam;
                TeamName = teamName;
                Attacker = Attacker;
            }
        }

        public TeamfightRequestWindow() : base("Teamfight")
        {
        }

        public override Func<DbPlayer, uint, int, uint, string, bool, bool> Show()
        {
            return (player, team, money, attackerTeam, teamName, attacker) => OnShow(new ShowEvent(player, team, money, attackerTeam, teamName, attacker));
        }

        [RemoteEvent]
        public void acceptTeamfight(Client player, uint attackerTeam, int money)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            // Check if fraction is already in a war
            if (dbPlayer.Team.IsInTeamfight())
            {
                dbPlayer.SendNewNotification("Deine Fraktion befindet sich bereits in einem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (TeamModule.Instance.Get(attackerTeam).IsInTeamfight())
            {
                dbPlayer.SendNewNotification("Die gegnerische Fraktion befindet sich bereits in einem Fraktionskampf!", notificationType: PlayerNotification.NotificationType.ADMIN);
                dbPlayer.Team.RequestedTeamfight = null;
                return;
            }

            // Get team shelter from attacker and defender fraction
            var shelterAttacker = Teams.Shelter.TeamShelterModule.Instance.GetByTeam(attackerTeam);
            var shelterDefender = Teams.Shelter.TeamShelterModule.Instance.GetByTeam(dbPlayer.TeamId);

            int defenderMoney = money / 2;

            // Check if the attacked fractions got enough money stored inside the fraction inventory
            if (shelterDefender.Container.GetItemAmount(640) < defenderMoney)
            {
                dbPlayer.SendNewNotification("Deine Fraktion hat nicht genügend Schwarzgeld im Fraktionslager gelagert!", notificationType: PlayerNotification.NotificationType.ADMIN);
                TeamModule.Instance.Get(attackerTeam).SendNotification("Die Gegnerische Fraktion hat nicht genügend Schwarzgeld gelagert!");
                return;
            }

            // Take money
            shelterAttacker.Container.RemoveItem(640, money);
            shelterDefender.Container.RemoveItem(640, defenderMoney);

            // set active and add to db and dictionary
            Teamfight fight = dbPlayer.Team.RequestedTeamfight;
            fight.Active = 1;

            TeamfightModule.Instance.createTeamFight(fight);

            // Store teamweapons inside container and add teamfight param to player
            TeamfightFunctions.StoreWeaponsInsideTeamfightContainer(TeamModule.Instance.GetById((int)attackerTeam));
            TeamfightFunctions.StoreWeaponsInsideTeamfightContainer(TeamModule.Instance.GetById((int)dbPlayer.TeamId));

            // Notify teams
            TeamModule.Instance.Get(attackerTeam).SendNotification("Achtung der Fraktionskampf hat begonnen!");
            TeamModule.Instance.Get(dbPlayer.TeamId).SendNotification("Achtung der Fraktionskampf hat begonnen!");

            // Reset data
            dbPlayer.Team.RequestedTeamfight = null;
        }

        [RemoteEvent]
        public void declineTeamfight(Client player, uint attackerTeam)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            // Reset requested teamfight
            dbPlayer.Team.RequestedTeamfight = null;

            // Notify teams
            TeamModule.Instance.Get(attackerTeam).SendNotification("Die gegnerische Partei hat den Fraktionskampf abgelehnt!");
            TeamModule.Instance.Get(dbPlayer.TeamId).SendNotification("Dein Team hat den Fraktionskampf abgelehnt!");
        }
    }
}
