using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class AccountLicenseMenuBuilder : MenuBuilder
    {
        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Lizenzen");
            switch (iPlayer.Lic_Car[0])
            {
                case 1:
                    menu.Add("Fuehrerschein PKW", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Fuehrerschein  PKW", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Car[0] < 0)
                        menu.Add("Fuehrerschein  PKW", "Vorhanden, Sperre: " + iPlayer.Lic_Car[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_LKW[0])
            {
                case 1:
                    menu.Add("Fuehrerschein LKW", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Fuehrerschein LKW", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_LKW[0] < 0)
                        menu.Add("Fuehrerschein LKW", "Vorhanden, Sperre: " + iPlayer.Lic_LKW[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Bike[0])
            {
                case 1:
                    menu.Add("Motorradschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Motorradschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Bike[0] < 0)
                        menu.Add("Motorradschein", "Vorhanden, Sperre: " + iPlayer.Lic_Bike[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_PlaneA[0])
            {
                case 1:
                    menu.Add("Flugschein A", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Flugschein A", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_PlaneA[0] < 0)
                        menu.Add("Flugschein A", "Vorhanden, Sperre: " + iPlayer.Lic_PlaneA[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_PlaneB[0])
            {
                case 1:
                    menu.Add("Flugschein B", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Flugschein B", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_PlaneB[0] < 0)
                        menu.Add("Flugschein B", "Vorhanden, Sperre: " + iPlayer.Lic_PlaneB[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Boot[0])
            {
                case 1:
                    menu.Add("Bootsschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Bootsschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Boot[0] < 0)
                        menu.Add("Bootsschein", "Vorhanden, Sperre: " + iPlayer.Lic_Boot[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Gun[0])
            {
                case 1:
                    menu.Add("Waffenschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Waffenschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Gun[0] < 0)
                        menu.Add("Waffenschein", "Vorhanden, Sperre: " + iPlayer.Lic_Gun[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Biz[0])
            {
                case 1:
                    menu.Add("Gewerbeschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Gewerbeschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Biz[0] < 0)
                        menu.Add("Gewerbeschein", "Vorhanden, Sperre: " + iPlayer.Lic_Biz[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Transfer[0])
            {
                case 1:
                    menu.Add("Personenbeförderungsschein", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Personenbeförderungsschein", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Transfer[0] < 0)
                        menu.Add("Personenbeförderungsschein",
                            "Vorhanden, Sperre: " + iPlayer.Lic_Transfer[0] + " Minuten");
                    break;
            }

            switch (iPlayer.Lic_Taxi[0])
            {
                case 1:
                    menu.Add("Taxilizenz", "Vorhanden");
                    break;
                case 2:
                    menu.Add("Taxilizenz", "Vorhanden (Plagiat)");
                    break;
                default:
                    if (iPlayer.Lic_Taxi[0] < 0)
                        menu.Add("Taxilizenz", "Vorhanden, Sperre: " + iPlayer.Lic_Taxi[0] + " Minuten");
                    break;
            }

            menu.Add(MSG.General.Close(), "");

            return menu;
        }

        public AccountLicenseMenuBuilder() : base(PlayerMenu.AccountLicense)
        {
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EvenntHandler();
        }

        private class EvenntHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                MenuManager.DismissMenu(iPlayer.Player, (uint) PlayerMenu.AccountLicense);
                return false;
            }
        }
    }
}