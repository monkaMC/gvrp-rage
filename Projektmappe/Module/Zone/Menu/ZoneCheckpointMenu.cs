//using GVRP.Module.Menu;
//using GVRP.Module.Players;
//using GVRP.Module.Players.Db;

//namespace GVRP.Module.Zone
//{
//    public class ZoneCheckpointMenuBuilder : MenuBuilder
//    {
//        public ZoneCheckpointMenuBuilder() : base(PlayerMenu.ZoneCPMenu)
//        {
//        }

//        public override Menu.Menu Build(DbPlayer iPlayer)
//        {
//            if (!iPlayer.IsACop() || iPlayer.TeamRank < 6 || !iPlayer.IsInDuty())
//            {
//                return null;
//            }

//            var menu = new Menu.Menu(Menu, "Checkpoint Verwaltung");

//            menu.Add($"Schließen");
//            menu.Add($"Checkpoint oeffnen");
//            menu.Add($"Checkpoint schließen");
//            return menu;
//        }

//        public override IMenuEventHandler GetEventHandler()
//        {
//            return new EventHandler();
//        }

//        private class EventHandler : IMenuEventHandler
//        {
//            public bool OnSelect(int index, DbPlayer iPlayer)
//            {
//                if (index == 1) // open
//                {
//                    ZoneModule.Instance.OpenCheckpoint(iPlayer.Player.Position, true);
//                    return true;
//                }
//                else if (index == 2) // close
//                {
//                    ZoneModule.Instance.OpenCheckpoint(iPlayer.Player.Position, false);
//                    return true;
//                }
//                else
//                {
//                    MenuManager.DismissCurrent(iPlayer);
//                    return false;
//                }
//            }
//        }
//    }
//}