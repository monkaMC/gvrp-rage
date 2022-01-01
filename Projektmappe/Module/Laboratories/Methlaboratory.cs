using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Chat;
using System.Threading.Tasks;
using GVRP.Module.Teams;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Gangwar;
using GVRP.Module.Spawners;
using GVRP.Module.Injury;
using GVRP.Module.Teams.Shelter;

namespace GVRP.Module.Laboratories
{
    public class Methlaboratory : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint TeamId { get; set; }
        public Container FuelContainer { get; set; }
        public List<int> JumpPointIds { get; set; }
        public JumpPoint JumpPointEingang { get; set; }
        public JumpPoint JumpPointAusgang { get; set; }
        public Vector3 StartPosition { get; set; }

        public Production LabProduction { get; set; }
        public bool ReadyToStart { get; set; }
        public List<DbPlayer> ProzessingPlayers { get; set; }
        public List<DbPlayer> PlayersInsideLaboratory { get; set; }
        public Random Rnd { get; set; }
        public bool Hacked { get; set; }
        public bool HackInProgess { get; set; }
        public bool FriskInProgess { get; set; }
        public bool ImpoundInProgress { get; set; }
        public bool Security { get; set; }
        public List<Parameter> Parameters { get; set; }
        public List<Marker> Marker { get; set; }
        public List<TextLabel> TextLabels { get; set; }
        public List<Vector3> ExploPos { get; set; }
        public double Quality { get; set; }

        public bool LaborMemberCheckedOnHack = false;
        public DateTime LastQualityChanged { get; set; }

        public int CalculatedValue { get; set; }

        public class Parameter
        {
            public string Name { get; set; }
            public string Einheit { get; set; }
            public float MinValue { get; set; }
            public float MaxValue { get; set; }
            public float ActValue { get; set; }

            public Parameter(string name, string einheit, float minValue, float maxValue, float actValue)
            {
                Name = name;
                Einheit = einheit;
                MinValue = minValue;
                MaxValue = maxValue;
                ActValue = actValue;
            }
        }
        
        public class Production
        {
            public List<uint> NeededItems { get; set; }
            public List<uint> EndProducts { get; set; }
            public uint MinEndProduct { get; set; }
            public uint MaxEndProduct { get; set; }
          
