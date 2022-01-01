using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.FIB;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Storage;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.InteriorVehicles;

namespace GVRP.Module.NSA
{
    public enum TransactionType
    {
        MONEY = 1,
        ENERGY = 2,
    }

    public class TransactionHistoryObject
    {
        public string Description { get; set; }
        public Vector3 Position { get; set; }
        public DateTime Added { get; set; }

        public TransactionType TransactionType { get; set; }
    }

    public class NSAModule : Module<NSAModule>
    {

        public static Vector3 NSAVehicleModifyPosition = new Vector3(204.332f, -995.061f, -99);
        public static List<TransactionHistoryObject> TransactionHistory = new List<TransactionHistoryObject>();

        protected override bool OnLoad()
        {
            TransactionHistory = new List<TransactionHistoryObject>();
            
            MenuManager.Instance.AddBuilder(new NSAVehicleListMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAVehicleModifyMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAComputerMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAListCallsMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAObservationsListMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAObservationsSubMenuMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSABankMenu());
            MenuManager.Instance.AddBuilder(new NSAPeilsenderMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSATransactionHistoryMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSAEnergyHistoryMenuBuilder());
            MenuManager.Instance.AddBuilder(new NSADoorUsedsMenuBuilder());
            
            return base.OnLoad();
        }
        
        public override void OnTenSecUpdate()
        {
            foreach(DbPlayer member in TeamModule.Instance.Get((uint)teams.TEAM_FIB).Members.Values.ToList())
            {
                if(member.HasData("nsaOrtung"))
                {
                    DbPlayer targetOne = Players.Players.Instance.FindPlayerById(member.GetData("nsaOrtung"));
                    if (targetOne == null || !targetOne.IsValid() || !targetOne.IsOrtable(member, true))
                    {
                        member.ResetData("nsaOrtung");
                        continue;
                    }

                    member.Player.TriggerEvent("setPlayerGpsMarker", targetOne.Player.Position.X, targetOne.Player.Position.Y);
                }

                if (member.HasData("nsaPeilsenderOrtung"))
                {
                    if (!UInt32.TryParse(member.GetData("nsaPeilsenderOrtung"), out uint vehicleId))
                    {
                        member.ResetData("nsaPeilsenderOrtung");
                        continue;
                    }
                    if (vehicleId != 0)
                    {
                        SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(vehicleId);
                        if (sxVeh == null || !sxVeh.IsValid())
                        {
                            member.ResetData("nsaPeilsenderOrtung");
                            continue;
                        }

                        // Orten
                        member.Player.TriggerEvent("setPlayerGpsMarker", sxVeh.entity.Position.X, sxVeh.entity.Position.Y);
                    }
                }
            }
        }

        public void StopNASCAll(int phoneNumber, int phoneNumber2 = 0)
        {
            foreach(DbPlayer xPlayer in TeamModule.Instance.Get((uint)teams.TEAM_FIB).Members.Values.ToList())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                if(xPlayer.HasData("nsa_activePhone"))
                {
                    if(xPlayer.GetData("nsa_activePhone") == phoneNumber || xPlayer.GetData("nsa_activePhone") == phoneNumber2)
                    {
                        xPlayer.ResetData("nsa_activePhone");
                        xPlayer.Player.TriggerEvent("setCallingPlayer", "");
                        xPlayer.SendNewNotification("Anruf wurde beendet!");
                    }
                }
            }
        }

        public void HandleFind(DbPlayer dbPlayer, DbPlayer playerFromPool)
        {
            Client player = dbPlayer.Player;
            switch (playerFromPool.DimensionType[0])
            {
                case DimensionType.World:
                    player.TriggerEvent("setPlayerGpsMarker", playerFromPool.Player.Position.X,
                        playerFromPool.Player.Position.Y);
                    break;
                case DimensionType.House:
                    if (!playerFromPool.HasData("inHouse")) return;
                    var house = HouseModule.Instance.Get(playerFromPool.GetData("inHouse"));
                    if (house == null || house.Position == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.MoneyKeller:
                case DimensionType.Basement:
                case DimensionType.Labor:
                    house = HouseModule.Instance.Get(playerFromPool.Player.Dimension);
                    if (house == null || house.Position == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", house.Position.X, house.Position.Y);
                    break;
                case DimensionType.Camper:
                    var vehicle =
                        VehicleHandler.Instance.GetByVehicleDatabaseId(playerFromPool.Player.Dimension);
                    if (vehicle == null) return;
                    player.TriggerEvent("setPlayerGpsMarker", vehicle.entity.Position.X, vehicle.entity.Position.Y);
                    break;
                case DimensionType.Business:
                    dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " befindet sich im BusinessTower!");
                    break;
                case DimensionType.Storage:
                    if(playerFromPool.HasData("storageRoomId"))
                    {
                        StorageRoom storageRoom = StorageRoomModule.Instance.Get((uint)playerFromPool.GetData("storageRoomId"));
                        if(storageRoom != null) player.TriggerEvent("setPlayerGpsMarker", storageRoom.Position.X, storageRoom.Position.Y);
                    }
                    break;
                case DimensionType.WeaponFactory:
                    break;
                default:
                    Logger.Crash(new ArgumentOutOfRangeException());
                    break;
            }

            playerFromPool.SetData("isOrted_" + dbPlayer.TeamId, DateTime.Now.AddMinutes(1));

            dbPlayer.SendNewNotification("Gesuchte Person " + playerFromPool.GetName() + " wurde geortet!");
            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat die Person {playerFromPool.GetName()} geortet!", 5000, 10);
            return;
        }
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E)
                return false;


