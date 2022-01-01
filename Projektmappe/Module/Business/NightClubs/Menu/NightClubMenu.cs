using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubMenuBuilder : MenuBuilder
    {
        public NightClubMenuBuilder() : base(PlayerMenu.NightClubMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.TryData("nightclubId", out uint nightClubId)) return null;
            NightClub nightClub = NightClubModule.Instance.Get(nightClubId);
            if (nightClub == null) return null;
            
            var menu = new Menu.Menu(Menu, nightClub.Name);

            menu.Add($"Schließen");

            if (!nightClub.IsOwnedByBusines())
            {
                if (iPlayer.IsMemberOfBusiness() && iPlayer.GetActiveBusinessMember().Owner && iPlayer.ActiveBusiness.BusinessBranch.NightClubId == 0 && iPlayer.ActiveBusiness.BusinessBranch.CanBuyBranch())
                {
                    menu.Add($"NightClub kaufen {nightClub.Price}$");
                }
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else
                {
                    if (!iPlayer.TryData("nightclubId", out uint nightClubId)) return false;
                    NightClub nightClub = NightClubModule.Instance.Get(nightClubId);
                    if (nightClub == null) return false;

                    if (!nightClub.IsOwnedByBusines())
                    {
                        if (iPlayer.IsMemberOfBusiness() && iPlayer.GetActiveBusinessMember().Owner && iPlayer.ActiveBusiness.BusinessBranch.NightClubId == 0 && iPlayer.ActiveBusiness.BusinessBranch.CanBuyBranch())
                        {
                            // Kaufen
                            if (iPlayer.ActiveBusiness.TakeMoney(nightClub.Price))
                            {
                                iPlayer.ActiveBusiness.BusinessBranch.SetNightClub(nightClub.Id);
                                iPlayer.SendNewNotification($"{nightClub.Name} erfolgreich fuer ${nightClub.Price} erworben!");
                                nightClub.OwnerBusiness = iPlayer.ActiveBusiness;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(nightClub.Price));
                            }
                        }
                        return true;
                    }
                    return false;
                }
            }
        }
    }
}