using System;
using GVRP.Module.Spawners;

namespace GVRP.Module.PointsOfInterest
{
    public class PointOfInterestModule : SqlModule<PointOfInterestModule, PointOfInterest, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `point_of_interest`;";
        }

        protected override void OnLoaded()
        {

            foreach (var item in GetAll())
            {
                PointOfInterest poi = item.Value;

                //If position needs to set 
                if (poi.Blip != 0 && poi.CategoryId != 0)
                {
                    Main.ServerBlips.Add(Blips.Create(new GTANetworkAPI.Vector3(poi.X, poi.Y, 0.0d), poi.Name, poi.Blip, 1.0f, color:(int)poi.BlipColor));
                }
            }





            base.OnLoaded();
        }




    }
}