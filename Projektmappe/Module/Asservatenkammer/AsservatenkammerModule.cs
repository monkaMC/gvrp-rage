using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Asservatenkammer
{
    public class AsservatenkammerModule : Module<AsservatenkammerModule>
    {
        public static uint AserBigWeaponId = 917;
        public static uint AserWeaponId = 918;
        public static uint AserItemId = 919;
        public static uint AserAmmoId = 920;
        public static uint AserDrugId = 921;

        public ColShape LSPDAservatenKammer;
        public static Vector3 LSPDAserPosition = new Vector3(477.549, -989.422, 24.9147);
        public static Vector3 AserHackPosition = new Vector3(473.265, -983.046, 24.9147);

        public bool AserHackActive = false;
        public DateTime LastAserHack = DateTime.Now.AddHours(2);

        public override bool Load(bool reload = false)
        {

            ColShape LSPDAservatenKammer = ColShapes.Create(LSPDAserPosition, 3.0f);
            LSPDAservatenKammer.SetData("aser_lspd", true);

            return reload;
        }

        public override void OnMinuteUpdate()
        {
            if (LastAserHack.AddMinutes(10) < DateTime.Now && AserHackActive)
            {
                StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD).Locked = false;
                AserHackActive = false;
                TeamModule.Instance.SendChatMessageToDepartments("Die Sicherheitssysteme der LSPD Asservatenkammer sind wieder online!");
            }

            if (LastAserHack.AddMinutes(10) < DateTime.Now && AserHackActive)
            {
                StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD).Locked = false;
                AserHackActive = false;
                TeamModule.Instance.SendChatMessageToDepartments("Die Sicherheitssysteme der LSPD Asservatenkammer sind wieder online!");
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.HasData("aser_lspd"))
                return false;
            
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                        return false;
                    dbPlayer.SetData("aser_lspd", true);
                    break;
                case ColShapeState.Exit:
                    dbPlayer.ResetData("aser_lspd");
                    break;
                default:
                    break;
            }

            return true;
        }

        public bool IsAserItem(uint ItemModelId)
        {
            if(ItemModelId == AserAmmoId || ItemModelId == AserBigWeaponId || ItemModelId == AserDrugId ||
                ItemModelId == AserItemId || ItemModelId == AserWeaponId)
            {
                return true;
            }
            return false;
        }

        public void SendNotice(DbPlayer dbPlayer, uint modelId, int amount)
        {
            dbPlayer.SendNewNotification($"Sie haben {amount} {ItemModelModule.Instance.Get(modelId).Name} in die Asservatenkammer geladen!");
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!dbPlayer.Player.IsInVehicle || !dbPlayer.IsACop() || !dbPlayer.IsInDuty()) return false;

            if (key != Key.K) return false;

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.teamid != dbPlayer.TeamId) return false;

            if(sxVehicle.entity.Position.DistanceTo(new Vector3(447.292, -996.242, 25.7748)) < 4.0f)
            {
                // Get Aserkammer
                StaticContainer staticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD);
                if (staticContainer == null) return true;

                // Check & Transfer Items
                int AmmoItems = sxVehicle.Container.GetItemAmount(AsservatenkammerModule.AserAmmoId);
                int WeaponItems = sxVehicle.Container.GetItemAmount(AsservatenkammerModule.AserWeaponId);
                int BigWeaponItems = sxVehicle.Container.GetItemAmount(AsservatenkammerModule.AserBigWeaponId);
                int DrugItems = sxVehicle.Container.GetItemAmount(AsservatenkammerModule.AserDrugId);
                int Items = sxVehicle.Container.GetItemAmount(AsservatenkammerModule.AserItemId);

                // Remove
                sxVehicle.Container.RemoveItemAll(AsservatenkammerModule.AserAmmoId);
                sxVehicle.Container.RemoveItemAll(AsservatenkammerModule.AserWeaponId);
                sxVehicle.Container.RemoveItemAll(AsservatenkammerModule.AserBigWeaponId);
                sxVehicle.Container.RemoveItemAll(AsservatenkammerModule.AserDrugId);
                sxVehicle.Container.RemoveItemAll(AsservatenkammerModule.AserItemId);

                // Add
                staticContainer.Container.AddItem(AsservatenkammerModule.AserAmmoId, AmmoItems);
                staticContainer.Container.AddItem(AsservatenkammerModule.AserWeaponId, WeaponItems);
                staticContainer.Container.AddItem(AsservatenkammerModule.AserBigWeaponId, BigWeaponItems);
                staticContainer.Container.AddItem(AsservatenkammerModule.AserItemId, Items);
                staticContainer.Container.AddItem(AsservatenkammerModule.AserDrugId, DrugItems);

                if (AmmoItems > 0) SendNotice(dbPlayer, AsservatenkammerModule.AserAmmoId, AmmoItems);
                if (WeaponItems > 0) SendNotice(dbPlayer, AsservatenkammerModule.AserWeaponId, WeaponItems);
                if (BigWeaponItems > 0) SendNotice(dbPlayer, AsservatenkammerModule.AserBigWeaponId, BigWeaponItems);
                if (Items > 0) SendNotice(dbPlayer, AsservatenkammerModule.AserItemId, Items);
                if (DrugItems > 0) SendNotice(dbPlayer, AsservatenkammerModule.AserDrugId, DrugItems);
                return true;
            }
            return false;
        }

        public uint GetConvertionItemId(uint itemId, bool weapon = false, bool ammo = false)
        {
            switch(itemId)
            {
                case 72:
                case 73:
                case 74:
                case 75:
                case 76:
                case 77:
                case 79:
                case 80:
                case 81:
                case 82:
                case 83:
                case 84:
                case 85:
                case 88:
                case 89:
                case 90:
                case 91:
                case 92:
                case 93:
                case 94:
                case 95:
                    return AserBigWeaponId; // Langwaffe
                case 292: // Kröten
                case 589: // geh. Kröten
                case 725: // Ephi Konz
                case 16: // Ephi
                case 159: // Joint
                case 8: // GWeed
                case 19: // Weed
                case 553: // Kokain
                case 554: // Kokain
                case 555: // Kokain
                case 556: // Kokain
                case 557: // Kokain
                case 558: // Kokain
                case 559: // Kokain
                case 729: // Kiste Meth
                case 728: // Kiste Meth
                case 727: // Kiste Meth
                case 726: // Kiste Meth
                case 1: // Meth
                    return AserDrugId;
                default:
                    return weapon? AserWeaponId : ( ammo? AserAmmoId : itemId);
            }
        }
    }
}
