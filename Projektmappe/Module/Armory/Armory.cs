using System;
using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Swat;
using GVRP.Module.Teams;
using GVRP.Module.Weapons.Component;

//Todo: module
namespace GVRP.Module.Armory
{
    public class Armory
    {
        public int Id { get; set; }
        public int Packets { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 LoadPosition { get; set; }
        public List<ArmoryWeapon> ArmoryWeapons { get; set; }
        public List<ArmoryItem> ArmoryItems { get; set; }
        public List<ArmoryArmor> ArmoryArmors { get; set; }
        public List<Team> AccessableTeams { get; set; }
        public bool UnlimitedPackets { get; set; }
        public bool HasArmor { get; set; }
        public bool HasHeavyArmor { get; set; }
        public ColShape ColShape { get; set; }
        public bool SwatDuty { get; set; }

        public void RemovePackets(int packets)
        {
            GetArmoryPacketsDb();
            Packets -= packets;
            SetArmoryPacketsDb();
        }

        public void AddPackets(int packets)
        {
            GetArmoryPacketsDb();
            Packets += packets;
            SetArmoryPacketsDb();
        }

        public int GetPackets()
        {
            GetArmoryPacketsDb();
            return Packets;
        }

        private void GetArmoryPacketsDb()
        {
            if (UnlimitedPackets)
            {
                Packets = 50000;
                return;
            }

            var query = string.Format($"SELECT `packets` FROM `armories` WHERE `id` = '{Id}' LIMIT 1");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        Packets = reader.GetInt32(0);
                        return;
                    }
                }
                conn.Close();
            }
        }

        private void SetArmoryPacketsDb()
        {
            MySQLHandler.ExecuteAsync(
                $"UPDATE `armories` SET `packets` = '{Packets}' WHERE `id` = '{Id}'");
        }

        public void LoadArmoryWeapons(ArmoryModule module)
        {
            var query = string.Format($"SELECT * FROM `armories_weapons` WHERE `Armory_id` = '{Id}' ORDER BY `restrictedRang` ASC");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {

                        var cComponents = new List<Weapons.Component.WeaponComponent>();
                        var componentsString = reader.GetString("components");
 
                        if (!string.IsNullOrEmpty(componentsString))
                        {
                            var splittedComponents = componentsString.Split(',');
                            foreach (var componentId in splittedComponents)
                            {
                                if (!int.TryParse(componentId, out var component)) continue;
                                if(WeaponComponentModule.Instance.Contains(component)) cComponents.Add(WeaponComponentModule.Instance.Get(component));
                            }
                        }

                        var ArmoryWeapon = new ArmoryWeapon
                        {
                            Weapon = (WeaponHash)Enum.Parse(typeof(WeaponHash), reader.GetString("weaponHash"), true),
                            WeaponName = reader.GetString("weaponHash"),
                            MagazinPrice = reader.GetInt32("magazin_price"),
                            RestrictedRang = reader.GetInt32("restrictedRang"),
                            Packets = reader.GetInt32("packets"),
                            Price = reader.GetInt32("price"),
                            Components = cComponents,
                            Defcon1Rang = reader.GetInt32("defcon1_rang"),
                            Defcon2Rang = reader.GetInt32("defcon2_rang"),
                            Defcon3Rang = reader.GetInt32("defcon3_rang"),
                        };


                        ArmoryWeapons.Add(ArmoryWeapon);
                        module.Log("ArmoryWeapon  ID " + reader.GetUInt32("id"));
                    }
                }
                conn.Close();
            }
        }

        public void LoadArmoryItems(ArmoryModule module)
        {
            var query = string.Format($"SELECT * FROM `armories_items` WHERE `Armory_id` = '{Id}' ORDER BY `restricted_rang` ASC");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        var armoryItem = new ArmoryItem
                        {
                            ItemId = reader.GetInt32("item_id"),
                            Item = ItemModelModule.Instance.Get(reader.GetUInt32("item_id")),
                            RestrictedRang = reader.GetInt32("restricted_rang"),
                            Packets = reader.GetInt32("packets"),
                            Price = reader.GetInt32("price")
                        };


                        ArmoryItems.Add(armoryItem);
                        module.Log("ArmoryItem " + armoryItem.Item.Name + " wurde geladen!");
                    }
                }
                conn.Close();
            }
        }

        public void LoadArmoryArmors(ArmoryModule module)
        {
            var query = string.Format($"SELECT * FROM `armories_armors` WHERE `Armory_id` = '{Id}' ORDER BY `restricted_rang` ASC");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        ArmoryArmor armoryArmor = new ArmoryArmor
                        {
                            Name = reader.GetString("name"),
                            ArmorValue = reader.GetInt32("armor_value"),
                            VisibleArmorType = reader.GetInt32("visible_armor_type"),
                            RestrictedRang = reader.GetInt32("restricted_rang")
                        };
                        ArmoryArmors.Add(armoryArmor);
                    }
                }
                conn.Close();
            }
        }
    }

    public sealed class ArmoryModule : Module<ArmoryModule>
    {
        private Dictionary<int, Armory> Armories;

        public int WeaponChestMultiplier = 20;

        public override Type[] RequiredModules()
        {
            return new[] {typeof(TeamModule), typeof(ItemModelModule) };
        }

        protected override bool OnLoad()
        {
            Armories = new Dictionary<int, Armory>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM `armories`";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var Armory = new Armory
                            {
                                Id = reader.GetInt32("id"),
                                Packets = reader.GetInt32("packets"),
                                Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z")),
                                LoadPosition = new Vector3(reader.GetFloat("load_pos_x"), reader.GetFloat("load_pos_y"), reader.GetFloat("load_pos_z")),
                                ArmoryWeapons = new List<ArmoryWeapon>(),
                                ArmoryItems = new List<ArmoryItem>(),
                                ArmoryArmors = new List<ArmoryArmor>(),
                                AccessableTeams = new List<Team>(),
                                HasArmor = reader.GetInt32("armor") == 1,
                                HasHeavyArmor = reader.GetInt32("heavyarmor") == 1,
                                SwatDuty = reader.GetInt32("swat_duty") == 1
                            };
                            Armory.ColShape = ColShapes.Create(Armory.Position, 3f);
                            Armory.ColShape.SetData("ArmoryId", Armory.Id);

                            Armory.UnlimitedPackets = Armory.Packets == -1;

                            var teams = reader.GetString("teams");
                            if (!string.IsNullOrEmpty(teams))
                            {
                                if (teams.Contains(","))
                                {
                                    var ts = teams.Split(',');
                                    foreach (var x in ts)
                                    {
                                        if (!uint.TryParse(x, out var teamId)) continue;
                                        Armory.AccessableTeams.Add(TeamModule.Instance.Get(teamId));
                                    }
                                }
                                else
                                    Armory.AccessableTeams.Add(TeamModule.Instance.Get(Convert.ToUInt32(teams)));
                            }

                            Armories.Add(Armory.Id, Armory);
                            Armory.LoadArmoryItems(this);
                            Armory.LoadArmoryWeapons(this);
                            Armory.LoadArmoryArmors(this);

                            Log("Armory " + reader.GetInt32(0) + " mit " + reader.GetInt32(7) +
                                " Paketen wurde geladen!");
                        }
                    }
                }
                conn.Close();
            }

            return true;
        }

        public Armory Get(int id)
        {
            return Armories.TryGetValue(id, out var Armory) ? Armory : null;
        }

        public Armory GetByLoadPosition(Vector3 position, float range = 10.0f)
        {
            foreach (var Armory in Armories)
            {
                if (Armory.Value.LoadPosition.DistanceTo(position) <= range)
                {
                    return Armory.Value;
                }
            }

            return null;
        }

        public bool TriggerPoint(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("ArmoryId")) return false;
            var ArmoryId = iPlayer.GetData("ArmoryId");
            Armory Armory = Get(ArmoryId);
            if (Armory == null) return false;
            // Wenn kein Cop return
            if(Armory.SwatDuty)
            {
                if (iPlayer.HasSwatRights() && !iPlayer.IsSwatDuty())
                {
                    iPlayer.SetSwatDuty(true);
                }
            }
            if (!Armory.AccessableTeams.Contains(iPlayer.Team)) return false;
            MenuManager.Instance.Build(PlayerMenu.Armory, iPlayer).Show(iPlayer);
            return true;
        }
    }
}