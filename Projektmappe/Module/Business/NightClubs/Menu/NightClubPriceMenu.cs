using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.GTAN;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubPriceMenuBuilder : MenuBuilder
    {
        public NightClubPriceMenuBuilder() : base(PlayerMenu.NightClubPriceMenu)
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
            foreach(NightClubItem nightClubItem in nightClub.NightClubShopItems)
            {
                menu.Add($"{nightClubItem.Name} | ${nightClubItem.Price}");
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
                if (iPlayer.Player.Dimension == 0) return false; ;
                NightClub nightClub = NightClubModule.Instance.Get(iPlayer.Player.Dimension);
                if (nightClub == null) return false;

                // Check Rights
                if (!nightClub.IsOwnedByBusines() || !iPlayer.IsMemberOfBusiness() || !iPlayer.GetActiveBusinessMember().NightClub || iPlayer.ActiveBusiness.BusinessBranch.NightClubId != nightClub.Id) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }
                var idx = 1;
                foreach (NightClubItem nightClubItem in nightClub.NightClubShopItems)
                {
                    if(idx == index)
                    {
                        iPlayer.SetData("nightClubItemEdit", nightClubItem.ItemId);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = $"Preis {nightClubItem.Name}", Callback = "SetNightClubItemPrice", Message = "Gib einen neuen Preis an." });
                        return true;
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}