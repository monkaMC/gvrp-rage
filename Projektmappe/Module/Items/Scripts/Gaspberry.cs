using GVRP.Module.Configurations;
using GVRP.Module.Laboratories;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Gaspberry(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.DimensionType[0] == DimensionType.Methlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryLaptopPosition) > 2.0f) return false;
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsACop()) return false;
                MethlaboratoryModule.Instance.HackMethlaboratory(dbPlayer);
            }

            if (dbPlayer.DimensionType[0] == DimensionType.Weaponlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) > 2.0f) return false;
                if (!dbPlayer.Team.IsWeaponTeam() && !dbPlayer.IsACop()) return false;
                WeaponlaboratoryModule.Instance.HackWeaponlaboratory(dbPlayer);
            }

            if (dbPlayer.DimensionType[0] == DimensionType.Cannabislaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) > 2.0f) return false;
                if (!dbPlayer.IsAGangster() && !dbPlayer.IsACop()) return false;
                CannabislaboratoryModule.Instance.HackCannabislaboratory(dbPlayer);
            }
            return true;
        }
    }
}