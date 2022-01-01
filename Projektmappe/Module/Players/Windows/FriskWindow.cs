using System;
using System.Collections.Generic;
using System.Security.Principal;
using GVRP.Module.Players.Db;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Items;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Asservatenkammer;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Players.Windows
{
    public class FriskWindow : Window<Func<DbPlayer, WeaponListObject, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "weaponListObject")] private WeaponListObject WeaponListObject { get; }

            public ShowEvent(DbPlayer dbPlayer, WeaponListObject weaponListObject) : base(dbPlayer)
            {
                WeaponListObject = weaponListObject;
            }
        }

        public FriskWindow() : base("Frisk")
        {
        }

        public override Func<DbPlayer, WeaponListObject, bool> Show()
        {
            return (player, weaponListObject) => OnShow(new ShowEvent(player, weaponListObject));
        }
    }

    public class WeaponListObject
    {
        public string PersonToFrisk { get; set; }
        public bool CanForceWeaponDrop { get; set; }
        public List<WeaponListContainer> WeaponList { get; set; }

        public WeaponListObject(string personToFrisk, bool canForceWeaponDrop, List<WeaponListContainer> weaponList)
        {
            PersonToFrisk = personToFrisk;
            CanForceWeaponDrop = canForceWeaponDrop;
            WeaponList = weaponList;
        }
    }

    public class WeaponListContainer
    {
        public string WeaponName { get; set; }
        public int WeaponCount { get; set; }
        public string WeaponIcon { get; set; }

        public WeaponListContainer(string weaponName, int weaponCount, string weaponIcon)
        {
            WeaponName = weaponName;
            WeaponCount = weaponCount;
            WeaponIcon = weaponIcon;
        }
    }

    public class FriskFunctions : Script
    {

        [RemoteEvent]
        public void closedWeaponFrisk(Client player, string friskedPersonName, bool wantsToDrop)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var editDbPlayer = Players.Instance.FindPlayer(friskedPersonName);
            if (editDbPlayer == null || !editDbPlayer.IsValid() || editDbPlayer.Player == null)
            {
                return;
            }

            dbPlayer.Player.TriggerEvent("closeFriskWindow");

            if (dbPlayer.IsACop() && dbPlayer.IsInDuty() && wantsToDrop && dbPlayer.HasData("disableFriskInv") && dbPlayer.GetData("disableFriskInv"))
            {
                if (dbPlayer.HasData("disableinv") && dbPlayer.GetData("disableinv")) return;

                if (dbPlayer.HasData("friskInvUserName") && !string.IsNullOrWhiteSpace(dbPlayer.GetData("friskInvUserName")))
                {
                    // Wenn Person zu weit weg
                    if (editDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.0f)
                    {
                        ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
                        ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);
                        return;
                    }
                    
                    if (dbPlayer.GetData("friskInvUserName") == friskedPersonName || dbPlayer.GetData("friskInvUserID") == editDbPlayer.Id)
                    {
                        var lWeapons = editDbPlayer.Weapons;
                        if (lWeapons.Count > 0)
                        {
                            // Get Vehicle With Kofferraum open rofl
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehiclesFromTeamWithContainerOpen(dbPlayer.Player.Position, (int)dbPlayer.TeamId).FirstOrDefault();

                            var lWeaponListContainer = new List<WeaponListContainer>();
                            foreach (var lWeapon in lWeapons.ToList())
                            {
                                var lData = WeaponDataModule.Instance.Contains(lWeapon.WeaponDataId) ? WeaponDataModule.Instance.Get(lWeapon.WeaponDataId) : null;
                                if (lData == null) continue;

                                var weapon = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(lData.Name.ToLower()));
                                if (weapon == null) continue;

                                ItemModel Converted = ItemModelModule.Instance.GetById(AsservatenkammerModule.Instance.GetConvertionItemId(weapon.Id, true));
                                if (Converted == null) continue;
                                
                                if (!editDbPlayer.IsACop() && sxVehicle != null && sxVehicle.IsValid() && sxVehicle.Container.CanInventoryItemAdded(Converted))
                                {
                                    sxVehicle.Container.AddItem(Converted, 1);

                                    dbPlayer.SendNewNotification($"Sie haben {Converted.Name} beschlagnahmt und ins Fahrzeug geladen!");
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification($"Sie haben {Converted.Name} beschlagnahmt und weggeworfen!");
                                }
                                editDbPlayer.RemoveWeapon((WeaponHash)lData.Hash);
                            }
                        }
                        ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
                        ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);

                        editDbPlayer.Save();
                    }
                }
            }

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);

            dbPlayer.SetData("friskInvUserID", editDbPlayer.Id);
            editDbPlayer.Container.ShowFriskInventory(dbPlayer, editDbPlayer, "Spieler", (editDbPlayer.money[0] + editDbPlayer.blackmoney[0]));

            if (editDbPlayer.blackmoney[0] > 0 && dbPlayer.TeamId == (int)teams.TEAM_FIB)
            {
                // Ab Rang 8 100%, Ab Rang 2 mit Chance 80%
                bool Chance = (new Random().Next(1, 100) > 20);
                if ((dbPlayer.TeamRank >= 2 && Chance) || dbPlayer.TeamRank >= 8)
                {
                    dbPlayer.SendNewNotification($"Sie konnten von ${(editDbPlayer.money[0] + editDbPlayer.blackmoney[0])} insgesamt ${editDbPlayer.blackmoney[0]} Schwarzgeld feststellen! (/takebm zum entfernen)");
                }
            }
        }
    }
}
