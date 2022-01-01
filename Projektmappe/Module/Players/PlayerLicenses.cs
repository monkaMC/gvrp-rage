using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.FIB;
using GVRP.Module.Players.Db;
using GVRP.Module.Swat;

namespace GVRP.Module.Players
{
    public static class PlayerLicenses
    {
        //ToDo: Add Custom Window

        public static void ShowLicenses(this DbPlayer dbPlayer, Client destinationPlayer)
        {
            string l_Name = dbPlayer.GetName();
            if ((dbPlayer.IsSwatDuty() || (dbPlayer.IsACop() && dbPlayer.IsInDuty())) && !dbPlayer.IsUndercover())
            {
                l_Name = "Unbekannt";
            }

            destinationPlayer.TriggerEvent("showLicense", l_Name, dbPlayer.Lic_FirstAID[0], dbPlayer.Lic_Gun[0], dbPlayer.Lic_Car[0], dbPlayer.Lic_LKW[0], dbPlayer.Lic_Bike[0], dbPlayer.Lic_Boot[0], dbPlayer.Lic_PlaneA[0], dbPlayer.Lic_PlaneB[0], dbPlayer.Lic_Taxi[0], dbPlayer.Lic_Transfer[0], 0, dbPlayer.marryLic);
        }
    }
}