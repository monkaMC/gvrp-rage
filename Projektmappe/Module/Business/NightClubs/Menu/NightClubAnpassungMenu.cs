using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubAnpassungMenuBuilder : MenuBuilder
    {
        public NightClubAnpassungMenuBuilder() : base(PlayerMenu.NightClubAnpassung)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (iPlayer.Player.Dimension == 0) return null;
            NightClub nightClub = NightClubModule.Instance.Get(iPlayer.Player.Dimension);
            if (nightClub == null) return null;

            // Check Rights
            if (!nightClub.IsOwnedByBusines() || !iPlayer.IsMemberOfBusiness() || !iPlayer.GetActiveBusinessMember().NightClub || iPlayer.ActiveBusiness.BusinessBranch.NightClubId != nightClub.Id) return null;

            var menu = new Menu.Menu(Menu, nightClub.Name);

            menu.Add($"Schließen");
            menu.Add($"Interrior");
            menu.Add($"Dekoration");
            menu.Add($"Lichter");
            menu.Add($"Effekte");
            menu.Add($"Clubname");
            menu.Add($"Eingangsbeleuchtung");
            menu.Add($"Sicherheitssystem");

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                if (iPlayer.Player.Dimension == 0) return false;
                NightClub nightClub = NightClubModule.Instance.Get(iPlayer.Player.Dimension);
                if (nightClub == null) return false;

                if (!nightClub.IsOwnedByBusines() || !iPlayer.IsMemberOfBusiness() || !iPlayer.GetActiveBusinessMember().NightClub || iPlayer.ActiveBusiness.BusinessBranch.NightClubId != nightClub.Id) return false;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }
                else if (index > 0)
                {
                    iPlayer.SetData("NightClubData", index);
                    MenuManager.Instance.Build(PlayerMenu.NightClubAnpassungHandler, iPlayer).Show(iPlayer);
                }

                return false;
            }
        }
    }
}