            public Production(List<uint> neededItems, List<uint> endProducts, uint minEndProduct, uint maxEndProduct, float smellrangePerPlayer = 2.0f, float smellRangeOffset = 0.0f)
            {
                NeededItems = neededItems;
                EndProducts = endProducts;
                MinEndProduct = minEndProduct;
                MaxEndProduct = maxEndProduct;
            }
        }
        public Methlaboratory(MySqlDataReader reader) : base(reader)
        {
            Rnd = new Random();
            Id = reader.GetUInt32("id");
            TeamId = reader.GetUInt32("teamid");
            StartPosition = Coordinates.MethlaboratoryStartPosition;
            LastQualityChanged = DateTime.Now;
            CalculatedValue = reader.GetInt32("calculated_value");

            LabProduction = new Production(
                new List<uint>
                {
                    MethlaboratoryModule.RessourceItemIds[0], //Ephi
                    MethlaboratoryModule.RessourceItemIds[1], //Batterien
                    MethlaboratoryModule.RessourceItemIds[2]  //Toilettenreiniger
                },
                MethlaboratoryModule.EndProductItemIds,      //Meth
                5,      //Minimale Anzahl von Meth
                6      //Maximale Anzahl von Meth
            );

            Parameters = new List<Parameter>
            {
                new Parameter("Temperatur", "°C", 100.0f, 1500.0f, reader.GetFloat("temperatur")),
                new Parameter("Druck", "Bar", 1.0f, 10.0f, reader.GetFloat("druck")),
                new Parameter("Ruehrgeschwindigkeit", "U/min", 1.0f, 300.0f, reader.GetFloat("ruehrgeschwindigkeit")),
                new Parameter("Menge", "Stück", 5.0f, 15.0f, reader.GetFloat("menge")),
            };
                        
            int DestinationId = reader.GetInt32("destination_id");
            Random rnd = new Random();
            List<JumpPoint> jumpPoints = JumpPointModule.Instance.jumpPoints.Values.Where(jumppoint => jumppoint.DestinationId == DestinationId && jumppoint.Id != DestinationId).ToList();

            int selectedJumpPoint = rnd.Next(jumpPoints.Count);
            int i = 0;
            jumpPoints.ForEach(jumpPoint =>
            {

                if (selectedJumpPoint == i)
                {
                    JumpPointEingang = jumpPoint;
                    JumpPointAusgang = JumpPointModule.Instance.Get(jumpPoint.DestinationId);
                    JumpPointAusgang.Destination = JumpPointEingang;
                    JumpPointAusgang.DestinationId = JumpPointEingang.Id;
                }
                else
                {
                    NAPI.Task.Run(() =>
                    {
                        if(jumpPoint != null)
                        {
                            if (jumpPoint.ColShape != null)
                            {
                                jumpPoint.ColShape.ResetData("jumpPointId");
                                //jumpPoint.ColShape.Delete();
                                NAPI.ColShape.DeleteColShape(jumpPoint.ColShape);
                            }
                            if (jumpPoint.Object != null)
                            {
                                jumpPoint.Object.Delete();
                            }
                            jumpPoints.Remove(jumpPoint);
                        }
                    });
                }
                i++;
            });

            ColShape ColShape = ColShapes.Create(JumpPointAusgang.Position, 30.0f, this.TeamId);
            ColShape.SetData("methInteriorColshape", this.TeamId);

            bool ready = true;
            
            ReadyToStart = ready;
            Hacked = false;
            HackInProgess = false;
            FriskInProgess = false;
            ImpoundInProgress = false;
            ProzessingPlayers = new List<DbPlayer>();
            PlayersInsideLaboratory = new List<DbPlayer>();
            Marker = new List<Marker>();
            TextLabels = new List<TextLabel>();
            FuelContainer = ContainerManager.LoadContainer(Id, ContainerTypes.METHLABORATORYFUEL, 0);
            CreateMarker();

            // Init Quality
            CalculateNewQuality();
        }
        public void StartProcess(DbPlayer dbPlayer)
        {
            int menge = 10;

            foreach (uint itemId in  LabProduction.NeededItems)
            {
                if (dbPlayer.MethlaboratoryInputContainer.GetItemAmount(itemId) < menge)
                {
                    StopProcess(dbPlayer);
                    dbPlayer.SendNewNotification("Es fehlen Materialien... ( " + menge + " " + ItemModelModule.Instance.GetById(itemId).Name + " )");
                    return;
                }
                uint fuelAmount = (uint)FuelContainer.GetItemAmount(MethlaboratoryModule.FuelItemId);
                if(fuelAmount < MethlaboratoryModule.FuelAmountPerProcessing)
                {
                    dbPlayer.SendNewNotification("Es fehlt Kraftstoff...");
                    StopProcess(dbPlayer);
                    return;
                }
            }
            if (ProzessingPlayers.ToList().Contains(dbPlayer))
            {
                dbPlayer.SendNewNotification("Der Prozess ist bereits im Gange...");
                return;
            }
            ProzessingPlayers.Add(dbPlayer);
            dbPlayer.SendNewNotification("Prozess gestartet!");
        }
        public void StopProcess(DbPlayer dbPlayer)
        {
            if (ProzessingPlayers.ToList().Contains(dbPlayer))
            {
                ProzessingPlayers.Remove(dbPlayer);
                if (dbPlayer.DimensionType[0] == DimensionType.Methlaboratory && dbPlayer.Player.Dimension != 0)
                    dbPlayer.SendNewNotification("Prozess gestoppt!");
            }
        }

        public void CalculateNewQuality()
        {
            if (CalculatedValue == 0) GenerateNewCalculatedValue();

            int SumMin = Convert.ToInt32(Parameters[0].MinValue + Parameters[1].MinValue + Parameters[2].MinValue + Parameters[3].MinValue);
            int SumMax = Convert.ToInt32(Parameters[0].MaxValue + Parameters[1].MaxValue + Parameters[2].MaxValue + Parameters[3].MaxValue);

            int SumActual = Convert.ToInt32(Parameters[0].ActValue + Parameters[1].ActValue + Parameters[2].ActValue + Parameters[3].ActValue);

            int QualityRange1 = Convert.ToInt32((SumMax - SumMin) * 0.04); // 8 % 
            int QualityRange2 = Convert.ToInt32((SumMax - SumMin) * 0.10); // 20 % 
            int QualityRange3 = Convert.ToInt32((SumMax - SumMin) * 0.20); // 40 % 
            
            if (Math.Abs(SumActual - CalculatedValue) <= QualityRange1) Quality = 0.99; // Best
            else if (Math.Abs(SumActual - CalculatedValue) > QualityRange1 && Math.Abs(SumActual - CalculatedValue) <= QualityRange2) Quality = 0.8; // Good
            else if (Math.Abs(SumActual - CalculatedValue) > QualityRange2 && Math.Abs(SumActual - CalculatedValue) <= QualityRange3) Quality = 0.7; // Normal
            else if (Math.Abs(SumActual - CalculatedValue) > QualityRange3) Quality = 0.4; // Bad

            return;
        }

