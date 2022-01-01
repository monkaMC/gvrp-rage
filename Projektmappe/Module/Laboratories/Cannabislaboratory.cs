using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Teams;

namespace GVRP.Module.Laboratories
{
    public class Cannabislaboratory : Loadable<uint>
    {
        public uint Id { get; }

        public uint TeamId { get; }

        public uint DestinationId { get; }
        public JumpPoint JumpPointEingang { get; set; }
        public JumpPoint JumpPointAusgang { get; set; }

        public List<DbPlayer> ActingPlayers { get; set; }

        public Container FuelContainer { get; set; }
        public List<Marker> Marker { get; set; }
        public List<Parameter> Parameters { get; set; }

        public bool HackInProgess = false;
        public bool Hacked = false;
        public bool FriskInProgess = false;
        public bool ImpoundInProgress = false;
        public bool LaborMemberCheckedOnHack = false;
        public double Quality { get; set; }
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

        public Cannabislaboratory(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            TeamId = reader.GetUInt32("teamid");
            DestinationId = reader.GetUInt32("destination_id");
            ActingPlayers = new List<DbPlayer>();
            FuelContainer = ContainerManager.LoadContainer(Id, ContainerTypes.CANNABISLABORATORYFUEL);
            HackInProgess = false;
            Hacked = false;
            FriskInProgess = false;
            ImpoundInProgress = false;
            CalculatedValue = reader.GetInt32("calculated_value");

            LastQualityChanged = DateTime.Now;

            Parameters = new List<Parameter>
            {
                new Parameter("Temperatur", "°C", 5.0f, 40.0f, reader.GetFloat("temperatur")),
                new Parameter("UV Lampenstärke", "Watt", 1.0f, 30.0f, reader.GetFloat("uvenergy")),
                new Parameter("Luftfeuchtigkeit", "%", 1.0f, 90.0f, reader.GetFloat("luftfeuchtigkeit")),
                new Parameter("Dünger", "Stück", 5.0f, 15.0f, reader.GetFloat("duenger")),
            };

            // Init Quality
            CalculateNewQuality();

            List<JumpPoint> JumpPoints = JumpPointModule.Instance.jumpPoints.Values.Where(jp => jp.DestinationId == DestinationId && jp.Id != DestinationId).ToList();


            Random rnd = new Random();
            int selectedJumpPoint = rnd.Next(JumpPoints.Count);
            int i = 0;
            JumpPoints.ForEach(jumpPoint =>
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
                        if (jumpPoint != null)
                        {
                            if (jumpPoint.ColShape != null)
                            {
                                jumpPoint.ColShape.ResetData("jumpPointId");
                                NAPI.ColShape.DeleteColShape(jumpPoint.ColShape);
                            }
                            if (jumpPoint.Object != null)
                            {
                                jumpPoint.Object.Delete();
                            }
                            JumpPoints.Remove(jumpPoint);
                        }
                    });
                }
                i++;
            });

            // Inventory Markers
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryInvFuelPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryInvInputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryInvOutputPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, TeamId);

            // E Markers
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryComputerPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryBatterieSwitch - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryCannabisPulver - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
            NAPI.Marker.CreateMarker(25, (Coordinates.CannabislaboratoryCheckBoilerQuality - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, TeamId);
        }
        public override uint GetIdentifier()
        {
            return Id;
        }

        public void Processing(DbPlayer dbPlayer)
        {
            if (!ActingPlayers.ToList().Contains(dbPlayer)) return;

            foreach (uint itemId in CannabislaboratoryModule.RessourceItemIds)
            {
                if (dbPlayer.CannabislaboratoryInputContainer.GetItemAmount(itemId) < 10 ||
                    !dbPlayer.CannabislaboratoryOutputContainer.CanInventoryItemAdded(CannabislaboratoryModule.EndProductItemIds.ElementAt(0)))
                {
                    StopProcess(dbPlayer);
                    return;
                }
            }
            foreach (uint itemId in CannabislaboratoryModule.RessourceItemIds)
                dbPlayer.CannabislaboratoryInputContainer.RemoveItem(itemId, 10);

            GivePlayerItem(dbPlayer, Quality);
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

            MySQLHandler.ExecuteAsync($"UPDATE `team_cannabislaboratories` SET `calculated_value` = '{CalculatedValue}' WHERE id = '{Id}';");
            return;
        }

        public void StopProcess(DbPlayer dbPlayer)
        {
            if (ActingPlayers.ToList().Contains(dbPlayer))
            {
                ActingPlayers.Remove(dbPlayer);
                if (dbPlayer.DimensionType[0] == DimensionType.Cannabislaboratory && dbPlayer.Player.Dimension != 0)
                    dbPlayer.SendNewNotification("Prozess gestoppt!");
            }
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
                    itemId = 726;
                    break;
                case double qual when qual < 0.98:
                    itemId = 726;
                    break;
                case double qual when qual >= 0.98:
                    itemId = 726;
                    break;
            }
            if (quality <= 0.01)
            {
                quality = 0.01;
                itemId = 726;
            }
            else if (quality > 1)
            {
                quality = 1;
            }
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>
            {
                { "quality", quality },
                { "amount", 50 }
            };

            dbPlayer.CannabislaboratoryOutputContainer.AddItem(itemId, 1, data);
        }

        public void StartProcess(DbPlayer dbPlayer)
        {
            int menge = 10;

            foreach (uint itemId in CannabislaboratoryModule.RessourceItemIds)
            {
                if (dbPlayer.CannabislaboratoryInputContainer.GetItemAmount(itemId) < menge)
                {
                    StopProcess(dbPlayer);
                    dbPlayer.SendNewNotification("Es fehlen Materialien... ( " + menge + " " + ItemModelModule.Instance.GetById(itemId).Name + " )");
                    return;
                }
                uint fuelAmount = (uint)FuelContainer.GetItemAmount(CannabislaboratoryModule.FuelItemId);
                if (fuelAmount < CannabislaboratoryModule.FuelAmountPerProcessing)
                {
                    dbPlayer.SendNewNotification("Es fehlt Kraftstoff...");
                    StopProcess(dbPlayer);
                    return;
                }
            }
            if (ActingPlayers.ToList().Contains(dbPlayer))
            {
                dbPlayer.SendNewNotification("Der Prozess ist bereits im Gange...");
                return;
            }
            ActingPlayers.Add(dbPlayer);
            dbPlayer.SendNewNotification("Prozess gestartet!");
        }

        public async Task<bool> HackLaboratory(DbPlayer dbPlayer)
        {
            if (CannabislaboratoryModule.Instance.HasAlreadyHacked.Contains(dbPlayer.Team))
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
            if (!CannabislaboratoryModule.Instance.CanCannabislaboratyRaided(this, dbPlayer))
            {
                dbPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                return false;
            }
            LaborMemberCheckedOnHack = true;
            int timeToHack = LaboratoryModule.TimeToHack;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Labor wird gehackt...", 30000);
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

            if (ActingPlayers.Count() > 0) ActingPlayers.Clear();

            if (dbPlayer.IsAGangster())
            {
                CannabislaboratoryModule.Instance.HasAlreadyHacked.Add(dbPlayer.Team);
            }

            Hacked = true;
            HackInProgess = false;
            LaborMemberCheckedOnHack = false;
            return true;

        }

        public async Task<bool> FriskLaboratory(DbPlayer dbPlayer)
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
            foreach (int itemId in CannabislaboratoryModule.RessourceItemIds)
            {
                if (itemsFound.ContainsKey(itemId)) continue;
                itemsFound.Add(itemId, 0);
            }
            foreach (int itemId in CannabislaboratoryModule.EndProductItemIds)
            {
                if (itemsFound.ContainsKey(itemId)) continue;
                itemsFound.Add(itemId, 0);
            }

            bool found = false;
            int timeToFrisk = LaboratoryModule.TimeToFrisk;
            timeToFrisk = timeToFrisk * 3;

            TeamModule.Instance.Get(this.TeamId).SendNotification("Das Labor wird durchsucht...", 60000);

            Chats.sendProgressBar(dbPlayer, timeToFrisk);
            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
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
                if (!kvp.Value.CannabislaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.CannabislaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            itemsFound[(int)kvpSlots.Value.Model.Id] += kvpSlots.Value.Amount;
                            found = true;
                        }
                    }
                }
                if (!kvp.Value.CannabislaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.CannabislaboratoryOutputContainer.Slots.ToList())
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

        public async Task<bool> ImpoundLaboratory(DbPlayer dbPlayer)
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
            foreach (int itemId in CannabislaboratoryModule.RessourceItemIds)
            {
                if (itemsImpounded.ContainsKey(itemId)) continue;
                itemsImpounded.Add(itemId, 0);
            }
            foreach (int itemId in CannabislaboratoryModule.EndProductItemIds)
            {
                if (itemsImpounded.ContainsKey(itemId)) continue;
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

            TeamModule.Instance.Get(this.TeamId).SendNotification("Die Inhalte des Labors werden entwendet...", 60000);
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
            foreach (KeyValuePair<uint, DbPlayer> kvp in TeamModule.Instance.GetById((int)TeamId).Members)
            {
                if (!kvp.Value.CannabislaboratoryInputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.CannabislaboratoryInputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount) || (dbPlayer.IsACop() && dbPlayer.IsInDuty()))
                            {
                                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Model.Id, kvpSlots.Value.Amount);
                                itemsImpounded[(int)kvpSlots.Value.Model.Id] = itemsImpounded[(int)kvpSlots.Value.Model.Id] + kvpSlots.Value.Amount;
                                kvp.Value.CannabislaboratoryInputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
                                impounded = true;
                            }
                        }
                    }
                }
                if (!kvp.Value.CannabislaboratoryOutputContainer.IsEmpty())
                {
                    foreach (KeyValuePair<int, Item> kvpSlots in kvp.Value.CannabislaboratoryOutputContainer.Slots.ToList())
                    {
                        if (kvpSlots.Value != null & kvpSlots.Value.Amount > 0)
                        {
                            if (closestVeh.Container.CanInventoryItemAdded(kvpSlots.Value.Id, kvpSlots.Value.Amount))
                            {
                                if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty())
                                    closestVeh.Container.AddItem(kvpSlots.Value.Id, kvpSlots.Value.Amount, kvpSlots.Value.Data);
                                itemsImpounded[(int)kvpSlots.Value.Id] = itemsImpounded[(int)kvpSlots.Value.Id] + kvpSlots.Value.Amount;
                                kvp.Value.CannabislaboratoryOutputContainer.RemoveFromSlot(kvpSlots.Key, kvpSlots.Value.Amount);
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
    }
}
