using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerHealth
    {
        public static void ApplyPlayerHealth(this DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid())
            {
                return;
            }
            if (iPlayer.Hp > 0) iPlayer.SetHealth(iPlayer.Hp);
            if (iPlayer.Armor[0] > 0)
            {
                if (iPlayer.Armor[0] >= 100)
                    iPlayer.SetArmor(iPlayer.Armor[0], true);
                else
                    iPlayer.SetArmor(iPlayer.Armor[0]);
            }
        }

        public static void SetHealth(this DbPlayer iPlayer, int health)
        {
            if (iPlayer == null || !iPlayer.IsValid())
            {
                return;
            }
            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            iPlayer.Hp = health;
            iPlayer.Player.Health = health;
        }

        public static void SetArmor(this DbPlayer iPlayer, int Armor, bool Schutzweste = false)
        {
            if (iPlayer == null || !iPlayer.IsValid())
            {
                return;
            }
            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            // Armor Types
            if(Armor > 100)
            {
                iPlayer.ArmorType = ArmorType.Strong;
            }
            else if(Armor > 200)
            {
                iPlayer.ArmorType = ArmorType.Admin;
            }
            else
            {
                iPlayer.ArmorType = ArmorType.Normal;
            }

            iPlayer.Armor[0] = Armor;
            iPlayer.Player.Armor = Armor;
            iPlayer.visibleArmor = Schutzweste;
            iPlayer.ApplyArmorVisibility();
        }
    }
}