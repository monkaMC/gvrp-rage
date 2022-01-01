using System;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Clothes
{
    public class Cloth : Loadable<uint>
    {
        public uint Id { get; }

        public string Name { get; }

        public int Slot { get; }

        public int Variation { get; }

        public int Texture { get; }

        public uint TeamId { get; }

        public int StoreId { get; }

        public int UndershirtId { get; }

        public int UndershirtTexture { get; }

        public int Gender { get; }

        public int Price { get; }

        public bool IsDefault { get; }

        public Tuple<int, uint, int> Tuple { get; }

        public Cloth(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Name = reader.GetString(3) + " | " + reader.GetInt32(4);
            Slot = reader.GetInt32(2);
            Variation = reader.GetInt32(3);
            Texture = reader.GetInt32(4);
            TeamId = reader.GetUInt32(5);
            StoreId = reader.GetInt32(6);
            UndershirtId = reader.GetInt32(7);
            UndershirtTexture = reader.GetInt32(8);
            Gender = reader.GetInt32(9);
            Price = reader.GetInt32(10);
            IsDefault = reader.GetInt32(11) == 1;
            Tuple = new Tuple<int, uint, int>(Slot, TeamId, Gender);
        }

        public Cloth(uint id, string name, int slot, int var, int text, uint team, int store, int unders, int underst, int gender, int price, bool def)
        {
            Id = id;
            Name = var + " | " + text;
            Slot = slot;
            Variation = var;
            Texture = text;
            TeamId = team;
            StoreId = store;
            UndershirtId = unders;
            UndershirtTexture = underst;
            Gender = gender;
            Price = price;
            IsDefault = def;
            Tuple = new Tuple<int, uint, int>(Slot, TeamId, Gender);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}