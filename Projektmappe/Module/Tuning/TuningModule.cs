using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Items;
using GVRP.Module.Node;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Tuning
{
    public class TuningModule : Module<TuningModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(ItemModelModule) };
        }

        public static List<uint> VehicleAvailableMods = new List<uint>();
        
        public override bool Load(bool reload = false)
        {
            VehicleAvailableMods = new List<uint>();

            // add only tunings what available
            foreach(ItemModel itemModel in ItemModelModule.Instance.GetAll().Values.ToList().Where(im => im.Script.ToLower().StartsWith("tune_")))
            {
                VehicleAvailableMods.Add(Convert.ToUInt32(itemModel.Script.ToLower().Split("tune_")[1]));
            }
            return true;
        }
        
    }

    public static class TuningVehicleExtension
    {
        private static Random l_Random = new Random();
        
        public static void SyncMods(this SxVehicle sxVehicle)
        {
            sxVehicle.ClearMods();
            foreach (KeyValuePair<int, int> kvp in sxVehicle.Mods.ToList())
            {
                if (kvp.Value != -1)
                {
                    sxVehicle.SetMod(kvp.Key, kvp.Value);
                }
            }
        }

        public static Dictionary<int, int> ConvertModsToDictonary(string tuning)
        {
            var mods = new Dictionary<int, int>();
            if (string.IsNullOrEmpty(tuning)) return mods;
            var splittedmodsString = tuning.Split(',');
            foreach (var modString in splittedmodsString)
            {
                if (string.IsNullOrEmpty(modString)) continue;
                var parts = modString.Split(':');
                if (parts.Length < 2) continue;
                if (!int.TryParse(parts[0], out var slot)) continue;
                if (!int.TryParse(parts[1], out var mod)) continue;
                if (mods.ContainsKey(slot))
                {
                    mods[slot] = mod;
                }
                else
                {
                    mods.Add(slot, mod);
                }
            }

            return mods;
        }

        public static string ConvertModsToString(Dictionary<int, int> mods)
        {
            var modsString = new StringBuilder();
            var count = 0;
            var lengthEnd = mods.Count - 1;
            foreach (var mod in mods)
            {
                modsString.Append($"{mod.Key}:{mod.Value}");
                if (count != lengthEnd)
                {
                    modsString.Append(",");
                }
                count++;
            }

            return modsString.ToString();
        }

        public static void SetMod(this SxVehicle sxVehicle, int type, int mod)
        {
            var l_NearPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(50.0f, sxVehicle.entity.Position);
            foreach (var l_Player in l_NearPlayers)
            {
                l_Player.TriggerEvent("syncTuning", sxVehicle.entity, type, mod);
            }
        }

        public static void RemoveMod(this SxVehicle sxVehicle, int type)
        {
            var l_NearPlayers = NAPI.Player.GetPlayersInRadiusOfPosition(50.0f, sxVehicle.entity.Position);
            foreach (var l_Player in l_NearPlayers)
            {
                l_Player.TriggerEvent("syncTuning", sxVehicle.entity, type, -1);
            }
        }

        public static void ClearMods(this SxVehicle sxVehicle)
        {
            foreach(int i in TuningModule.VehicleAvailableMods.ToList())
            {
                sxVehicle.RemoveMod(i);
            }

            
            sxVehicle.entity.PrimaryColor = sxVehicle.color1;
            sxVehicle.entity.SecondaryColor = sxVehicle.color2;
        }

        public static void AddSavedMod(this SxVehicle sxVehicle, int type, int mod)
        {        
            if (!sxVehicle.Mods.ContainsKey(type)) sxVehicle.Mods.Add(type, -1);
            sxVehicle.Mods[type] = mod;
            sxVehicle.SetMod(type, mod);

            sxVehicle.SaveMods();

        }

        public static void RemoveSavedMod(this SxVehicle sxVehicle, int type, int mod)
        {            
            if (sxVehicle.Mods.ContainsKey(type)) sxVehicle.Mods.Remove(type);
            sxVehicle.Mods[type] = -1;
            sxVehicle.RemoveMod(type);

            sxVehicle.SaveMods();
        }
        
        public static void SaveMods(this SxVehicle sxVehicle)
        {        
            if (sxVehicle.IsPlayerVehicle())
            {
                var query = $"UPDATE `vehicles` SET tuning = '{ConvertModsToString(sxVehicle.Mods)}' WHERE id = '{sxVehicle.databaseId}';";
                MySQLHandler.ExecuteAsync(query);
            }
            else if (sxVehicle.IsTeamVehicle())
            {
                var query = $"UPDATE `fvehicles` SET tuning = '{ConvertModsToString(sxVehicle.Mods)}' WHERE id = '{sxVehicle.databaseId}';";
                MySQLHandler.ExecuteAsync(query);
            }
        }
    }
}