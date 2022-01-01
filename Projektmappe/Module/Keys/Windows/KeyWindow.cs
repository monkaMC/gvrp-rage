using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Houses;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Storage;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Keys.Windows
{
    public class KeyWindow : Window<Func<DbPlayer, string, Dictionary<string, List<VHKey>>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "Spielername")]
            private string PlayerName { get; }
            
            [JsonProperty(PropertyName = "Keys")]
            private Dictionary<string, List<VHKey>> Keys { get; }

            public ShowEvent(DbPlayer dbPlayer,string playerName, Dictionary<string, List<VHKey>> keys) :
                base(dbPlayer)
            {
                PlayerName = playerName;
                Keys = keys;
            }
        }

        public KeyWindow() : base("Keys")
        {

        }

        public override Func<DbPlayer, string, Dictionary<string, List<VHKey>>, bool> Show()
        {
            return (player, playerName, keys) => OnShow(new ShowEvent(player, playerName, keys));
        }



        [RemoteEvent]
        public void GivePlayerKey(Client player, string toPlayer, uint id, string type)
        {
            //Spieler will sich selber schlüssel geben
            if (player.Name.Equals(toPlayer)) return;

            //dbPlayer
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
            //targetPlayer
            DbPlayer targetPlayer = null;
            uint business = 0;
            //Spieler zu Spieler
            if (toPlayer != "0")
            {
                targetPlayer = Players.Players.Instance.FindPlayer(toPlayer);

                if (targetPlayer == null || !targetPlayer.IsValid()) return;
                if (targetPlayer == null)
                {
                    dbPlayer.SendNewNotification("Der Buerger ist nicht erreichbar.");
                    return;
                }

                //targetplayer is available check if he is near
                if (dbPlayer.Player.Position.DistanceTo(targetPlayer.Player.Position) > 5.0f)
                {
                    dbPlayer.SendNewNotification(
                         MSG.General.notInRange);
                    return;
                }

               
            }
            //Spieler zu Business
            else
            {
                //toPlayer could be a business id
                if (dbPlayer.ActiveBusiness == null)
                {
                    dbPlayer.SendNewNotification("Du bist nicht in einem Business!");
                    return;
                }

                //player wants to put key into business
                business = dbPlayer.ActiveBusiness.Id;
            }




            switch (type)
            {
                //Häuser
                case "Häuser":

                    if (business != 0)
                    {
                        dbPlayer.SendNewNotification( "Hausschlüssel können aktuell nicht im Business hinterlegt werden.");
                        return;
                    }

                    if (dbPlayer.ownHouse[0] != id)
                    {
                        dbPlayer.SendNewNotification( "Dieses Haus gehoert dir nicht!");
                        return;
                    }
                    
                    if (targetPlayer == null || !targetPlayer.IsValid()) return;

                    if (targetPlayer.HouseKeys.Contains(id))
                    {
                        dbPlayer.SendNewNotification( "Der Buerger besitzt diesen Schluessel bereits!");
                        return;
                    }
                    HouseKeyHandler.Instance.AddHouseKey(targetPlayer, HouseModule.Instance[id]);
                    dbPlayer.SendNewNotification("Sie haben " + targetPlayer.GetName() + " einen Schluessel fuer Ihr Haus gegeben.");
                    targetPlayer.SendNewNotification(dbPlayer.GetName() + " hat ihnen einen Schluessel das Haus " + id + " gegeben.");
                    break;

                //Lagerräume
                case "Lagerräume":

                    if (!StorageRoomModule.Instance.GetAll().ContainsKey(id)) return;

                    if (StorageRoomModule.Instance.Get(id).OwnerId != dbPlayer.Id)
                    {
                        dbPlayer.SendNewNotification( "Dieser Lagerraum gehoert dir nicht!");
                        return;
                    }

                    if (business != 0)
                    {
                        if (!dbPlayer.IsMemberOfBusiness()) return;
                        var biz = dbPlayer.ActiveBusiness;
                        //schlüssel geht ans business
                        if (biz.StorageKeys.Contains(id))
                        {
                            dbPlayer.SendNewNotification( "Das Business besitzt diesen Schluessel bereits!");
                            return;
                        }
                        Business.BusinessStorageExtension.AddStorageKey(biz, id);
                        dbPlayer.SendNewNotification("Sie haben " + biz.Name + " einen Schluessel Ihren Lageraum (" + id + ") gegeben.");
                        biz.SendMessageToMembers(dbPlayer.GetName() + " hat dem Business einen Schluessel seinen Lagerraum (" + id + ") gegeben.");
                        return;
                    }
                    if (targetPlayer == null || !targetPlayer.IsValid()) return;

                    if (targetPlayer.StorageKeys.Contains(id))
                    {
                        dbPlayer.SendNewNotification( "Der Buerger besitzt diesen Schluessel bereits!");
                        return;
                    }

                    StorageKeyHandler.Instance.AddStorageKey(targetPlayer, StorageRoomModule.Instance[id]);
                    dbPlayer.SendNewNotification("Sie haben " + targetPlayer.GetName() + " einen Schluessel fuer Ihren Lagerraum gegeben.");
                    targetPlayer.SendNewNotification(dbPlayer.GetName() + " hat ihnen einen Schluessel den Lagerraum " + id + " gegeben.");
                    break;

                //Fahrzeuge
                case "Fahrzeuge":

                    var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(id);

                    if (vehicle == null || !dbPlayer.IsOwner(vehicle))
                    {
                        dbPlayer.SendNewNotification( "Dieses Fahrzeug gehört ihnen nicht darf nicht in der garage sein.");
                        return;
                    }

                    String carName = vehicle.Data.modded_car == 1 ? carName = vehicle.Data.mod_car_name : carName = vehicle.Data.Model;

                    if (business != 0)
                    {
                        if (vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Lkw || vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.SmallLkw || vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Trailer)
                        {
                            var biz = dbPlayer.ActiveBusiness;

                            //schlüssel geht ans business
                            if (biz.VehicleKeys.ContainsKey(id))
                            {
                                dbPlayer.SendNewNotification("Das Business besitzt diesen Schluessel bereits!");
                                return;
                            }

                            Business.BusinessVehicleExtension.AddVehicleKey(biz, id, carName);
                            dbPlayer.SendNewNotification("Sie haben " + biz.Name + " einen Schluessel fuer Fahrzeug " + carName + " (" + id + ") gegeben.");
                            biz.SendMessageToMembers(dbPlayer.GetName() + " hat dem Business einen Schluessel fuer Fahrzeug " + carName + " (" + id + ") gegeben.");
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Dieser Fahrzeug Typ  kann nicht im Business hinterlegt werden Versuch es erneut!");
                            return;
                        }
                    }
                    else
                    {
                        if (targetPlayer == null || !targetPlayer.IsValid()) return;

                        if (VehicleKeyHandler.Instance.GetVehicleKeyCount(id) >= 1)
                        {
                            dbPlayer.SendNewNotification("Es wurden bereits zu viele Schlüssel für dieses Fahrzeug vergeben!");
                            return;
                        }

                        //schlüssel geht an player
                        if (targetPlayer.VehicleKeys.ContainsKey(id))
                        {
                            dbPlayer.SendNewNotification( "Der Buerger besitzt diesen Schluessel bereits!");
                            return;
                        }

                        VehicleKeyHandler.Instance.AddPlayerKey(targetPlayer, id, carName);

                        dbPlayer.SendNewNotification("Sie haben " + targetPlayer.GetName() + " einen Schluessel fuer Fahrzeug " + carName + " (" + id + ") gegeben.");
                        targetPlayer.SendNewNotification(dbPlayer.GetName() + " hat ihnen einen Schluessel fuer Fahrzeug " + carName + " (" + id + ") gegeben.");
                    }
                    break;
            }
        }

        [RemoteEvent]
        public void DropPlayerKey(Client player, uint id, string type)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            switch (type)
            {
                //Häuser
                case "Häuser":
                    if (id != 0 && id == dbPlayer.ownHouse[0])
                    {
                        //Spieler ist Besitzer von dem Haus, kann also nicht wegwerfen
                        dbPlayer.SendNewNotification("Du kannst den Hauptschlüssel nicht wegwerfen.");
                    }
                    //Schlüssel wegwerfen
                    HouseKeyHandler.Instance.DeleteHouseKey(dbPlayer, HouseModule.Instance.Get(id));

                    break;
                //Häuser
                case "Lagerräume":
                    if (id != 0 && dbPlayer.GetStoragesOwned().ContainsKey(id))
                    {
                        //Spieler ist Besitzer von dem Haus, kann also nicht wegwerfen
                        dbPlayer.SendNewNotification( "Du kannst den Hauptschlüssel nicht wegwerfen.");
                    }
                    //Schlüssel wegwerfen
                    StorageKeyHandler.Instance.DeleteStorageKey(dbPlayer, StorageRoomModule.Instance.Get(id));

                    break;
                //Fahrzeuge
                case "Fahrzeuge":
                    var vehicle = VehicleHandler.Instance.GetByVehicleDatabaseId(id);

                    if (vehicle == null)
                    {
                        //TODO : Wenn Auto nicht ausgeparkt ist, kann man schlüssel nicht wegwerfen [ENTFERNEN]
                        dbPlayer.SendNewNotification("Dieses Fahrzeug darf nicht in der Garage sein!");
                        return;
                    }
                    else
                    {
                        if ( dbPlayer.IsOwner(vehicle))
                        {
                            dbPlayer.SendNewNotification("Du kannst den Hauptschlüssel nicht wegwerfen.");
                            return;
                        }
                    }


                    dbPlayer.SendNewNotification("Sie haben den Schluessel fuer das Fahrzeug " + dbPlayer.VehicleKeys.GetValueOrDefault(id) +" (" + id +") fallen gelassen!");
                    VehicleKeyHandler.Instance.DeletePlayerKey(dbPlayer, id);
                    break;
            }
        }



    }
}
