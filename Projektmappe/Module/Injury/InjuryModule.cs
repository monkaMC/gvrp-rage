using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Gangwar;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Events;
using GVRP.Module.Players.Phone;
using GVRP.Module.Teams;
using GVRP.Module.Voice;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Service;
using GVRP.Module.Zone;
using GVRP.Module.Teamfight;
using GVRP.Module.Vehicles;
using GVRP.Module.Commands;
using GVRP.Module.Teams.Blacklist;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Racing;
using GVRP.Module.Einreiseamt;

namespace GVRP.Module.Injury
{
    public sealed class InjuryModule : Module<InjuryModule>
    {
        public static List<Vector3> HospitalPositions = new List<Vector3>();

        public uint InjuryDeathScreenId = 100;
        public uint InjuryKrankentransport = 101;
        public uint InjuryGangwar = 102;
        public uint InjuryTeamfight = 105;

        public override bool Load(bool reload = false)
        {
            HospitalPositions = new List<Vector3>();
            HospitalPositions.Add(new Vector3(-243.489, 6325.19, 32.4262));
            HospitalPositions.Add(new Vector3(1816.03, 3678.88, 34.2764));
            HospitalPositions.Add(new Vector3(391.001, -1432.46, 29.4338));
            HospitalPositions.Add(new Vector3(355.637, -596.391, 28.7746));
            HospitalPositions.Add(new Vector3(-498.409, -335.794, 34.5018));

            return base.Load(reload);
        }

