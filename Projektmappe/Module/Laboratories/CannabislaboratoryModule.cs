using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Configurations;
using GVRP.Module.Gangwar;
using GVRP.Module.Items;
using GVRP.Module.Laboratories.Windows;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Teams;

namespace GVRP.Module.Laboratories
{
    public class CannabislaboratoryModule : SqlModule<CannabislaboratoryModule, Cannabislaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 984, 966, 12270 }; //dünger, batteriezelle, hanfsamenpulver
        public static List<uint> EndProductItemIds = new List<uint> { 831, 832, 833, 834 }; //Pures Cannabis
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<Team> HasAlreadyHacked = new List<Team>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_cannabislaboratories`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }
        protected override void OnLoaded()
        {
            HasAlreadyHacked = new List<Team>();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            Console.WriteLine("debug: 1");
            if (dbPlayer == null || key == null) return false;
            Console.WriteLine("debug: 2");
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionType.Cannabislaboratory) return false;
            Console.WriteLine("debug: 3");

            Cannabislaboratory Cannabislaboratory = CannabislaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            Console.WriteLine("debug: 4");
            if (Cannabislaboratory != null && Cannabislaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) < 1.0f)
            {
                Console.WriteLine("debug: 5");
                // Processing
                ComponentManager.Get<CannabislaboratoryStartWindow>().Show()(dbPlayer, Cannabislaboratory);
                Console.WriteLine("debug: 6");
                return true;
            }
       


            if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryBatterieSwitch) < 1.0f)
            {
                Console.WriteLine("debug: 15");
                int BatterieAmount = dbPlayer.Container.GetItemAmount(15);
                int addableAmount = BatterieAmount * 5;
                // 725 -> 966
                if (BatterieAmount >= 1)
                {
                    Console.WriteLine("debug: 16");
                    if (addableAmount > dbPlayer.Container.GetMaxItemAddedAmount(966))
                    {
                        addableAmount = dbPlayer.Container.GetMaxItemAddedAmount(966);
                    }
                    Console.WriteLine("debug: 17");
                    if (addableAmount > 0)
                    {
                        Console.WriteLine("debug: 18");
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            
                            Chats.sendProgressBar(dbPlayer, 100 * addableAmount);
                            
                            dbPlayer.Container.RemoveItem(15, addableAmount / 5);

                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            await Task.Delay(100 * addableAmount);
                            
                            if (dbPlayer == null || !dbPlayer.IsValid()) return;
                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            
                            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                            dbPlayer.Container.AddItem(966, addableAmount);

                            dbPlayer.SendNewNotification($"{addableAmount / 5} {ItemModelModule.Instance.Get(15).Name} wurde in {addableAmount} {ItemModelModule.Instance.Get(966).Name} zerlegt.");

                        }));
                        return true;
                    }
                }
            }
            
            if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryCannabisPulver) < 1.0f)
            {
                
                int Hanfsamenamount = dbPlayer.Container.GetItemAmount(979);
                int addableAmount = Hanfsamenamount / 2;
                // 979 -> 997
                if (Hanfsamenamount >= 2)
                {
                    
                    if (addableAmount > dbPlayer.Container.GetMaxItemAddedAmount(979))
                    {
                        addableAmount = dbPlayer.Container.GetMaxItemAddedAmount(979);
                    }
                    
                    if (addableAmount > 0)
                    {
                        
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            
                            Chats.sendProgressBar(dbPlayer, 500 * addableAmount);
                            
                            dbPlayer.Container.RemoveItem(979, addableAmount * 2);

                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);

                            await Task.Delay(500 * addableAmount);
                            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);

                            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                            dbPlayer.Container.AddItem(997, addableAmount);
                            
                            dbPlayer.SendNewNotification($"{addableAmount * 2} {ItemModelModule.Instance.Get(979).Name} wurde zu {addableAmount} {ItemModelModule.Instance.Get(997).Name} verarbeitet.");

                        }));
                        return true;
                    }
                }
            }
            
            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                string query = "";
                foreach (Cannabislaboratory Cannabislaboratory in GetAll().Values.ToList())
                {
                    uint fuelAmount = (uint)Cannabislaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in Cannabislaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            Cannabislaboratory.Processing(dbPlayer);
                        }
                        else
                            Cannabislaboratory.StopProcess(dbPlayer);
                    }
                    if (Cannabislaboratory.ActingPlayers.Count > 0)
                    {
                        Cannabislaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsWeedTeamId(dbPlayer.TeamId))
            {
                dbPlayer.CannabislaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.CANNABISLABORATORYINPUT);
                dbPlayer.CannabislaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.CANNABISLABORATORYOUTPUT);
            }
        }
        public void HackCannabislaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] != DimensionType.Cannabislaboratory) return;
            Cannabislaboratory Cannabislaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (Cannabislaboratory == null) return;
            Cannabislaboratory.HackLaboratory(dbPlayer);
        }

        public bool CanCannabislaboratyRaided(Cannabislaboratory Cannabislaboratory, DbPlayer dbPlayer)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(Cannabislaboratory.TeamId))) return false;
            if (TeamModule.Instance.Get(Cannabislaboratory.TeamId).Members.Count < 15 && !Cannabislaboratory.LaborMemberCheckedOnHack) return false;
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public Cannabislaboratory GetLaboratoryByDimension(uint dimension)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByJumppointId(int id)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Cannabislaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return CannabislaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
