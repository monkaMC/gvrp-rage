using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Stadthalle
{
    public class StadtHalleMenu : MenuBuilder
    {

        public StadtHalleMenu() : base(PlayerMenu.StadtHalleMenu)
        {

        }


        public override Menu.Menu Build(DbPlayer dbPlayer)
        {
            var menu = new Menu.Menu(Menu, "Stadthalle");

            menu.Add($"Schließen");
            menu.Add("Mietvertrag kündigen");
            menu.Add("Name ändern");
            menu.Add("Zufalls-Handynummer");
            menu.Add("Wunsch-Handynummer");

            if (dbPlayer.married[0] != 0)
            {
                menu.Add("Scheidung einreichen");
            }


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

                int kosten = 0;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    if(dbPlayer.IsTenant())
                    {
                        HouseRent tenant = dbPlayer.GetTenant();
                        dbPlayer.SendNewNotification($"Sie haben Ihren Mietvertag der Immobilie {tenant.HouseId} gekündigt!");
                        dbPlayer.RemoveTenant();
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Sie besitzen keinen aktiven Mietvertrag!");
                        return false;
                    }

                }
                else if (index == 2)
                {
                    //Name ändern
                    if (dbPlayer.Container.GetItemAmount(670) >= 1)
                    {
                        kosten = dbPlayer.Level * 10000;
                        ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Namensänderung beantragen | Kosten: ${kosten}", "Die Kosten belaufen sich auf 10.000$ pro Visumsstufe. Diese Entscheidung ist einmalig & endgültig. Bei erfolgreicher Namensänderung wirst du automatisch vom Server getrennt. Vergiss nicht deinen neuen Namen in den RageMP Launcher einzutragen! Vorname_Nachname und zwar auch hier mit einem Unterstrich. (Titel sind verboten!)", "NameChangeMarryConfirm", "", ""));
                    }
                    else
                    {
                        kosten = dbPlayer.Level * 50000;
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = $"Namensänderung beantragen | Kosten: ${kosten}", Callback = "NameChange",
                            Message = "Die Kosten sind 50.000$ * Visumsstufe Grundgebuehr, solltest du einen Doppelnamen wollen (z.B. Max_Mustermann-Klein >!Titel sind verboten!<) kostet der Doppelte Teil pro Buchstabe 150.000$ extra. " +
                            "Bei erfolgreicher Namensänderung wirst du automatisch vom Server getrennt. Vergiss nicht deinen neuen Namen in den RageMP-Launcher einzutragen! " +
                            "Vorname_Nachname und zwar auch hier mit einem Unterstrich." });
                    }

                }
                else if (index == 3)
                {
                    if(dbPlayer.LastPhoneNumberChange.AddMonths(StadthalleModule.PhoneNumberChangingMonths) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst deine Telefonnummer nur alle 4 Monate ändern!");
                        return false;
                    }

                    int money = 10000 * dbPlayer.Level;

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject()
                    {
                        Title = $"Zufalls Nummer kaufen",
                        Callback = "changePhoneNumberRandom",
                        Message = $"Du kannst hier eine Telefonnummer automatisch generieren lassen. Dies kostetn 10.000$ * Visumsstufe (In deinem Fall {money}$)," +
                        $" gib [KAUFEN] ein um eine neue Nummer zu beantragen, um abzubrechen nutze [ABBRECHEN]:"
                    });
                }
                else if (index == 4)
                {
                    if (dbPlayer.LastPhoneNumberChange.AddMonths(StadthalleModule.PhoneNumberChangingMonths) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst deine Telefonnummer nur alle 4 Monate ändern!");
                        return false;
                    }

                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject()
                    {
                        Title = $"Handynummer ändern",
                        Callback = "changePhoneNumber",
                        Message = "Du kannst eine Nummer auswählen zwischen 4-7 Stellen. Die Preise sind wie folgt: 5-7 Stellen jeweils 25.000$ * Visumsstufe. Reine 4 Stellige Nummern kosten 200.000$ * Visumsstufe. " +
                            "Gib bitte deine Wunschnummer ein:"
                    });

                }
                else if (index == 5)
                {
                    //Scheidung einreichen
                    if (dbPlayer.married[0] != 0)
                    {

                        string marryName = "";
                        int marryLevel = 0;
                        using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                        using (MySqlCommand cmd = conn.CreateCommand())
                        {
                            conn.Open();
                            cmd.CommandText = $"SELECT name, level FROM player WHERE id = '{dbPlayer.married[0]}' LIMIT 1";
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    if (reader.HasRows)
                                    {
                                        marryName = reader.GetString("name");
                                        marryLevel = reader.GetInt32("level");
                                    }
                                }
                                conn.Close();
                            }
                            
                            kosten = (dbPlayer.Level + marryLevel) * 40000 / 2;
                            ComponentManager.Get<ConfirmationWindow>().Show()(dbPlayer, new ConfirmationObject($"Scheidung beantragen | Kosten: ${kosten}", "Die Kosten belaufen sich auf (Visumsstufen der Eheleute) 40.000 $ * (Ehepartner1 + Ehepartner2) / 2 . Beispiel : Visumsstufe 1 und Visumsstufe 50 -> 40.000 $ * (1 + 50) / 2 = 1020000 $! Diese Entscheidung ist einmalig & endgültig.", "DivorceConfirm", "", ""));
                        }


                    }
                }
                return true;
            }
        }

    }
}

