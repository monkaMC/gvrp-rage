using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Attachments;
using GVRP.Module.Banks;
using GVRP.Module.Business;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.Business.NightClubs;
using GVRP.Module.Business.Raffinery;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes;
using GVRP.Module.Commands;
using GVRP.Module.Configurations;
using GVRP.Module.Customization;
using GVRP.Module.Dealer;
using GVRP.Module.Gangwar;
using GVRP.Module.Guenther;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Jails;
using GVRP.Module.Kasino;
using GVRP.Module.Laboratories;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Node;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Commands;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Drunk;
using GVRP.Module.Players.Events;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Players.Ranks;
using GVRP.Module.Players.Windows;
using GVRP.Module.Spawners;
using GVRP.Module.Storage;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Permission;
using GVRP.Module.Time;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.Garages;
using GVRP.Module.Voice;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Zone;
using static GVRP.Module.Chat.Chats;
using static GVRP.Module.Players.PlayerNotification;
using Configuration = GVRP.Module.Configurations.Configuration;
using Task = System.Threading.Tasks.Task;
using Vehicle = GTANetworkAPI.Vehicle;

namespace GVRP.Module.Admin
{
    public class AdminModuleCommands : Script
    {
        private static readonly bool Devmode = Configuration.Instance.DevMode;


