using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubManageMenuBuilder : MenuBuilder
    {
        public NightClubManageMenuBuilder() : base(PlayerMenu.NightClubManageMenu)
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
            menu.Add($"NightClub umbenennen");
            menu.Add($"Verkaufspreise anpassen");
            menu.Add($"NightClub anpassen");
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

                // Check Rights
                if (!nightClub.IsOwnedByBusines() || !iPlayer.IsMemberOfBusiness() || !iPlayer.GetActiveBusinessMember().NightClub || iPlayer.ActiveBusiness.BusinessBranch.NightClubId != nightClub.Id) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }
                else if (index == 1) // Namechange
                {
                    // Name
                    ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "NightClub Name", Callback = "SetNightClubName", Message = "Gib einen neuen Namen ein (max 32 Stellen)." });
                    return true;
                }
                else if (index == 2) // Edit prices
                {
                    MenuManager.Instance.Build(PlayerMenu.NightClubPriceMenu, iPlayer).Show(iPlayer);
                }
                else if (index == 3) // Nightclub anpassen
                {
                    MenuManager.Instance.Build(PlayerMenu.NightClubAnpassung, iPlayer).Show(iPlayer);
                }
                
                return false;
            }
        }
    }
}