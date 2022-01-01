using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tuning;
using GVRP.Module.Vehicles;

namespace GVRP.Module.LSCustoms
{
    public class LSCEvents : Script
    {
        [RemoteEvent]
        public void SetCarColorLSC(Client player, string returnstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
            {
                try
                {
                    var dbPlayer = player.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    if (!dbPlayer.HasData("tuneVeh")) return;

                    if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

                    string[] splittedReturn = returnstring.Split(" ");
                    if (splittedReturn.Length != 2) return;

                    if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
                    if (!Int32.TryParse(splittedReturn[1], out int color2)) return;

                    SxVehicle l_SxVehicle = VehicleHandler.Instance.GetAllVehicles().Where(x => x.databaseId == dbPlayer.GetData("tuneVeh")).FirstOrDefault();
                    if (l_SxVehicle == null || !l_SxVehicle.IsValid())
                        return;
                    
                    if (dbPlayer.HasData("inTuning"))
                    {
                        dbPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                        return;
                    }
                    dbPlayer.SetData("inTuning", true);

                    int time = 30;
                    if (Configurations.Configuration.Instance.DevMode)
                        time = 1;
                    Chats.sendProgressBar(dbPlayer, (time * 1000));
                    await Task.Delay(time * 1000);

                    l_SxVehicle.color1 = color1;
                    l_SxVehicle.color2 = color2;

                    l_SxVehicle.entity.PrimaryColor = color1;
                    l_SxVehicle.entity.SecondaryColor = color2;

                    var l_Query = $"UPDATE `vehicles` SET `color1`={color1.ToString()}, `color2`={color2.ToString()} WHERE `id`={l_SxVehicle.databaseId.ToString()};";
                    MySQLHandler.ExecuteAsync(l_Query);

                    dbPlayer.ResetData("inTuning");
                    dbPlayer.SendNewNotification($"Fahrzeugfarbe auf {color1} {color2} geändert!");
                    return;
                }
                catch(Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }));
        }
        [RemoteEvent]
        public void SetCarPearlLSC(Client player, string returnstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
            {
                try
                {
                    var dbPlayer = player.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    if (!dbPlayer.HasData("tuneVeh")) return;

                    if (!Int32.TryParse(returnstring, out int color1)) return;

                    SxVehicle l_SxVehicle = VehicleHandler.Instance.GetAllVehicles().Where(x => x.databaseId == dbPlayer.GetData("tuneVeh")).FirstOrDefault();
                    if (l_SxVehicle == null || !l_SxVehicle.IsValid())
                        return;

                    if (dbPlayer.HasData("inTuning"))
                    {
                        dbPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                        return;
                    }
                    dbPlayer.SetData("inTuning", true);

                    int time = 30;
                    Chats.sendProgressBar(dbPlayer, (time * 1000));
                    await Task.Delay(time * 1000);

                    l_SxVehicle.AddSavedMod(98, color1);
                    l_SxVehicle.SyncMods();
                    dbPlayer.ResetData("inTuning");
                    dbPlayer.SendNewNotification($"Perllack auf {color1} geändert!");
                    return;
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            })); 
        }
        [RemoteEvent]
        public void SetCarRimColorLSC(Client player, string returnstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
            {
                try
                {
                    var dbPlayer = player.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    if (!dbPlayer.HasData("tuneVeh")) return;

                    if (!Int32.TryParse(returnstring, out int color1)) return;

                    SxVehicle l_SxVehicle = VehicleHandler.Instance.GetAllVehicles().Where(x => x.databaseId == dbPlayer.GetData("tuneVeh")).FirstOrDefault();
                    if (l_SxVehicle == null || !l_SxVehicle.IsValid())
                        return;

                    if (dbPlayer.HasData("inTuning"))
                    {
                        dbPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                        return;
                    }
                    dbPlayer.SetData("inTuning", true);

                    int time = 30;
                    if (Configurations.Configuration.Instance.DevMode)
                        time = 1;
                    Chats.sendProgressBar(dbPlayer, (time * 1000));
                    await Task.Delay(time * 1000);

                    l_SxVehicle.AddSavedMod(99, color1);
                    l_SxVehicle.SyncMods();

                    dbPlayer.ResetData("inTuning");
                    dbPlayer.SendNewNotification($"Felgenfarbe auf {color1} geändert!");
                    return;
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }));
        }
        [RemoteEvent]
        public void SetCarTyreSmokeColorLSC(Client player, string returnstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(async () =>
            {
                try
                {
                    var dbPlayer = player.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid()) return;

                    if (!dbPlayer.HasData("tuneVeh")) return;

                    if (returnstring.Length < 3 || !returnstring.Contains(" ")) return;

                    string[] splittedReturn = returnstring.Split(" ");
                    if (splittedReturn.Length != 3) return;

                    if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
                    if (!Int32.TryParse(splittedReturn[1], out int color2)) return;
                    if (!Int32.TryParse(splittedReturn[2], out int color3)) return;

                    SxVehicle l_SxVehicle = VehicleHandler.Instance.GetAllVehicles().Where(x => x.databaseId == dbPlayer.GetData("tuneVeh")).FirstOrDefault();
                    if (l_SxVehicle == null || !l_SxVehicle.IsValid())
                        return;

                    if (dbPlayer.HasData("inTuning"))
                    {
                        dbPlayer.SendNewNotification("Sie bringen gerade ein Tuningteil an!");
                        return;
                    }
                    dbPlayer.SetData("inTuning", true);

                    int time = 30;
                    if (Configurations.Configuration.Instance.DevMode)
                        time = 1;
                    Chats.sendProgressBar(dbPlayer, (time * 1000));

                    if (color1 >= 255)
                        color1 = 255;
                    if (color2 >= 255)
                        color2 = 255;
                    if (color3 >= 255)
                        color3 = 255;
                    l_SxVehicle.AddSavedMod(95, color1);
                    await Task.Delay(3000);
                    l_SxVehicle.AddSavedMod(96, color2);
                    await Task.Delay(3000);
                    l_SxVehicle.AddSavedMod(97, color3);
                    l_SxVehicle.SyncMods();

                    dbPlayer.ResetData("inTuning");
                    dbPlayer.SendNewNotification($"Reifenrauchfarbe auf {color1} {color2} {color3} geändert!");
                    return;
                }
                catch (Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }));
        }
    }
}
