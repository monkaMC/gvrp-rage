using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubAnpassungHandlerMenuBuilder : MenuBuilder
    {
        public NightClubAnpassungHandlerMenuBuilder() : base(PlayerMenu.NightClubAnpassungHandler)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (iPlayer.Player.Dimension == 0) return null;
            NightClub nightClub = NightClubModule.Instance.Get(iPlayer.Player.Dimension);
            if (nightClub == null) return null;

            if (!nightClub.IsOwnedByBusines() || !iPlayer.IsMemberOfBusiness() || !iPlayer.HasData("NightClubData") || !iPlayer.GetActiveBusinessMember().NightClub || iPlayer.ActiveBusiness.BusinessBranch.NightClubId != nightClub.Id) return null;

            var nightClubData = iPlayer.GetData("NightClubData");

            var menu = new Menu.Menu(Menu, $"{nightClub.Name} - {getCategory(nightClubData)}");
            menu.Add($"Schließen");

            if (nightClubData == 1)
            {
                menu.Add($"Stufe 1");
                menu.Add($"Stufe 2");
                menu.Add($"Stufe 3");
            }
            else if (nightClubData == 2)
            {
                menu.Add($"Leer");
                menu.Add($"Stufe 1");
                menu.Add($"Stufe 2");
                menu.Add($"Stufe 3");
            }
            else if (nightClubData == 3)
            {
                menu.Add($"Leer");
                menu.Add($"Stäbchen Gelb");
                menu.Add($"Raster Gelb");
                menu.Add($"Bahnen Gelb");
                menu.Add($"Stäbchen Grün");
                menu.Add($"Raster Grün");
                menu.Add($"Bahnen Grün");
                menu.Add($"Stäbchen Weiss");
                menu.Add($"Raster Weiss");
                menu.Add($"Bahnen Weiss");
                menu.Add($"Stäbchen Türkis");
                menu.Add($"Raster Türkis");
                menu.Add($"Bahnen Türkis");
            }
            else if (nightClubData == 4)
            {
                menu.Add($"Leer");
                menu.Add($"Strahlen Gelb");
                menu.Add($"Strahlen Grün");
                menu.Add($"Strahlen Weiss");
                menu.Add($"Strahlen Türkis");
            }
            else if (nightClubData == 5)
            {
                menu.Add($"Leer");
                menu.Add($"Galaxy");
                menu.Add($"Studio LS");
                menu.Add($"Omega");
                menu.Add($"Technologie");
                menu.Add($"Gefängnis");
                menu.Add($"Maisonette");
                menu.Add($"Tonys Fun House");
                menu.Add($"The Palace");
                menu.Add($"Paradise");
            }
            else if (nightClubData == 6)
            {
                menu.Add($"Leer");
                menu.Add($"Eingangsbeleuchtung");
            }
            else if (nightClubData == 7)
            {
                menu.Add($"Leer");
                menu.Add($"Sicherheitssystem");
            }

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
                    return false;
                }
                else
                {
                    var nightclubCat = iPlayer.GetData("NightClubData");

                    nightClub.Upgrade(iPlayer, nightclubCat, index);
                    return true;
                }
            }
        }

        private string getCategory(int cat)
        {
            switch (cat)
            {
                case 1:
                    return "Interieur";
                case 2:
                    return "Dekoration";
                case 3:
                    return "Lichter";
                case 4:
                    return "Effekte";
                case 5:
                    return "Clubname";
                case 6:
                    return "Eingangsbeleuchtung";
                case 7:
                    return "Sicherheitssystem";
                default:
                    return "";
            }
        }
    }
}