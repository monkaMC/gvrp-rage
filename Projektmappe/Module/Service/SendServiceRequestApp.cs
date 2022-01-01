using GTANetworkAPI;
using System.Linq;
using System.Text.RegularExpressions;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.MarketplaceApp;
using GVRP.Module.Players;
using GVRP.Module.Teams;

namespace GVRP.Module.Service
{
    public class SendServiceRequestApp : SimpleApp
    {
        public SendServiceRequestApp() : base("ServiceSendRequestApp") { }

        [RemoteEvent]
        public void addServiceRequest(Client client, string department, string message, bool number)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var requestSuccess = false;

            if (dbPlayer.HasData("service") && dbPlayer.GetData("service") > 0)
            {
                dbPlayer.SendNewNotification("Sie haben bereits einen Notruf/Service offen!");
                return;
            }

            var telnr = "0";
            if (number)
            {
                telnr = dbPlayer.handy[0].ToString();
            }

            message = replaceContent(message);

            switch (department)
            {
                case "police":
                    
                    if ((dbPlayer.IsACop() || dbPlayer.TeamId == (uint)teams.TEAM_GOV) && dbPlayer.IsInDuty())
                    {
                        return;
                    }
                    
                    if (TeamModule.Instance[(int)teams.TEAM_POLICE].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_POLICE].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (TeamModule.Instance[(int)teams.TEAM_COUNTYPD].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_COUNTYPD].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (TeamModule.Instance[(int)teams.TEAM_FIB].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_FIB].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_POLICE, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_POLICE, service);

                        dbPlayer.SetData("service", 1);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Notruf zur Polizei abgesendet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Die Leitstelle ist derzeit nicht besetzt!");
                    }

                    break;
                case "medic":
                    if (TeamModule.Instance[(int)teams.TEAM_MEDIC].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_MEDIC].SendNotification($"Ein Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_MEDIC, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_MEDIC, service);

                        dbPlayer.SetData("service", 7);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Notruf zur Rettungswache abgesendet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Die Leitstelle ist derzeit nicht besetzt!");
                    }

                    break;
                case "fahrlehrer":
                    if (dbPlayer.Player.Position.DistanceTo(new Vector3(-810.6085, -1347.864, 5.166561)) >= 20.0f)
                    {
                        dbPlayer.SendNewNotification("Sie muessen an der Fahrschule sein um einen Fahrlehrer zu rufen!");
                        return;
                    }

                    if (TeamModule.Instance[(int)teams.TEAM_DRIVINGSCHOOL].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_DRIVINGSCHOOL].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_DRIVINGSCHOOL, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_DRIVINGSCHOOL, service);

                        dbPlayer.SetData("service", 3);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Fahrlehrer gerufen!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist kein Fahrlehrer im Dienst!");
                    }

                    break;
                case "tow":
                    if (TeamModule.Instance[(int)teams.TEAM_DPOS].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_DPOS].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_DPOS, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_DPOS, service);

                        dbPlayer.SetData("service", 16);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben einen Abschleppdienst gerufen!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist der Abschleppdienst nicht verfuegbar!");
                    }

                    break;
                case "news":
                    if (TeamModule.Instance[(int)teams.TEAM_NEWS].Members.Where(c => c.Value.AccountStatus == AccountStatus.LoggedIn).Count() > 0)
                    {
                        TeamModule.Instance[(int)teams.TEAM_NEWS].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_NEWS, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_NEWS, service);

                        dbPlayer.SetData("service", 4);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben die News kontaktiert!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist kein Weazel News Mitarbeiter im Dienst!");
                    }

                    break;
                case "lsc":
                    TeamModule.Instance[(int)teams.TEAM_LSC].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                    requestSuccess = true;

                    // Trzdm Prüfung weil man sonst für jeden case andere Variablen-Namen drin lassen muss. #Much
                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint)teams.TEAM_LSC, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_LSC, service);

                        dbPlayer.SetData("service", 26);

                        if (status)
                            dbPlayer.SendNewNotification("Sie haben den Los Santos Customs informiert!");
                    }

                    break;
                case "government":
                    if (TeamModule.Instance[(int) teams.TEAM_GOV].Members.Where(c => c.Value.Duty).Count() > 0)
                    {
                        TeamModule.Instance[(int) teams.TEAM_GOV].SendNotification($"Eine Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) mit dem Grund: { message } ist eingegangen!");
                        requestSuccess = true;
                    }

                    if (requestSuccess)
                    {
                        Service service = new Service(dbPlayer.Player.Position, message, (uint) teams.TEAM_GOV, dbPlayer, "", telnr);
                        bool status = ServiceModule.Instance.Add(dbPlayer, (uint) teams.TEAM_GOV, service);

                        dbPlayer.SetData("service", 14);

                        if (status)
                        {
                            dbPlayer.SendNewNotification("Sie haben die Regierung informiert!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Derzeit ist die Regierung nicht verfuegbar!");
                    }

                    break;
            }
        }
        private string replaceContent(string input)
        {
            input = input.Replace("\"", "");
            input = input.Replace("'", "");
            input = input.Replace("`", "");
            input = input.Replace("´", "");
            //return Regex.Replace(input, @"^[a-zA-Z0-9\s]+$", "");
            return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
        }
    }
}