            if (dbPlayer.TeamId == (int)teams.TEAM_FIB && dbPlayer.IsUndercover())
            {
                if (dbPlayer.Player.Position.DistanceTo(NSAVehicleModifyPosition) < 2.0f)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAVehicleListMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }
            
            return false;
        }
        
        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandtakebm(Client player, string commandParams)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.TeamId != (int)teams.TEAM_FIB || !dbPlayer.IsInDuty()) return;

            try
            {
                if (!dbPlayer.IsACop()) return;
                if (string.IsNullOrWhiteSpace(commandParams))
                {
                    dbPlayer.SendNewNotification(MSG.General.Usage("/takebm", "name"));
                    return;
                }

                if (dbPlayer.TeamRank < 4)
                {
                    dbPlayer.SendNewNotification("Beschlagnahmungen von Schwarzgeld können erst ab Rang 4 vollzogen werden.");
                    return;
                }

                var findPlayer = Players.Players.Instance.FindPlayer(commandParams);

                if (findPlayer == null || !findPlayer.IsValid() || findPlayer.Player.Position.DistanceTo(player.Position) >= 3.0f)
                {
                    dbPlayer.SendNewNotification("Person nicht gefunden oder außerhalb der Reichweite!");
                    return;
                }

                if ((!findPlayer.IsCuffed && !findPlayer.IsTied) || findPlayer.isInjured())
                {
                    return;
                }

                if(findPlayer.blackmoney[0] <= 0)
                {
                    dbPlayer.SendNewNotification("Person hat kein Schwarzgeld auf der Hand!");
                    return;
                }

                int amount = findPlayer.blackmoney[0];
                findPlayer.TakeBlackMoney(amount);

                dbPlayer.SendNewNotification($"Sie haben von {findPlayer.GetName()} ${amount} Schwarzgeld konfisziert!");
                findPlayer.SendNewNotification("Ein Beamter hat ihr Schwarzgeld konfisziert!");
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcreatevoltage(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null) return;

            if (!iPlayer.IsValid() || !Configurations.Configuration.Instance.DevMode) return;

            NAPI.Marker.CreateMarker(3, (iPlayer.Player.Position - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, 0);

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");

            string query = String.Format(
                "INSERT INTO `houses_voltage` (`pos_x`, `pos_y`, `pos_z`) VALUES ('{0}', '{1}', '{2}');",
                x, y, z);

            MySQLHandler.ExecuteAsync(query);
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandpeilsender(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null) return;

            if (!iPlayer.IsValid()) return;

            if ((iPlayer.TeamId == (int)teams.TEAM_FIB || iPlayer.TeamId == (int)teams.TEAM_FIB) && iPlayer.Container.GetItemAmount(696) >= 1)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Peilsender", Callback = "NSASetPeilsender", Message = "Bitte Peilsender benennen:" });
            }
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsuspendieren(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null) return;

            if (!iPlayer.IsValid() || (iPlayer.TeamId != (int)teams.TEAM_FIB && iPlayer.TeamId != (int)teams.TEAM_GOV) || iPlayer.TeamRank < 11) return;
            
            ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Suspendierung", Callback = "NSASuspendate", Message = "Bitte einen Namen angeben:" });
            return;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandcloneplayer(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.CanAccessMethod()) return;

            if (!Configurations.Configuration.Instance.DevMode) return;
            if (!iPlayer.IsValid()) return;

            if(iPlayer.HasData("clonePerson"))
            {
                iPlayer.ResetData("clonePerson");
                iPlayer.SendNewNotification("Cloning beendet!");
                return;
            }

            if (iPlayer.TeamId == (int)teams.TEAM_FIB)
            {
                ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "PIS - Person Identifying System", Callback = "NSAClonePlayer", Message = "Bitte geben Sie einen Namen ein:" });
            }
            return;
        }
        
        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandsetgovlevel(Client player, string args)
        {
            if (!args.Contains(' ')) return;

            string[] argsSplit = args.Split(' ');
            if (argsSplit.Length < 2) return;

            string name = argsSplit[0];
            string Level = argsSplit[1];


            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || (dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.TeamId != (int)teams.TEAM_POLICE && dbPlayer.TeamId != (int)teams.TEAM_COUNTYPD && dbPlayer.TeamId != (int)teams.TEAM_ARMY && dbPlayer.TeamId != (int)teams.TEAM_GOV) || dbPlayer.TeamRank < 12) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            if (Level.Length <= 0 || Level.Length > 2) return;

            findPlayer.SetGovLevel(Level);

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} die Sicherheitsfreigabe {Level} erteilt!");
            findPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Sicherheitsfreigabe {Level} erteilt!");
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandresetgovlevel(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || (dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.TeamId != (int)teams.TEAM_POLICE && dbPlayer.TeamId != (int)teams.TEAM_COUNTYPD && dbPlayer.TeamId != (int)teams.TEAM_ARMY && dbPlayer.TeamId != (int)teams.TEAM_GOV) || dbPlayer.TeamRank < 12) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;
            
            findPlayer.RemoveGovLevel();

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} die Sicherheitsfreigabe entzogen!");
            findPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Sicherheitsfreigabe entzogen!");
        }
    }
}
