using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Commands;
using GVRP.Module.Customization;
using GVRP.Module.InteriorProp;
using GVRP.Module.Menu;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Storage
{
    public sealed class StorageModule : Module<StorageModule>
    {
        public Vector3 InteriorPosition = new Vector3(1088.81, -3188, -38);
        public float InteriorHeading = 178.862f;

        public Vector3 InventoryPosition = new Vector3(1103.59, -3195.75, -38.9934);

        public int LimitPlayerStorages = 3;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(PlayerNameModule) };
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreatestorage(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.CanAccessMethod()) return;

            if (!iPlayer.IsValid()) return;

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string rotz = player.Rotation.Z.ToString().Replace(",", ".");

            MySQLHandler.ExecuteAsync(
                $"INSERT INTO storage_rooms (ausbaustufe, owner_id, price, pos_x, pos_y, pos_z, heading) VALUES ('1', '0', '250000', '{x}', '{y}', '{z}', '{rotz}');");
            player.SendNotification("Storage Saved");
        }


        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.StorageKeys = new HashSet<uint>();
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            StorageKeyHandler.Instance.LoadStorageKeys(dbPlayer);
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShape.HasData("storageRoomDataId"))
            {
                // Get Nightclub
                StorageRoom storageRoom = StorageRoomModule.Instance.Get(colShape.GetData("storageRoomDataId"));
                if (storageRoom == null) return false;

                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("storageRoomDataId", storageRoom.Id);
                    Dictionary<String, String> temp = new Dictionary<string, string>();
                    if (storageRoom.OwnerId != 0)
                    {
                        temp.Add("Im Besitz", "");
                    }
                    else
                    {
                        temp.Add("Kein Eigentümer", $"Zum Verkauf (${storageRoom.Price})");
                    }
                    dbPlayer.Player.TriggerEvent("sendInfocard", "Lagerraum " + storageRoom.Id, "red", "storage.jpg", 20000, JsonConvert.SerializeObject(temp));
                    return true;
                }
                else
                {
                    if (dbPlayer.HasData("storageRoomDataId")) dbPlayer.ResetData("storageRoomDataId");
                    return true;
                }
            }
            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if(key == Key.E)
            {
                StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(dbPlayer);
                if (storageRoom != null)
                {
                    // Open Menu
                    MenuManager.Instance.Build(PlayerMenu.StorageMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
                if (dbPlayer.HasData("storageRoomId"))
                {
                    storageRoom = StorageRoomModule.Instance.Get((uint)dbPlayer.GetData("storageRoomId"));
                    if(storageRoom != null && InteriorPosition.DistanceTo(dbPlayer.Player.Position) < 2.0f && !storageRoom.Locked)
                    {
                        // Player out of StorageRoom 
                        if (storageRoom.Locked)
                        {
                            dbPlayer.SendNewNotification("Lager ist abgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.ERROR);
                            return false;
                        }
                        dbPlayer.ResetData("storageRoomId");
                        dbPlayer.Player.SetPosition(storageRoom.Position);
                        dbPlayer.Player.SetRotation(storageRoom.Heading);
                        dbPlayer.Player.Dimension = 0;

                        if (storageRoom.CocainLabor)
                        {
                            InteriorPropModule.Instance.UnloadInteriorForPlayer(dbPlayer, InteriorPropListsType.Kokainlabor);
                            if (dbPlayer.HasData("cocainOutfit"))
                            {
                                dbPlayer.ApplyCharacter();
                                dbPlayer.ResetData("cocainOutfit");
                            }
                        }
                        else
                        {
                            InteriorPropModule.Instance.UnloadInteriorForPlayer(dbPlayer, InteriorPropListsType.Lagerraum);
                        }
                    }
                }
            }
            if (key == Key.L)
            {
                StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(dbPlayer);
                if (storageRoom != null && dbPlayer.CanLockStorage(storageRoom))
                {
                    if (storageRoom.Locked)
                    {
                        storageRoom.Locked = false;
                        dbPlayer.SendNewNotification("Lager aufgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    }
                    else
                    {
                        storageRoom.Locked = true;
                        dbPlayer.SendNewNotification("Lager abgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.ERROR);
                    }
                    return true;
                }
                if (dbPlayer.HasData("storageRoomId"))
                {
                    storageRoom = StorageRoomModule.Instance.Get((uint)dbPlayer.GetData("storageRoomId"));
                    if (storageRoom != null && dbPlayer.CanLockStorage(storageRoom))
                    {
                        if (storageRoom.Locked)
                        {
                            storageRoom.Locked = false;
                            dbPlayer.SendNewNotification("Lager aufgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        }
                        else
                        {
                            storageRoom.Locked = true;
                            dbPlayer.SendNewNotification("Lager abgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.ERROR);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
    
    public static class StorageModulePlayerExtensions
    {
        public static bool CanLockStorage(this DbPlayer dbPlayer, StorageRoom storageRoom)
        {
            return (dbPlayer.StorageKeys.Contains(storageRoom.Id) || storageRoom.OwnerId == dbPlayer.Id || (dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness != null && dbPlayer.ActiveBusiness.StorageKeys.Contains(storageRoom.Id)));
        }
    }
}