        public Vector3 GetClosestHospital(Vector3 Position)
        {
            Vector3 returnPos = HospitalPositions.First();
            foreach(Vector3 pos in HospitalPositions)
            {
                if(Position.DistanceTo(pos) < Position.DistanceTo(returnPos))
                {
                    returnPos = pos;
                }
            }

            return returnPos;
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint hash)
        {
            dbPlayer.CancelPhoneCall();
            
            var killedByPlayer = killer.ToPlayer() != dbPlayer.Player;
            DbPlayer iKiller = killer.ToPlayer().GetPlayer();

            if (hash == 3452007600)
            {
                if (iKiller != null && iKiller.IsValid() && iKiller.Player.IsInVehicle)
                {
                    hash = 133987706; // Run over by car (if other player is involved in Fall)
                }
            }

            string killerweapon = Convert.ToString((WeaponHash)hash) != "" ? Convert.ToString((WeaponHash)hash) : "unbekannt";

            if (iKiller != null && iKiller.IsValid() && iKiller.IsACop() && killerweapon == "SMG")
            {
                dbPlayer.SetData("SMGkilledPos", dbPlayer.Player.Position);
                dbPlayer.SetData("SMGkilledDim", dbPlayer.Player.Dimension);
                return;
            }

            if (dbPlayer.DimensionType[0] == DimensionType.RacingArea || dbPlayer.HasData("inRacing"))
            {
                dbPlayer.RemoveFromRacing();
                dbPlayer.dead_x[0] = RacingModule.RacingMenuPosition.X;
                dbPlayer.dead_y[0] = RacingModule.RacingMenuPosition.Y;
                dbPlayer.dead_z[0] = RacingModule.RacingMenuPosition.Z;
            }

            if (dbPlayer.NeuEingereist())
            {
                dbPlayer.revive();
                return;
            }

            // SetTMP Dimension
            dbPlayer.SetData("tmpDeathDimension", dbPlayer.Player.Dimension);

            if (!dbPlayer.isAlive()) return; // Erneuter Tot verhindern

            // if death in jail add jailtime +10
            if (dbPlayer.jailtime[0] > 0)
            {
                dbPlayer.jailtime[0] += 10;
            }

            dbPlayer.Player.TriggerEvent("startScreenEffect", "DeathFailMPIn", 5000, true);


            var rnd = new Random();
            var injuryCauseOfDeath = InjuryCauseOfDeathModule.Instance.GetAll().Values.ToList().Find(iCoD => iCoD.Hash == hash) ??
                                     InjuryCauseOfDeathModule.Instance.GetAll()[5];

            var injuryType = injuryCauseOfDeath.InjuryTypes.OrderBy(x => rnd.Next()).ToList().First() ??
                             injuryCauseOfDeath.InjuryTypes.First(i => i.Id == 1);
            
            // Player Died in Gangwar
            // TODO: Move to according Module
            if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
            {
                // in GW Gebiet
                if (dbPlayer.HasData("gangwarId"))
                {
                    GangwarTown gangwarTown = GangwarTownModule.Instance.Get(dbPlayer.GetData("gangwarId"));
                    if (gangwarTown != null)
                    {
                        // Player is in Range
                        injuryType = InjuryTypeModule.Instance.Get(InjuryGangwar);

                        if (dbPlayer.Team.Id == gangwarTown.AttackerTeam.Id)
                        {
                            gangwarTown.IncreasePoints(GangwarModule.Instance.KillPoints, 0);
                            gangwarTown.DefenderTeam.SendNotification($"+ {GangwarModule.Instance.KillPoints} Punkte fuer toeten eines Gegners!");
                        }
                        else if (dbPlayer.Team.Id == gangwarTown.DefenderTeam.Id)
                        {
                            gangwarTown.IncreasePoints(0, GangwarModule.Instance.KillPoints);
                            gangwarTown.AttackerTeam.SendNotification($"+ {GangwarModule.Instance.KillPoints} Punkte fuer toeten eines Gegners!");
                        }
                    }
                }
            }

            // Blacklist
            // TODO: Move to according Module
            if (iKiller != null && iKiller.IsValid() && iKiller.IsAGangster())
            {
                if (dbPlayer.IsOnBlacklist((int)iKiller.TeamId))
                {
                    iKiller.Team.IncreaseBlacklist(dbPlayer);

                    int type = dbPlayer.GetBlacklistType((int)iKiller.TeamId);
                    int blCosts = 0;
                    
                    // Costs on Blacklist Death
                    if (type == 1)
                    {
                        blCosts = 5000;
                    }
                    else if (type == 2)
                    {
                        blCosts = 8000;
                    }
                    else
                    {
                        blCosts = 3000;
                    }
                    // multiplier w level  R: (Kosten/25 * Level) + Kosten
                    // BSP: Level 31 bei Type 1(ist eigtl typ 2 da aber mit 0 bla..)  Sind es 5000 + (5000/25 * Level) = 11.200$
                    blCosts = ((blCosts / 25) * dbPlayer.Level) + blCosts;

                    dbPlayer.TakeBankMoney(blCosts);
                    dbPlayer.SendNewNotification($"Durch deinen Blacklisteintrag hast du nun ${blCosts} zusätzlich gezahlt!");
                }
            }

            // Assign Injurys Data to Player (Zeit wird hochgesetzt beim Tot aufzählend... von Daher nicht nötig die Deadtime zu setzen)
            dbPlayer.Injury = injuryType;
            dbPlayer.dead_x[0] = dbPlayer.Player.Position.X;
            dbPlayer.dead_y[0] = dbPlayer.Player.Position.Y;
            dbPlayer.dead_z[0] = dbPlayer.Player.Position.Z;
            dbPlayer.deadtime[0] = 0;

            if (injuryType.Id != InjuryTeamfight && dbPlayer.jailtime[0] <= 0 && dbPlayer.Container.GetItemAmount(174) > 0 && !dbPlayer.phoneSetting.flugmodus && injuryType.Id != InjuryGangwar)
            {

                string deathmessage = $"Verletzung: {dbPlayer.Injury.Name} | {dbPlayer.Injury.TimeToDeath - dbPlayer.deadtime[0]} Min";

                TeamModule.Instance.SendMessageToTeam("Es wurde eine Schwerverletze Person gemeldet!", teams.TEAM_MEDIC);

                ServiceModule.Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);

                // Add Service
                Service.Service service = new Service.Service(dbPlayer.Player.Position, deathmessage, (uint)teams.TEAM_MEDIC, dbPlayer);
                ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_MEDIC, service);
            }
            
            dbPlayer.ApplyDeathEffects();


            var l_WeaponDatas = WeaponDataModule.Instance.GetAll().Values.ToList();
            foreach (var l_Data in l_WeaponDatas)
            {
                if (dbPlayer.Weapons.Exists(d => d.WeaponDataId == l_Data.Id))
                {
                    WeaponDetail l_Detail = dbPlayer.Weapons.Find(d => d.WeaponDataId == l_Data.Id);
                    dbPlayer.Player.TriggerEvent("getWeaponAmmo", l_Detail.Ammo);
                }
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.isInjured()) return;

