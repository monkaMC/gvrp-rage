using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
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
    class MethlaboratoryModule : SqlModule<MethlaboratoryModule, Methlaboratory, uint>
    {
        public List<ColShape> InteriorColShapes = new List<ColShape>();
        public static List<uint> RessourceItemIds = new List<uint> { 14, 966, 965 }; //Toilettenreiniger, Batterien, Ephedrinkonzentrat (965) 
        public static List<uint> EndProductItemIds = new List<uint> { 726, 727, 728, 729 }; //Pures Meth
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        
        public static int temp = 0;
        public static uint RankNeededForParameter = 9;
        public string PlayerIds = "";
        public int AmountPerProcess = 0;
        public List<Team> HasAlreadyHacked = new List<Team>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_methlaboratories`"; //Random in Query rein
        }
        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }

        protected override void OnLoaded()
        {
            HasAlreadyHacked = new List<Team>();
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return;
            Methlaboratory methlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return;
            if (methlaboratory.ProzessingPlayers.Contains(dbPlayer))
                methlaboratory.ProzessingPlayers.Remove(dbPlayer);
        }


        private bool StartMenu(DbPlayer dbPlayer, Methlaboratory methlaboratory)
        {
            if (methlaboratory.TeamId != dbPlayer.TeamId) return false;
            //MenuManager.Instance.Build(PlayerMenu.MethlaboratoryStartMenu, dbPlayer).Show(dbPlayer);
            ComponentManager.Get<MethlaboratoryStartWindow>().Show()(dbPlayer, methlaboratory);
            return true;
        }
        
        public override void OnMinuteUpdate()
        {
            if (Configurations.Configuration.Instance.DevMode)
                OnFifteenMinuteUpdate();
        }

        public override void OnFifteenMinuteUpdate()
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return;
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                string query = "";
                foreach (Methlaboratory methlaboratory in MethlaboratoryModule.Instance.GetAll().Values)
                {
                    PlayerIds = "";
                    AmountPerProcess = 0;
                    uint fuelAmount = (uint)methlaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in methlaboratory.ProzessingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            methlaboratory.Processing(dbPlayer);
                        }
                        else
                            methlaboratory.StopProcess(dbPlayer);
                    }
                    if (methlaboratory.ProzessingPlayers.Count > 0)
                    {
                        methlaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                        string qualityString = methlaboratory.Quality.ToString();
                        qualityString = qualityString.Replace(",", ".");
                        if(PlayerIds.Length >= 3)
                            query += $"INSERT INTO `log_methlaboratory` (`team_id`, `player_ids`, `quality`, `amount`, `temperatur`, `druck`, `ruehrgeschwindigkeit`, `menge`) VALUES ('{methlaboratory.TeamId}', '{PlayerIds.Substring(0, PlayerIds.Length - 2)}', '{qualityString}', '{AmountPerProcess}', '{methlaboratory.Parameters[0].ActValue}', '{methlaboratory.Parameters[1].ActValue}', '{methlaboratory.Parameters[2].ActValue}', '{methlaboratory.Parameters[3].ActValue}');";
                    }
                }
                if (!query.Equals("")) MySQLHandler.ExecuteAsync(query);
            }));
        }
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            if (!colShape.HasData("methInteriorColshape")) return false;
            if (colShapeState == ColShapeState.Enter)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    Methlaboratory methlaboratory = GetLaboratoryByDimension(dbPlayer.Player.Dimension);
                    if (methlaboratory == null) return false;
                    methlaboratory.LoadInterior(dbPlayer);
                    return true;
                }
            }
            if (colShapeState == ColShapeState.Exit)
            {
                if (dbPlayer.HasData("inMethLaboraty"))
                {
                    Methlaboratory methlaboratory = GetLaboratoryByDimension(colShape.Dimension);
                    if (methlaboratory == null)
                    {
                        return false;
                    }
                    methlaboratory.UnloadInterior(dbPlayer);
                    return true;
                }
            }
            return false;
        }
        public void HackMethlaboratory(DbPlayer dbPlayer)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return;
            if (dbPlayer.DimensionType[0] != DimensionType.Methlaboratory) return;
            Methlaboratory methlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (methlaboratory == null) return;
            methlaboratory.HackMethlaboratory(dbPlayer);
        }
        
        public bool CanMethLaboratyRaided(Methlaboratory methlaboratory, DbPlayer dbPlayer)
        {
            if (!Configurations.Configuration.Instance.MethLabEnabled) return false;
            //if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(methlaboratory.TeamId))) return false;
            if (Configurations.Configuration.Instance.DevMode)
            {
                if (TeamModule.Instance.Get(methlaboratory.TeamId).Members.Count < 3 && !methlaboratory.LaborMemberCheckedOnHack) return false;
            }
            else
            {
                if (TeamModule.Instance.Get(methlaboratory.TeamId).Members.Count < 15 && !methlaboratory.LaborMemberCheckedOnHack) return false;
            }
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsMethTeamId(dbPlayer.TeamId))
            {
                dbPlayer.MethlaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.METHLABORATORYOUTPUT);
                dbPlayer.MethlaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.METHLABORATORYINPUT);
            }
        }

        public Methlaboratory GetLaboratoryByDimension(uint dimension)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByJumppointId(int id)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Methlaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return MethlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
