using GVRP.Module.Armory;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Staatskasse;

namespace GVRP
{
    public class ArmoryItemMenuBuilder : MenuBuilder
    {
        public ArmoryItemMenuBuilder() : base(PlayerMenu.ArmoryItems)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Armory Items");

            menu.Add(MSG.General.Close(), "");
            menu.Add("Zurueck", "");

            if (!iPlayer.HasData("ArmoryId")) return null;
            var ArmoryId = iPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return null;
            foreach (var ArmoryItem in Armory.ArmoryItems)
            {
                menu.Add((ArmoryItem.Price > 0 ? ("$" + ArmoryItem.Price + " ") : "") + ArmoryItem.Item.Name,
                    "");
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
                if (!iPlayer.HasData("ArmoryId")) return false;
                var ArmoryId = iPlayer.GetData("ArmoryId");
                Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return false;

                switch (index)
                {
                    case 0:
                        MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.ArmoryItems);
                        return false;
                    case 1:
                        MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.Armory);
                        return false;
                    default:
                        var actualIndex = 0;
                        foreach (var ArmoryItem in Armory.ArmoryItems)
                        {
                            if (actualIndex == index - 2)
                            {
                                // Rang check
                                if (iPlayer.TeamRank < ArmoryItem.RestrictedRang)
                                {
                                    iPlayer.SendNewNotification(
                                        "Sie haben nicht den benötigten Rang fuer diese Waffe!");
                                    return false;
                                }

                                if (!iPlayer.IsInDuty())
                                {
                                    iPlayer.SendNewNotification(
                                        "Sie muessen dafuer im Dienst sein!");
                                    return false;
                                }

                                // Check Armory
                                if (Armory.GetPackets() < ArmoryItem.Packets)
                                {
                                    iPlayer.SendNewNotification(
                                        $"Die Waffenkammer hat nicht mehr genuegend Materialien! (Benötigt: {ArmoryItem.Packets} )");
                                    return false;
                                }

                                // Check inventory
                                if (!iPlayer.Container.CanInventoryItemAdded(ArmoryItem.Item, 1))
                                {
                                    iPlayer.SendNewNotification(
                                        $"Sie können das nicht mehr tragen, Ihr Inventar ist voll!");
                                    return false;
                                }
                                if (ArmoryItem.Price > 0 && !iPlayer.TakeBankMoney(ArmoryItem.Price))
                                {
                                    iPlayer.SendNewNotification(
                                        $"Dieses Item kostet {ArmoryItem.Price}$ (Bank)!");
                                    return false;
                                }

                                // Found
                                iPlayer.Container.AddItem(ArmoryItem.Item, 1);
                                Armory.RemovePackets(ArmoryItem.Packets);

                                if (ArmoryItem.Price > 0)
                                {
                                    iPlayer.SendNewNotification($"{ArmoryItem.Item.Name} für ${ArmoryItem.Price} ausgerüstet!");
                                    KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, +ArmoryItem.Price);
                                }
                                return false;
                            }

                            actualIndex++;
                        }

                        break;
                }

                return false;
            }
        }
    }
}