using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;


namespace GVRP.Anticheat
{
    public static class Anticheat
    {

        public static void ValidePlayerComponents(Client player)

        {

            var comps = player.GetAllWeaponComponents(player.CurrentWeapon);

            // Defaultweapons hasent any components so....
            foreach (var comp in comps)
            {
                Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                    $"ANTICHEAT (WEAPON CLIP HACK) {player.Name} :: {comp}");
            }
        }

        private static readonly List<WeaponHash> ForbiddenWeapons =
        new List<WeaponHash>(new[] {
            WeaponHash.Railgun, WeaponHash.RPG, WeaponHash.Minigun, WeaponHash.ProximityMine, WeaponHash.StickyBomb, WeaponHash.PipeBomb, WeaponHash.RPG 
        });

        public static void CheckForbiddenWeapons(DbPlayer iPlayer)
        {
            var currW = iPlayer.Player.CurrentWeapon;

            if (!ForbiddenWeapons.Contains(currW)) return;
        }
    }
}