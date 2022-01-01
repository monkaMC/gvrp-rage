using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Chat;
using GVRP.Module.Crime;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.Shops;
using GVRP.Module.Teams;
using GVRP.Module.Voice;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Zone;

namespace GVRP.Module.Robbery
{
    public sealed class RobberyModule : Module<RobberyModule>
    {
        // Global Defines

        public static uint MarkierteScheineID = 881;

        public const int Juwelier = -2;

        public Dictionary<int, Rob> Robberies;
        
        public override bool Load(bool reload = false)
        {
            Robberies = new Dictionary<int, Rob>();
            return true;
        }
        
        public bool CanJuweRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (StaatsbankRobberyModule.Instance.IsActive)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 30)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }
        
        public Rob Get(int id)
        {
            return Robberies.TryGetValue(id, out var rob) ? rob : null;
        }
        
        public void Add(int id, DbPlayer iPlayer, int startinterval, RobType type = RobType.Shop, int copinterval = 2, int endinterval = 30)
        {
            if (Robberies.ContainsKey(id))
            {
                Robberies[id].Player = iPlayer;
            }
            else
            {
                var rob = new Rob
                {
                    Id = id,
                    Player = iPlayer,
                    Interval = startinterval,
                    CopInterval = copinterval,
                    EndInterval = endinterval,
                    Disabled = false,
                    Type = type
                };
                Robberies.Add(id, rob);
            }
        }

        public void RemovePlayerRobs(DbPlayer dbPlayer)
        {
            var index = 0;
            while (index < Robberies.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = Robberies.ElementAt(index++).Value;
                if (rob.Player.Id == dbPlayer.Id)
                {
                    Robberies.Remove(rob.Id);
                }
            }
        }

        public bool IsActive(int robid)
        {
            var rob = Get(robid);
            if (rob == null) return false;
            return !rob.Disabled && rob.Interval > 0;
        }

        public List<Rob> GetActiveShopRobs()
        {
            return (from rob in Robberies
                    where rob.Value.Id > 0 && rob.Value.Type == RobType.Shop && rob.Value.Player != null &&
                          !rob.Value.Disabled
                    select rob.Value).ToList();
        }

        public bool IsAnyShopInRobbing()
        {
            return Robberies.Any(rob =>
                rob.Value.Id > 0 && !rob.Value.Disabled && rob.Value.Interval > 0 && rob.Value.Player != null &&
                rob.Value.Type == RobType.Shop);
        }

        public Dictionary<int, Rob> GetRobs()
        {
            return Robberies;
        }

        public List<Rob> GetActiveRobs(bool displayonly = false)
        {
            return (from rob in Robberies where !rob.Value.Disabled && (rob.Value.Interval > 2 || rob.Value.Type != RobType.Shop || !displayonly) && rob.Value.Player != null select rob.Value)
                .ToList();
        }

        public override void OnMinuteUpdate()
        {
            var rnd = new Random();
            var hour = DateTime.Now.Hour;
            
            var robs = GetActiveRobs();
            if (robs == null) return;
            var index = 0;
            while (index < robs.Count)
            {
                if (index >= Robberies.Count) break;
                var rob = robs[index++];
                if (rob == null) continue;

                var iPlayer = rob.Player;
                // Cancelrob On missing Player?!
                if (iPlayer == null || iPlayer.Player == null)
                {
                    rob.Disabled = true;
                }
                else
                {
                    iPlayer.IsInRob = true;
                    if (rob.Type == RobType.Juwelier)
                    {
                        if (rob.Player.Player.Position.DistanceTo(new Vector3(-622.5494, -229.5598, 38.05706)) < 10.0f && !iPlayer.isInjured())
                        {
                            if (rob.Interval > 0)
                            {
                                if (rob.Interval == 19)
                                {
                                    iPlayer.SendNewNotification(

                                        "Du wurdest bei dem Raubueberfall von den Kameras aufgezeichnet!");
                                    iPlayer.SendNewNotification(

                                        "Deine Identitaet wurde nicht festgestellt, allerdings die Polizei verstaendigt.");
                                }
                                if (rob.Interval < 17)
                                {
                                    var erhalt = rnd.Next(5, 9);
                                    if (!iPlayer.Container.AddItem(21, erhalt))
                                    {
                                        iPlayer.SendNewNotification("Dein Inventar ist leider voll!");
                                    }
                                }
                                rob.Interval--;
                            }
                            else
                            {
                                var erhalt = rnd.Next(75, 90);
                                if (!iPlayer.Container.AddItem(21, erhalt))
                                {
                                    iPlayer.SendNewNotification("Deine Taschen sind jetzt randvoll!");
                                }
                                iPlayer.SendNewNotification(
                                    "Es sind keine Diamanten mehr auf Lager! (Du solltest nun fluechten)");
                                rob.Disabled = true;
                                iPlayer.IsInRob = false;
                                Logger.AddRobLog(iPlayer.Id, rob.Type.ToString(), 20 - rob.Interval, true);
                            }
                        }
                        else
                        {
                            iPlayer.SendNewNotification(

                                "Der Raubueberfall auf den Juwelier wurde abgebrochen!");
                            TeamModule.Instance.Get((int)teams.TEAM_POLICE).SendNotification(
                                "An Alle Einheiten, der Juwelier Raubueberfall wurde erfolgreich verhindert!");
                            TeamModule.Instance.Get((int)teams.TEAM_COUNTYPD).SendNotification(
                                "An Alle Einheiten, der Juwelier Raubueberfall wurde erfolgreich verhindert!");
                            TeamModule.Instance.Get((int)teams.TEAM_FIB).SendNotification(
                                "An Alle Einheiten, der Juwelier Raubueberfall wurde erfolgreich verhindert!");
                            rob.Disabled = true;
                            iPlayer.IsInRob = false;
                            Logger.AddRobLog(iPlayer.Id, rob.Type.ToString(), 20 - rob.Interval, false);
                        }
                    }
                    else if (rob.Type == (int)RobType.Shop)
                    {
                        var shop = ShopsModule.Instance.GetThisShop(iPlayer.Player.Position, 15.0f);
                        if (shop != null && !iPlayer.isInjured() && (Math.Abs(iPlayer.Player.Position.Z - shop.Position.Z) <= 2f))
                        {
                            if (rob.Interval >= 2)
                            {
                                if(rob.Interval == rob.CopInterval)
                                {
                                    Zone.Zone zone = ZoneModule.Instance.GetZone(iPlayer.Player.Position);
                                    TeamModule.Instance.Get((int)teams.TEAM_POLICE).SendNotification($"An Alle Einheiten, ein Einbruch in einen Store in {zone.Name} wurde gemeldet!");
                                    TeamModule.Instance.Get((int)teams.TEAM_COUNTYPD).SendNotification($"An Alle Einheiten, ein Einbruch in einen Store in {zone.Name} wurde gemeldet!");
                                }
                                else if (rob.Interval >= rob.EndInterval)
                                {
                                    iPlayer.SendNewNotification("Es ist kein Geld mehr in der Kasse! (Du solltest nun fluechten)");
                                    iPlayer.IsInRob = false;
                                    rob.Disabled = true;
                                    Logger.AddRobLog(iPlayer.Id, rob.Type.ToString(), rob.Interval, true);
                                }
                                else
                                {                          
                                    var erhalt = rnd.Next(110, 190) * rob.Interval;
                                    iPlayer.Container.AddItem(MarkierteScheineID, erhalt);
                                    iPlayer.SendNewNotification($"${erhalt} erbeutet!", title: "Raubüberfall");
                                }
                            }
                            rob.Interval++;
                        }
                        else
                        {
                            iPlayer.SendNewNotification("Der Raubueberfall auf den Store wurde abgebrochen!");
                            TeamModule.Instance.Get((int)teams.TEAM_POLICE).SendNotification("An Alle Einheiten, der Raubueberfall wurde erfolgreich verhindert!");
                            TeamModule.Instance.Get((int)teams.TEAM_COUNTYPD).SendNotification("An Alle Einheiten, der Raubueberfall wurde erfolgreich verhindert!");

                            if (rob.Interval < rob.CopInterval + 2)
                                iPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get(159), "Wurde vom Sicherheitssystem eines Shops erfasst.");

                            iPlayer.IsInRob = false;
                            rob.Disabled = true;
                            Logger.AddRobLog(iPlayer.Id, rob.Type.ToString(), rob.Interval, false);
                        }
                    }
                }
            }
        }
    }
}