            if (dbPlayer.Injury.Id != InjuryModule.Instance.InjuryKrankentransport && !dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
            }

            // Increase Deadtime of Player
            dbPlayer.deadtime[0]++;

            dbPlayer.Player.TriggerEvent("startScreenEffect", "DeathFailMPIn", 5000, true);

            // Deadtime > max injury Time?
            if (dbPlayer.deadtime[0] <= dbPlayer.Injury.TimeToDeath) return;
            Console.WriteLine(dbPlayer.deadtime[0]);
            if (dbPlayer.Injury.Id == InjuryGangwar)
            {

                dbPlayer.revive();
                PlayerSpawn.OnPlayerSpawn(dbPlayer.Player);

                if (!GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                {
                    TeamfightFunctions.RemoveFromGangware(dbPlayer);
                }
                else TeamfightFunctions.SetToGangware(dbPlayer);
                return;
            }
            else if (dbPlayer.Injury.Id == InjuryTeamfight)
            {
                dbPlayer.revive();
                PlayerSpawn.OnPlayerSpawn(dbPlayer.Player);
                return;
            }
            else
            {
                if (dbPlayer.Injury.Id != Instance.InjuryDeathScreenId)
                    dbPlayer.SetDeathScreen();
                else
                    dbPlayer.SetPlayerDied();
                return;
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShapeState == ColShapeState.Enter && colShape.HasData("injuryDeliverId"))
            {
                if (dbPlayer.isInjured() && dbPlayer.Player.IsInVehicle)
                {
                    SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                    if (sxVehicle == null || !sxVehicle.IsTeamVehicle()) return false;
                    
                    InjuryDeliver injuryDelivery = InjuryDeliverModule.Instance.Get((uint)colShape.GetData("injuryDeliverId"));

                    if (injuryDelivery == null) return false;

                    // Get Driver of Vehicle
                    DbPlayer medic = sxVehicle.Occupants.Values.Where(p => p.Player.VehicleSeat == -1).FirstOrDefault();
                    if (medic == null || !medic.IsValid()) return false;

                    // Check Bad Medic and license stuff
                    if (!medic.IsAMedic() && !medic.ParamedicLicense)
                        return false;

                    if (!injuryDelivery.BadMedics && medic.IsAGangster()) return false;

                    InjuryDeliverIntPoint injuryDeliverIntPoint = injuryDelivery.GetFreePoint();
                    if (injuryDeliverIntPoint == null)
                    {
                        medic.SendNewNotification("Keine Liege im Krankenhaus zur Verfügung!");
                        return false;
                    }

                    // Deliver him to Desk
                    dbPlayer.Player.Position = injuryDeliverIntPoint.Position;
                    dbPlayer.Player.SetRotation(injuryDeliverIntPoint.Heading);
                    dbPlayer.Player.Dimension = (uint)injuryDeliverIntPoint.Dimension;

                    // Resett time in KH and redo pos save
                    dbPlayer.dead_x[0] = dbPlayer.Player.Position.X;
                    dbPlayer.dead_y[0] = dbPlayer.Player.Position.Y;
                    dbPlayer.dead_z[0] = dbPlayer.Player.Position.Z;
                    dbPlayer.deadtime[0] = 0;

                    dbPlayer.Player.TriggerEvent("noweaponsoninjury", false);
                    dbPlayer.SendNewNotification($"Sie wurden ins Krankenhaus eingewiesen!");
                    dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");
                    return true;
                }
            }
            return false;
        }

