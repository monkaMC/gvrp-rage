using GTANetworkAPI;
using System.Collections.Generic;
using GVRP.Module.Government;

namespace GVRP.Module.Armory
{
    public class ArmoryWeapon
    {
        public string WeaponName { get; set; }
        public WeaponHash Weapon { get; set; }

        public int MagazinPrice { get; set; }

        public int RestrictedRang { get; set; }
        public int Packets { get; set; }

        public int Price { get; set; }

        public int Defcon3Rang { get; set; }
        public int Defcon2Rang { get; set; }
        public int Defcon1Rang { get; set; }
        public List<Weapons.Component.WeaponComponent> Components { get; set; }

        public int GetDefconRequiredRang()
        {
            if(GovernmentModule.Defcon.Level == 3 && Defcon3Rang > 0)
            {
                return Defcon3Rang-1;
            }
            else if (GovernmentModule.Defcon.Level == 2 && Defcon2Rang > 0)
            {
                return Defcon2Rang-1;
            }
            else if (GovernmentModule.Defcon.Level == 1 && Defcon1Rang > 0)
            {
                return Defcon1Rang-1;
            }

            return RestrictedRang;
        }
    }
}