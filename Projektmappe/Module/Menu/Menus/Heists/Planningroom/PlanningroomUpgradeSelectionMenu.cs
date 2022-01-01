using System;
using System.Collections.Generic;
using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomUpgradeSelectionMenuBuilder : MenuBuilder
    {
        public PlanningroomUpgradeSelectionMenuBuilder() : base(PlayerMenu.PlanningroomUpgradeSelectionMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

            Menu menu = new Menu(Menu, "Menu");
            menu.Add($"Schließen");
            if (!iPlayer.HasData("planningRoomUpgradeSelection")) return menu;
            int selection = iPlayer.GetData("planningRoomUpgradeSelection");
            if (!PlanningUpgrades.Upgrades.TryGetValue(selection, out Upgrade upgrade)) return menu;

            Dictionary<int, int> upgrades = new Dictionary<int, int>();
            for(int i = 0; i < upgrade.UpgradeNames.Count; i++)
            {
                if(upgrade.UpgradeNames[i] == "Holzoptik")
                {
                    if (room.MainFloorMirrorLevel != 0 && room.MainFloorInteriorLevel != 0 && room.MainFloorSlotMachineLevel != 0)
                    {
                        menu.Add($"{upgrade.UpgradeNames[i]} ausbauen");
                    }
                }
                else
                {
                    menu.Add($"{upgrade.UpgradeNames[i]} ausbauen");
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
                PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

                // Close menu
                if (index == 0)
                {
                    iPlayer.ResetData("planningRoomUpgradeSelection");
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                // Planningroom upgrade
                else
                {
                    if (!iPlayer.HasData("planningRoomUpgradeSelection")) return false;
                    int selection = iPlayer.GetData("planningRoomUpgradeSelection");
                    if (!PlanningUpgrades.Upgrades.TryGetValue(selection, out Upgrade upgrade)) return false;
                    if (upgrade.Id == 1)
                        index++;
                    room.UpgradePlanningRoom(iPlayer, (int)upgrade.Id, index);

                    iPlayer.ResetData("planningRoomUpgradeSelection");
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
            }
        }
    }
}
