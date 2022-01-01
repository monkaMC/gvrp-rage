using GVRP.Module.Business;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class BusinessBankMenuBuilder : MenuBuilder
    {
        public BusinessBankMenuBuilder() : base(PlayerMenu.BusinessBank)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Business Verwaltung");

            menu.Add(MSG.General.Close(), "");

            if (!iPlayer.IsMemberOfBusiness())
            {
                menu.Description = $"Ein Unternehmen Gruenden!";
                menu.Add("Business kaufen - 250.000$", "");
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
                switch (index)
                {
                    case 1: // Kaufen
                        
                        if (!iPlayer.IsMemberOfBusiness() && !iPlayer.IsHomeless())
                        {
                                                                                     
                            if (!iPlayer.TakeBankMoney(250000))
                            {
                                iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(250000));
                                break;
                            }

                            BusinessModule.Instance.CreatePlayerBusiness(iPlayer);
                            iPlayer.SendNewNotification("Business erfolgreich fuer $250000 erworben!");
                            
                        }
                        else
                        {
                            iPlayer.SendNewNotification("Sie haben bereits ein Business oder haben keinen Wohnsitz!");
                        }
                        break;
                    default:
                        break;
                }

                return true;
            }
        }
    }
}