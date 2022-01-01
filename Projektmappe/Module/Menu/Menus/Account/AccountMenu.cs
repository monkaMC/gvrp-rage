using GVRP.Module.Jobs;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class AccountMenuBuilder : MenuBuilder
    {
        public AccountMenuBuilder() : base(PlayerMenu.Account)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, iPlayer.GetName());
            //Todo: outsource
            int rp_multiplikator = 4;

            string str = "";
            string str2 = "";
            str = iPlayer.Team.Name;

            menu.Add("Name: " + iPlayer.GetName() + " | Level: " + iPlayer.Level,
                ((iPlayer.Level * rp_multiplikator) - iPlayer.rp[0]) + " Stunden bis zum Levelaufstieg!");


            menu.Add("ID: " + iPlayer.ForumId, "");
            menu.Add("Academic Punkte: " + iPlayer.uni_points[0],
                "Geschaeftsmann: " + iPlayer.uni_business[0] + " | Sparfuchs: " + iPlayer.uni_economy[0] +
                " | Workaholic: " + iPlayer.uni_workaholic[0]);
            menu.Add($"Firma:" + (iPlayer.ActiveBusiness != null ? iPlayer.ActiveBusiness.Name : "Keine"));
            menu.Add("GWD Note: " + Content.GetGwdText(iPlayer.grade[0]), "");

            str2 = iPlayer.Rank.GetDisplayName();
            if (iPlayer.RankId == 0 && iPlayer.donator[0] > 0)
            {
                str2 = Content.General.GetDonorName(iPlayer.donator[0]);
            }

            menu.Add("Nexus: ~b~" + str2, "");

            menu.Add("Bargeld: ~g~" + iPlayer.money[0] + "$", "");
            menu.Add("Wanteds: ~r~" + iPlayer.wanteds[0] + "/59", "");
            menu.Add("Immobilie: ~y~" + iPlayer.ownHouse[0],
                "HausID ist fuer supportzwecke wichtig!");

            if (iPlayer.TeamRank > 0)
            {
                menu.Add("Organisation: " + str, "Rang: " + iPlayer.TeamRank);
            }
            else
            {
                menu.Add("Organisation: " + str, "");
            }

            Job iJob;
            if ((iJob = iPlayer.GetJob()) != null)
            {
                menu.Add(
                    "Beruf: ~b~" + iJob.Name + "~w~ | Erfahrung: ~g~" + iPlayer.jobskill[0] + "~w~/5000", "");
            }
            else
            {
                menu.Add("Beruf: ~b~Keiner", "");
            }

            menu.Add("Zeit seit PayDay: ~y~" + iPlayer.payday[0] + " Minuten",
                "Noch " + ((iPlayer.Level * rp_multiplikator) - iPlayer.rp[0]) +
                " Stunden bis zum Levelaufstieg!");
            menu.Add("Lizenzen", "");
            menu.Add("Fahrzeugschluessel", "");
            menu.Add("Hausschluessel", "");
            menu.Add("Warns: " + iPlayer.warns[0] + "/3", "");
            menu.Add("Handynummer: " + iPlayer.handy[0],
                "Guthaben: $" + iPlayer.guthaben[0]);
            menu.Add(MSG.General.Close(), "");
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
                    case 12:
                        MenuManager.Instance.Build(PlayerMenu.AccountLicense, iPlayer).Show(iPlayer);
                        break;
                    case 13:
                        MenuManager.Instance.Build(PlayerMenu.AccountVehicleKeys, iPlayer).Show(iPlayer);
                        break;
                    case 14:
                        MenuManager.Instance.Build(PlayerMenu.AccountHouseKeys, iPlayer).Show(iPlayer);
                        break;
                    default:
                        MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.Account);
                        break;
                }

                return false;
            }
        }
    }
}