using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Menu;

namespace GVRP.Module.Laboratories
{
    public class LaboratoryModule : Module<LaboratoryModule>
    {
        public static int TimeToImpound = 90000;
        public static int TimeToFrisk = 30000;
        public static int TimeToAnalyze = 30000;
        public static int TimeToBreakDoor = 600000;
        public static int TimeToHack = 60000;
        
        protected override bool OnLoad()
        {
            // Frisk Menu
            MenuManager.Instance.AddBuilder(new LaboratoryOpenInvMenu());
            if (Configurations.Configuration.Instance.DevMode)
            {
                TimeToImpound = 3000;
                TimeToFrisk = 3000;
                TimeToAnalyze = 3000;
                TimeToBreakDoor = 3000;
                TimeToHack = 3000;
            }
            return true;
        }
        public bool IsImpoundVehicle(uint Model)
        {
            return Model == (uint)VehicleHash.Brickade ||
                    Model == (uint)VehicleHash.Burrito ||
                    Model == (uint)VehicleHash.Burrito2 ||
                    Model == (uint)VehicleHash.Burrito3 ||
                    Model == (uint)VehicleHash.Burrito4 ||
                    Model == (uint)VehicleHash.Burrito5 ||
                    Model == (uint)VehicleHash.GBurrito ||
                    Model == (uint)VehicleHash.GBurrito2;
        }
    }
}
