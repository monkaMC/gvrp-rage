using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Configurations;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP.Module.Clothes.Shops
{
    public class ClothesShopModule : Module<ClothesShopModule>
    {
        public const int MaxSlots = 12;

        private Dictionary<int, string> slots;

        private Dictionary<int, string> propsSlots;

        private Dictionary<uint, ClothesShop> shops;

        public override Type[] RequiredModules()
        {
            return new[] {typeof(ClothModule), typeof(PropModule)};
        }

        protected override bool OnLoad()
        {
            slots = LoadClothesSlots();

            propsSlots = LoadPropsSlots();

            shops = LoadShops();
            return true;
        }

        public Dictionary<uint, ClothesShop> LoadShops()
        {
            var shopList = new Dictionary<uint, ClothesShop>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * FROM clothes_shops WHERE !(pos_x = 0);";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return shopList;
                    while (reader.Read())
                    {
                        var shopId = 1;
                        var shopSlotClothes = new Dictionary<Tuple<int, uint, int>, List<Cloth>>();
                        var shopSlotProps = new Dictionary<Tuple<int, uint, int>, List<Prop>>();
                        var shopPropsSlots = new Dictionary<int, string>();
                        var shopClothesSlots = new Dictionary<int, string>();
                        var shopClothes = ClothModule.Instance.GetClothesForShop(1);
                        var shopProps = PropModule.Instance.GetPropsForShop(1);
                        foreach (var cloth in shopClothes)
                        {
                            List<Cloth> slotClothes;

                            if (!shopClothesSlots.ContainsKey(cloth.Slot))
                            {
                                if (slots.ContainsKey(cloth.Slot))
                                {
                                    shopClothesSlots.Add(cloth.Slot, slots[cloth.Slot]);
                                }
                                else
                                {
                                }
                            }

                            if (!shopSlotClothes.ContainsKey(cloth.Tuple))
                            {
                                slotClothes = new List<Cloth>();
                                shopSlotClothes.Add(cloth.Tuple, slotClothes);
                            }
                            else
                            {
                                slotClothes = shopSlotClothes[cloth.Tuple];
                            }

                            slotClothes.Add(cloth);
                        }

                        foreach (var prop in shopProps)
                        {
                            List<Prop> slotProps;

                            if (!shopPropsSlots.ContainsKey(prop.Slot))
                            {
                                if (propsSlots.ContainsKey(prop.Slot))
                                {
                                    shopPropsSlots.Add(prop.Slot, propsSlots[prop.Slot]);
                                }
                                else
                                {
                                }
                            }

                            if (!shopSlotProps.ContainsKey(prop.Tuple))
                            {
                                slotProps = new List<Prop>();
                                shopSlotProps.Add(prop.Tuple, slotProps);
                            }
                            else
                            {
                                slotProps = shopSlotProps[prop.Tuple];
                            }

                            slotProps.Add(prop);
                        }

                        var clothesShop = new ClothesShop(reader, shopSlotClothes, shopSlotProps,
                            shopClothesSlots, shopPropsSlots);
                        shopList.Add(clothesShop.Id, clothesShop);
                        OnShopSpawn(clothesShop);
                    }
                }
                conn.Close();
            }

            return shopList;
        }

        public Dictionary<int, string> LoadClothesSlots()
        {
            var slotList = new Dictionary<int, string>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * FROM clothes_slots ORDER BY clothes_slots.order;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return slotList;
                    while (reader.Read())
                    {
                        slotList.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                conn.Close();
            }

            return slotList;
        }

        public Dictionary<int, string> LoadPropsSlots()
        {
            var slotList = new Dictionary<int, string>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = "SELECT * FROM props_slots ORDER BY props_slots.order;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return slotList;
                    while (reader.Read())
                    {
                        slotList.Add(reader.GetInt32(0), reader.GetString(1));
                    }
                }
                conn.Close();
            }

            return slotList;
        }

        public Dictionary<int, string> GetSlots()
        {
            return slots;
        }

        public Dictionary<int, string> GetPropsSlots()
        {
            return propsSlots;
        }

        public ClothesShop GetShopById(uint shopId)
        {
            return shops.TryGetValue(shopId, out var rob) ? rob : null;
        }

        public int GetActualClothesPrice(DbPlayer iPlayer)
        {
            var maxprice = 0;
            var character = iPlayer.Character;
            for (var i = 0; i < MaxSlots; i++)
            {
                if (!iPlayer.HasData("clothesActualItem-" + i)) continue;
                var cloth =
                    ClothModule.Instance[(uint) iPlayer.GetData("clothesActualItem-" + i)];
                if (!character.Wardrobe.Contains(cloth.Id))
                {
                    maxprice += cloth.Price;
                }
            }

            for (var i = 0; i < MaxSlots; i++)
            {
                if (!iPlayer.HasData("propsActualItem-" + i)) continue;
                var prop =
                    PropModule.Instance[(uint) iPlayer.GetData("propsActualItem-" + i)];
                if (!character.Props.Contains(prop.Id))
                {
                    maxprice += prop.Price;
                }
            }

            return maxprice;
        }

        public void Buy(DbPlayer iPlayer)
        {
            for (var i = 0; i < MaxSlots; i++)
            {
                if (!iPlayer.HasData("clothesActualItem-" + i)) continue;

                var cloth =
                    ClothModule.Instance[(uint) iPlayer.GetData("clothesActualItem-" + i)];

                // Resett slot
                iPlayer.ResetData("clothesActualItem-" + i);

                var character = iPlayer.Character;
                var wardrobe = iPlayer.Character.Wardrobe;

                if (!wardrobe.Contains(cloth.Id))
                {
                    ClothModule.AddNewCloth(iPlayer, cloth.Id);
                }

                character.Clothes[cloth.Slot] = cloth.Id;
            }

            for (var i = 0; i < MaxSlots; i++)
            {
                if (!iPlayer.HasData("propsActualItem-" + i)) continue;

                var props =
                    PropModule.Instance[(uint) iPlayer.GetData("propsActualItem-" + i)];

                // Resett slot
                iPlayer.ResetData("propsActualItem-" + i);

                var character = iPlayer.Character;

                if (!character.Props.Contains(props.Id))
                {
                    ClothModule.AddNewProp(iPlayer, props.Id);
                }

                if (!character.EquipedProps.ContainsKey(props.Slot))
                {
                    character.EquipedProps.Add(props.Slot, props.Id);
                }
                else
                {
                    character.EquipedProps[props.Slot] = props.Id;
                }
            }

            ClothModule.SaveCharacter(iPlayer);

            ClothModule.Instance.ApplyPlayerClothes(iPlayer);
        }

        public static void OnShopSpawn(ClothesShop shop)
        {
            Blips.Create(shop.Position, shop.Name, 73, 1.0f);
        }
    }
}