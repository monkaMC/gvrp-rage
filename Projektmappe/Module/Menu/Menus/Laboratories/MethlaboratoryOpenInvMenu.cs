using System;
using System.Collections.Generic;
using GVRP.Module.Items;
using GVRP.Module.Laboratories;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP
{
    public class LaboratoryOpenInvMenu : MenuBuilder
    {
        public LaboratoryOpenInvMenu() : base(PlayerMenu.LaboratoryOpenInvMenu)
        {

        }

        public override Menu Build(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] == DimensionType.Methlaboratory)
            {
                Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                if (methlaboratory == null) return null;
            }
            else if (dbPlayer.DimensionType[0] == DimensionType.Cannabislaboratory)
            {
                Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                if (cannabislaboratory == null) return null;
            }
            else if (dbPlayer.DimensionType[0] == DimensionType.Weaponlaboratory)
            {
                Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                if (weaponlaboratory == null) return null;
            }
            else return null;

            Menu menu = new Menu(Menu, "Labor Inhalte");
            menu.Add("Schließen", "");
            menu.Add("Durchsuchen", "");
            if(dbPlayer.IsACop())
                menu.Add("Beschlagnahmen", "");
            else
                menu.Add("Ausrauben", "");
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (dbPlayer.DimensionType[0] == DimensionType.Methlaboratory)
                {
                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (methlaboratory == null) return false;

                    switch (index)
                    {
                        case 0: //Schließen
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                        case 1: //Durchsuchung
                            methlaboratory.FriskMethlaboratory(dbPlayer);
                            break;
                        case 2: //Beschlagnahmung / Zinken
                            methlaboratory.ImpoundMethlaboratory(dbPlayer);
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                    }
                    return true;
                }
                else if (dbPlayer.DimensionType[0] == DimensionType.Cannabislaboratory)
                {
                    Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (cannabislaboratory == null) return false;

                    switch (index)
                    {
                        case 0: //Schließen
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                        case 1: //Durchsuchung
                            cannabislaboratory.FriskLaboratory(dbPlayer);
                            break;
                        case 2: //Beschlagnahmung / Zinken
                            cannabislaboratory.ImpoundLaboratory(dbPlayer);
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                    }

                    return true;
                }
                else if (dbPlayer.DimensionType[0] == DimensionType.Weaponlaboratory)
                {
                    Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (weaponlaboratory == null) return false;

                    switch (index)
                    {
                        case 0: //Schließen
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                        case 1: //Durchsuchung
                            weaponlaboratory.FriskLaboratory(dbPlayer);
                            break;
                        case 2: //Beschlagnahmung / Zinken
                            weaponlaboratory.ImpoundLaboratory(dbPlayer);
                            MenuManager.DismissCurrent(dbPlayer);
                            break;
                    }
                    return true;
                }
                else return false;
            }
        }
    }
}
