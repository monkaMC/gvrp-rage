using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Gangwar;
using GVRP.Module.Items;
using GVRP.Module.Laboratories.Menu;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Teams;

namespace GVRP.Module.Laboratories
{
    public class WeaponlaboratoryModule : SqlModule<WeaponlaboratoryModule, Weaponlaboratory, uint>
    {
        public static List<uint> RessourceItemIds = new List<uint> { 300, 468, 470, 462, 464 }; //Eisenbarren, Plastik, Abzug, Alubarren, Broncebarren
        public static uint EndProductItemId = 976; //Waffenset
        public static uint FuelItemId = 537; //Benzin
        public static uint FuelAmountPerProcessing = 5; //Fuelverbrauch pro 15-Minuten-Kochvorgang (Spielerunabhängig)
        public List<Team> HasAlreadyHacked = new List<Team>();

        // Item Id, Price (Aufpreis)
        public Dictionary<uint, int> WeaponHerstellungList = new Dictionary<uint, int>()
        {
            { 996, 40000 }, // Advanced
            { 995, 65000 }, // Sniperrifle
            { 994, 15000 }, // shawnoffshotgun
            { 993, 15000 }, // pumpshotgun
            { 992, 15000 }, // doubleshotgun
            { 991, 15000 }, // smg
            { 990, 15000 }, // pdw
            { 989, 35000 }, // compact
            { 988, 50000 }, // Gusenberg
            { 987, 40000 }, // M4
            { 986, 40000 }, // Bullpup
            { 985, 35000 } // AK
        };

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_weaponlaboratories`";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(JumpPointModule) };
        }
        protected override void OnLoaded()
        {
            HasAlreadyHacked = new List<Team>();
            MenuManager.Instance.AddBuilder(new WeaponlaboratoryWeaponMenu());
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.DimensionType[0] != DimensionType.Weaponlaboratory) return false;

            Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetAll().Values.Where(laboratory => laboratory.TeamId == dbPlayer.Player.Dimension).FirstOrDefault();
            if (weaponlaboratory != null && weaponlaboratory.TeamId == dbPlayer.TeamId && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) < 1.0f)
            {
                // Processing
                if (weaponlaboratory.ActingPlayers.Contains(dbPlayer)) weaponlaboratory.StopProcess(dbPlayer);
                else weaponlaboratory.StartProcess(dbPlayer);
                return true;
            }

            if (weaponlaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryComputerPosition) < 1.0f)
            {
                if (weaponlaboratory.Hacked)
                {
                    MenuManager.Instance.Build(PlayerMenu.LaboratoryOpenInvMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }
            if (weaponlaboratory != null && dbPlayer.Player.Position.DistanceTo(Coordinates.WeaponlaboratoryWeaponBuildMenuPosition) < 1.0f)
            {
                MenuManager.Instance.Build(PlayerMenu.LaboratoryWeaponMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        public override void OnFifteenMinuteUpdate()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Random rnd = new Random();
                string query = "";
                foreach (Weaponlaboratory weaponlaboratory in GetAll().Values.ToList())
                {
                    uint fuelAmount = (uint)weaponlaboratory.FuelContainer.GetItemAmount(FuelItemId);
                    foreach (DbPlayer dbPlayer in weaponlaboratory.ActingPlayers.ToList())
                    {
                        if (dbPlayer == null || !dbPlayer.IsValid()) continue;
                        if (fuelAmount >= FuelAmountPerProcessing)
                        {
                            weaponlaboratory.Processing(dbPlayer);
                        }
                        else
                            weaponlaboratory.StopProcess(dbPlayer);
                    }
                    if (weaponlaboratory.ActingPlayers.Count > 0)
                    {
                        weaponlaboratory.FuelContainer.RemoveItem(FuelItemId, (int)FuelAmountPerProcessing);
                    }
                }
            }));
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if (TeamModule.Instance.IsWeaponTeamId(dbPlayer.TeamId))
            {
                dbPlayer.WeaponlaboratoryInputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WEAPONLABORATORYINPUT);
                dbPlayer.WeaponlaboratoryOutputContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WEAPONLABORATORYOUTPUT);
            }
        }
        public void HackWeaponlaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.DimensionType[0] != DimensionType.Weaponlaboratory) return;
            Weaponlaboratory weaponlaboratory = this.GetLaboratoryByDimension(dbPlayer.Player.Dimension);
            if (weaponlaboratory == null) return;
            weaponlaboratory.HackLaboratory(dbPlayer);
        }

        public bool CanWeaponLaboratyRaided(Weaponlaboratory weaponlaboratory, DbPlayer dbPlayer)
        {
            if (Configurations.Configuration.Instance.DevMode) return true;
            if (dbPlayer.IsACop() && dbPlayer.IsInDuty()) return true;
            if (GangwarTownModule.Instance.IsTeamInGangwar(TeamModule.Instance.Get(weaponlaboratory.TeamId))) return false;
            if (TeamModule.Instance.Get(weaponlaboratory.TeamId).Members.Count < 15 && !weaponlaboratory.LaborMemberCheckedOnHack) return false;
            // Geht nicht wenn in Gangwar, weniger als 10 UND der Typ kein Cop im Dienst ist (macht halt kein sinn wenn die kochen können < 10 und mans nicht hochnehmen kann (cops))
            return true;
        }

        public Weaponlaboratory GetLaboratoryByDimension(uint dimension)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == dimension).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByPosition(Vector3 position)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => position.DistanceTo(Lab.JumpPointEingang.Position) < 3.0f).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByJumppointId(int id)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.JumpPointEingang.Id == id).FirstOrDefault();
        }
        public Weaponlaboratory GetLaboratoryByTeamId(uint teamId)
        {
            return WeaponlaboratoryModule.Instance.GetAll().Values.Where(Lab => Lab.TeamId == teamId).FirstOrDefault();
        }
    }
}
