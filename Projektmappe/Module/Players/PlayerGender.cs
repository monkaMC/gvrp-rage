using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerGender
    {
        public static bool IsFemale(this DbPlayer iPlayer)
        {
            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return false;
            return iPlayer.Customization?.Gender == 1;
        }

        public static bool IsMale(this DbPlayer iPlayer)
        {
            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return false;
            return iPlayer.Customization?.Gender == 0;
        }
    }
}