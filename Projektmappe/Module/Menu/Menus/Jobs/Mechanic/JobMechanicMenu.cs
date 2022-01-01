
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Helper;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tuning;
using GVRP.Module.Vehicles;

namespace GVRP
{
    public class JobMechanmicMenuBuilder : MenuBuilder
    {
        public JobMechanmicMenuBuilder() : base(PlayerMenu.MechanicTune)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("tuneVeh")) return null;

            SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(iPlayer.GetData("tuneVeh"));
            if (sxVeh == null || !sxVeh.IsValid()) return null;

            iPlayer.SetData("isTuning", false);

            Dictionary<int, int> l_Dic = new Dictionary<int, int>();

            var menu = new Menu(Menu, $"Tuning");
            menu.Add(MSG.General.Close(), "");
            menu.Add("Anbringen", "");
            menu.Add("Standard", "");
            if (!iPlayer.TryData("tuneSlot", out int tuneSlot)) return menu;
            Console.WriteLine("TuneSlot: " + tuneSlot);
            
            Tuning tuning = Helper.m_Mods.Values.ToList().Where(tun => tun.ID == tuneSlot).FirstOrDefault();
            if (tuning == null) return menu;
            
            int i = 0;
            for (var l_Itr = tuning.StartIndex + 1; l_Itr <= tuning.MaxIndex; l_Itr++)
            {
                i++;
                menu.Add($"Teil {i.ToString()}", "");
                l_Dic.Add(l_Itr + 3, l_Itr);
            }
            
            iPlayer.SetData("tuningList", l_Dic);
            iPlayer.SetData("tuneIndex", 0);

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
                if (!iPlayer.HasData("tuneSlot"))
                {
                    iPlayer.SendNewNotification($"TuneSlot nicht gesetzt!");
                    return false;
                }

                if (!iPlayer.HasData("tuneVeh"))
                {
                    iPlayer.SendNewNotification($"TuneVeh nicht gesetzt!");
                    return false;
                }

                if (!iPlayer.HasData("tuneIndex"))
                {
                    iPlayer.SendNewNotification($"tuneIndex nicht gesetzt!");
                    return false;
                }

                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    // get vehicle
                    if (!iPlayer.HasData("tuneVeh"))
                        return;

                    // Irgendwie war die Data obwohl sie als uint gesetzt wurde, kein Uint mehr. Illuminati?
                    string l_DataString = iPlayer.GetData("tuneVeh").ToString();
                    if (!uint.TryParse(l_DataString, out uint l_VehID))
                        return;

                    SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(l_VehID);
                    if (sxVeh == null)
                    {
                        iPlayer.SendNewNotification($"Fehler bei der Fahrzeugauswahl!");
                        return;
                    }

                    if (!iPlayer.HasData("tuneSlot"))
                    {
                        iPlayer.SendNewNotification($"Async tuneSlot");
                        return;
                    }

                    if (iPlayer.GetData("isTuning"))
                        return;

                    int l_TuneSlot = (int)iPlayer.GetData("tuneSlot");
                    int l_TuneIndex = (int)iPlayer.GetData("tuneIndex");

                    Tuning tuning = Helper.m_Mods.Values.ToList().Where(tun => tun.ID == l_TuneSlot).FirstOrDefault();

                    if (tuning == null)
                    {
                        iPlayer.SendNewNotification("tuning not found!");
                        return;
                    }

                    if (index == 0)
                    {
                        sxVeh.SyncMods();
                        iPlayer.ResetData("tuneIndex");
                        iPlayer.ResetData("tuneSlot");
                        iPlayer.ResetData("tuneVeh");
                        MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.MechanicTune);
                        return;
                    }

                    if (index == 1)
                    {
                        MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.MechanicTune);

                        if(iPlayer.TeamRank < 10 && !sxVeh.InTuningProcess)
                        {
                            iPlayer.SendNewNotification("Fahrzeug ist nicht im Tuning Besitz!");
                            return;
                        }

                        int time = 60;
                        if(iPlayer.HasData("inTuning"))
                        {
                            iPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                            return;
                        }
                        iPlayer.SetData("inTuning", true);

                        Chats.sendProgressBar(iPlayer, (time * 1000));
                        
                        await Task.Delay(time * 1000);

                        sxVeh.AddSavedMod(l_TuneSlot, l_TuneIndex);
                        iPlayer.ResetData("tuneIndex");
                        iPlayer.ResetData("tuneSlot");
                        iPlayer.ResetData("isTuning");
                        iPlayer.ResetData("inTuning");
                        sxVeh.SyncMods();
                    }

                    if (index == 2)
                    {
                        iPlayer.SetData("isTuning", false);
                        iPlayer.SetData("tuneIndex", tuning.StartIndex);
                        sxVeh.SetMod(l_TuneSlot, tuning.StartIndex);
                    }

                    if (!iPlayer.HasData("tuningList"))
                        return;

                    Dictionary<int, int> l_Dic = iPlayer.GetData("tuningList");
                    if (l_Dic.ContainsKey(index))
                    {
                        iPlayer.SetData("isTuning", false);
                        iPlayer.SetData("tuneIndex", l_Dic[index]);
                        sxVeh.SetMod(l_TuneSlot, l_Dic[index]);
                    }
                }));

                return false;
            }
        }
    }
}
