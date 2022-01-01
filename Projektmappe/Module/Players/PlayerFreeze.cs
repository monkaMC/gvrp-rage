
using GTANetworkAPI;
using GVRP.Module.Injury;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerFreeze
    {
        public static void Freeze(this DbPlayer dbPlayer, bool freeze)
        {
            dbPlayer.Player.TriggerEvent("freezePlayer", freeze);
        }

        public static void Freeze(this DbPlayer dbPlayer, bool freeze, bool shapefreeze = false,
            bool interrupt = false)
        {
            if (!freeze)
            {
                if (dbPlayer.deadtime[0] > 0 || dbPlayer.isInjured() ||
                    dbPlayer.Player.IsInVehicle
                    || dbPlayer.IsCuffed || dbPlayer.IsTied)
                {
                    return;
                }
            }
            dbPlayer.Player.TriggerEvent("freezePlayer", freeze);
            //dbPlayer.Player.FreezePosition = freeze;
        }
        
        public static void Freeze(this Client player, bool freeze, bool shapefreeze = false,
            bool interrupt = false)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer != null)
            {
                dbPlayer.Freeze(freeze, shapefreeze, interrupt);
            }
            else
            {
                //player.TriggerEvent("freezePlayer", freeze);
                //player.FreezePosition = freeze;
            }
        }
    }
}