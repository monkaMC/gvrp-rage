using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Clothes.Props;
using GVRP.Module.GTAN;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP.Module.Clothes.Shops
{
    public class ClothesShop
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public int Teamid { get; }
        public string Name { get; }
        public ColShape Colshape { get; }

        public bool CouponUsable { get; set; }

        // Slot, Cloth
        private readonly Dictionary<Tuple<int, uint, int>, List<Cloth>> clothes;

        //Todo: this is not needed
        private readonly Dictionary<int, string> clothesSlots;

        //Slit, Prop
        private readonly Dictionary<Tuple<int, uint, int>, List<Prop>> props;

        //Todo: this is not needed
        private readonly Dictionary<int, string> propsSlots;

        public ClothesShop(MySqlDataReader reader, Dictionary<Tuple<int, uint, int>, List<Cloth>> clothes,
            Dictionary<Tuple<int, uint, int>, List<Prop>> props, Dictionary<int, string> clothesSlots,
            Dictionary<int, string> propsSlots)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Name = reader.GetString("name");
            Colshape = ColShapes.Create(
                Position, 5.0f);
            Colshape.SetData("clothShopId", Id);
            this.clothes = clothes;
            this.props = props;
            this.clothesSlots = clothesSlots;
            this.propsSlots = propsSlots;
            CouponUsable = reader.GetInt32("no_voucher") == 0;
        }

        public List<Cloth> GetClothesBySlotAndTeam(int slot, uint teamId, int gender)
        {
            var slotTeam = new Tuple<int, uint, int>(slot, teamId, gender);
            return clothes.ContainsKey(slotTeam) ? clothes[slotTeam] : null;
        }

        public List<Prop> GetPropsBySlotAndTeam(int slot, uint teamId, int gender)
        {
            var slotTeam = new Tuple<int, uint, int>(slot, teamId, gender);
            return props.ContainsKey(slotTeam) ? props[slotTeam] : null;
        }

        public Dictionary<int, string> GetClothesSlots()
        {
            return clothesSlots;
        }

        public Dictionary<int, string> GetPropsSlots()
        {
            return propsSlots;
        }

        public Dictionary<int, string> GetClothesSlotsForPlayer(DbPlayer player)
        {
            var availableSlots = new Dictionary<int, string>();
            foreach (var slot in ClothesShopModule.Instance.GetSlots())
            {
                var clothesBySlot = GetClothesBySlotForPlayer(slot.Key, player);
                if (clothesBySlot != null && clothesBySlot.Any())
                {
                    availableSlots[slot.Key] = slot.Value;
                }
            }

            return availableSlots;
        }

        public Dictionary<int, string> GetPropsSlotsForPlayer(DbPlayer player)
        {
            var availableSlots = new Dictionary<int, string>();
            foreach (var slot in ClothesShopModule.Instance.GetPropsSlots())
            {
                var clothesBySlot = GetPropsBySlotForPlayer(slot.Key, player);
                if (clothesBySlot != null && clothesBySlot.Any())
                {
                    availableSlots[slot.Key] = slot.Value;
                }
            }

            return availableSlots;
        }

        public List<Cloth> GetClothesBySlotForPlayer(int slot, DbPlayer player)
        {
            var currClothes = new List<Cloth>();
            var slotClothes = GetClothesBySlotAndTeam(slot, player.TeamId, player.Customization.Gender);
            if (slotClothes != null)
            {
                currClothes.AddRange(slotClothes);
            }

            var slotClothesAllGender = GetClothesBySlotAndTeam(slot, player.TeamId, 3);
            if (slotClothesAllGender != null)
            {
                currClothes.AddRange(slotClothesAllGender);
            }

            if (player.TeamId == (int) teams.TEAM_CIVILIAN) return currClothes;
            var slotTeamClothes =
                GetClothesBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, player.Customization.Gender);
            if (slotTeamClothes != null)
            {
                currClothes.AddRange(slotTeamClothes);
            }

            var slotTeamClothesAllGender = GetClothesBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, 3);
            if (slotTeamClothesAllGender != null)
            {
                currClothes.AddRange(slotTeamClothesAllGender);
            }

            return currClothes;
        }

        public List<Prop> GetPropsBySlotForPlayer(int slot, DbPlayer player)
        {
            var currClothes = new List<Prop>();
            var slotClothes = GetPropsBySlotAndTeam(slot, player.TeamId, player.Customization.Gender);
            if (slotClothes != null)
            {
                currClothes.AddRange(slotClothes);
            }

            var slotClothesAllGender = GetPropsBySlotAndTeam(slot, player.TeamId, 3);
            if (slotClothesAllGender != null)
            {
                currClothes.AddRange(slotClothesAllGender);
            }

            if (player.TeamId == (int) teams.TEAM_CIVILIAN) return currClothes;
            var slotTeamClothes =
                GetPropsBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, player.Customization.Gender);
            if (slotTeamClothes != null)
            {
                currClothes.AddRange(slotTeamClothes);
            }

            var slotTeamClothesAllGender = GetPropsBySlotAndTeam(slot, (int) teams.TEAM_CIVILIAN, 3);
            if (slotTeamClothesAllGender != null)
            {
                currClothes.AddRange(slotTeamClothesAllGender);
            }

            return currClothes;
        }
    }
}