        public void GenerateNewCalculatedValue()
        {
            int SumMin = Convert.ToInt32(Parameters[0].MinValue + Parameters[1].MinValue + Parameters[2].MinValue + Parameters[3].MinValue);
            int SumMax = Convert.ToInt32(Parameters[0].MaxValue + Parameters[1].MaxValue + Parameters[2].MaxValue + Parameters[3].MaxValue);

            Random random = new Random();

            CalculatedValue = random.Next(SumMin, SumMax);

            MySQLHandler.ExecuteAsync($"UPDATE `team_methlaboratories` SET `calculated_value` = '{CalculatedValue}' WHERE id = '{Id}';");
            return;
        }

        private void GivePlayerItem(DbPlayer dbPlayer, double quality)
        {
            uint itemId = 0;

            switch (quality)
            {
                case double qual when qual < 0.5:
                    itemId = 726;
                    break;
                case double qual when qual < 0.75:
                    itemId = 727;
                    break;
                case double qual when qual < 0.98:
                    itemId = 728;
                    break;
                case double qual when qual >= 0.98:
                    itemId = 729;
                    break;
            }

            if (quality <= 0.01)
            {
                quality = 0.01;
                itemId = 726;
            }
            else if(quality > 1)
            {
                quality = 1;
            }

            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>
            {
                { "quality", quality },
                { "amount", 50 }
            };

            dbPlayer.MethlaboratoryOutputContainer.AddItem(itemId, 1, data);
        }
        public void Processing(DbPlayer dbPlayer)
        {
            if (!ProzessingPlayers.ToList().Contains(dbPlayer)) return;

            float menge = 10;

            foreach (uint itemId in LabProduction.NeededItems)
            {
                if (dbPlayer.MethlaboratoryInputContainer.GetItemAmount(itemId) < menge || !dbPlayer.MethlaboratoryOutputContainer.CanInventoryItemAdded(LabProduction.EndProducts.ElementAt(0), 1))
                {
                    StopProcess(dbPlayer);
                    return;
                }
            }
            foreach (uint itemId in LabProduction.NeededItems)
                dbPlayer.MethlaboratoryInputContainer.RemoveItem(itemId, (int)menge);
            
            GivePlayerItem(dbPlayer, Quality);
        }
       
        private void ReloadInterior(DbPlayer dbPlayer)
        {
            int boilerState = 2;
            int tableState = 1;
            int securityState = 1;
            dbPlayer.Player.TriggerEvent("loadMethInterior", tableState, boilerState, securityState);
            CreateMarker();
        }
        public void LoadInterior(DbPlayer dbPlayer)
        {
            int boilerState = 2;
            int tableState = 1;
            int securityState = 1;

            dbPlayer.Player.TriggerEvent("loadMethInterior", tableState, boilerState, securityState);

            if (!PlayersInsideLaboratory.Contains(dbPlayer))
            {
                PlayersInsideLaboratory.Add(dbPlayer);
            }
        }
        public void UnloadInterior(DbPlayer dbPlayer)
        {
            if (PlayersInsideLaboratory.Contains(dbPlayer))
            {
                PlayersInsideLaboratory.Remove(dbPlayer);
            }
        }