        public override void OnPlayerFirstSpawnAfterSync(DbPlayer dbPlayer)
        {
            if (dbPlayer == null)
                return;

            if (dbPlayer.isInjured())
            {
                if (dbPlayer.Injury.Id != InjuryGangwar && dbPlayer.Injury.Id != InjuryTeamfight)
                {
                    string deathmessage = $"Verletzung: {dbPlayer.Injury.Name} | {dbPlayer.Injury.TimeToDeath - dbPlayer.deadtime[0]} Min";

                    TeamModule.Instance.SendMessageToTeam("Es wurde eine Schwerverletze Person gemeldet!", teams.TEAM_MEDIC);

                    ServiceModule.Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);

                    // Add Service
                    Service.Service service = new Service.Service(dbPlayer.Player.Position, deathmessage, (uint)teams.TEAM_MEDIC, dbPlayer);
                    ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_MEDIC, service);
                }
            }
        }
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.Injury = InjuryTypeModule.Instance.Get((uint)reader.GetInt32("deadstatus"));

            dbPlayer.deadtime = new int[2];
            dbPlayer.deadtime[1] = reader.GetInt32("deadtime");
            dbPlayer.deadtime[0] = reader.GetInt32("deadtime");
            dbPlayer.dead_x = new float[2];
            dbPlayer.dead_x[1] = reader.GetFloat("dead_x");
            dbPlayer.dead_x[0] = reader.GetFloat("dead_x");
            dbPlayer.dead_y = new float[2];
            dbPlayer.dead_y[1] = reader.GetFloat("dead_y");
            dbPlayer.dead_y[0] = reader.GetFloat("dead_y");
            dbPlayer.dead_z = new float[2];
            dbPlayer.dead_z[1] = reader.GetFloat("dead_z");
            dbPlayer.dead_z[0] = reader.GetFloat("dead_z");

            if (dbPlayer.isInjured() && dbPlayer.Container.GetItemAmount(174) > 0)
            {
                if (dbPlayer.Injury.Id != InjuryGangwar && dbPlayer.Injury.Id != Instance.InjuryDeathScreenId)
                {
                    string deathmessage = $"Verletzung: {dbPlayer.Injury.Name} | {dbPlayer.Injury.TimeToDeath - dbPlayer.deadtime[0]} Min";

                    TeamModule.Instance.SendMessageToTeam("Es wurde eine Schwerverletze Person gemeldet!", teams.TEAM_MEDIC);

                    ServiceModule.Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);

                    // Add Service
                    Service.Service service = new Service.Service(new Vector3(dbPlayer.dead_x[0] , dbPlayer.dead_y[0] , dbPlayer.dead_z[0]), deathmessage, (uint)teams.TEAM_MEDIC, dbPlayer);
                    ServiceModule.Instance.Add(dbPlayer, (uint)teams.TEAM_MEDIC, service);
                }
            }
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandinstrev(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.TeamId != (int)teams.TEAM_MEDIC) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || findPlayer.isAlive()) return;

            findPlayer.revive();
            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
            dbPlayer.SendNewNotification(
          "Sie haben " + findPlayer.GetName() +
                " per INSTANT revived!");
            findPlayer.SendNewNotification(
          "Medic " + player.Name +
                " hat Sie per INSTANT revived!");


            Players.Players.Instance.SendMessageToAuthorizedUsers("log",
                "MEDIC " + dbPlayer.GetName() + " hat " + findPlayer.GetName() +
                " per INSTANT revived!");

            PlayerSpawn.OnPlayerSpawn(findPlayer.Player);
        }

        [CommandPermission()]
        [Command(GreedyArg = true)]
        public void Commandgiveparalic(Client player, string name)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || dbPlayer.TeamId != (int)teams.TEAM_MEDIC) return;


            var findPlayer = Players.Players.Instance.FindPlayer(name);
            if (findPlayer == null || !findPlayer.IsValid()) return;

            // Check Team and Slots
            if (findPlayer.Team.MedicSlots == 0 || findPlayer.Team.MedicSlots <= findPlayer.Team.MedicSlotsUsed)
            {
                dbPlayer.SendNewNotification("Diese Fraktion hat bereits die maximale Anzahl erreicht!");
                return;
            }

            if (findPlayer.ParamedicLicense)
            {
                dbPlayer.SendNewNotification("Spieler hat bereits eine Notfallmedizin Lizenz!");
                return;
            }

            if (!findPlayer.TakeMoney(350000))
            {
                dbPlayer.SendNewNotification("Eine Notfallmedizin Lizenz kostet 350.000$!");
                return;
            }

            findPlayer.SetParamedicLicense();

            dbPlayer.SendNewNotification($"Sie haben {findPlayer.GetName()} eine Notfallmedizin Lizenz für 350.000$ ausgestellt!");
            findPlayer.SendNewNotification($"Arzt {dbPlayer.GetName()} hat ihnen eine Notfallmedizin Lizenz für 350.000$ ausgestellt!");
        }
        
    }
}