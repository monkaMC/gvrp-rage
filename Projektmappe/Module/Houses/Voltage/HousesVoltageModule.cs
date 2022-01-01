using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Houses.Menu;
using GVRP.Module.Menu;

namespace GVRP.Module.Houses
{
    public class HousesVoltageModule : SqlModule<HousesVoltageModule, HousesVoltage, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `houses_voltage`;";
        }
        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new VoltageMenuBuilder());
            MenuManager.Instance.AddBuilder(new HackingVoltageMenuBuilder());
            base.OnLoaded();
        }

        public HousesVoltage GetClosestFromPosition(Vector3 position)
        {
            HousesVoltage returnVoltage = GetAll().Values.ToList().First();

            foreach (HousesVoltage housesvoltage in GetAll().Values.ToList().Where(h => h.Position.DistanceTo(position) < 250))
            {
                if (housesvoltage.Position.DistanceTo(position) < returnVoltage.Position.DistanceTo(position))
                {
                    returnVoltage = housesvoltage;
                }
            }
            return returnVoltage;
        }
    }
}