        public int getTeamColor(int rankId)
        {
            switch (rankId)
            {
                case 1: return 0;
                case 2: return 2;
                case 3: return 4;
                case 4: return 3;
                case 5:
                case 6: return 9;
                case 12:
                case 8: return 10;

                default: return 0;

            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vdoor(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.Player.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }

            var l_Vehicle = p_Player.Vehicle;
         //   var l_Handler = new VehicleEventHandler();
         //   l_Handler.ToggleDoorState(p_Player, l_Vehicle, Convert.ToUInt32(p_Door));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void avoice(Client player, string args)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var findPlayer = Players.Players.Instance.FindPlayer(args, true);
            if (findPlayer == null || findPlayer.isAlive()) return;
            Console.WriteLine("Player found: " + findPlayer.GetName());

            if(findPlayer.Id == dbPlayer.Id)
            {
                dbPlayer.SendNewNotification("Sich selber abhören macht keinen Sinn, du Fuchs.");
                return;
            }

            string voiceHashPush = findPlayer.VoiceHash;
            dbPlayer.SetData("adminHearing", voiceHashPush);
            dbPlayer.Player.TriggerEvent("setAdminVoice", voiceHashPush);
            dbPlayer.SendNewNotification($"Abhören begonnen! Spieler: {findPlayer.GetName()}");
        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void endavoice(Client player, string args)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (dbPlayer.HasData("adminHearing"))
            {
                dbPlayer.Player.TriggerEvent("clearAdminVoice");
                dbPlayer.ResetData("adminHearing");
                dbPlayer.SendNewNotification("Abhören beendet!");
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadcheckpoint(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.Player.TriggerEvent("loadcheckpoint");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unloadcheckpoint(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.Player.TriggerEvent("unloadcheckpoint");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadcrate(Client p_Player, string args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 4) return;
            if (!int.TryParse(strings[0], out int size)) return;
            if (!int.TryParse(strings[1], out int number)) return;
            if (!int.TryParse(strings[2], out int weapon)) return;
            if (!int.TryParse(strings[3], out int ramp)) return;

            if (number < 0 || number > 6) return;
            if (size < 1 || size > 4)
                l_DbPlayer.SendNewNotification($"Event triggert: {number}");
            l_DbPlayer.SendNewNotification($"Dimension: {l_DbPlayer.Player.Dimension}");
            l_DbPlayer.Player.TriggerEvent("loadStorageRoom", size, number, weapon, ramp, l_DbPlayer.Player.Dimension);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unloadcrate(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            l_DbPlayer.Player.TriggerEvent("unloadStorageRoom");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void carmod(Client player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.Player.TriggerEvent("carmod", sxVeh.entity, strings[0], strings[1]);
            }
        }

        public static List<Blip> labblips = new List<Blip>();
        public static Marker innerMarker;
        public static Marker outerMarker;
        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void marker(Client player, string args)
        {
            if (!Configuration.Instance.DevMode)
                return;

            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;

            if (!int.TryParse(strings[0], out int min)) return;
            if (!int.TryParse(strings[1], out int max)) return;

            if (labblips.Count > 0)
                labblips.ForEach(blip => blip.Delete());
            labblips.Clear();
            //innerMarker = NAPI.Marker.CreateMarker(1, dbPlayer.Player.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), min, new Color(255, 0, 0), dimension: 0);
            //outerMarker = NAPI.Marker.CreateMarker(1, dbPlayer.Player.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), max, new Color(0, 255, 0), dimension: 0);
            for (int i = 0; i < 360; i += 6)
            {
                double angle = Math.PI * i / 180.0;
                double sinAngle = Math.Sin(angle);
                double cosAngle = Math.Cos(angle);
                Vector3 innerPos = dbPlayer.Player.Position.Add(new Vector3(min * cosAngle, min * sinAngle, 0));
                Vector3 outerPos = dbPlayer.Player.Position.Add(new Vector3(max * cosAngle, max * sinAngle, 0));
                if (i % 18 == 0)
                    labblips.Add(Blips.Create(innerPos, "", 103, 1.0f, true, 49, 255));
                labblips.Add(Blips.Create(outerPos, "", 103, 1.0f, true, 69, 255));
            }
        }

        public static List<Blip> methBlips = new List<Blip>();
        public static List<Marker> methMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadplanningroom(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 6) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.Player.TriggerEvent("loadplanningroom", strings[0], strings[1], strings[2], strings[3], strings[4], strings[5]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void unloadplanningroom(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.Player.TriggerEvent("unloadplanningroom");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showlabs(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            if (!int.TryParse(myString, out int scale)) return;
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (methBlips.Count > 0)
            {
                methBlips.ForEach(blip => blip.Delete());
                if (methMarker.Count > 0)
                    methMarker.ForEach(marker => marker.Delete());
                methBlips.Clear();
                methMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `jump_points` WHERE `destionation` in ('287','293','309','315','321','327','333','339','345','351','357') order by `teams`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    int teamId = reader.GetInt32("teams");
                                    NAPI.Task.Run(async () =>
                                    {
                                        methBlips.Add(Blips.Create(pos, $"Labor - TeamId: {teamId}", 403, 1.0f, true, TeamModule.Instance.GetById(teamId).BlipColor, 255));
                                        methMarker.Add(Markers.Create(1, pos.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotostorage(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint id)) return;
            StorageRoom storageRoom = StorageRoomModule.Instance.Get(id);

            l_DbPlayer.Player.SetPosition(storageRoom.Position);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void dstorage(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            StorageRoomModule.Instance.GetAll().Values.ToList().ForEach(room =>
            {
                StorageRoomModule.Instance.GetAll().Values.ToList().ForEach(innerRoom =>
                {
                    if (room.Id != innerRoom.Id && room.Position.DistanceTo(innerRoom.Position) <= 2.0f)
                    {
                        Console.WriteLine($"StorageRoom Problem! {room.Id} & {innerRoom.Id} are too close!");
                    }
                });
            });
        }

        public static List<Blip> storageRoomBlips = new List<Blip>();
        public static List<Marker> storageRoomMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showstorages(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!int.TryParse(myString, out int scale)) return;
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (storageRoomBlips.Count > 0)
            {
                storageRoomBlips.ForEach(blip => blip.Delete());
                if (storageRoomMarker.Count > 0)
                    storageRoomMarker.ForEach(marker => marker.Delete());
                storageRoomBlips.Clear();
                storageRoomMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `storage_rooms`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    NAPI.Task.Run(() =>
                                    {
                                        storageRoomBlips.Add(Blips.Create(pos, $"StorageRoom", 478, 1.0f, true, 1, 255));
                                        storageRoomMarker.Add(Markers.Create(1, pos.Add(new Vector3(0, 0, -1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static List<Blip> gangwarBlips = new List<Blip>();
        public static List<Marker> gangwarMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showgangwar(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = (0.1f));

            if (gangwarBlips.Count > 0)
            {
                gangwarBlips.ForEach(blip => blip.Delete());
                if (gangwarMarker.Count > 0)
                    gangwarMarker.ForEach(marker => marker.Delete());
                gangwarBlips.Clear();
                gangwarMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `gangwar_towns`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    int id = reader.GetInt32("id");
                                    Vector3 Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    Vector3 Flag_1 = new Vector3(reader.GetFloat("flag_1_pos_x"), reader.GetFloat("flag_1_pos_y"), reader.GetFloat("flag_1_pos_z"));
                                    Vector3 Flag_2 = new Vector3(reader.GetFloat("flag_2_pos_x"), reader.GetFloat("flag_2_pos_y"), reader.GetFloat("flag_2_pos_z"));
                                    Vector3 Flag_3 = new Vector3(reader.GetFloat("flag_3_pos_x"), reader.GetFloat("flag_3_pos_y"), reader.GetFloat("flag_3_pos_z"));
                                    float Range = reader.GetFloat("range");
                                    Color color = GangwarModule.Instance.StandardColor;
                                    NAPI.Task.Run(() =>
                                    {
                                        gangwarBlips.Add(Blips.Create(Position, "Gangwargebiet", 543, 1.0f, color: 0));
                                        gangwarBlips.Add(Blips.Create(Flag_1, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarBlips.Add(Blips.Create(Flag_2, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarBlips.Add(Blips.Create(Flag_3, "Gangwargebiet", 38, 1.0f, color: 1));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_1, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_2, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(Spawners.Markers.Create(4, Flag_3, new Vector3(), new Vector3(), 1.0f, color.Alpha, color.Red, color.Green, color.Blue, 0));
                                        gangwarMarker.Add(NAPI.Marker.CreateMarker(1, Position.Add(new Vector3(0,0,-30f)), new Vector3(), new Vector3(), 2 * Range, color, true, 0));
                                        gangwarMarker.Add(NAPI.Marker.CreateMarker(0, Position, new Vector3(), new Vector3(), 2.0f, new Color(255,0,0), true, 0));
                                        for (int i = 0; i < 360; i += 6)
                                        {
                                            double angle = Math.PI * i / 180.0;
                                            double sinAngle = Math.Sin(angle);
                                            double cosAngle = Math.Cos(angle);
                                            Vector3 innerPos = Position.Add(new Vector3(Range * cosAngle, Range * sinAngle, 0));
                                            labblips.Add(Blips.Create(innerPos, "", 103, 1.0f, true, 2, 255));
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void dhouse(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            HouseModule.Instance.GetAll().Values.ToList().ForEach(house =>
            {
                HouseModule.Instance.GetAll().Values.ToList().ForEach(innerHouse =>
                {
                    if (house.Id != innerHouse.Id && house.Position.DistanceTo(innerHouse.Position) <= 1.5f)
                    {
                        Console.WriteLine($"House Problem! {house.Id} & {innerHouse.Id} are too close!");
                    }
                });
            });
        }

        public static List<Blip> houseBlips = new List<Blip>();
        public static List<Marker> houseMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showhouses(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = myString.Split(" ");
            int scale = 10;
            int markerIndex = 0;
            if (strings.Length >= 1)
                int.TryParse(strings[0], out scale);
            if (strings.Length == 2)
                int.TryParse(strings[1], out markerIndex);
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (houseBlips.Count > 0)
            {
                houseBlips.ForEach(blip => blip.Delete());
                if (houseMarker.Count > 0)
                    houseMarker.ForEach(marker => marker.Delete());
                houseBlips.Clear();
                houseMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `houses`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("posX"), reader.GetFloat("posY"), reader.GetFloat("posZ"));
                                    Vector3 colShapePos = new Vector3(reader.GetFloat("colshapeX"), reader.GetFloat("colshapeY"), reader.GetFloat("colshapeZ"));
                                    int id = reader.GetInt32("id");
                                    int type = reader.GetInt32("type");
                                    int price = reader.GetInt32("price");
                                    int ownerId = reader.GetInt32("ownerId");
                                    int maxrents = reader.GetInt32("maxrents");
                                    int garage = reader.GetInt32("garage");
                                    int BlipColor = 77;
                                    switch (type)
                                    {
                                        case 1:
                                            BlipColor = 1;
                                            break;
                                        case 2:
                                            BlipColor = 2;
                                            break;
                                        case 3:
                                            BlipColor = 5;
                                            break;
                                    }

                                    NAPI.Task.Run(() =>
                                    {
                                        houseBlips.Add(Blips.Create(pos, $"House: Id ({id}), type ({type}), price ({price}, maxrents ({maxrents}), garage ({maxrents})", 40, 1.0f, true, BlipColor, 255));
                                        if (markerIndex == 0)
                                        {
                                            if (l_DbPlayer.Player.Position.DistanceTo(pos) < 250.0f)
                                            {
                                                houseMarker.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                                houseMarker.Add(Markers.Create(1, colShapePos.Add(new Vector3(0, 0, 0.1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));

                                            }
                                        }
                                        else if (id >= markerIndex)
                                        {
                                            houseMarker.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));
                                            houseMarker.Add(Markers.Create(1, colShapePos.Add(new Vector3(0, 0, 0.1f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static List<Blip> farmspotsBlips = new List<Blip>();
        public static List<Marker> farmspotsMarker = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void showfarmspots(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = myString.Split(" ");
            int scale = 10;
            int markerIndex = 0;
            if (strings.Length >= 1)
                int.TryParse(strings[0], out markerIndex);
            if (Main.ServerBlips.Count > 0)
                Main.ServerBlips.ForEach(blip => blip.Scale = ((float)scale) / 100);

            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            if (farmspotsBlips.Count > 0)
            {
                farmspotsBlips.ForEach(blip => blip.Delete());
                if (farmspotsMarker.Count > 0)
                    farmspotsMarker.ForEach(marker => marker.Delete());
                farmspotsBlips.Clear();
                farmspotsMarker.Clear();
                return;
            }
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    string query =
                        $"SELECT * FROM `farm_positions`;";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    Vector3 pos = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
                                    int id = reader.GetInt32("id");
                                    int farm_id = reader.GetInt32("farm_id");
                                    int range = reader.GetInt32("range");
                                    uint BlipType = 685 + (uint)farm_id;

                                    NAPI.Task.Run(() =>
                                    {
                                        farmspotsBlips.Add(Blips.Create(pos, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255));
                                        if (farm_id == markerIndex)
                                        {
                                            farmspotsMarker.Add(Markers.Create(1, pos.Add(new Vector3(0,0,10f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 3.0f, 255, 255, 0, 0, 0));
                                            for (double i = 0; i < 360; i += 90)
                                            {
                                                double angle = Math.PI * i / 180.0;
                                                double sinAngle = Math.Sin(angle);
                                                double cosAngle = Math.Cos(angle);
                                                Vector3 innerPos = pos.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                                                farmspotsMarker.Add(Markers.Create(1, innerPos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 5.0f, 255, 0, 255, 0, 0));
                                            }
                                        }
                                    });
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        public static Blip previewBlip;
        public static List<Marker> previewMarkers = new List<Marker>();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void previewfarmspot(Client p_Player, string args)
        {
            if (!Configuration.Instance.DevMode)
                return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            string[] strings = args.Split(" ");
            if (!int.TryParse(strings[0], out int farm_id)) return;
            if (!int.TryParse(strings[1], out int range)) return;
            uint BlipType = 685 + (uint)farm_id;

            if(previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            Vector3 pos;
            if (l_DbPlayer.HasData("mark"))
            {
                pos = l_DbPlayer.GetData("mark");
            }
            else
            {
                pos = l_DbPlayer.Player.Position;
            }

            NAPI.Task.Run(() =>
            {
                previewBlip = Blips.Create(pos, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255);
                previewMarkers.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 2.0f, 255, 255, 0, 0, 0));
                for (double i = 0; i < 360; i += (360/range))
                {
                    double angle = Math.PI * i / 180.0;
                    double sinAngle = Math.Sin(angle);
                    double cosAngle = Math.Cos(angle);
                    Vector3 innerPos = pos.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                    previewMarkers.Add(Markers.Create(1, innerPos.Add(new Vector3(0,0,2f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                }
            });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void previewjails(Client p_Player)
        {
            if (!Configuration.Instance.DevMode)
                return;

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
           
            uint BlipType = 685;

            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());
            
            foreach(JailCell jail in JailCellModule.Instance.GetAll().Values)
            {
                NAPI.Task.Run(() =>
                {
                    Vector3 pos = jail.Position;

                    previewMarkers.Add(Markers.Create(1, pos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 2.0f, 255, 255, 0, 0, 0));
                    for (double i = 0; i < 360; i += (360 / jail.Range))
                    {
                        double angle = Math.PI * i / 180.0;
                        double sinAngle = Math.Sin(angle);
                        double cosAngle = Math.Cos(angle);
                        Vector3 innerPos = pos.Add(new Vector3(jail.Range * cosAngle, jail.Range * sinAngle, 0));
                        previewMarkers.Add(Markers.Create(1, innerPos.Add(new Vector3(0, 0, 2f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 0, 255, 0, 0));
                    }
                });
            }
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createshop(Client p_Player)
        {

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `shops` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `ped_hash`, `deliver_pos_x`, `deliver_pos_y`, `deliver_pos_z`, `schwarzgelduse`, `marker`, `team`, `rob_pos_x`, `rob_pos_y`, `rob_pos_z`)" +
                $"VALUES ('Supermarkt', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '-1950698411', '9999', '9999', '9999', '0', '1', '', '9999', '9999', '9999');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void pos(Client p_Player, String args)
        {

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] command = args.Split(" ");


                l_DbPlayer.SendNewNotification("Erfolgreich!");
            p_Player.Position = new Vector3(-1158.62, -2007.49, 13.1803);
               

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createshopitem(Client p_Player, String args)
        {

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] command = args.Split(" ");

            int i = 0;

            while (i < 21)
            {
                l_DbPlayer.SendNewNotification("Erfolgreich erstellt");
                string query = $"INSERT INTO `shops_items` (`shop_id`, `item_id`)" +
                $"VALUES ('{i}', '{command[0]}');";
                Console.WriteLine(query);
                MySQLHandler.ExecuteAsync(query);
                i = i + 1;
            }

        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void creategas(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `business_fuelstations_gas` (`fuelstation_id`, `pos_x`, `pos_y`, `pos_z`)" +
                $"VALUES ('{args}', '{pos_x}', '{pos_y}', '{pos_z}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createfuel(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `business_fuelstations` (`name`, `price`, `buy_price`, `pos_x`, `pos_y`, `pos_z`, `infocard_x`, `infocard_y`, `infocard_z`, `range`)" +
                $"VALUES ('Tankstelle {args}', '25', '1200000', '{pos_x}', '{pos_y}', '{pos_z}', '{pos_x}', '{pos_y}', '{pos_z}', '25');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [CommandPermission(PlayerRankPermission = true)]

        [Command]
        public void way(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            p_Player.TriggerEvent("way");
            l_DbPlayer.SendNewNotification("HurensohN!");

        }

        [Command]
        public void createpump(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `barber_shops` (`buy_price`, `pos_x`, `pos_y`, `pos_z`, `ausbaustufe`)" +
                $"VALUES ('1500000', '{pos_x}', '{pos_y}', '{pos_z}', '1');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [Command]
        public void createblip(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            string[] strings = args.Split(" ");

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `point_of_interest` (`name`, `description`, `category_id`, `pos_x`, `pos_y`, `blip`, `blip_color`)" +
                $"VALUES ('{strings[0]}', ' ', '{strings[1]}', '{pos_x}', '{pos_y}', '{strings[2]}', '{strings[3]}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
            string query2 = $"INSERT INTO `point_of_interest_category` (`name`, `description`, `category_id`, `pos_x`, `pos_y`, `pos_z`, `blip`, `blip_color`)" +
    $"VALUES ('{strings[0]}', ' ', '{strings[1]}', '{pos_x}', '{pos_y}', '{pos_z}', '{strings[2]}', '{strings[3]}');";
            Console.WriteLine(query2);
            MySQLHandler.ExecuteAsync(query2);
        }
        [Command]
        public void createnightclub(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            string[] strings = args.Split(" ");

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `business_nightclubs` (`name`, `price`, `pos_x`, `pos_y`, `pos_z`, `float`, `garage_pos_x`, `garage_pos_y`, `garage_pos_z`, `garage_float`)" +
                $"VALUES ('{args}', '160000000', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '-1634.354', '-3001.627', '-78.14376', '274.7817');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void createbarber(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `barber_shops` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`)" +
                $"VALUES ('Friseur', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void createchair(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `barber_shop_chairs` (`barber_shop_id`, `pos_x`, `pos_y`, `pos_z`, `heading`)" +
                $"VALUES ('{args}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void createatm(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `bank` (`name`, `pos_x`, `pos_y`, `pos_z`, `actual_money`, `max_money`, `type`)" +
                $"VALUES ('ATM', '{pos_x}', '{pos_y}', '{pos_z}', '-1', '-1', '0');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void creategarage(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `garages` (`name`, `npc_pos_x`, `npc_pos_y`, `npc_pos_z`, `npc_heading`)" +
                $"VALUES ('Garage', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void createpoint(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `garages_spawns` (`garage_id`, `pos_x`, `pos_y`, `pos_z`, `heading`)" +
                $"VALUES ('{args}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [Command]
        public void haircolor(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }


            l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

            string query = $"INSERT INTO `assets_hair` (`name`, `price`, `gender`, `shop_id`)" +
                $"VALUES ('{args}', '250', '1', '1');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [Command]
        public void cloth(Client p_Player, String args)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");

            if (!int.TryParse(strings[0], out int slots)) return;
            if (!int.TryParse(strings[1], out int component)) return;
            if (!int.TryParse(strings[2], out int texture)) return;
            Console.WriteLine(strings[0] + " " + strings[1] + " " + strings[2]);
            int i = 0;

            if (texture == 0)
            {
                l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

                string query = $"INSERT INTO `clothes` (`name`, `slot`, `variation`, `texture`, `teamId`)" +
                    $"VALUES ({slots}, '{slots}', '{component}', '{i}', '0');";
                Console.WriteLine(query);
                MySQLHandler.ExecuteAsync(query);
            } else
            {
                texture = texture + 1;
            }

            while (i < texture)
            {
                l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

                string query = $"INSERT INTO `clothes` (`name`, `slot`, `variation`, `texture`, `teamId`)" +
                    $"VALUES ({slots}, '{slots}', '{component}', '{i}', '0');";
                Console.WriteLine(query);
                MySQLHandler.ExecuteAsync(query);

                i = i + 1;

            }
        }
        [Command]
        public void addhair(Client p_Player)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            int i = 1;

            while (i < 16) {
                l_DbPlayer.SendNewNotification("Erfolgreich erstellt");

                string query = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
                    $"VALUES ('Brusthaare', '{i}', '200', '1');";
                Console.WriteLine(query);
                MySQLHandler.ExecuteAsync(query);
                string query2 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
    $"VALUES ('Brusthaare', '{i}', '200', '2');";
                Console.WriteLine(query2);
                MySQLHandler.ExecuteAsync(query2);
                string query3 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
$"VALUES ('Brusthaare', '{i}', '200', '3');";
                Console.WriteLine(query3);
                MySQLHandler.ExecuteAsync(query3);
                string query4 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
$"VALUES ('Brusthaare', '{i}', '200', '4');";
                Console.WriteLine(query4);
                MySQLHandler.ExecuteAsync(query4);
                string query5 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
$"VALUES ('Brusthaare', '{i}', '200', '5');";
                Console.WriteLine(query5);
                MySQLHandler.ExecuteAsync(query5);
                string query6 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
$"VALUES ('Brusthaare', '{i}', '200', '6');";
                Console.WriteLine(query6);
                MySQLHandler.ExecuteAsync(query6);
                string query7 = $"INSERT INTO `assets_chest` (`name`, `customisation_id`, `price`, `shop_id`)" +
$"VALUES ('Brusthaare', '{i}', '200', '7');";
                Console.WriteLine(query7);
                MySQLHandler.ExecuteAsync(query7);
                i = i + 1;

            }
        }







    [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createfarmspot(Client p_Player, string args)
        {

            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (!int.TryParse(strings[0], out int farm_id)) return;
            if (!int.TryParse(strings[1], out int range)) return;
            uint BlipType = 685 + (uint)farm_id;


            if (previewBlip != null)
                previewBlip.Delete();
            if (previewMarkers.Count > 0)
                previewMarkers.ForEach(marker => marker.Delete());

            NAPI.Task.Run(() =>
            {
                farmspotsBlips.Add(Blips.Create(l_DbPlayer.Player.Position, $"FarmSpot-ID: {farm_id}", BlipType, 1.0f, true, 2, 255));
                farmspotsMarker.Add(Markers.Create(1, l_DbPlayer.Player.Position.Add(new Vector3(0, 0, 10f)), new Vector3(0, 0, 0), new Vector3(0, 0, 0), 3.0f, 255, 255, 0, 0, 0));
                for (double i = 0; i < 360; i += 90)
                {
                    double angle = Math.PI * i / 180.0;
                    double sinAngle = Math.Sin(angle);
                    double cosAngle = Math.Cos(angle);
                    Vector3 innerPos = l_DbPlayer.Player.Position.Add(new Vector3(range * cosAngle, range * sinAngle, 0));
                    farmspotsMarker.Add(Markers.Create(1, innerPos, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 5.0f, 255, 0, 255, 0, 0));
                }
            });

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `farm_positions` (`farm_id`, `pos_x`, `pos_y`, `pos_z`, `range`)" +
                $"VALUES ('{farm_id}', '{pos_x}', '{pos_y}', '{pos_z}', '{range}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createplanningroom(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();

            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint teamId)) return;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null) return;

            string teamName = TeamModule.Instance.GetById((int)teamId).Name;

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");

            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Eingang - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '0', '{teamId}', '1.2', '1', '1', '1');";

            string query2 = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Ausgang - {team.Name}', '2737.84', '-374.079', '-47.993', '186.073', '{teamId}', '0', '{teamId}', '1.2', '1', '1', '1');";

            MySQLHandler.ExecuteAsync(query);
            MySQLHandler.ExecuteAsync(query2);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createplanningroomveh(Client p_Player, string myString)
        {
            if (!Configuration.Instance.DevMode) return;

            var l_DbPlayer = p_Player.GetPlayer();

            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!uint.TryParse(myString, out uint teamId)) return;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null) return;

            string teamName = TeamModule.Instance.GetById((int)teamId).Name;

            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");

            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `inside_vehicle`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Fahrzeug Eingang - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '0', '{teamId}', '1', '3', '1', '1', '1');";

            string query2 = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `inside_vehicle`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Planningroom Fahrzeug Ausgang - {team.Name}', '2681.15', '-361.22', '-55.6107', '271.308', '{teamId}', '0', '{teamId}', '1', '3', '1', '1', '1');";

            MySQLHandler.ExecuteAsync(query);
            MySQLHandler.ExecuteAsync(query2);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createweedlab(Client p_Player, string myString)
        {
            //    if (!Configuration.Instance.DevMode)
            //          return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            //    if (!Configuration.Instance.DevMode) return;
            if (!uint.TryParse(myString, out uint teamId)) return;

            int destinationId = CannabislaboratoryModule.Instance.GetLaboratoryByTeamId(teamId).JumpPointEingang.DestinationId;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null)
                return;
            methBlips.Add(Blips.Create(l_DbPlayer.Player.Position, $"Labor - TeamId: {teamId}", 403, 1.0f, true, TeamModule.Instance.GetById((int)teamId).BlipColor, 255));
            methMarker.Add(Markers.Create(1, l_DbPlayer.Player.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));

            int number = methBlips.Where(blip => blip.Color == TeamModule.Instance.GetById((int)teamId).BlipColor).Count();
            string teamName = TeamModule.Instance.GetById((int)teamId).Name;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification($"{teamName}: {number}");
            }


            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Weedlab - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '{destinationId}', '{teamId}', '3', '1', '1', '1');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createlab(Client p_Player, string myString)
        {
           if (!Configuration.Instance.DevMode)
               return;
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.DevMode) return;
            if (!uint.TryParse(myString, out uint teamId)) return;

            int destinationId = MethlaboratoryModule.Instance.GetLaboratoryByTeamId(teamId).JumpPointEingang.DestinationId;

            Team team = TeamModule.Instance.GetById((int)teamId);
            if (team == null)
                return;
            methBlips.Add(Blips.Create(l_DbPlayer.Player.Position, $"Labor - TeamId: {teamId}", 403, 1.0f, true, TeamModule.Instance.GetById((int)teamId).BlipColor, 255));
            methMarker.Add(Markers.Create(1, l_DbPlayer.Player.Position, new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1.0f, 255, 255, 0, 0, 0));

            int number = methBlips.Where(blip => blip.Color == TeamModule.Instance.GetById((int)teamId).BlipColor).Count();
            string teamName = TeamModule.Instance.GetById((int)teamId).Name;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification($"{teamName}: {number}");
            }


            string pos_x = l_DbPlayer.Player.Position.X.ToString().Replace(",", ".");
            string pos_y = l_DbPlayer.Player.Position.Y.ToString().Replace(",", ".");
            string pos_z = l_DbPlayer.Player.Position.Z.ToString().Replace(",", ".");
            string heading = l_DbPlayer.Player.Rotation.Z.ToString().Replace(",", ".");
            string query = $"INSERT INTO `jump_points` (`name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `dimension`, `destionation`, `teams`, `range`, `locked`, `unbreakable`, `hide_infos`)" +
                $"VALUES ('Methlaboratory - {team.Name}', '{pos_x}', '{pos_y}', '{pos_z}', '{heading}', '0', '{destinationId}', '{teamId}', '3', '1', '1', '1');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehh(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.Player.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }

            var l_Vehicle = p_Player.Vehicle;
            if (float.TryParse(p_Door, out float value))
            {
                NAPI.Vehicle.SetVehicleHealth(l_Vehicle, value);
            }
            else
            {
                l_DbPlayer.SendNewNotification($"VEH HEALTH {NAPI.Vehicle.GetVehicleHealth(l_Vehicle)}");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehbodyh(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.Player.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }
            var l_Vehicle = p_Player.Vehicle;
            if (float.TryParse(p_Door, out float value))
            {
                NAPI.Vehicle.SetVehicleBodyHealth(l_Vehicle, value);
            }
            else
            {
                l_DbPlayer.SendNewNotification($"VEH BODY HEALTH {NAPI.Vehicle.GetVehicleBodyHealth(l_Vehicle)}");
            }



        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehengineh(Client p_Player, string p_Door)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
            {
                l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!l_DbPlayer.Player.IsInVehicle)
            {
                l_DbPlayer.SendNewNotification("Du musst in einem Fahrzeug sein!", NotificationType.ADMIN, "Fehler");
                return;
            }
            var l_Vehicle = p_Player.Vehicle;
            if (float.TryParse(p_Door, out float value))
            {
                NAPI.Vehicle.SetVehicleEngineHealth(l_Vehicle, value);
            }
            else
            {
                l_DbPlayer.SendNewNotification($"VEH ENGINE HEALTH {NAPI.Vehicle.GetVehicleEngineHealth(l_Vehicle)}");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadproperty(Client player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            if (strings.Length < 2)
                strings.Append("1");
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Lade Prop: " + strings[0]);
                user.Player.TriggerEvent("loadprop", strings[0], strings[1], dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, dbPlayer.Player.Position.Z);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeproperty(Client player, string args)
        {
            //int interiorid, string propname
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Entlade Prop: " + strings[0]);
                user.Player.TriggerEvent("removeprop", strings[0], dbPlayer.Player.Position.X, dbPlayer.Player.Position.Y, dbPlayer.Player.Position.Z);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadiplproperty(Client player, string args)
        {
            //int interiorid, string propname, int color
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 1) return;
            if (strings.Length < 2)
                strings.Append("1");
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Lade Prop: " + strings[0]);
                user.Player.TriggerEvent("loadiplprop", strings[0], strings[1], strings[2]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeiplproperty(Client player, string args)
        {
            //int interiorid, string propname
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");
            if (strings.Length < 2) return;
            foreach (var user in Players.Players.Instance.GetValidPlayers())
            {
                user.SendNewNotification("Entlade Prop: " + strings[0]);
                user.Player.TriggerEvent("removeiplprop", strings[0], strings[1]);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void restart(Client p_Player, string p_Args)
        {
            if (Configuration.Instance.DevMode) return;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var l_DbPlayer = p_Player.GetPlayer();
                if (l_DbPlayer == null || !l_DbPlayer.CanAccessMethod())
                {
                    l_DbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var l_Command = p_Args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (l_Command.Length != 2)
                {
                    l_DbPlayer.SendNewNotification(MSG.General.Usage("/restart", "Minuten", "Grund"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (!uint.TryParse(l_Command[0], out uint l_Minuten))
                {
                    l_DbPlayer.SendNewNotification(MSG.General.Usage("/restart", "Minuten", "Grund"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                Main.ScheduleRestart(l_Minuten, l_Command[1]);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setdmg(Client p_Player, string args)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!float.TryParse(args, out float damagemulti))
            {
                dbPlayer.SendNewNotification("ERROR!", NotificationType.ADMIN, "Fehler");
                return;
            }

            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                xPlayer.Player.TriggerEvent("setPlayerDamageMultiplier", damagemulti, 0.5f);
                xPlayer.SendNewNotification("Waffen-Multiplikator geändert auf " + damagemulti.ToString());
            }
        }
        
        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testobjectdata(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            string[] strings = args.Split(" ");

            string function = strings[0];
            int bone = 0;
            Int32.TryParse(strings[1], out bone);
            int model = 0;
            Int32.TryParse(strings[2], out model);
            float offset_x = 0;
            float.TryParse(strings[3], out offset_x);
            float offset_y = 0;
            float.TryParse(strings[4], out offset_y);
            float offset_z = 0;
            float.TryParse(strings[5], out offset_z);
            float rotation_x = 0;
            float.TryParse(strings[6], out rotation_x);
            float rotation_y = 0;
            float.TryParse(strings[7], out rotation_y);
            float rotation_z = 0;
            float.TryParse(strings[8], out rotation_z);

            dbPlayer.Player.TriggerEvent("addattachmenttest", function, bone, model, offset_x, offset_y, offset_z, rotation_x, rotation_y, rotation_z);
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void hash(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var cmd = args.Split(" ");


            uint l_Hash = NAPI.Util.GetHashKey(cmd[0]);
            dbPlayer.SendNewNotification(l_Hash.ToString());
            Console.WriteLine(l_Hash.ToString());
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createdoor(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var cmd = args.Split(" ");
            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            uint l_Hash = NAPI.Util.GetHashKey(cmd[0]);
            dbPlayer.SendNewNotification(l_Hash.ToString());
            string query = $"INSERT INTO `doors` (`model`, `team`, `pos_x`, `pos_y`, `pos_z`)" +
    $"VALUES ('{l_Hash.ToString()}', '{cmd[1]}', '{x}', '{y}', '{z}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
            Console.WriteLine(l_Hash.ToString());
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createveh(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var cmd = args.Split(" ");


            uint l_Hash = NAPI.Util.GetHashKey(cmd[0]);
            dbPlayer.SendNewNotification(l_Hash.ToString());
            string query = $"INSERT INTO `vehicledata` (`hash`, `model`, `mod_car`, `mod_car_name`, `is_shop_vehicle`, `carsell_category`)" +
    $"VALUES ('{l_Hash.ToString()}', '{cmd[0]}', '1', '{cmd[0]}', '1', '0');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
            Console.WriteLine(l_Hash.ToString());
        }

        // Attach object function
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void attach(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.AddAttachment(dbPlayer, (Attachment)attachid);
            dbPlayer.Player.TriggerEvent("courierSetCarrying", true);
        }

        // Attach object function
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void attachveh(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.AddAttachmentVehicle(sxVehicle, (Attachment)attachid);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void detachveh(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.RemoveVehicleAttachment(sxVehicle, (Attachment)attachid);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void detach(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            if (!Int32.TryParse(args, out int attachid))
            {
                return;
            }

            AttachmentModule.Instance.RemoveAttachment(dbPlayer, (Attachment)attachid);
            dbPlayer.Player.TriggerEvent("courierSetCarrying", false);
        }
        

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void blockcallstest(Client p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }


            dbPlayer.phoneSetting.blockCalls ^= true;
            dbPlayer.SendNewNotification("blockCalls umgeschalten");

        }
        
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void updatemode(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            Configuration.Instance.IsUpdateModeOn = !Configuration.Instance.IsUpdateModeOn;
            if (Configuration.Instance.IsUpdateModeOn)
                dbPlayer.SendNewNotification("Update Mode aktiviert");
            else
                dbPlayer.SendNewNotification("Update Mode deaktiviert");
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggletuning(Client p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (dbPlayer.RankId != 11 && dbPlayer.RankId != (int)adminlevel.Projektleitung && dbPlayer.RankId != 8 && dbPlayer.RankId != (int)adminlevel.Manager)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            Configuration.Instance.TuningActive = !Configuration.Instance.TuningActive;
            if (!Configuration.Instance.TuningActive)
                dbPlayer.SendNewNotification("Tuning deaktiviert!");
            else
                dbPlayer.SendNewNotification("Tuning aktiviert!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglelipsync(Client p_Player)
        {
            var dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (dbPlayer.RankId != 11 && dbPlayer.RankId != (int)adminlevel.Projektleitung && dbPlayer.RankId != 8 && dbPlayer.RankId != (int)adminlevel.Manager)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            Configuration.Instance.LipsyncActive = !Configuration.Instance.LipsyncActive;
            if (!Configuration.Instance.LipsyncActive)
                dbPlayer.SendNewNotification("Lippen-Synchro deaktiviert!");
            else
                dbPlayer.SendNewNotification("Lippen-Synchro aktiviert!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setlevel(Client player, string commandParams = "")
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
                if (!Configuration.Instance.DevMode) return;

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (command.Length != 2)
                {
                    dbPlayer.SendNewNotification(

                        MSG.General.Usage("/setlevel", "Name", "Level"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (command[0].Length < 2) return;
                if (command[1].Length < 1) return;

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null || !findPlayer.IsValid())
                {
                    dbPlayer.SendNewNotification("Der Buerger konnte nicht gefunden werden ", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }

                if (!int.TryParse(command[1], out var level)) return;
                if (command[1] == null || string.IsNullOrWhiteSpace(command[1])) return;
                if (level <= 0) return;

                findPlayer.Level = level;
                findPlayer.SendNewNotification($"Dein Level wurde auf {level.ToString()} geändert!");
                dbPlayer.SendNewNotification($"Du hast das Level von {findPlayer.GetName()} auf {level.ToString()} geänddert!");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglejumppoints(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.JumpPointsEnabled)
                dbPlayer.SendNewNotification("Jump Points aktiviert!");
            else
                dbPlayer.SendNewNotification("Jump Points deaktiviert!");

            Configuration.Instance.JumpPointsEnabled = !Configuration.Instance.JumpPointsEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void togglemethlab(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.MethLabEnabled)
                dbPlayer.SendNewNotification("Methlabor-Systeme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Methlabor-Systeme abgeschalten!");

            Configuration.Instance.MethLabEnabled = !Configuration.Instance.MethLabEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggledigging(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.JailescapeEnabled)
                dbPlayer.SendNewNotification("Alle Gefängnis-Tunnel-Ausbruch Koikarpfensysteme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Gefängnis-Tunnel-Ausbruch Koikarpfensysteme abgeschalten!");

            Configuration.Instance.JailescapeEnabled = !Configuration.Instance.JailescapeEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggletug(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.MeertraeubelEnabled)
                dbPlayer.SendNewNotification("Alle Meerträubel-Relevanten Systeme wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Meerträubel-Relevanten Systeme abgeschalten!");

            Configuration.Instance.MeertraeubelEnabled = !Configuration.Instance.MeertraeubelEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleblackmoney(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.BlackMoneyEnabled)
                dbPlayer.SendNewNotification("Schwarzgeldsystem wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Alle Schwarzgeldsysteme abgeschalten!");

            Configuration.Instance.BlackMoneyEnabled = !Configuration.Instance.BlackMoneyEnabled;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleekey(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.InventoryActivated)
                dbPlayer.SendNewNotification("E-Muskel wurde wieder angeschalten.");
            else
                dbPlayer.SendNewNotification("E-Muskel wurde abgeschalten!");

            Configuration.Instance.EKeyActivated = !Configuration.Instance.EKeyActivated;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void toggleinvstate(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.InventoryActivated)
                dbPlayer.SendNewNotification("Inventar wurde wieder angeschalten!");
            else
                dbPlayer.SendNewNotification("Inventar wurde abgeschalten!");

            Configuration.Instance.InventoryActivated = !Configuration.Instance.InventoryActivated;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void adealer(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            Random rand = new Random();
            player.SetPosition(DealerModule.Instance.Get((uint)rand.Next(0, DealerModule.Instance.GetAll().Count)).Position);
            
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void duty(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.IsACop())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInDuty())
            {
                dbPlayer.SetDuty(true);
                dbPlayer.SendNewNotification("Dienst aktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
            }
            else
            {
                dbPlayer.SetDuty(false);
                dbPlayer.SendNewNotification("Dienst deaktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void aduty(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInAdminDuty())
            {
                dbPlayer.SetAdminDuty(true);

                int maskId = 135;
                int chestId = 287;
                int leggingsId = 114;
                int bootId = 78;
                int bodyId = 166;

                string skinModel = "FreeModeMale01";

                if (dbPlayer.Customization.Gender == 1)
                {
                    chestId = 300;
                    leggingsId = 121;
                    bootId = 82;
                    bodyId = 207;
                    dbPlayer.SetClothes(8, 2, 0);
                    skinModel = "FreeModeFemale01";
                }
                else
                {
                    dbPlayer.SetClothes(8, 15, 0);
                }

                if (!Enum.TryParse(skinModel, true, out PedHash skin)) return;

                dbPlayer.Player.SetSkin(skin);

                int color = 0;

                switch (dbPlayer.Rank.Id)
                {
                    case 1:
                        color = 5;
                        break;
                    case 2:
                        color = 4;
                        break;
                    case 3:
                        color = 3;
                        break;
                    case 4:
                        color = 12;
                        break;
                    case 5:
                        color = 2;
                        break;
                    case 6:
                        color = 2;
                        break;
                    case 8:
                        color = 2;
                        break;
                    case 11:
                        color = 2;
                        break;
                    case 21:
                        color = 11;
                        break;

                    default:
                        return;
                }

                dbPlayer.SetClothes(1, maskId, color);
                dbPlayer.SetClothes(11, chestId, color);
                dbPlayer.SetClothes(4, leggingsId, color);
                dbPlayer.SetClothes(6, bootId, color);
                dbPlayer.SetClothes(3, bodyId, 12);
                dbPlayer.SetClothes(2, 0, 0); 
                dbPlayer.SetClothes(9, 0, 0);



                dbPlayer.SendNewNotification("Adminduty aktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                Discord.SendMessage(player.Name + "ist nun im aduty");
            }
            else
            {
                dbPlayer.SetAdminDuty(false);
                Discord.SendMessage(player.Name + "ist nicht mehr im aduty");
                dbPlayer.ApplyCharacter();
                dbPlayer.SendNewNotification("Adminduty deaktiviert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void ahorn(Client player, string commandParams = "")
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null) return;

            int time = 5000;

            if (command[1].Equals("purge", StringComparison.OrdinalIgnoreCase)) time = 75000;
            if (command[1].Equals("halloween", StringComparison.OrdinalIgnoreCase)) time = 104000;

            findPlayer.SendNewNotification($"1337Sexuakbar${command[1]}", duration: time);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void testdrugeffect(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            CustomDrugModule.Instance.SetTrip(dbPlayer, "s_m_y_clown_01", "clown");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void testdrugeffectgay(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            CustomDrugModule.Instance.SetTrip(dbPlayer, "u_m_y_staggrm_01", "gay");
        }

        public class Zone
        {
            public string name;
            public bounds bounds;
        }
        public class bounds
        {
            public float minX;
            public float minY;
            public float minZ;
            public float maxX;
            public float maxY;
            public float maxZ;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void loadzoneapi(Client player, string commandParams = "")
        {
            if (Configuration.Instance.DevMode) return;
            try
            {
                using (StreamReader r = new StreamReader("zonedata.json"))
                {
                    string json = r.ReadToEnd();
                    string query = "";
                    int i = 0;
                    List<Zone> items = JsonConvert.DeserializeObject<List<Zone>>(json);
                    foreach (var item in items)
                    {
                        query += $"INSERT INTO `zones` (`name`, `min_x`, `min_y`, `min_z`, `max_x`, `max_y`, `max_z`) VALUES ('{item.name}', '{item.bounds.minX.ToString().Replace(",", ".")}', '{item.bounds.minY.ToString().Replace(",", ".")}', '{item.bounds.minZ.ToString().Replace(",", ".")}', '{item.bounds.maxX.ToString().Replace(",", ".")}', '{item.bounds.maxY.ToString().Replace(",", ".")}', '{item.bounds.maxZ.ToString().Replace(",", ".")}');";

                        if (i < 5)
                        {
                            Console.WriteLine(query);
                            i++;
                        }
                    }
                    MySQLHandler.ExecuteAsync(query);
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void bhorn(Client player, string name = "")
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            int time = 5000;

            if (name.Equals("purge", StringComparison.OrdinalIgnoreCase)) time = 75000;
            if (name.Equals("halloween", StringComparison.OrdinalIgnoreCase)) time = 104000;

            try
            {
                var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPlayer(25.0f, dbPlayer.Player);
                foreach (var user in surroundingUsers)
                {
                    if (user.Dimension == dbPlayer.Player.Dimension)
                    {
                        var targetPlayer = user.GetPlayer();
                        if (targetPlayer == null || !targetPlayer.IsValid()) continue;
                        targetPlayer.SendNewNotification($"1337Sexuakbar${name}", duration: time);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        [Command]
        public void halloween(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            int time = 5000;



            try
            {
                var surroundingUsers = NAPI.Pools.GetAllPlayers();
                foreach (var user in surroundingUsers)
                {
                    if (user.Dimension == dbPlayer.Player.Dimension)
                    {
                        var targetPlayer = user.GetPlayer();
                        if (targetPlayer == null || !targetPlayer.IsValid()) continue;
                        targetPlayer.SendNewNotification($"1337Sexuakbar$halloween", duration: 104000);
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void features(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            foreach (var feature in dbPlayer.Rank.GetFeatures())
            {
                dbPlayer.SendNewNotification(
                    $"{feature}: {(dbPlayer.Player.HasFeatureIgnored(feature) ? "ignoriert" : "aktiv")}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sethaar(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var command = commandParams.Split(new[] { ' ' }, 1, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 0) return;
            if (!int.TryParse(command[0], out int hairId)) return;
            dbPlayer.SetClothes(2, hairId, 0);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void serverfeature(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 1) return;

            bool newStatus = !ServerFeatures.IsActive(args[0]);
            ServerFeatures.SetActive(args[0], newStatus);
            dbPlayer.SendNewNotification($"Server Feature {args[0]} {(newStatus ? "aktiviert" : "deaktiviert")}", NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void syncdata(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 2) return;
            if (!int.TryParse(args[0], out int pla)) return;
            if (!int.TryParse(args[1], out int veh)) return;

            dbPlayer.SendNewNotification($"Set Sync State | Player {(pla > 0 ? "aktiviert" : "deaktiviert")} Vehicle {(veh > 0 ? "aktiviert" : "deaktiviert")}", NotificationType.ADMIN);
            Players.Players.Instance.GetValidPlayers().ToList().ForEach(p => p.Player.TriggerEvent("setSyncDataState", pla > 0, veh > 0));
            Configurations.Configuration.Instance.PlayerSync = pla > 0;
            Configurations.Configuration.Instance.VehicleSync = veh > 0;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void feature(Client player, string featureName)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (featureName == "all")
            {
                foreach (var feature in dbPlayer.Rank.GetFeatures())
                {
                    if (!dbPlayer.Player.HasFeatureIgnored(feature))
                    {
                        dbPlayer.Player.SetFeatureIgnored(feature);
                        dbPlayer.SendNewNotification($"{feature} ausgeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else
                    {
                        dbPlayer.Player.RemoveFeatureIgnored(feature);
                        dbPlayer.SendNewNotification($"{feature} eingeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                }
                return;
            }

            if (!dbPlayer.Player.HasFeatureIgnored(featureName))
            {
                dbPlayer.Player.SetFeatureIgnored(featureName);
                dbPlayer.SendNewNotification($"{featureName} ausgeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                dbPlayer.Player.RemoveFeatureIgnored(featureName);
                dbPlayer.SendNewNotification($"{featureName} eingeschaltet.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afrisk(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);

            var toFriskPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (toFriskPlayer == null) return;

            if (dbPlayer.RankId < (int)adminlevel.SuperAdministrator)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    player.Name + " durchsucht nun " + toFriskPlayer.Player.Name);


            var lWeapons = toFriskPlayer.Weapons;
            if (lWeapons.Count > 0)
            {
                var lWeaponListContainer = new List<WeaponListContainer>();
                foreach (var lWeapon in lWeapons)
                {
                    var lData = WeaponDataModule.Instance.Contains(lWeapon.WeaponDataId) ? WeaponDataModule.Instance.Get(lWeapon.WeaponDataId) : null;
                    var weapon = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(lData.Name.ToLower()));
                    if (weapon == null || lData == null) continue;
                    lWeaponListContainer.Add(new WeaponListContainer(lData.Name, lWeapon.Ammo, weapon.ImagePath));
                }

                dbPlayer.SetData("disableinv", true);

                var lWeaponListObject = new WeaponListObject(toFriskPlayer.Player.Name, dbPlayer.IsACop(), lWeaponListContainer);
                ComponentManager.Get<FriskWindow>().Show()(dbPlayer, lWeaponListObject);
                return;
            }

            toFriskPlayer.Container.ShowFriskInventory(dbPlayer, toFriskPlayer, toFriskPlayer.Player.Name, (toFriskPlayer.money[0] + toFriskPlayer.blackmoney[0]));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afind(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(name);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            dbPlayer.SendNewNotification(foundPlayer.GetName() +
                " gefunden");

            switch (foundPlayer.DimensionType[0])
            {
                case DimensionType.World:
                    player.TriggerEvent("setPlayerGpsMarker", foundPlayer.Player.Position.X,
                        foundPlayer.Player.Position.Y);
                    break;
                case DimensionType.House:
                    if (!foundPlayer.HasData("inHouse")) return;
                    var house = HouseModule.Instance.Get(foundPlayer.GetData("inHouse"));
                    if (house == null || house.Position == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Basement:
                case DimensionType.Labor:
                    house = HouseModule.Instance.Get(foundPlayer.Player.Dimension);
                    if (house == null || house.Position == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Camper:
                    var vehicle =
                        VehicleHandler.Instance.GetByVehicleDatabaseId(foundPlayer.Player.Dimension);
                    if (vehicle == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", vehicle.entity.Position.X, vehicle.entity.Position.Y);
                    break;
                case DimensionType.Business:
                    break;
                case DimensionType.Storage:
                    break;
                case DimensionType.WeaponFactory:
                    break;
                case DimensionType.Methlaboratory:
                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByDimension(foundPlayer.Player.Dimension);
                    if (methlaboratory == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", methlaboratory.JumpPointEingang.Position.X, methlaboratory.JumpPointEingang.Position.Y);
                    break;
                default:
                    Logger.Crash(new ArgumentOutOfRangeException());
                    break;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void speed(Client player, string speed)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod() || !dbPlayer.Player.IsInVehicle) return;

            if (!Int32.TryParse(speed, out int x)) return;

            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;
            
            sxVeh.DynamicMotorMultiplier = x;
            dbPlayer.SendNewNotification($"Du hast den Speed auf {x}x gestellt");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehcount(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var count = NAPI.Pools.GetAllVehicles().Count;
            var count2 = VehicleHandler.Instance.GetAllVehicles().Count();

            dbPlayer.SendNewNotification("Vehicle count: " + count);
            dbPlayer.SendNewNotification("Vehicle count: " + count2);
        }

        [CommandPermission(PlayerRankPermission = true)]

        [Command]

        public void weapon(Client player, string weaponHash)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                dbPlayer.GiveWeapon(weapon, 600, true);
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setitem(Client player, string commandParams = "")
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                var command = commandParams.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

                if (command.Length != 3)
                {
                    dbPlayer.SendNewNotification(

                        MSG.General.Usage("/setitem", "Name", "Itemname Anzahl"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (command[0].Length < 2) return;
                if (command[1].Length < 1) return;

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null || !findPlayer.IsValid()
                                       || findPlayer.Dimension[0] != dbPlayer.Dimension[0])
                {
                    dbPlayer.SendNewNotification(
                                                    "Der Buerger konnte nicht gefunden werden " +
                                                    "oder ist zu weit entfernt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }

                if (!int.TryParse(command[2], out var amount)) return;

                if (command[1] == null || string.IsNullOrWhiteSpace(command[1]))
                {
                    return;
                }

                var Item = ItemModelModule.Instance.GetItemByNameOrTag(command[1].ToLower());
                if (amount <= 0 || amount > int.MaxValue) return;
                if (Item == null) return;

                if (!findPlayer.Container.CanInventoryItemAdded(Item, amount))
                {
                    dbPlayer.SendNewNotification(
                         "Inventar des Spielers ist voll!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                findPlayer.Container.AddItem(Item, amount);

                dbPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name + " " +
                    amount + " " +
                    Item.Name + " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification(dbPlayer.Player.Name + " hat ihnen " + amount + " " +
                    Item.Name +
                    " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

                if (dbPlayer.RankId < (int)adminlevel.Manager)
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                        player.Name + " hat " + findPlayer.Player.Name + " " + amount + " " +
                    Item.Name + " gegeben.");


                Discord.SendMessage("log",
                        player.Name + " hat " + findPlayer.Player.Name + " " + amount + " " +
                    Item.Name + " gegeben.");
                //DBLogging.LogAdminAction(player, findPlayer.Player.Name, adminLogTypes.setitem, amount + " " + Item.Name, 0, Configuration.Instance.DevMode);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void component(Client player, string weaponHash, string componentHash)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            dbPlayer.RemoveWeapons();

            if (Enum.TryParse(weaponHash, true, out WeaponHash weapon))
            {
                dbPlayer.GiveWeapon(weapon, 999, true);
                if (Enum.TryParse(componentHash, true, out WeaponComponent weaponComponent))
                {
                    dbPlayer.Player.SetWeaponComponent(weapon, weaponComponent);
                }
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setskin(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var searchedPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (searchedPlayer == null) return;
            if (!Enum.TryParse(command[1], true, out PedHash skin)) return;

            searchedPlayer.Player.SetSkin(skin);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadhouse(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (!dbPlayer.HasData("houseId")) return;

            uint houseId = dbPlayer.GetData("houseId");
            var house = HouseModule.Instance[houseId];
            if (house == null) return;

            house.ReloadHouse();
            dbPlayer.SendNewNotification($"Haus {house.Id} wurde reloaded!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadfuel(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            FuelStation fuel = FuelStationModule.Instance.GetThis(player.Position);
            if (fuel == null) return;
            fuel.ReloadData();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadraff(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Raffinery raff = RaffineryModule.Instance.GetThis(player.Position);
            if (raff == null) return;
            raff.ReloadData();
        }
        public static DiscordHandler Discord = new DiscordHandler();

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void writepos(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            Vector3 _pos = player.Position;
            String posx = "" + player.Position.X;
            posx = posx.Replace(",", ".");
            String posy = "" + player.Position.Y;
            posy = posy.Replace(",", ".");
            String posz = "" + player.Position.Z;
            posz = posz.Replace(",", ".");
            String heading = "" + player.Heading;
            heading = heading.Replace(",", ".");


            Discord.SendMessage("INSERT INTO `shops` (`id`, `name`, `pos_x`, `pos_y`, `pos_z`, `heading`, `ped_hash`, `deliver_pos_x`, `deliver_pos_y`, `deliver_pos_z`, `schwarzgelduse`, `marker`, `team`, `rob_pos_x`, `rob_pos_y`, `rob_pos_z`) VALUES('2', 'Supermarkt', '" + posx + "', '" + posy + "', '" + posz + "', '" + heading + "', '-1950698411', '9999', '9999', '9999', '0', '1', '', '9999', '9999.305', '999.49705')", "POS");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadnightclub(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            NightClub nightclub = NightClubModule.Instance.GetThis(player.Position);
            if (nightclub == null) return;
            nightclub.ReloadData();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void giveweapon(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null) return;

            if (Enum.TryParse(command[1], true, out WeaponHash weapon))
            {
                findPlayer.GiveWeapon(weapon, 999, true);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void spawn(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null) return;
            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            findPlayer.DimensionType[0] = DimensionType.World;

            dbPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name + " respawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + dbPlayer.Player.Name +
                                       " hat Sie respawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void clearairvehicles(Client player)
        {
            try
            {
                List<SxVehicle> possibleVehicles = new List<SxVehicle>();
                DbPlayer dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;
                VehicleHandler.SxVehicles.Values.ToList().ForEach(vehicle =>
                {
                    if (vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Helikopter || vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Flugzeug)
                    {
                        if (vehicle.Occupants.Count == 0 && vehicle.entity.Position.Z >= 100 && vehicle.LastInteracted.AddMinutes(15) < DateTime.Now)
                        {
                            possibleVehicles.Add(vehicle);
                        }
                    }
                });
                foreach (DbPlayer iPlayer in Players.Players.Instance.players)
                {
                    possibleVehicles.RemoveAll(vehicle => vehicle.entity.Position.DistanceTo(iPlayer.Player.Position) < 20.0f);
                }
                foreach (SxVehicle vehicle in possibleVehicles)
                {
                    if (vehicle.IsPlayerVehicle()) vehicle.SetPrivateCarGarage(1);
                    else if (vehicle.IsTeamVehicle())
                        vehicle.SetTeamCarGarage(true);
                    else
                        VehicleHandler.Instance.DeleteVehicleByEntity(vehicle.entity);
                    dbPlayer.SendNewNotification($"Fahrzeug (ID: {vehicle.databaseId}) respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void kickplayer(Client player, string commandParams)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            await Chats.SendGlobalMessage(dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " hat " + findPlayer.Player.Name + " vom Server gekickt! (Grund: " + command[1] + ")", COLOR.RED, ICON.GLOB);
            dbPlayer.SendNewNotification($"Sie haben {findPlayer.Player.Name} vom Server gekickt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            Discord.SendMessage(dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " hat " + findPlayer.Player.Name + " vom Server gekickt! (Grund: " + command[1]);
            //DBLogging.LogAdminAction(player, findPlayer.Player.Name, adminLogTypes.kick, command[1], 0, Configuration.Instance.DevMode);
            findPlayer.Save();
            //findPlayer.SendNewNotification("Saved your stuff!");
            findPlayer.Player.SendNotification($"Sie wurden gekickt. Grund {command[1]}");
            findPlayer.Player.Kick();
            dbPlayer.SendNewNotification("Kicked.");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void powernap(Client player, string commandParams)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length != 1) return;

            var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            //await Chats.SendGlobalMessage("Administrator " + dbPlayer.Player.Name + " hat " +
            //                        findPlayer.Player.Name + " vom Server gekickt! (Grund: " + command[1] + ")", COLOR.RED, ICON.GLOB);
            dbPlayer.SendNewNotification($"Sie haben {findPlayer.Player.Name} vom Server gekickt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            //DBLogging.LogAdminAction(player, findPlayer.Player.Name, adminLogTypes.kick, "Powernap!", 0, Configuration.Instance.DevMode);
            findPlayer.Save();
            //findPlayer.SendNewNotification("Saved your stuff!");
            findPlayer.Player.SendNotification($"Sie wurden gekickt. Grund: Powernap!");
            findPlayer.Player.Kick();
            dbPlayer.SendNewNotification("Successfully Kicked.");

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goup(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z + 2));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void godown(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 2));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goleft(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X - 2, player.Position.Y, player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goright(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            player.SetPosition(new Vector3(player.Position.X, player.Position.Y - 2, player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goforward(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!float.TryParse(args, out float step)) return;

            float angle = dbPlayer.Player.Heading;

            player.SetPosition(new Vector3(player.Position.X + (Math.Cos(angle) * step), player.Position.Y + (Math.Sin(angle) * step), player.Position.Z));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void goatm(Client player, string args)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!UInt32.TryParse(args, out uint id)) return;
            Bank bank = null;
            BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
            {
                if (atm.Id == id)
                    bank = atm;
            });
            if (bank == null)
            {
                dbPlayer.SendNewNotification("ATM nicht gefunden");
                return;
            }
            dbPlayer.Player.SetPosition(bank.Position);
            string atms = "ATMS in der Nähe: ";
            BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
            {
                if (atm.Position.DistanceTo(bank.Position) < 10.0f && atm.Id != bank.Id)
                    atms += atm.Id + ", ";
            });
            dbPlayer.SendNewNotification(atms);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void fillatm(Client player, string args)
        {
            try
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!UInt32.TryParse(args, out uint id)) return;
                BankModule.Instance.GetAll().Values.ToList().ForEach(atm =>
                {
                    if (id != 0 && atm.Id == id && atm.Type == 1)
                    {
                        if (atm == null)
                        {
                            dbPlayer.SendNewNotification("ATM nicht gefunden");
                            return;
                        }
                        atm.ActMoney = Convert.ToInt32(Convert.ToDouble(atm.MaxMoney) * 0.7);
                        atm.SaveActMoneyToDb();
                        dbPlayer.SendNewNotification($"ATM {atm.Id} befüllt!");
                        return;
                    }
                });
                if (id == 0)
                {
                    string query = $"UPDATE `bank` SET actual_money = max_money * 0.7 WHERE type = '1';";
                    MySQLHandler.ExecuteAsync(query);
                }
                dbPlayer.SendNewNotification("ATMs befüllt.");
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testchar(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            ComponentManager.Get<CustomizationWindow>().Show()(dbPlayer, dbPlayer.Customization);

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setbart(Client player, string bartid)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            AppearanceItem appearanceItem = new AppearanceItem((byte)Convert.ToInt32(bartid), 255.0f);

            dbPlayer.Customization.Appearance[1] = appearanceItem;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void setchest(Client player, string chestid)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            AppearanceItem appearanceItem = new AppearanceItem((byte)Convert.ToInt32(chestid), 255.0f);

            dbPlayer.Customization.Appearance[10] = appearanceItem;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void sethair(Client player, string bartid)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            HairData hairData = new HairData((byte)Convert.ToInt32(bartid), 0, 2);

            dbPlayer.Customization.Hair = hairData;

            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void cleartattoo(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.Customization.Tattoos.Clear();
            dbPlayer.SaveCustomization();
            dbPlayer.ApplyCharacter();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void fixvedh(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            var sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh == null) return;
            sxVeh.Repair();
            sxVeh.fuel = sxVeh.Data.Fuel;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void guideduty(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            if (!dbPlayer.IsInGuideDuty())
            {
                dbPlayer.SetGuideDuty(true);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo",
                    dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " ist nun im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun im " + dbPlayer.Rank.Name + "dienst!");

            }
            else
            {
                dbPlayer.SetGuideDuty(false);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo",
                    dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " ist nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gdduty(Client player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!dbPlayer.IsInGameDesignDuty())
            {
                dbPlayer.SetGameDesignDuty(true);

                int maskId = 135;
                int chestId = 287;
                int leggingsId = 114;
                int bootId = 78;
                int bodyId = 166;

                string skinModel = "FreeModeMale01";

                if (dbPlayer.Customization.Gender == 1)
                {
                    chestId = 300;
                    leggingsId = 121;
                    bootId = 82;
                    bodyId = 207;
                    dbPlayer.SetClothes(8, 2, 0);
                    skinModel = "FreeModeFemale01";
                }
                else
                {
                    dbPlayer.SetClothes(8, 15, 0);
                }

                if (!Enum.TryParse(skinModel, true, out PedHash skin)) return;

                dbPlayer.Player.SetSkin(skin);

                int color = 0;

                switch (dbPlayer.Rank.Id)
                {
                    case 13:
                        color = 7;
                        break;
                    case 14:
                        color = 10;
                        break;
                    case 23:
                        color = 10;
                        break;
                    case 26:
                        color = 10;
                        break;
                    default:
                        return;
                }

                dbPlayer.SetClothes(1, maskId, color);
                dbPlayer.SetClothes(11, chestId, color);
                dbPlayer.SetClothes(4, leggingsId, color);
                dbPlayer.SetClothes(6, bootId, color);
                dbPlayer.SetClothes(3, bodyId, 12);
                dbPlayer.SetClothes(2, 0, 0);
                dbPlayer.SetClothes(9, 0, 0);

                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo", dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " ist nun im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun im " + dbPlayer.Rank.Name + "dienst!");
            }
            else
            {
                dbPlayer.SetGameDesignDuty(false);
                Players.Players.Instance.SendMessageToAuthorizedUsers("dutyinfo", dbPlayer.Rank.Name + " " + dbPlayer.Player.Name + " ist nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
                dbPlayer.SendNewNotification("Sie befinden sich nun nicht mehr im " + dbPlayer.Rank.Name + "dienst!");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void guideveh(Client player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

                if (!dbPlayer.HasData("guideveh"))
                {
                    dbPlayer.SendNewNotification("Sie haben ein Guide-Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Task.Run(async () =>
                    {
                        Vehicle myveh = VehicleHandler.Instance.CreateServerVehicle(1237, true, dbPlayer.Player.Position, dbPlayer.Player.Rotation.Z, 131, 131, dbPlayer.Player.Dimension, true, false, false, 0, dbPlayer.Player.Name, 0, 999, (uint)dbPlayer.Id, 100, 1000, "", "", 0, null, null, true).entity;
                        dbPlayer.SetData("guideveh", myveh);

                        await Task.Delay(2000);
                        if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                        if (myveh != null) player.SetIntoVehicle(myveh, -1);
                    });
                }
                else
                {
                    Vehicle xveh = dbPlayer.GetData("guideveh");
                    VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                    dbPlayer.SendNewNotification("Sie haben ein Guide-Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                    dbPlayer.ResetData("guideveh");
                }
            }));
        }




        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task o(Client player, string text)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            await Chats.SendGlobalMessage($" {dbPlayer.Player.Name}: {text}", COLOR.ORANGE, ICON.GLOB);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task dev(Client player, string text)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            await Chats.SendGlobalMessage($" {dbPlayer.Player.Name}: {text}", COLOR.RED, ICON.DEV);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task casinoopen(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            await Chats.SendGlobalMessage($"Diamond Casino : Das Casino hat nun geöffnet!", COLOR.LIGHTGREEN, ICON.CASINO);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task casinoclose(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            KasinoModule.Instance.CasinoGuests = new List<DbPlayer>();
            await Chats.SendGlobalMessage($"Diamond Casino : Das Casino schließt nun!", COLOR.LIGHTGREEN, ICON.CASINO);
        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void time(Client player, string time)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;
            if (!int.TryParse(time, out var timeInt)) return;
            if (timeInt < 0 || timeInt > 23) return;
            NAPI.World.SetTime(timeInt, 0, 0);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void a(Client player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("adminchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void h(Client player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("highteamchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void t(Client player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessMethod()) return;

            Players.Players.Instance.SendChatMessageToAuthorizedUsers("teamchat", dbPlayer, message);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void arev(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null || findPlayer.isAlive()) return;

            findPlayer.DimensionType[0] = DimensionType.World;
            findPlayer.revive();

            dbPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name + " revived!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + dbPlayer.Player.Name + " hat Sie revived!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            //DBLogging.LogAdminAction(player, dbPlayer.Player.Name, adminLogTypes.arev, $"{dbPlayer.Player.Name} (ID: {dbPlayer.Id}) belebte {findPlayer.Player.Name} (ID: {findPlayer.Id}) wieder.", 0, Configuration.Instance.DevMode);
            Discord.SendMessage("Administrator " + dbPlayer.Player.Name + player.Name + "revvied");
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void resetcasino(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            KasinoDevice device = KasinoModule.Instance.GetClosest(dbPlayer);

            if (device.IsInUse)
            {
                dbPlayer.SendNewNotification("casino reset successfully");
                device.IsInUse = false;
            }
            else
            {
                dbPlayer.SendNewNotification("casino not in use");
            }
        }





        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void casino(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var destinationDbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;


            if (KasinoModule.Instance.CasinoGuests.Contains(destinationDbPlayer))
            {
                dbPlayer.SendNewNotification($"Casino Zugang entzogen für Kunde {destinationDbPlayer.Player.Name}", PlayerNotification.NotificationType.ERROR);
                KasinoModule.Instance.CasinoGuests.Remove(destinationDbPlayer);
            }
            else
            {
                dbPlayer.SendNewNotification($"Casino Zugang gewährt für Kunde {destinationDbPlayer.Player.Name}", PlayerNotification.NotificationType.SUCCESS);
                KasinoModule.Instance.CasinoGuests.Add(destinationDbPlayer);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void cduty(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            if (findPlayer.IsInCasinoDuty())
            {
                findPlayer.SetCasinoDuty(false);
                findPlayer.ApplyCharacter();
                dbPlayer.SendNewNotification($"Du hast {findPlayer.Player.Name} erfolgreich in den Normalen Modus gesetzt alla", NotificationType.SUCCESS);
            }
            else
            {
                findPlayer.SetCasinoDuty(true);

                string pedhash = "";
                switch (findPlayer.Id)
                {
                    case 51035:
                        pedhash = "3488666811";
                        break;
                    case 37565:
                        pedhash = "337826907";
                        break;
                    case 57067:
                        pedhash = "736659122";
                        break;
                    default:
                        switch (findPlayer.Customization.Gender)
                        {
                            case 0:
                                pedhash = "520636071";
                                break;
                            case 1:
                                pedhash = "3163733717";
                                break;
                        }
                        break;
                }
                Enum.TryParse(pedhash, true, out PedHash skin);
                findPlayer.Player.SetSkin(skin);

                dbPlayer.SendNewNotification($"Du hast {findPlayer.Player.Name} erfolgreich in den Casino Modus gesetzt alla", NotificationType.ERROR);

            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotohouse(Client player, string houseId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            if (!uint.TryParse(houseId, out var houseIdUInt)) return;
            var xHouse = HouseModule.Instance.Get(houseIdUInt);
            if (xHouse == null) return;

            dbPlayer.Player.Dimension = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xHouse.Position);
            dbPlayer.SendNewNotification("Du hast dich zu Haus " + xHouse.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotogarage(Client player, string garageName)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            Garage garage = null;

            if (uint.TryParse(garageName, out uint garageid))
                garage = GarageModule.Instance.GetAll().Where(c => c.Value.Id == garageid).First().Value;
            else
                garage = GarageModule.Instance.GetAll().Where(c => c.Value.Name.ToLower().Contains(garageName.ToLower())).First().Value;

            if (garage == null) return;

            dbPlayer.Player.Dimension = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(garage.Position);
            dbPlayer.SendNewNotification($"Du hast dich zur Garage {garage.Name}({garage.Id}) teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            return;

        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void gotofarm(Client player, string farmId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            if (!uint.TryParse(farmId, out var farmIdUInt)) return;
            var xFarm = Farming.FarmPositionModule.Instance.Get(farmIdUInt);
            if (xFarm == null) return;

            dbPlayer.Player.Dimension = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.Player.SetPosition(xFarm.Position);
            dbPlayer.SendNewNotification("Du hast dich zum Farm Spot " + xFarm.Id + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void go(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            var destinationPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (destinationPlayer == null) return;

            if (dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.Player.Vehicle.Position = destinationPlayer.Player.Position;
                dbPlayer.Player.Vehicle.Dimension = destinationPlayer.Player.Dimension;
            }
            else
            {
                dbPlayer.Player.SetPosition(destinationPlayer.Player.Position);
            }

            dbPlayer.DimensionType[0] = destinationPlayer.DimensionType[0];
            dbPlayer.Player.Dimension = destinationPlayer.Player.Dimension;

            // TODO: Find solution for particle effect
            //dbPlayer.Player.CreateParticleEffect("scr_rcbarry1", "scr_alien_teleport", new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1);
            dbPlayer.SendNewNotification("Sie haben sich zu " + destinationPlayer.Player.Name +
                                     " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            //if (GuentherModule.DbPlayers.Contains(destinationPlayer))
            //{
            //    MySQLHandler.ExecuteAsync($"INSERT INTO `log_guentherclub` (`player_id`, `info`) VALUES ('{dbPlayer.Id}', 'Teleport: {dbPlayer.Player.Name} (Id: {dbPlayer.Id}) zu {destinationPlayer.Player.Name} (Id: {destinationPlayer.Id})');");
            //}

            if (dbPlayer.Rank.CanAccessFeature("silentTeleport")) return;
            if (dbPlayer.IsInGuideDuty())
            {
                destinationPlayer.SendNewNotification("Guide " + destinationPlayer.Player.Name +
                                                  " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else if (dbPlayer.RankId < 6)
            {
                destinationPlayer.SendNewNotification("Administrator " + dbPlayer.Player.Name +
                                                  " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void makeceo(Client p_Player, string p_Args)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var l_Player = p_Player.GetPlayer();
                if (l_Player == null)
                    return;

                if (!l_Player.CanAccessMethod())
                {
                    l_Player.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var l_Command = p_Args.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (l_Command.Length <= 1)
                {
                    l_Player.SendNewNotification("/makeceo business_id spieler_name", title: "SYNTAX", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                bool l_Result = UInt32.TryParse(l_Command[1], out uint l_TeamID);
                if (!l_Result)
                {
                    l_Player.SendNewNotification("Falsche Business ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var l_Business = BusinessModule.Instance.GetById(l_TeamID);
                if (l_Business == null)
                {
                    l_Player.SendNewNotification("Falsche Business ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var l_TargetPlayer = Players.Players.Instance.FindPlayer(l_Command[0], true);
                if (l_TargetPlayer == null)
                {
                    l_Player.SendNewNotification("Spieler nicht gefunden!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                l_TargetPlayer.AddBusinessOwnership(l_Business);
                l_TargetPlayer.UpdateApps();
                l_TargetPlayer.ActiveBusiness = l_Business;
            }));
        }

        //da war was komisch. besonders testen.
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void makeleader(Client player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;
                uint teamId;

                bool result = UInt32.TryParse(command[1], out teamId);
                if (!result || TeamModule.Instance[teamId] == null)
                {
                    iPlayer.SendNewNotification("Falsche Team ID!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);
                if (findPlayer == null) return;

                findPlayer.RemoveParamedicLicense();

                findPlayer.SetTeam(teamId);
                findPlayer.SetTeamRankPermission(true, 2, true, "");
                findPlayer.SendNewNotification("Administrator " + player.Name +
                                           " hat Sie zum Leader der Fraktion " + TeamModule.Instance.Get(teamId).Name +
                                           " ernannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                iPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name +
                                        " zum Leader der Fraktion " + TeamModule.Instance.Get(teamId).Name + " ernannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.UpdateApps();
                findPlayer.Player.TriggerEvent("updateDuty", false);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setrank(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            try
            {
                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int value;
                bool result = Int32.TryParse(command[1], out value);
                if (!result)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                Rank rank = RankModule.Instance.Get((uint)value);

                if (rank == null) return;
                findPlayer.SetRank((uint)value);
                findPlayer.Rank = rank;
                findPlayer.Save();

                iPlayer.SendNewNotification("Sie haben den Rang von " + findPlayer.Player.Name +
                                            " auf " + rank.Name + " gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setarmor(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            try
            {
                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int value;
                bool result = Int32.TryParse(command[1], out value);
                if (!result)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                if (value > 200 && iPlayer.RankId < (int)adminlevel.Projektleitung) value = 200;

                findPlayer.SetArmor(value);
                iPlayer.SendNewNotification("Sie haben die Ruestung von " + findPlayer.Player.Name +
                                        " auf " + value + " gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception ex)
            {
                Logging.Logger.Crash(ex);
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void sethandmoney(Client player, string commandParams)
        {
            await AsyncCommands.Instance.SetHandMoney(player, commandParams);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void setblackmoney(Client player, string commandParams)
        {
            await AsyncCommands.Instance.SetBlackMoney(player, commandParams);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmoney(Client player, string commandParams)
        {
            try
            {
                var iPlayer = player.GetPlayer();
                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 1) return;

                int amount;
                bool result = Int32.TryParse(command[1], out amount);
                if (!result || amount == 0)
                {
                    iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                if (findPlayer == null) return;

                if (amount > 0)
                {
                    findPlayer.GiveBankMoney(amount);

                    iPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name + " $" + amount +
                                            " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    findPlayer.SendNewNotification("Administrator" + player.Name + " hat ihnen $" +
                                               amount + " gegeben.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    if (iPlayer.RankId < (int)adminlevel.Projektleitung)
                        Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                            "Admin " + player.Name + " hat " + findPlayer.Player.Name + " $" + amount + " gegeben!");

                    Discord.SendMessage("Admin" + player.Name + "hat" + findPlayer.Player.Name + "$" + amount + "gegeben");
                    //DBLogging.LogAdminAction(player, findPlayer.Player.Name, adminLogTypes.log, $"{amount}$ Givemoney");
                    return;
                }

                findPlayer.TakeBankMoney(Math.Abs(amount));

                iPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name + " $" + amount +
                                        " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                findPlayer.SendNewNotification("Administrator" + player.Name + " hat ihnen $" +
                                           amount + " entfernt.", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                if (iPlayer.RankId < (int)adminlevel.Projektleitung)
                    Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                        "Admin " + player.Name + " hat " + findPlayer.Player.Name + " $" + amount + " entfernt!");

                //DBLogging.LogAdminAction(player, findPlayer.Player.Name, adminLogTypes.log, $"-{amount}$ Givemoney");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sethp(Client player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    var iPlayer = player.GetPlayer();

                    if (!iPlayer.CanAccessMethod())
                    {
                        iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                        return;
                    }

                    var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    if (command.Length <= 1) return;

                    int hp;
                    bool result = Int32.TryParse(command[1], out hp);
                    if (!result || hp == 0)
                    {
                        iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }

                    var findPlayer = Players.Players.Instance.FindPlayer(command[0], true);

                    if (findPlayer == null) return;

                    findPlayer.SetHealth(hp);
                    iPlayer.SendNewNotification(
                                            $"Sie haben die HP von {findPlayer.Player.Name} auf {hp} gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void module(Client player, string module)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.SendNewNotification(Modules.Instance.Reload(module) ? "Reloaded" : "Module not found", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void reloadmodule(Client player, string module)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;

            dbPlayer.SendNewNotification(Modules.Instance.Reload(module) ? "Reloaded" : "Module not found", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void supportinsel(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            player.SetPosition(new Vector3(3639.863, 4999.76, 12.46784));
            iPlayer.SendNewNotification("Sie haben sich zur Supportinsel geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void noobspawn(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            player.SetPosition(new Vector3(-1042.308, -2745.383, 21.35941));
            iPlayer.SendNewNotification("Sie haben sich zum Zivilisten Spawn geportet!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void freeplayer(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var findPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (findPlayer == null) return;

            if (iPlayer.RankId < (int)adminlevel.SuperAdministrator)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    player.Name + " hat " + findPlayer.Player.Name + " aus dem Gefaengnis entlassen!");
            //Main.freePlayer(iPlayer, findPlayer, true);
            findPlayer.jailtime[0] = 1;
            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            iPlayer.SendNewNotification("Sie haben " + findPlayer.Player.Name +
                                    " aus dem Gefaengnis entlassen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            findPlayer.SendNewNotification("Administrator " + iPlayer.Player.Name +
                                       " hat Sie aus dem Gefaengnis entlassen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setgarage(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 0) return;
            if (!uint.TryParse(command[0], out uint dbId)) return;

            SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
            if (Vehicle == null) return;
            if (Vehicle.IsPlayerVehicle())
            {
                Vehicle.SetPrivateCarGarage(1);
                iPlayer.SendNewNotification("Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setdpos(Client player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                try
                {
                    var iPlayer = player.GetPlayer();
                    if (iPlayer == null || !iPlayer.IsValid()) return;
                    if (!iPlayer.CanAccessMethod())
                    {
                        iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                        return;
                    }
                    if (String.IsNullOrEmpty(commandParams)) return;
                    var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    if (command.Length <= 0) return;
                    if (!uint.TryParse(command[0], out uint dbId)) return;

                    SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
                    if (Vehicle == null) return;
                    if (Vehicle.IsPlayerVehicle())
                    {
                        Vehicle.SetPrivateCarGarage(1, (uint)716);
                        iPlayer.SendNewNotification("Fahrzeug wurde in die Admingarage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                    else if (Vehicle.IsTeamVehicle())
                    {
                        Vehicle.SetTeamCarGarage(true);
                        iPlayer.SendNewNotification("Team-Fahrzeug wurde in die Garage gesetzt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    }
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotoveh(Client player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var command = commandParams.Split(new[] { ' ' }, 1, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                if (command.Length <= 0) return;

                if (!uint.TryParse(command[0], out uint dbId)) return;
                SxVehicle Vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(dbId);
                if (Vehicle == null) return;

                Vector3 _pos = Vehicle.entity.Position;
                if (player.IsInVehicle)
                {
                    player.Vehicle.Position = _pos;
                }
                else
                {
                    player.SetPosition(_pos);
                }

                iPlayer.SendNewNotification("Sie haben sich zu Fahrzeug " + Vehicle.databaseId +
                                        " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void id(Client player, string name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(name, true);

                if (findPlayer == null) return;

                iPlayer.SendNewNotification(" Level " + findPlayer.Level +
                                        " Dimension " + findPlayer.DimensionType[0] +
                                        " Dimension-Id " + findPlayer.Player.Dimension +
                                        " VoiceHash: " + findPlayer.VoiceHash, title: $"INFO ({findPlayer.ForumId} - {findPlayer.Player.Name})", notificationType: PlayerNotification.NotificationType.ADMIN);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vh(Client player, string name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                var findPlayer = Players.Players.Instance.FindByVoiceHash(name);

                if (findPlayer == null) return;

                iPlayer.SendNewNotification(" Level " + findPlayer.Level +
                                        " Dimension " + findPlayer.DimensionType[0] +
                                        " Dimension-Id " + findPlayer.Player.Dimension +
                                        " VoiceHash: " + findPlayer.VoiceHash, title: $"INFO ({findPlayer.ForumId} - {findPlayer.Player.Name})", notificationType: PlayerNotification.NotificationType.ADMIN);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void modifywater(Client player, string heightstring)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!float.TryParse(heightstring, out float height)) return;

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                Weather.WeatherModule.Instance.SetWaterHeight(height);

            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void kickall(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            for (int index = 0; index < Players.Players.Instance.players.Count; index++)
            {
                if (!Players.Players.Instance.players[index].IsValid()) continue;
                if (Players.Players.Instance.players[index].Player != null)
                {
                    Players.Players.Instance.players[index].Player.Kick();
                }
            }
            Configuration.Instance.IsServerOpen = false;

            Main.Discord.SendMessage($"{ NAPI.Server.GetServerName() + NAPI.Server.GetServerPort()} - Es wurde /kickall durchgeführt", $"Ausgeführt von: {player.Name}"); // Geht nur an einen internen Channel, nicht #ankündigungen
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void devrestart(Client player)
        {
            
            var iPlayer = player.GetPlayer();

            if (!Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            NAPI.Task.Run(async () =>
            {
                for (int index = 0; index < Players.Players.Instance.players.Count; index++)
                {
                    if (!Players.Players.Instance.players[index].IsValid()) continue;
                    if (Players.Players.Instance.players[index].Player != null)
                    {
                        Players.Players.Instance.players[index].Player.Kick();
                    }
                }
                await Task.Delay(1000);
                if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;
                string strCmdText;
                strCmdText = "taskkill /F /IM Jefferosn.exe";
                System.Diagnostics.Process.Start("CMD.exe", strCmdText);
            });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void blackout(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            Weather.WeatherModule.Instance.SetBlackout(!Weather.WeatherModule.Instance.Blackout);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void disapi(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            Configuration.Instance.disableAPILogin = !Configuration.Instance.disableAPILogin;
            iPlayer.SendNewNotification("API Login: " + (Configuration.Instance.disableAPILogin ? "deaktiviert" : "aktiviert"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task warnplayer(Client player, string commandParams)
        {

            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);

            if (dbPlayer == null) return;

            dbPlayer.warns[0] += 1;
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name + " verwarnt Warns: (" +
                                    dbPlayer.warns[0] + "/3), Grund: " + command[1] + "!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            await Chats.SendGlobalMessage("Administrator " + iPlayer.Player.Name + " hat " +
                                           dbPlayer.Player.Name + " verwarnt, Grund " + command[1] + "!", COLOR.RED, ICON.GLOB);

            dbPlayer.SendNewNotification("Du hast nun: " + dbPlayer.warns[0] + "/3 Warns!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            //DBLogging.LogAdminAction(player, dbPlayer.Player.Name, adminLogTypes.warn, command[1], 0, Devmode);
            Discord.SendMessage("Administrator " + iPlayer.Player.Name + " hat " +
                                           dbPlayer.Player.Name + " verwarnt, Grund " + command[1]);
            if (dbPlayer.warns[0] >= 3)
            {
                dbPlayer.Player.SendNotification("Account gesperrt, (3 Warns) Grund: " + command[1]);
                dbPlayer.Player.Kick("Account gesperrt, (3 Warns) Grund: " + command[1]);
            }

        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void clearwarn(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;

            if (dbPlayer.warns[0] > 0) dbPlayer.warns[0] -= 1;
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name +
                                    " eine verwarnung gelöscht! Warns: (" + dbPlayer.warns[0] + "/3)", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Du hast nun: " + dbPlayer.warns[0] + "/3 Warns!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async void banplayer(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);

            if (dbPlayer == null) return;

            var reason = "";

            if (command[1] != null && String.IsNullOrWhiteSpace(command[1]))
            {
                reason = command[1];
            }

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name + " vom Server gebannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            //NAPI.Chat.SendChatMessageToAll("Administrator " + iPlayer.Player.Name + " hat " +
            //                               dbPlayer.Player.Name + " vom Server gebannt! (Grund: " + command[1] + ")");
            await Chats.SendGlobalMessage("Administrator " + iPlayer.Player.Name + " hat " +
                                    dbPlayer.Player.Name + " vom Server gebannt! (Grund: " + command[1] + ")", COLOR.RED, ICON.GLOB);

            Discord.SendMessage("Administrator " + iPlayer.Player.Name + " hat " +
                                    dbPlayer.Player.Name + " vom Server gebannt! (Grund: " + command[1] + ")");

            //DBLogging.LogAdminAction(player, dbPlayer.Player.Name, adminLogTypes.perm, command[1], 0, Devmode);
            dbPlayer.warns[0] = 3;
            dbPlayer.Player.SendNotification("Sie wurden gebannt! Grund: " + command[1]);
            dbPlayer.Player.Kick("Sie wurden gebannt! Grund: " + command[1]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async void permban(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name +
                                    " von der Community ausgeschlossen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            if (!iPlayer.Rank.CanAccessFeature("hiddenBans"))
                await Chats.SendGlobalMessage(iPlayer.Rank.Name + " " + iPlayer.Player.Name + " hat " +
                                               dbPlayer.Player.Name + " von der Community ausgeschlossen!", COLOR.RED, ICON.GLOB);

            //DBLogging.LogAdminAction(player, dbPlayer.Player.Name, adminLogTypes.perm, "Community-Ausschluss", 0, Devmode);
            dbPlayer.warns[0] = 3;
            dbPlayer.Player.SendNotification("Permanenter Ausschluss!");
            PlayerLoginDataValidationModule.SyncUserBanToForum(dbPlayer.ForumId);
            dbPlayer.Player.Kick("Permanenter Ausschluss!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void spectate(Client player, string name)
        {
            if (player == null)
                return;

            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid())
                return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (name == null || name.Length < 3) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SetData("lastPositionSpectate", player.Position);
            dbPlayer.SetData("lastDimensionSpectate", player.Dimension);
            player.Transparency = 0;
            player.Collisionless = true;

            player.Dimension = dbPlayer.Player.Dimension;

            var pos = dbPlayer.Player.Position;

            if (dbPlayer.Player.IsInVehicle)
            {
                pos.Z += 5;
            }
            else if (dbPlayer.DimensionType[0] != DimensionType.World)
            {
                pos.Z += 1;
            }
            else
            {
                pos.Z += 3;
            }


            player.SetPosition(pos);
            iPlayer.Player.Freeze(true, true, true);
            player.Spectate(dbPlayer.Player);

            if (iPlayer.RankId < (int)adminlevel.SuperAdministrator)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    player.Name + " spectated nun " + dbPlayer.Player.Name);

            Discord.SendMessage("*Wichtiger LOG*",
                    player.Name + " spectated nun " + dbPlayer.Player.Name);

            iPlayer.SendNewNotification("Sie schauen nun " + dbPlayer.Player.Name + " zu!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            if (GuentherModule.DbPlayers.Contains(dbPlayer))
            {
                MySQLHandler.ExecuteAsync($"INSERT INTO `log_guentherclub` (`player_id`, `info`) VALUES ('{dbPlayer.Id}', 'Spectate: {dbPlayer.Player.Name} (Id: {dbPlayer.Id}) zu {dbPlayer.Player.Name} (Id: {dbPlayer.Id})');");
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void spectatecar(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;
            if (!dbPlayer.Player.IsInVehicle) return;
            player.Transparency = 0;
            player.Collisionless = true;

            player.Dimension = dbPlayer.Player.Dimension;

            if (!VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(dbPlayer.Player.Vehicle.GetVehicle(), iPlayer))
            {
                iPlayer.SendNewNotification("Es sind keine freien Sitze mehr verfuegbar!", title: "Fahrzeug", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }


            //player.Spectate(dbPlayer.Player);

            if (iPlayer.RankId < (int)adminlevel.SuperAdministrator)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    player.Name + " spectated nun " + dbPlayer.Player.Name);

            iPlayer.SendNewNotification("Sie schauen nun " + dbPlayer.Player.Name + " zu!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }



        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void stopspectate(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!iPlayer.HasData("lastPositionSpectate")) return;
            if (!iPlayer.HasData("lastDimensionSpectate")) return;
            player.Position = iPlayer.GetData("lastPositionSpectate");
            player.Dimension = iPlayer.GetData("lastDimensionSpectate");

            iPlayer.ResetData("lastPositionSpectate");
            iPlayer.ResetData("lastDimensionSpectate");

            player.Transparency = 255;
            player.Collisionless = false;
            player.StopSpectating();

            iPlayer.SendNewNotification("Spectating beendet ", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setmark(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

                iPlayer.Player.TriggerEvent("setmark", iPlayer.Player.Position.X, iPlayer.Player.Position.Y, iPlayer.Player.Position.Z, iPlayer.Player.Dimension);

            iPlayer.SetData("mark", player.Position);
            iPlayer.SendNewNotification("Position erfolgreich zwischengespeichert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gotomark(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!iPlayer.HasData("mark")) return;

            Vector3 mark = iPlayer.GetData("mark");

            if (player.IsInVehicle)
            {
                player.Vehicle.Position = mark;
            }
            else
            {
                player.SetPosition(mark);
            }

            iPlayer.SendNewNotification("Sie haben sich zur gespeicherten Position teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void getrange(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            if (!iPlayer.HasData("mark")) return;

            Vector3 mark = iPlayer.GetData("mark");

            if (Configuration.Instance.DevMode)
                iPlayer.Player.TriggerEvent("setPlayerGpsMarker", iPlayer.Player.Position.X,
                        iPlayer.Player.Position.Y);

            float distance = iPlayer.Player.Position.DistanceTo(mark);
            iPlayer.SendNewNotification($"Die Distanz beträgt: {distance}!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void freezeplayer(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            dbPlayer.Player.Freeze(true, true, true);
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name + " geFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + player.Name + " hat Sie geFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unfreezeplayer(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            dbPlayer.Player.Freeze(false, true, true);
            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name + " entFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + player.Name + " hat Sie entFreezed!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SetCuffed(false);
            dbPlayer.SetMedicCuffed(false);
            dbPlayer.SetTied(false);
            //dbPlayer.Player.FreezePosition = false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setwhisper(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            if (!dbPlayer.HasData("tmpWhisper"))
            {
                dbPlayer.SetData("tmpWhisper", true);
                iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name +
                                        " Whisper Rechte gegeben!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                dbPlayer.SendNewNotification("Administrator " + iPlayer.Player.Name +
                                         " hat ihnen Whisper Rechte (/w Name Text) gegeben!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                dbPlayer.ResetData("tmpWhisper");
                iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name +
                                        " Whisper Rechte entzogen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                dbPlayer.SendNewNotification("Administrator " + iPlayer.Player.Name +
                                         " hat ihnen Whisper Rechte entzogen!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void w(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod() && !iPlayer.HasData("tmpWhisper"))
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length <= 1) return;

            var dbPlayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (dbPlayer == null) return;

            dbPlayer.SendNewNotification(command[1], title: $"{player.Name} fluestert dir:", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 10000);
            iPlayer.SendNewNotification(command[1], title: $"Sie fluestern: {dbPlayer.Player.Name}", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 10000);
            //DBLogging.LogAdminAction(iPlayer.Player, dbPlayer.Player.Name, adminLogTypes.whisper, command[1]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethere(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            try { 
            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null) return;

            if (dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.Player.Vehicle.Position = iPlayer.Player.Position;
                dbPlayer.Player.Vehicle.Dimension = iPlayer.Player.Dimension;
                dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
            }
            else
            {
                dbPlayer.Player.SetPosition(iPlayer.Player.Position);
                dbPlayer.Player.Dimension = player.Dimension;
                dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
            }

            // TODO: Find solution for particle effect
            //dbPlayer.Player.CreateParticleEffect("scr_rcbarry1", "scr_alien_teleport", new Vector3(0, 0, 0), new Vector3(0, 0, 0), 1);

            iPlayer.SendNewNotification("Sie haben " + dbPlayer.Player.Name +
                                    " zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            dbPlayer.SendNewNotification("Administrator " + iPlayer.Player.Name +
                                     " hat sie teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethereveh(Client player, string id)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!uint.TryParse(id, out uint vehid))
            {
                iPlayer.SendNewNotification("Keine Gültige Fahrzeug ID " + id, NotificationType.ADMIN, "ADMIN");
                return;
            }

            SxVehicle vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(vehid);

            if (vehicle == null)
            {
                iPlayer.SendNewNotification("Fahrzeug nicht ausgeparkt " + id, NotificationType.ADMIN, "ADMIN");
            }
            else
            {
                vehicle.entity.Position = player.Position;
                iPlayer.SendNewNotification("Fahrzeug mit der Nummer " + id +
                                    " teleportiert", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

            [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void veh(Client player, string commandParams)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                int color1 = 0;
                int color2 = 0;

                if (commandParams == "") return;
                var command = commandParams.Split(" ");

                if (command.Length >= 2) int.TryParse(command[1], out color1);
                if (command.Length == 3) int.TryParse(command[2], out color2);

                var data = uint.TryParse(command[0], out var id)
                    ? VehicleDataModule.Instance.GetDataById(id)
                    : VehicleDataModule.Instance.GetDataByName(command[0]);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    NetHandle myveh = VehicleHandler.Instance.CreateServerVehicle(
                    data.Id, true, player.Position,
                    player.Rotation.Z, color1, color2, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.Player.Name,
                    0, 999, (uint)iPlayer.Id, 200, 1000, "", "", 0, null, null, true).entity;

                    await Task.Delay(2000);
                    if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;

                    if (myveh != null) player.SetIntoVehicle(myveh, -1);
                });

                Discord.SendMessage(player.Name + "auto gespawnt");
                //DBLogging.LogAdminAction(player, iPlayer.Player.Name, adminLogTypes.veh, $"{iPlayer.Player.Name} hat das Fahrzeug {data.Model} (ID: {data.Id}) gespawnt", 0, Configuration.Instance.DevMode);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehtest(Client player, string args)
        {
            //int fahrzeugid
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            if (dbPlayer.Player.IsInVehicle)
            {
                // player is in vehicle and remove
                var sxVeh = player.Vehicle.GetVehicle();

                if (sxVeh == null) return;

                if (sxVeh.IsPlayerVehicle()) sxVeh.SetPrivateCarGarage(1);
                else if (sxVeh.IsTeamVehicle())
                    sxVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(player.Vehicle);
                dbPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }

            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            dbPlayer.Player.SetPosition(new Vector3(-689.973f, 8941.77f, 320.589f));
            dbPlayer.Player.SetRotation(180f);
            dbPlayer.Player.Dimension = dbPlayer.Id;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {

                if (!dbPlayer.CanAccessMethod())
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                if (args == "") return;
                var command = args.Split(" ");

                var data = uint.TryParse(command[0], out var id)
                    ? VehicleDataModule.Instance.GetDataById(id)
                    : VehicleDataModule.Instance.GetDataByName(command[0]);

                if (data == null) return;
                if (data.Disabled) return;

                NAPI.Task.Run(async () =>
                {
                    NetHandle myveh = VehicleHandler.Instance.CreateServerVehicle(
                    data.Id, true, new Vector3(-689.973f, 8941.77f, 320.589f),
                    180f, 0, 0, dbPlayer.Id, true, false, false, 0, dbPlayer.Player.Name,
                    0, 999, (uint)dbPlayer.Id, 200, 1000, "", "", 0, null, null, true).entity;

                    await Task.Delay(2000);
                    if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;
                    dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    if (myveh != null) player.SetIntoVehicle(myveh, -1);
                });
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void migveh(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            for (var i = 0; i < 20; i++)
            {
                NAPI.Vehicle.CreateVehicle(VehicleHash.T20, iPlayer.Player.Position, iPlayer.Player.Rotation, 131, 131,
                    "", 255, false, true, 0);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public async Task timeban(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
            if (command.Length < 2) return;
            int hours;
            bool result = Int32.TryParse(command[1], out hours);
            if (!result)
            {
                iPlayer.SendNewNotification("Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var findplayer = Players.Players.Instance.FindPlayer(command[0], true);
            if (findplayer == null)
            {
                iPlayer.SendNewNotification("Spieler nicht gefunden", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (hours > 99 || hours < 1)
            {
                iPlayer.SendNewNotification("Fehlerhafte Stundenanzahl", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var banstamp = DateTime.Now.AddHours(hours).GetTimestamp();
            iPlayer.SendNewNotification("Sie haben " + findplayer.Player.Name + " fuer " + hours +
                                    " Stunden vom Server gebannt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            if (!iPlayer.Rank.CanAccessFeature("hiddenBans"))
            {
                await Chats.SendGlobalMessage("Administrator " + iPlayer.Player.Name + " hat " +
                                               findplayer.Player.Name + " fuer " + hours +
                                               " Stunden vom Server gebannt! (Grund: " + command[2] + ")", COLOR.RED, ICON.GLOB);
            }

            //DBLogging.LogAdminAction(player, findplayer.Player.Name, adminLogTypes.timeban, command[2], hours, Devmode);
            findplayer.timeban[0] = banstamp;
            findplayer.Save();
            findplayer.Player.SendNotification("Timeban " + hours + " Stunden, Grund: " + command[2]);
            findplayer.Player.Kick("Timeban " + hours + " Stunden, Grund: " + command[2]);
            findplayer.Player.Kick();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void killer(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null)
            {
                iPlayer.SendNewNotification("Spieler nicht gefunden", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (dbPlayer.isAlive())
            {
                iPlayer.SendNewNotification("Spieler am Leben", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!dbPlayer.HasData("killer"))
            {
                iPlayer.SendNewNotification("killer unbekannt", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            NetHandle killer = dbPlayer.GetData("killer");

            if (killer == null)
            {
                iPlayer.SendNewNotification("killer unbekannt (null)", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (killer.GetEntityType() == EntityType.Player)
            {
                var killerClient = NAPI.Entity.GetEntityFromHandle<Client>(killer);
                iPlayer.SendNewNotification("Killer: " + killerClient.Name, title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                iPlayer.SendNewNotification("Killer: " + killer.GetEntityType(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void players(Client player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var iPlayer = player.GetPlayer();

                if (!iPlayer.CanAccessMethod())
                {
                    iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                    return;
                }

                DialogMigrator.CreateMenu(player, Dialogs.menu_player,
                    "Spielerliste (" + Players.Players.Instance.GetValidPlayers().Count + ")", "Alle verbundenen Spieler");
                DialogMigrator.AddMenuItem(player, Dialogs.menu_player, MSG.General.Close(), "");

                foreach (var user in Players.Players.Instance.GetValidPlayers())
                {
                    if (!user.IsValid()) continue;
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_player, user.Player.Name, "");
                }

                DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_player);
            }));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void removeveh(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (player.IsInVehicle)
            {
                // player is in vehicle and remove
                var sxVeh = player.Vehicle.GetVehicle();

                if (sxVeh == null) return;

                if (sxVeh.IsPlayerVehicle()) sxVeh.SetPrivateCarGarage(1);
                else if (sxVeh.IsTeamVehicle())
                    sxVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(player.Vehicle);
                iPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else
            {
                // player is not in vehicle and remove
                var delVeh = VehicleHandler.Instance.GetClosestVehicle(player.Position);
                if (delVeh == null) return;

                if (delVeh.IsPlayerVehicle()) delVeh.SetPrivateCarGarage(1);
                else if (delVeh.IsTeamVehicle())
                    delVeh.SetTeamCarGarage(true);
                else
                    VehicleHandler.Instance.DeleteVehicleByEntity(delVeh.entity);
                iPlayer.SendNewNotification("Fahrzeug respawnt / geloescht", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void vehinfo(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var sxVeh = VehicleHandler.Instance.GetClosestVehicle(player.Position);
            if (sxVeh == null) return;
            var name = PlayerNameModule.Instance.Get(sxVeh.ownerId).Name;
            iPlayer.SendNewNotification(
                $"Besitzer: {name} BesitzerID: {sxVeh.ownerId} Fahrer: {(sxVeh.Occupants.TryGetValue(0, out DbPlayer dbPlayer) ? dbPlayer.Player.Name : "Keiner")} " +
                $"Letzter-Fahrer: {sxVeh.LastDriver} Fraktion: {sxVeh.Team.Name}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 15000);

            if (iPlayer.RankId < (int)adminlevel.SuperAdministrator)
                Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                    player.Name + " sieht sich das Fahrzeug von " + name + " an");

            //DBLogging.LogAdminAction(player, player.Name, adminLogTypes.log, $"vehinfo {sxVeh.databaseId}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void names(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!iPlayer.CanSeeNames)
            {
                iPlayer.SetNames(true);
            }
            else
            {
                iPlayer.SetNames(false);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void afly(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            try
            {

                    if (!iPlayer.HasData("adminfly"))
                {
                    iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug gespawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    Discord.SendMessage(player.Name + " hat afly gespawnt");


                    int color = 64;
                    if (iPlayer.RankId == (int)adminlevel.SuperAdministrator) color = 145;
                    else if (iPlayer.RankId == (int)adminlevel.Administrator)
                        color = 89;
                    else if (iPlayer.RankId == (int)adminlevel.Moderator)
                        color = 64;
                    else if (iPlayer.RankId >= (int)adminlevel.Manager)
                        color = 44;
                    else if (iPlayer.RankId == (int)adminlevel.Supporter)
                        color = 92;

                    if (!Enum.TryParse("2069146067", true, out VehicleHash hash)) return;
                    NAPI.Task.Run(async () =>
                    {
                        var myveh = VehicleHandler.Instance.CreateServerVehicle(
                        VehicleDataModule.Instance.GetData((uint)hash).Id, true,
                        player.Position, player.Rotation.Z, 1, color, iPlayer.Player.Dimension, true, false, false, 0, iPlayer.Player.Name, 0,
                        999, iPlayer.Id, 100, 1000, plate: null).entity;
                        iPlayer.SetData("adminfly", myveh);

                        await Task.Delay(2000);

                        if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;

                        if (myveh != null)
                        {
                            //WarpPlayerIntoVehicle(player, myveh, -1);
                            player.SetIntoVehicle(myveh, -1);
                        }
                    });
                }
                else
                {
                    Vehicle xveh = iPlayer.GetData("adminfly");
                    if (xveh == null)
                    {
                        iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                        iPlayer.ResetData("adminfly");
                        return;
                    }
                    if (xveh != null)
                    {
                        VehicleHandler.Instance.DeleteVehicleByEntity(xveh);
                        iPlayer.SendNewNotification("Sie haben ein Administrator Fahrzeug despawnt!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ERROR);
                        Discord.SendMessage(player.Name + "hat afly despawnt");
                        iPlayer.ResetData("adminfly");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void weather(Client player, string weatherId)
        {
            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.CanAccessMethod()) return;
            //TOOD: rage enum
            if (!Enum.TryParse<GTANetworkAPI.Weather>(weatherId, out var weather)) return;
            NAPI.World.SetWeather(weather);
            Main.m_CurrentWeather = weather;
            Main.WeatherOverride = true;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void team(Client player)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            DialogMigrator.CreateMenu(player, Dialogs.menu_player, "Nexus Team", "Teammitglieder");

            DialogMigrator.AddMenuItem(player, Dialogs.menu_player, MSG.General.Close(), "");
            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (!dbPlayer.IsValid()) continue;
                if (dbPlayer.Rank.Id != 0)
                {
                    DialogMigrator.AddMenuItem(player, Dialogs.menu_player,
                        dbPlayer.Rank.GetDisplayName() + " " +
                        dbPlayer.Player.Name, "");
                }
            }

            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_player);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void coord(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            if (command.Length < 2) return;
            float x;
            bool result = float.TryParse(command[0], out x);
            if (!result)
            {
                iPlayer.SendNewNotification("X - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            float y;
            result = float.TryParse(command[1], out y);
            if (!result)
            {
                iPlayer.SendNewNotification("Y - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            float z;
            result = float.TryParse(command[2], out z);
            if (!result)
            {
                iPlayer.SendNewNotification("Z - Not a number!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }


            iPlayer.SendNewNotification("Zu Koordinaten X=" + x + " Y=" + y + " Z=" + z, title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            player.SetPosition(new Vector3(x, y, z));
            //DBLogging.LogAdminAction(player, iPlayer.Player.Name, adminLogTypes.coord, $"X: {x}, Y: {y}, Z: {z}", 0, Configuration.Instance.DevMode);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savecoordsFile(Client player, string comment)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            File.AppendAllText("savepos.txt",
                string.Format("{0}: new Vector3({1}, {2}, {3}), {4}f \r\n", comment, x, y, z, heading));
            iPlayer.SendNewNotification(string.Format($"Position (x: {x} | y: {y} | z: {z}) saved as: {comment}"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savefrakveh(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            if (command.Length <= 1) return;

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }
            if (!Int32.TryParse(command[0], out int intAutoHausId)) return;
            if (!Int32.TryParse(command[1], out int intDimension)) return;


            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Heading.ToString().Replace(",", ".");

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO carshop_vehicles (carshopId, model, vehicleHashName, pos_x, pos_y, pos_z, heading, primary_color, secondary_color, dimension) VALUES('{MySqlHelper.EscapeString(command[0])}', '0', '0','{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '131', '131','{MySqlHelper.EscapeString(command[1])}' )");
            iPlayer.SendNewNotification(string.Format("Carshop Position saved as ID: {0}", command[0]), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void savecoords(Client player, string comment)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");

            if (iPlayer.Player.IsInVehicle)
            {
                x = player.Vehicle.Position.X.ToString().Replace(",", ".");
                y = player.Vehicle.Position.Y.ToString().Replace(",", ".");
                z = player.Vehicle.Position.Z.ToString().Replace(",", ".");
                heading = player.Vehicle.Rotation.Z.ToString().Replace(",", ".");
            }

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO savedcoords (x, y, z, heading, comment) VALUES('{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '{MySqlHelper.EscapeString(comment)}')");
            iPlayer.SendNewNotification(string.Format($"Position (x: {x} | y: {y} | z: {z}) saved as: {comment}"), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration: 30000);
        }

        #region devmode




        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void addperso(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod()) return;

            if (!Devmode) return;

            iPlayer.Container.AddItem(ItemModelModule.Instance.GetByType(ItemModelTypes.Perso), 1, new Dictionary<string, dynamic> { { "Id", iPlayer.Id }, { "Name", iPlayer.Player.Name } });
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void addfunk(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!Devmode) return;

            iPlayer.Container.AddItem(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio), 1, new Dictionary<string, dynamic> { { "Fq", 0.0 }, { "Volume", 5 } });
            VoiceModule.Instance.ChangeFrequenz(iPlayer, 0.0f);

            iPlayer.UpdateApps();
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setfunk(Client player, string frequenz)
        {
            var iPlayer = player.GetPlayer();

            Double.TryParse(frequenz, out double fq);

            if (!Devmode) return;

            VoiceModule.Instance.ChangeFrequenz(iPlayer, fq);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void testwmod(Client player, string component)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (string.IsNullOrEmpty(component))
            {
                iPlayer.SendNewNotification(MSG.General.Usage("/testwmod", "Component"));
                /*foreach (var comp in NAPI.Player.GetPlayerWeaponComponents(player.CurrentWeapon))
                {
                    player.SendNewNotification(comp.ToString());
                }*/

                return;
            }

            if (Enum.TryParse(component, true, out WeaponComponent weaponComponent))
            {
                player.SetWeaponComponent(player.CurrentWeapon, weaponComponent);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void testnpc(Client player, string npcname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (string.IsNullOrEmpty(npcname))
            {
                iPlayer.SendNewNotification(MSG.General.Usage("/testnpc", "npcname"));
                /*foreach (var comp in NAPI.Player.GetPlayerWeaponComponents(player.CurrentWeapon))
                {
                    player.SendNewNotification(comp.ToString());
                }*/

                return;
            }

            if (Enum.TryParse(npcname, true, out PedHash npc))
            {
                iPlayer.Player.TriggerEvent("loadNpc", npc, iPlayer.Player.Position.X, iPlayer.Player.Position.Y, iPlayer.Player.Position.Z, iPlayer.Player.Heading, iPlayer.Player.Dimension);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public async Task closeServerX(Client player)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)adminlevel.Projektleitung)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!Configuration.Instance.IsServerOpen)
            {

                Configuration.Instance.IsServerOpen = true;
                iPlayer.SendNewNotification("Ended Restart!");
                return;
            }

            try
            {
                iPlayer.SendNewNotification("Initialize Server Restart!");

                foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
                {
                    if (!dbPlayer.IsValid()) continue;
                    if (dbPlayer.Player != null && dbPlayer.Player != player)
                    {
                        dbPlayer.Save();
                        dbPlayer.Player.Kick();
                    }
                }

                Configuration.Instance.IsServerOpen = false;

                iPlayer.SendNewNotification("Bitte 5min warten!");
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void rideme(Client player, string name)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)adminlevel.Projektleitung)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            try
            {
                var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

                if (dbPlayer == null) return;

                dbPlayer.Player.AttachTo(iPlayer.Player, "SKEL_Spine1",
                    new Vector3(-0.2f, 0, 0.2f), new Vector3(0, 0, -90));

                dbPlayer.PlayAnimation(AnimationScenarioType.Animation,
                    "missswitch", "base_passenger", -1, true,
                    AnimationLevels.User,
                    (int)(AnimationFlags.Loop |
                           AnimationFlags.AllowPlayerControl), true);
            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void unattach(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod() && !Devmode && iPlayer.RankId != (int)adminlevel.Projektleitung)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);

            if (dbPlayer == null) return;

            dbPlayer.Player.Detach();
            dbPlayer.Player.Position = iPlayer.Player.Position;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void objectspeed(Client player, float speed)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() && !Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (iPlayer.adminObject != null)
            {
                iPlayer.adminObjectSpeed = speed;
                return;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void dimension(Client player, string dimension)
        {
            if (!uint.TryParse(dimension, out uint dimension_int)) return;
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            iPlayer.Player.Dimension = dimension_int;
        }

        [Command]
        public void dimensioninfo(Client player)
        {
            var iPlayer = player.GetPlayer();

            iPlayer.SendNewNotification($"Dimension {iPlayer.Player.Dimension} | Zone : {(ZoneModule.Instance.IsInNorthZone(player.Position) ? "North" : "South")}");
        }

        [Command]
        public void ipinfo(Client player)
        {
            var iPlayer = player.GetPlayer();

            iPlayer.SendNewNotification($"IP: {player.Address}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void gethereall(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;


            foreach (DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (!dbPlayer.IsValid()) continue;
                if (dbPlayer.Player != null)
                {
                    if (dbPlayer.AccountStatus == AccountStatus.LoggedIn)
                    {
                        dbPlayer.Player.SetPosition(player.Position);
                        dbPlayer.Player.Dimension = player.Dimension;
                        dbPlayer.DimensionType[0] = iPlayer.DimensionType[0];
                    }
                }
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void vehmod(Client player, int type, int mod)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            NAPI.Vehicle.SetVehicleMod(player.Vehicle.Handle, type, mod);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testlivery(Client player, int livery)
        {
            var iPlayer = player.GetPlayer();

            player.Vehicle.Livery = livery;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testanim(Client player, string commandParams)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 0) return;

            if (!int.TryParse(args[0], out int flag)) return;

            player.SendNotification($"{flag} {args[1]} {args[2]}");

            NAPI.Player.PlayPlayerAnimation(player, flag, args[1], args[2]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testevent(Client player, string commandParams)
        {
            if (!Devmode)
            {
                return;
            }

            var args = commandParams.Split(" ");

            if (!int.TryParse(args[0], out int flag)) return;
            if (args.Length <= 2) return;

            player.SendNotification($"{flag} {args[1]} {args[2]}");

            NAPI.Player.PlayPlayerAnimation(player, flag, args[1], args[2]);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testscene(Client player, string p1)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            iPlayer.PlayAnimation(AnimationScenarioType.Scenario, p1, "", 10);
            iPlayer.SendNewNotification("scene " + p1 + " ausgefuehrt!");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testtrain(Client player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod() || !Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            NAPI.Vehicle.CreateVehicle(0x33C9E158, iPlayer.Player.Position, iPlayer.Player.Heading, 1, 1);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void getzone(Client player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            Module.Zone.Zone zone = ZoneModule.Instance.GetZone(iPlayer.Player.Position);
            iPlayer.SendNewNotification($"Zone: {zone.Name} - {zone.Id}");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void loadipl(Client player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            NAPI.World.RequestIpl(iplname);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void removeipl(Client player, string iplname)
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            NAPI.World.RemoveIpl(iplname);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void createhouse(Client player, string commandParams = " ")
        {
            var iPlayer = player.GetPlayer();


            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var command = commandParams.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();

            int rent = 0;
            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");
            Random rnd = new Random();
            int month = rnd.Next(1, 5);
            string query = $"INSERT INTO `houses` (`price`, `interiorid`, `posX`, `posY`, `posZ`, `colshapeX`, `colshapeY`, `colshapeZ`, `heading`, `maxrents`)" +
                $"VALUES ('{command[0]}', '{month}', '{x}', '{y}', '{z}', '{x}', '{y}', '{z}', '{heading}', '{command[1]}');";
            Console.WriteLine(query);
            MySQLHandler.ExecuteAsync(query);
            Spawners.Blips.Create(player.Position, "Haus", 40, 1, false, 0, 255);
            
            iPlayer.SendNewNotification("Haus erstellt Price:" + command[0] + " Type: " + month +
                                    " Rents:" + command[1] +
                                    " Pos:" + x + " " + y + " " + z + " " + heading, title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void count(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            iPlayer.SendNewNotification("Count: " + Players.Players.Instance.GetValidPlayers().Count);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void testextra(Client player, int extra, int enabled)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!player.IsInVehicle) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            bool xb = false;
            if (enabled == 1) xb = true;
            player.Vehicle.SetExtra(extra, xb);
        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void node(Client player, string commandParams)
        {
            if (!Devmode)
            {
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 2) return;

            if (!int.TryParse(args[0], out int slot)) return;
            if (!int.TryParse(args[1], out int drawable)) return;
            if (!int.TryParse(args[2], out int texture)) return;

            NodeModule.Instance.SetPlayerProp(player.GetPlayer(), slot, drawable, texture);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void prop(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 2) return;

            if (!int.TryParse(args[0], out int slot)) return;
            if (!int.TryParse(args[1], out int drawable)) return;
            if (!int.TryParse(args[2], out int texture)) return;

            NAPI.Player.SetPlayerAccessory(player, slot, drawable, texture);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void cloffgfth(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 4) return;

            if (!int.TryParse(args[1], out int slot)) return;
            if (!int.TryParse(args[2], out int drawable)) return;
            if (!int.TryParse(args[3], out int texture)) return;

            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(args[0]);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.SetClothes(slot, drawable, texture);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void setcloth(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length < 2) return;

            if (!uint.TryParse(args[1], out uint id)) return;
            if (!ClothModule.Instance.Contains(id)) return;

            Cloth cloth = ClothModule.Instance[id];
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(args[0]);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.SetClothes(cloth.Slot, cloth.Variation, cloth.Texture);

            if (foundPlayer.Character.Clothes.ContainsKey(cloth.Slot))
            {
                foundPlayer.Character.Clothes[cloth.Slot] = cloth.Id;
            }
            else
            {
                foundPlayer.Character.Clothes.Add(cloth.Slot, cloth.Id);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void serien(Client player, string nr)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (player.IsInVehicle)
            {
                uint id = 0;

                if (nr == "")
                {
                    id = (uint)new Random().Next(300000, 600000);
                }
                else
                {
                    id = uint.TryParse(nr, out uint result) ? result : 0;
                }

                var sxVehicle = player.Vehicle.GetVehicle();
                if (sxVehicle == null)
                {
                    player.Vehicle.SetData("vehicle", new SxVehicle() { databaseId = id });
                }
                else
                {
                    sxVehicle.databaseId = id;
                }
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void alpha(Client player, string alpha)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (player.IsInVehicle)
            {
                player.Vehicle.Transparency = int.TryParse(alpha, out int result) ? result : 255;
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void color(Client player, string arguments)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = arguments.Split(" ");
            if (args.Count() != 2) return;

            if (!int.TryParse(args[0], out int color1)) return;
            if (!int.TryParse(args[1], out int color2)) return;

            if (player.IsInVehicle)
            {
                player.Vehicle.PrimaryColor = color1;
                player.Vehicle.SecondaryColor = color2;
            }
        }
        GTANetworkAPI.Object obj;
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void createobj(Client player, string commandArgs)
        {

            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var args = commandArgs.Split(" ");
            if (args.Length != 3) return;
            if (!float.TryParse(args[0], out float x)) return;
            if (!float.TryParse(args[1], out float y)) return;
            if (!float.TryParse(args[2], out float z)) return;
            if (obj != null)
            {
                obj.Delete();
                Console.WriteLine("Deleting old Obj");
            }
            //obj = NAPI.Object.CreateObject(3358237751, new Vector3(2727.282, -371.9337, -47.10417), new Vector3(x, y, z), 255, iPlayer.Player.Dimension);
            obj = NAPI.Object.CreateObject(3358237751, iPlayer.Player.Position, new Vector3(x, y, z), 255, iPlayer.Player.Dimension);

            Console.WriteLine("Created new Obj");
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void chris(Client player)
        {
            if (!player.GetPlayer().CanAccessMethod())
            {
                return;
            }

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(player.Position);
            if (sxVehicle != null)
            {
                sxVehicle.SyncExtension.SetLocked(!sxVehicle.SyncExtension.Locked);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void chris2(Client player, string commandParams)
        {
            if (!player.GetPlayer().CanAccessMethod())
            {
                return;
            }

            var args = commandParams.Split(" ");
            if (args.Length <= 1) return;
            if (!int.TryParse(args[0], out int modType)) return;
            if (!int.TryParse(args[1], out int modIndex)) return;

            if (player.IsInVehicle)
            {
                var call = new NodeCallBuilder("setMod").AddInt(modType).AddInt(modIndex).Build();
                player.Vehicle.Call(call);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void chris3(Client player, string plate)
        {
            if (!player.GetPlayer().CanAccessMethod())
            {
                return;
            }

            if (player.IsInVehicle)
            {
                player.Vehicle.Set("numberPlate", plate);
            }
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void chris4(Client player)
        {
            if (!player.GetPlayer().CanAccessMethod())
            {
                return;
            }

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(player.Position);
            if (sxVehicle != null)
            {
                sxVehicle.SyncExtension.SetEngineStatus(!sxVehicle.SyncExtension.EngineOn);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void clearinv(Client player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.CanAccessMethod())
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            for (int i = 0; i < dbPlayer.Container.MaxSlots; i++)
            {
                Item item = dbPlayer.Container.GetItemOnSlot(i);
                dbPlayer.Container.RemoveItem(item.Model, item.Amount);
            }

            dbPlayer.SendNewNotification("Inventar gelöscht");
        }

        [Command]
        public void casinoadmin(Client player)
        {
            var dbPlayer = player.GetPlayer();

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.Rank.CanAccessFeature("casino"))
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            player.Position = new Vector3(1085.23, 214.348, -49.2004);
            player.SetRotation(314.38f);
        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void starteffect(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }

            iPlayer.Player.TriggerEvent("startScreenEffect", commandParams, 5000, true);

        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void stopeffect(Client player, string commandParams)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;
            if (!Devmode)
            {
                return;
            }

            iPlayer.Player.TriggerEvent("stopScreenEffect", commandParams);

        }



        [CommandPermission]
        [Command]
        public void test4(Client player, int input)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.IsValid()) return;

            if (!Devmode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (iPlayer.HasData("AttachedObject"))
            {
                GTANetworkAPI.Object currObj = iPlayer.GetData("AttachedObject");
                currObj.Detach();
            }

            var newObj = ObjectSpawn.Create(input, player.Position, player.Rotation);
            iPlayer.SetData("AttachedObject", newObj);
            newObj.AttachTo(player, "SKEL_L_Finger01", new Vector3(0, 0, 0), new Vector3(0, 0, 0));
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command(GreedyArg = true)]
        public void sirene(Client player, float x, float y, float z)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            //var offset = new Vector3(0, 1, 0);

            var obj = ObjectSpawn.Create(-1110462287, new Vector3(0, 0, 0), new Vector3(0, 0, 0));

            obj.AttachTo(player.Vehicle, "chassis_dummy", new Vector3(x, y, z), new Vector3(180, 0, 0));

            obj.SetSharedData("light", true);

            List<GTANetworkAPI.Object> sirenes;

            if (player.Vehicle.HasData("sirenes"))
            {
                sirenes = player.Vehicle.GetData("sirenes");
            }
            else
            {
                sirenes = new List<GTANetworkAPI.Object>();
            }

            sirenes.Add(obj);

            player.Vehicle.SetData("sirenes", sirenes);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sirene2(Client player)
        {
            if (!Devmode) return;
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!player.IsInVehicle) return;
            if (!player.Vehicle.HasData("sirenes")) return;
            List<GTANetworkAPI.Object> sirenes = player.Vehicle.GetData("sirenes");
            foreach (var sirene in sirenes)
            {
                sirene.SetSharedData("light", false);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void sirene3(Client player, string input)
        {
            if (!Devmode) return;
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            if (!player.IsInVehicle) return;
            if (!player.Vehicle.HasData("sirenes")) return;
            List<GTANetworkAPI.Object> sirenes = player.Vehicle.GetData("sirenes");
            foreach (var sirene in sirenes)
            {
                sirene.SetSharedData("light", true);
            }
        }

        #endregion

        /*blanko
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void b(Client player, string name)
        {
            var iPlayer = player.GetPlayer();

            if (!iPlayer.CanAccessMethod())
            {
                iPlayer.SendNewNotification( MSG.Error.NoPermissions());
                return;
            }

            var dbPlayer = Players.Players.Instance.FindPlayer(name, true);
            if (dbPlayer == null)
            {
                iPlayer.SendNewNotification( "Spieler nicht gefunden");
                return;
            }


        }*/

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void drunktest(Client player)
        {
            if (!Devmode) return;

            DrunkModule.Instance.SetPlayerDrunk(player.GetPlayer(), true);
        }

        [CommandPermission(PlayerRankPermission = false)]
        [Command]
        public void drunkofftest(Client player)
        {
            if (!Devmode) return;

            DrunkModule.Instance.SetPlayerDrunk(player.GetPlayer(), false);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void teamveh(Client player, string input)
        {
            var iPlayer = player.GetPlayer();
            if (!iPlayer.CanAccessMethod())
            {

                iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }
            var args = input.Split(" ");
            if (!int.TryParse(args[0], out int teamid)) return;
            if (!int.TryParse(args[1], out int range)) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicleFromTeamFilter(iPlayer.Player.Position, teamid, range);
            iPlayer.SendNewNotification("Erkenne " + sxVehicle.databaseId);
        }
    }
}