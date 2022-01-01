using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons
{
    public class WeaponDetail
    {
        public int WeaponDataId;
        public List<int> Components;
        public int Ammo;
    }
}
