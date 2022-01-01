using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Logging;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Items
{

    public enum ItemModelTypes
    {
        Perso = 1, // Id, Name
        Magazin = 2, // ?
        Radio = 3, // Fq, Volume
        BargeldKoffer = 4, // DateTime, Mins bsp (datetime, 60)
    }

    public class ItemModel : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int Weight { get; }
        public bool Illegal { get; }
        public string Script { get; }
        public string MeMessage { get; }
        public int MaximumStacksize { get; }
        public bool RemoveOnUse { get; }
        public bool Durable { get; } // Gibt an ob ein ItemData einen Verfall hat
        public int Durability { get; } // Gibt den initialisierungs Wert an (zb 1000 ab dort wirt dann dir LossMin runtergetickt -> bei 0 zerstören)
        public int DurabilityLossMin { get; } // Gibt den Wer pro Min an der von der Durability abgezogen wird
        public ItemModelTypes ItemModelType { get; }
        public string ImagePath { get; }
        public HashSet<uint> AllowedVehicleModels { get; }
        public HashSet<uint> RestrictedToTeams { get; }

        public ItemModel(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name").Trim();
            Weight = reader.GetInt32("weight");
            Illegal = reader.GetInt32("illegal") == 1 ? true : false;
            Script = reader.GetString("script").Trim();
            MeMessage = reader.GetString("me_message");
            MaximumStacksize = reader.GetInt32("maximum_stacksize");
            RemoveOnUse = reader.GetInt32("remove_on_use") == 1 ? true : false;
            Durability = reader.GetInt32("durability");
            DurabilityLossMin = reader.GetInt32("durability_loss_min");
            Durable = (Durability > 0);
            ItemModelType = (ItemModelTypes)reader.GetInt32("type");
            ImagePath = reader.GetString("image_path");
            if (ImagePath.Length <= 0) ImagePath = "Default.png";

            var teamString = reader.GetString("restricted_to_teams");
            RestrictedToTeams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId)) continue;
                    RestrictedToTeams.Add(teamId);
                }
            }

            AllowedVehicleModels = new HashSet<uint>();
            var allowedVehicleModelString = reader.GetString("allowed_vehiclemodels");
            
            if (!string.IsNullOrEmpty(allowedVehicleModelString))
            {
                var splittedVehicleModels = allowedVehicleModelString.Split(',');
                foreach (var splittedVehicle in splittedVehicleModels)
                {
                    if (!uint.TryParse(splittedVehicle, out var splittedVehicleModelId)) continue;
                    AllowedVehicleModels.Add(splittedVehicleModelId);
                }
            }
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool CanDrugInfect()
        {
            return (Id == 1  || // Meth
                    Id == 8  || // GWeed
                    Id == 16 || // Ephedrin
                    Id == 19 || // Weed
                    Id == 18 || // Hanfsamen
                    Id == 302 // Hanfknospe
                );
        }
    }
}