        public void CreateMarker()
        {
            DeleteMarker();
            Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryLaptopPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId));
            Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvUpgradePosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId));
            if (this.ReadyToStart)
            {
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryStartPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvInputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvOutputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryInvFuelPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryAnalyzePosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 0, 255, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryBatterieSwitch - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryEphePulver - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId));
                Marker.Add(NAPI.Marker.CreateMarker(25, (Coordinates.MethlaboratoryCheckBoilerQuality - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId));
            }
        }
        public void DeleteMarker()
        {
            Marker.ForEach(marker => marker.Delete());
        }

        public void DeleteTextLabels()
        {
            TextLabels.ForEach(textLabel => textLabel.Delete());
        }

        public async Task<bool> HackMethlaboratory(DbPlayer dbPlayer)
        {
            if (MethlaboratoryModule.Instance.HasAlreadyHacked.Contains(dbPlayer.Team))
            {
                dbPlayer.SendNewNotification("Deine Fraktion hat bereits ein Labor gehackt...");
                return false;
            }

            if (HackInProgess)
            {
                dbPlayer.SendNewNotification("Das Labor wird bereits gehackt...");
                return false;
            }
            if (Hacked)
            {
                if (dbPlayer.TeamId != this.TeamId)
                {
                    dbPlayer.SendNewNotification("Das Labor ist bereits gehackt worden...");
                    return false;
                }
            }
            if(!MethlaboratoryModule.Instance.CanMethLaboratyRaided(this, dbPlayer))
            {
                dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                return false;
            }
            HackInProgess = true;
            int timeToHack = LaboratoryModule.TimeToHack;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Methlabor wird gehackt...", 30000);
            timeToHack = timeToHack * 3;

            dbPlayer.SendNewNotification("Labor wird gehackt...");
            Chats.sendProgressBar(dbPlayer, timeToHack);
            dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            await Task.Delay(timeToHack);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                HackInProgess = false;
                return false;
            }
            dbPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            
            dbPlayer.SendNewNotification("Labor erfolgreich gehackt...");

            if (ProzessingPlayers.Count() > 0) ProzessingPlayers.Clear();

            if (dbPlayer.IsAGangster())
            {
                MethlaboratoryModule.Instance.HasAlreadyHacked.Add(dbPlayer.Team);
            }

            Hacked = true;
            HackInProgess = false;
            LaborMemberCheckedOnHack = false;
            return true;

        }
        
        public async Task<bool> FriskMethlaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!Hacked)
            {
                dbPlayer.SendNewNotification("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (FriskInProgess)
            {
                dbPlayer.SendNewNotification("Das Labor wird schon durchsucht...");
                return false;
            }
            FriskInProgess = true;

            Dictionary<int, int> itemsFound = new Dictionary<int, int>();
            foreach (int itemId in LabProduction.NeededItems)
            {
                itemsFound.Add(itemId, 0);
            }
            foreach (int itemId in LabProduction.EndProducts)
            {
                itemsFound.Add(itemId, 0);
            }
            bool found = false;
            int timeToFrisk = LaboratoryModule.TimeToFrisk;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Methlabor wird durchsucht...", 60000);
            timeToFrisk = timeToFrisk * 3;

            Chats.sendProgressBar(dbPlayer, timeToFrisk);
            dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            await Task.Delay(timeToFrisk);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                FriskInProgess = false;
                return false;
            }
            dbPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            foreach (KeyValuePair<uint, DbPlayer> kvp in TeamModule.Instance.GetById((int)TeamId).Members)
            {
                if (!kvp.Value.MethlaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.MethlaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.Value.Model.Id] += kvpSlots.Value.Amount;
                            found = true;
                        }
                    }
                }
                if (!kvp.Value.MethlaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.MethlaboratoryOutputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.Value.Model.Id] += kvpSlots.Value.Amount;
                            found = true;
                        }
                    }
                }
            }
            if (found)
            {
                string info = "Funde: ";
                foreach (KeyValuePair<int, int> kvp in itemsFound)
                    info += kvp.Value + " " + ItemModelModule.Instance.GetById((uint)kvp.Key).Name + ", ";
                info = info.Substring(0, info.Length - 1);
                dbPlayer.SendNewNotification(info);
            }
            else
            {
                dbPlayer.SendNewNotification("Nichts gefunden");
            }
            FriskInProgess = false;
            return false;
        }
        
        public async Task<bool> ImpoundMethlaboratory(DbPlayer dbPlayer)
        {
            if (dbPlayer.TeamId == 0)
            {
                return false;
            }
            if (!Hacked)
            {
                dbPlayer.SendNewNotification("Das Labor muss zuerst gehackt werden...");
                return false;
            }
            if (ImpoundInProgress)
            {
                dbPlayer.SendNewNotification("Die Laborinhalte werden schon beschlagnahmt...");
                return false;
            }
            ImpoundInProgress = true;

            Dictionary<int, int> itemsImpounded = new Dictionary<int, int>();
            foreach (int itemId in LabProduction.NeededItems)
            {
                itemsImpounded.Add(itemId, 0);
            }
            foreach(int itemId in LabProduction.EndProducts)
            {
                itemsImpounded.Add(itemId, 0);
            }
            bool impounded = false;
            SxVehicle closestVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(JumpPointEingang.Position, (int)dbPlayer.TeamId, 15.0f);
            if (closestVeh == null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein passendes Fraktionsfahrzeug vor dem Tor...");
                ImpoundInProgress = false;
                return false;
            }
            if (!(closestVeh.entity.Model == (uint)VehicleHash.Brickade ||
                    closestVeh.entity.Model == (uint)VehicleHash.Burrito ||
                    closestVeh.entity.Model == (uint)VehicleHash.Burrito2 ||
                    closestVeh.entity.Model == (uint)VehicleHash.Burrito3 ||
                    closestVeh.entity.Model == (uint)VehicleHash.Burrito4 ||
                    closestVeh.entity.Model == (uint)VehicleHash.Burrito5 ||
                    closestVeh.entity.Model == (uint)VehicleHash.GBurrito ||
                    closestVeh.entity.Model == (uint)VehicleHash.GBurrito2)
                    && closestVeh.teamid == dbPlayer.TeamId && 
                    (!closestVeh.entity.HasData("Door_KRaum") || 
                    closestVeh.entity.GetData("Door_KRaum") != 1) && closestVeh.Container != null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein Lagerfahrzeug (Burrito, Brickade, ...) mit offenem Kofferraum.");
                ImpoundInProgress = false;
                return false;
            }
            int timeToImpound = LaboratoryModule.TimeToImpound;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Die Inhalte des Methlabors werden entwendet...", 60000);
            timeToImpound = timeToImpound * 3;

            Chats.sendProgressBar(dbPlayer, timeToImpound);
            dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            await Task.Delay(timeToImpound);
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract())
            {
                ImpoundInProgress = false;
                return false;
            }
            dbPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            closestVeh = VehicleHandler.Instance.GetClosestVehicleFromTeam(JumpPointEingang.Position, (int)dbPlayer.TeamId, 15.0f);
            if (closestVeh == null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein passendes Fraktionsfahrzeug vor dem Tor...");
                ImpoundInProgress = false;
                return false;
            }
            if (!LaboratoryModule.Instance.IsImpoundVehicle(closestVeh.entity.Model) || closestVeh.teamid != dbPlayer.TeamId ||
                    !closestVeh.entity.HasData("Door_KRaum") || closestVeh.entity.GetData("Door_KRaum") != 1 || closestVeh.Container == null)
            {
                dbPlayer.SendNewNotification("Du benötigst ein Lagerfahrzeug (Burrito, Brickade, ...) mit offenem Kofferraum.");
                ImpoundInProgress = false;
                return false;
            }
            foreach (KeyValuePair<uint, DbPlayer> kvp in TeamModule.Instance.GetById((int)TeamId).Members)
            {
                if (!kvp.Value.MethlaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.MethlaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount) || (dbPlayer.IsACop() && dbPlayer.IsInDuty()))
                            {
                                if(!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Model.Id, kvpSlots.Value.Amount);
                                itemsImpounded[(int)kvpSlots.Value.Model.Id] = itemsImpounded[(int)kvpSlots.Value.Model.Id] + kvpSlots.Value.Amount;
                                kvp.Value.MethlaboratoryInputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
                                impounded = true;
                            }
                        }
                    }
                }
                if (!kvp.Value.MethlaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.MethlaboratoryOutputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount))
                            {
                                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Id, kvpSlots.Value.Amount, kvpSlots.Value.Data);
                                itemsImpounded[(int)kvpSlots.Value.Id] = itemsImpounded[(int)kvpSlots.Value.Id] + kvpSlots.Value.Amount;
                                kvp.Value.MethlaboratoryOutputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
                                impounded = true;
                            }
                        }
                    }
                }
            }

            if (impounded == true)
            {
                string info = "Waren: ";
                foreach (KeyValuePair<int, int> kvp in itemsImpounded)
                {
                    info += kvp.Value + " " + ItemModelModule.Instance.GetById((uint)kvp.Key).Name + ", ";
                }
                info = info.Substring(0, info.Length - 1);
                dbPlayer.SendNewNotification(info);
            }
            else
            {
                dbPlayer.SendNewNotification("Nichts gefunden oder kein Platz im Fahrzeug");
            }
            ImpoundInProgress = false;
            return false;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
