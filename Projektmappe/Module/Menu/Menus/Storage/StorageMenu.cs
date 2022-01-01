using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Freiberuf;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Houses;
using GVRP.Module.InteriorProp;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Storage;
using GVRP.Module.Vehicles.Data;

namespace GVRP
{
    public class StorageMenuBuilder : MenuBuilder
    {
        public StorageMenuBuilder() : base(PlayerMenu.StorageMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(iPlayer);
            if (storageRoom != null)
            {
                var menu = new Menu(Menu, $"Lagerraum ({storageRoom.Id})");
                if (storageRoom.IsBuyable())
                {
                    menu.Add("Lagerraum kaufen $" + storageRoom.Price);
                }
                else
                {
                    menu.Add("Lagerraum betreten");
                    menu.Add("Lagerraum ausbauen");
                    if (!storageRoom.CocainLabor) menu.Add("Kokainlabor ausbauen");
                }
                menu.Add(MSG.General.Close());
                return menu;
            }
            return null;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                StorageRoom storageRoom = StorageRoomModule.Instance.GetClosest(iPlayer);
                if(storageRoom != null)
                {
                    if(index == 0)
                    {
                        //Kaufen
                        if(storageRoom.IsBuyable())
                        {
                            if(iPlayer.GetStoragesOwned().Count >= StorageModule.Instance.LimitPlayerStorages)
                            {
                                iPlayer.SendNewNotification($"Sie haben die maximale Anzahl an Lager gekauft ({StorageModule.Instance.LimitPlayerStorages})!");
                                return true;
                            }
                            if(iPlayer.TakeBankMoney(storageRoom.Price))
                            {
                                storageRoom.SetOwnerTo(iPlayer);
                                iPlayer.SendNewNotification("Lager für $" + storageRoom.Price + " gekauft!");
                                return true;
                            }
                        }
                        else // betreten
                        {
                            if (!storageRoom.Locked)
                            {
                                // Player Into StorageRoom 
                                iPlayer.SetData("storageRoomId", storageRoom.Id);
                                iPlayer.Player.SetPosition(StorageModule.Instance.InteriorPosition);
                                iPlayer.Player.SetRotation(StorageModule.Instance.InteriorHeading);
                                iPlayer.Player.Dimension = storageRoom.Id;

                                if(storageRoom.CocainLabor)
                                {
                                    InteriorPropModule.Instance.LoadInteriorForPlayer(iPlayer, InteriorPropListsType.Kokainlabor);
                                }
                                else
                                {
                                    InteriorPropModule.Instance.LoadInteriorForPlayer(iPlayer, InteriorPropListsType.Lagerraum);
                                }
                                return true;
                            }
                            else
                            {
                                iPlayer.SendNewNotification("Lager ist abgeschlossen!", title: "Lager", notificationType: PlayerNotification.NotificationType.ERROR);
                                return true;
                            }
                        }
                    }
                    else if(index == 1)
                    {
                        if (iPlayer.Id != storageRoom.OwnerId) return true;
                        storageRoom.Upgrade(iPlayer);
                        return true;
                    }
                    else if (index == 2)
                    {
                        if (iPlayer.Id != storageRoom.OwnerId) return true;
                        if (storageRoom.CocainLabor) return true;
                        else
                            storageRoom.UpgradeCocain(iPlayer);
                        return true;
                    }
                    else
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}