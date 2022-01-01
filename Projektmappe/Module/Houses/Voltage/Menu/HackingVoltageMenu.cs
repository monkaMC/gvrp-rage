using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Houses.Menu
{
    public class HackingVoltageMenuBuilder : MenuBuilder
    {
        public HackingVoltageMenuBuilder() : base(PlayerMenu.HackingVoltageMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Sicherheitssystem Hacking", "");

            menu.Add($"Schließen");
            menu.Add($"Paleto Police");
            menu.Add($"Mission Row Police");
            menu.Add($"Sandy Police");
            menu.Add($"Pier Police");
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
                // Close menu
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if(index == 1) // Paleto
                {
                    // Get first door
                    Door door = DoorModule.Instance.GetAll().Values.ToList().Where(d => d.Group == (int)DoorGroups.PaletoPolice).First();
                    if (door == null) return false;

                    // Check Range
                    if(dbPlayer.Player.Position.DistanceTo(door.Position) > 500)
                    {
                        dbPlayer.SendNewNotification($"Von hier aus kommen Sie nicht an das Stromnetz dran!");
                        return false;
                    }

                    // 653
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, 45000);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        await Task.Delay(45000);

                        dbPlayer.SetData("userCannotInterrupt", false);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        dbPlayer.StopAnimation();

                        if (dbPlayer.Container.GetItemAmount(653) <= 0) return;
                        dbPlayer.Container.RemoveItem(653);

                        foreach(KeyValuePair<uint, Door> kvp in DoorModule.Instance.GetAll().Where(d => d.Value.Group == (int)DoorGroups.PaletoPolice))
                        {
                            kvp.Value.LessSecurity = true;
                            kvp.Value.LessSecurityChanged = DateTime.Now;
                        }

                        dbPlayer.SendNewNotification($"Die Sicherheitssysteme wurden deaktiviert und lösen nun später den Alarm aus!");
                        dbPlayer.SendNewNotification($"Achtung es kann jederzeit wieder hochfahren!");
                    }));

                    return true;
                }

                else if (index == 2) // Mission Row
                {
                    // Get first door
                    Door door = DoorModule.Instance.GetAll().Values.ToList().Where(d => d.Group == (int)DoorGroups.MissionRowPolice).First();
                    if (door == null) return false;

                    // Check Range
                    if (dbPlayer.Player.Position.DistanceTo(door.Position) > 500)
                    {
                        dbPlayer.SendNewNotification($"Von hier aus kommen Sie nicht an das Stromnetz dran!");
                        return false;
                    }

                    // 653
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, 45000);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        await Task.Delay(45000);

                        dbPlayer.SetData("userCannotInterrupt", false);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        dbPlayer.StopAnimation();

                        if (dbPlayer.Container.GetItemAmount(653) <= 0) return;
                        dbPlayer.Container.RemoveItem(653);

                        foreach (KeyValuePair<uint, Door> kvp in DoorModule.Instance.GetAll().Where(d => d.Value.Group == (int)DoorGroups.MissionRowPolice))
                        {
                            kvp.Value.LessSecurity = true;
                            kvp.Value.LessSecurityChanged = DateTime.Now;
                        }

                        dbPlayer.SendNewNotification($"Die Sicherheitssysteme wurden deaktiviert und lösen nun später den Alarm aus!");
                        dbPlayer.SendNewNotification($"Achtung es kann jederzeit wieder hochfahren!");
                    }));

                    return true;
                }

                else if (index == 3) // Sandy
                {
                    // Get first door
                    Door door = DoorModule.Instance.GetAll().Values.ToList().Where(d => d.Group == (int)DoorGroups.SandyPolice).First();
                    if (door == null) return false;

                    // Check Range
                    if (dbPlayer.Player.Position.DistanceTo(door.Position) > 500)
                    {
                        dbPlayer.SendNewNotification($"Von hier aus kommen Sie nicht an das Stromnetz dran!");
                        return false;
                    }

                    // 653
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, 45000);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        await Task.Delay(45000);

                        dbPlayer.SetData("userCannotInterrupt", false);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        dbPlayer.StopAnimation();

                        if (dbPlayer.Container.GetItemAmount(653) <= 0) return;
                        dbPlayer.Container.RemoveItem(653);

                        foreach (KeyValuePair<uint, Door> kvp in DoorModule.Instance.GetAll().Where(d => d.Value.Group == (int)DoorGroups.SandyPolice))
                        {
                            kvp.Value.LessSecurity = true;
                            kvp.Value.LessSecurityChanged = DateTime.Now;
                        }

                        dbPlayer.SendNewNotification($"Die Sicherheitssysteme wurden deaktiviert und lösen nun später den Alarm aus!");
                        dbPlayer.SendNewNotification($"Achtung es kann jederzeit wieder hochfahren!");
                    }));


                    return true;
                }
                else if (index == 4) // Pier
                {
                    // Get first door
                    Door door = DoorModule.Instance.GetAll().Values.ToList().Where(d => d.Group == (int)DoorGroups.PierPolice).First();
                    if (door == null) return false;

                    // Check Range
                    if (dbPlayer.Player.Position.DistanceTo(door.Position) > 500)
                    {
                        dbPlayer.SendNewNotification($"Von hier aus kommen Sie nicht an das Stromnetz dran!");
                        return false;
                    }

                    // 653
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        Chats.sendProgressBar(dbPlayer, 45000);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.SetData("userCannotInterrupt", true);

                        dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                        await Task.Delay(45000);

                        dbPlayer.SetData("userCannotInterrupt", false);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.ResetData("userCannotInterrupt");
                        dbPlayer.StopAnimation();

                        if (dbPlayer.Container.GetItemAmount(653) <= 0) return;
                        dbPlayer.Container.RemoveItem(653);

                        foreach (KeyValuePair<uint, Door> kvp in DoorModule.Instance.GetAll().Where(d => d.Value.Group == (int)DoorGroups.PierPolice))
                        {
                            kvp.Value.LessSecurity = true;
                            kvp.Value.LessSecurityChanged = DateTime.Now;
                        }

                        dbPlayer.SendNewNotification($"Die Sicherheitssysteme wurden deaktiviert und lösen nun später den Alarm aus!");
                        dbPlayer.SendNewNotification($"Achtung es kann jederzeit wieder hochfahren!");
                    }));

                    return true;
                }
                return false;
            }
        }
    }
}
