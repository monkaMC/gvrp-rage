using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Workstation
{
    public class Workstation : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public Vector3 NpcPosition { get; }
        public float NpcHeading { get; }
        public PedHash PedHash { get; }
        public Vector3 SourcePosition { get; }
        public Vector3 FuelPosition { get; }
        public Vector3 EndPosition { get; }

        public int Source5MinAmount { get; }
        public uint SourceItemId { get; }
        public int Fuel5MinAmount { get; }
        public uint FuelItemId { get; }
        public int End5MinAmount { get; }
        public uint EndItemId { get; }

        public int LimitedSourceSize { get; }

        public int RequiredLevel { get; }
        public uint Dimension { get; }

        public Workstation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            NpcPosition = new Vector3(reader.GetFloat("npc_pos_x"), reader.GetFloat("npc_pos_y"), reader.GetFloat("npc_pos_z"));
            NpcHeading = reader.GetFloat("npc_heading");
            PedHash = Enum.TryParse(reader.GetString("npc_model"), true, out PedHash skin) ? skin : PedHash.ShopKeep01;
            SourcePosition = new Vector3(reader.GetFloat("source_pos_x"), reader.GetFloat("source_pos_y"), reader.GetFloat("source_pos_z"));
            FuelPosition = new Vector3(reader.GetFloat("fuel_pos_x"), reader.GetFloat("fuel_pos_y"), reader.GetFloat("fuel_pos_z"));
            EndPosition = new Vector3(reader.GetFloat("end_pos_x"), reader.GetFloat("end_pos_y"), reader.GetFloat("end_pos_z"));

            LimitedSourceSize = reader.GetInt32("limited_source_amount");

            SourceItemId = reader.GetUInt32("source_item_id");
            Source5MinAmount = reader.GetInt32("source_convert_amount");
            FuelItemId = reader.GetUInt32("fuel_item_id");
            Fuel5MinAmount = reader.GetInt32("fuel_convert_amount");
            EndItemId = reader.GetUInt32("end_item_id");
            End5MinAmount = reader.GetInt32("end_convert_amount");

            Dimension = reader.GetUInt32("dimension");
            RequiredLevel = reader.GetInt32("required_level");

            ColShape shape = Spawners.ColShapes.Create(NpcPosition, 1.5f, 0);
            shape.SetData("workstation", Id);

            // NPC
            new Npc(PedHash, NpcPosition, NpcHeading, 0);

            // Markers
            NAPI.Marker.CreateMarker(25, (EndPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);
            NAPI.Marker.CreateMarker(25, (SourcePosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);

            // Optional
            if (FuelItemId != 0) NAPI.Marker.CreateMarker(25, (FuelPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Dimension);
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
