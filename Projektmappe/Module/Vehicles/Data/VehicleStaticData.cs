using System.Collections.Generic;

namespace GVRP.Module.Vehicles.Data
{
    public class ModData
    {
        public string Name;
        
        public List<string> flags = new List<string>();
    }
    
    public class ModTypeData
    {   
        public int amount;
        public Dictionary<int, ModData> list = new Dictionary<int, ModData>();
    }
    
    public class VehicleStaticData
    {
        public Dictionary<int, ModTypeData> mods;
    }
}