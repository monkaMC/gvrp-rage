using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Gangwar;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Teamfight
{
    public class Teamfight : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint Team_a { get; set; }
        public uint Team_b { get; set; }
        public int Kills_team_a { get; set; }
        public int Kills_team_b { get; set; }
        public int Team_a_money { get; set; }
        public int Team_b_money { get; set; }
        public DateTime Timestamp { get; set; }
        public uint Active { get; set; }

        public Teamfight(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Team_a = reader.GetUInt32("team_a");
            Team_b = reader.GetUInt32("team_b");
            Kills_team_a = reader.GetInt32("kills_team_a");
            Kills_team_b = reader.GetInt32("kills_team_b");
            Team_a_money = reader.GetInt32("team_a_money");
            Team_b_money = reader.GetInt32("team_b_money");
            Timestamp = DateTime.Now;
            Active = reader.GetUInt32("active");
        }

        public Teamfight(uint id, uint team_a, uint team_b, int kills_team_a, int kills_team_b, int team_a_money, int team_b_money, uint active)
        {
            Id = id;
            Team_a = team_a;
            Team_b = team_b;
            Kills_team_a = kills_team_a;
            Kills_team_b = kills_team_b;
            Team_a_money = team_a_money;
            Team_b_money = team_b_money;
            Timestamp = DateTime.Now;
            Active = active;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public static class TeamfightFunctions
    {
        // Finish teamfight
        public static async Task finishTeamFight(this Teamfight fight)
        {
            // Update fight data
            fight.Active = 0;
            TeamfightModule.Instance.Update(fight.Id, fight, "teamfight", $"id={fight.Id}", "kills_team_a", fight.Kills_team_a, "kills_team_b", fight.Kills_team_b, "active", "0");

            // Get teams
            Team team_a = TeamModule.Instance.Get(fight.Team_a);
            Team team_b = TeamModule.Instance.Get(fight.Team_b);

            // Calculate the winner and loser reward
            int rewardWinner = (int)Math.Round(TeamfightModule.Instance.ConversionFactorWinner * (float)(fight.Team_a_money + fight.Team_b_money), 0);
            int rewardLoser = (int)Math.Round(TeamfightModule.Instance.ConversionFactorLoser * (float)(fight.Team_a_money + fight.Team_b_money), 0);

            if (fight.Kills_team_a == TeamfightModule.Instance.TeamfightEndPoints)
            {
                // Add money to the bank
                TeamShelterModule.Instance.Get(fight.Team_a).GiveMoney(rewardWinner);
                TeamShelterModule.Instance.Get(fight.Team_b).GiveMoney(rewardLoser);

                // Add reward for team a
                //GenerateRandomReward(team_a);

                // Notify teams
                team_a.SendNotification($"Dein Team hat den Fraktionskampf gewonnen. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
                team_b.SendNotification($"Dein Team hat den Fraktionskampf verloren. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
            }
            else
            {
                // Add money to the bank
                TeamShelterModule.Instance.Get(fight.Team_a).GiveMoney(rewardLoser);
                TeamShelterModule.Instance.Get(fight.Team_b).GiveMoney(rewardWinner);

                // Add reward for team b
                //GenerateRandomReward(team_b);

                // Notify teams
                team_b.SendNotification($"Dein Team hat den Fraktionskampf gewonnen. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
                team_a.SendNotification($"Dein Team hat den Fraktionskampf verloren. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
            }

            // remove weapons
            RemoveWeaponsForTeam(team_a);
            RemoveWeaponsForTeam(team_b);
        }

        // Surrender teamfight
        public static void surrenderTeamfight(this Teamfight fight, Team surrenderTeam)
        {
            // Update fight data
            fight.Active = 0;

            // Get teams
            Team team_a = TeamModule.Instance.Get(fight.Team_a);
            Team team_b = TeamModule.Instance.Get(fight.Team_b);

            // Calculate the winner and loser reward
            int rewardWinner = (int)Math.Round(TeamfightModule.Instance.ConversionFactorWinner * (float)(fight.Team_a_money + fight.Team_b_money), 0);
            int rewardLoser = (int)Math.Round(TeamfightModule.Instance.ConversionFactorLoser * (float)(fight.Team_a_money + fight.Team_b_money), 0);

            if (fight.Team_a == surrenderTeam.Id)
            {
                // Update score
                fight.Kills_team_b = TeamfightModule.Instance.TeamfightEndPoints;

                // Team a surrendered
                TeamfightModule.Instance.Update(fight.Id, fight, "teamfight", $"id={fight.Id}", "kills_team_a", fight.Kills_team_a, "kills_team_b", fight.Kills_team_b, "active", "0");

                // Add money to the bank
                TeamShelterModule.Instance.Get(fight.Team_a).GiveMoney(rewardLoser);
                TeamShelterModule.Instance.Get(fight.Team_b).GiveMoney(rewardWinner);

                // Add reward for team b
                //GenerateRandomReward(team_b);

                // Notify teams
                team_b.SendNotification($"Dein Team hat den Fraktionskampf gewonnen durch Aufgabe des Gegners. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
                team_a.SendNotification($"Dein Team hat den Fraktionskampf aufgegeben. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
            }
            else
            {
                // Update score
                fight.Kills_team_a = TeamfightModule.Instance.TeamfightEndPoints;

                // Team b surrendered
                TeamfightModule.Instance.Update(fight.Id, fight, "teamfight", $"id={fight.Id}", "kills_team_a", fight.Kills_team_a, "kills_team_b", fight.Kills_team_b, "active", "0");

                // Add money to the bank
                TeamShelterModule.Instance.Get(fight.Team_a).GiveMoney(rewardWinner);
                TeamShelterModule.Instance.Get(fight.Team_b).GiveMoney(rewardLoser);

                // Add reward for team a
                //GenerateRandomReward(team_a);

                // Notify teams
                team_b.SendNotification($"Dein Team hat den Fraktionskampf aufgegeben. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
                team_a.SendNotification($"Dein Team hat den Fraktionskampf gewonnen durch Aufgabe des Gegners. {team_a.Name} {fight.Kills_team_a} : {team_b.Name} {fight.Kills_team_b}");
            }

            // remove weapons
            RemoveWeaponsForTeam(team_a);
            RemoveWeaponsForTeam(team_b);
        }

        // Store weapons for player inside teamfight container
        public static void StorePlayerweaponsInsideTeamfightContainer(DbPlayer dbPlayer)
        {
            if (dbPlayer == null) return;

            if (dbPlayer.Weapons.Count != 0)
            {
                foreach (var weapon in dbPlayer.Weapons.ToList())
                {
                    // Get weapon data
                    WeaponData weaponData = WeaponDataModule.Instance[weapon.WeaponDataId];
                    if (weaponData == null) continue;

                    var gun = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(weaponData.Name));
                    if (gun == null) continue;

                    // Add weapon
                    dbPlayer.TeamFightContainer.AddItem(gun);

                    // Get Ammo
                    var ammo = weapon.Ammo;

                    if (ammo > 0)
                    {
                        // Get magazine data
                        var magazin = ItemModelModule.Instance.GetByScript("ammo_" + Convert.ToString(gun.Name));
                        if (magazin == null) continue;

                        var magazinAmmo = Convert.ToInt32(magazin.Script.ToLower().Replace("ammo_", "").Split('_')[1]);

                        var magazines = ammo / magazinAmmo;
                        // Add magazines
                        dbPlayer.TeamFightContainer.AddItem(magazin, magazines);
                    }
                }

                // Remove weapons and send notification to user
                dbPlayer.RemoveWeapons();
                //dbPlayer.SendNewNotification("Deine Waffen wurden in den Fraktionskampf Container gelegt!");
            }
        }

        // Store weapons for team inside teamfight container
        public static void StoreWeaponsInsideTeamfightContainer(Team team)
        {
            if (team == null) return;

            foreach (var member in team.Members.Values)
            {
                if (member.Weapons.Count != 0)
                {
                    foreach (var weapon in member.Weapons.ToList())
                    {
                        // Get weapon data
                        WeaponData weaponData = WeaponDataModule.Instance[weapon.WeaponDataId];
                        if (weaponData == null) continue;

                        var gun = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(weaponData.Name));
                        if (gun == null) continue;

                        // Add weapon
                        member.TeamFightContainer.AddItem(gun);

                        // Get Ammo
                        var ammo = weapon.Ammo;

                        if (ammo > 0)
                        {
                            // Get magazine data
                            var magazin = ItemModelModule.Instance.GetByScript("ammo_" + Convert.ToString(gun.Name));
                            if (magazin == null) continue;

                            var magazinAmmo = Convert.ToInt32(magazin.Script.ToLower().Replace("ammo_", "").Split('_')[1]);

                            var magazines = ammo / magazinAmmo;

                            // Add magazines
                            member.TeamFightContainer.AddItem(magazin, magazines);
                        }
                    }

                    // Remove weapons and send notification to user
                    member.RemoveWeapons();
                    //member.SendNewNotification("Deine Waffen wurden in den Fraktionskampf Container gelegt!");
                }

                member.SetTeamfight();
            }
        }

        // Generate random rewards for winner team
        public static void GenerateRandomReward(Team team)
        {
            if (team == null) return;

            // List containing rewards
            List<int> rewards = new List<int>(new int[] { 40, 159, 556 });

            foreach (var member in team.Members.Values)
            {
                // Get a random reward
                int rewardToAdd = rewards.ElementAt(new Random().Next(rewards.Count));

                // Handle reward and add it to the teamfight container
                switch (rewardToAdd)
                {
                    case 40:
                        member.TeamFightContainer.AddItem(40, 5);
                        break;
                    case 159:
                        member.TeamFightContainer.AddItem(159, 50);
                        break;
                    case 556:
                        member.TeamFightContainer.AddItem(556, 25);
                        break;
                }
            }
        }

        // Add killstreak reward to players inventory
        public static void AddKillstreakReward(DbPlayer dbPlayer, ItemModel item)
        {
            if (dbPlayer == null) return;
            if (!dbPlayer.Container.CanInventoryItemAdded(item.Id, 1)) return;

            dbPlayer.Container.AddItem(item, 1);
        }

        // Remove weapons for team and reset kill streaks
        public static void RemoveWeaponsForTeam(Team team)
        {
            if (team == null) return;

            foreach (var member in team.Members.Values)
            {
                member.RemoveWeapons();
                member.SetTeamfight();
                member.TeamfightKillCounter = 0;
            }
        }

        // Increase teamfight kills / points
        public static void increaseTeamfightPoints(this Teamfight fight, int kills_team_a, int kills_team_b)
        {
            fight.Kills_team_a += kills_team_a;
            fight.Kills_team_b += kills_team_b;

            if (fight.Kills_team_a == TeamfightModule.Instance.TeamfightEndPoints || fight.Kills_team_b == TeamfightModule.Instance.TeamfightEndPoints)
            {
                finishTeamFight(fight);
            }
        }

        public static void SetToGangware(DbPlayer dbPlayer)
        {
            TeamfightFunctions.StorePlayerweaponsInsideTeamfightContainer(dbPlayer);

            dbPlayer.Player.Dimension = GangwarModule.Instance.DefaultDimension;
            dbPlayer.DimensionType[0] = DimensionType.Gangwar;

            var player = dbPlayer.Player;
            player.GiveWeapon(WeaponHash.HeavyPistol, 600);
            player.GiveWeapon(WeaponHash.AssaultRifle, 0);
            player.GiveWeapon(WeaponHash.AdvancedRifle, 0);
            player.GiveWeapon(WeaponHash.BullpupRifle, 600);
            player.GiveWeapon(WeaponHash.Gusenberg, 600);

            player.Health = 100;
            player.Armor = 100;

            dbPlayer.Container.AddItem(655, 5);
            dbPlayer.Container.AddItem(654, 5);
        }

        public static void RemoveFromGangware(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Dimension != GangwarModule.Instance.DefaultDimension) return;

            dbPlayer.Player.Dimension = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.RemoveAllWeapons();

            dbPlayer.Container.RemoveItemAll(654);
            dbPlayer.Container.RemoveItemAll(655);
        }
    }
}
