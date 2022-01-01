using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Workstation
{
    public class WorkstationModule : SqlModule<WorkstationModule, Workstation, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `workstations`;";
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.WorkstationId = reader.GetUInt32("workstation_id");   

            // Load Containers/Create
            dbPlayer.WorkstationEndContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONOUTPUT);
            dbPlayer.WorkstationFuelContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONFUEL);
            dbPlayer.WorkstationSourceContainer = ContainerManager.LoadContainer(dbPlayer.Id, ContainerTypes.WORKSTATIONINPUT);
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.HasWorkstation()))
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer == null || !dbPlayer.IsValid()
                    || dbPlayer.WorkstationEndContainer == null || dbPlayer.WorkstationFuelContainer == null || dbPlayer.WorkstationSourceContainer == null) continue;

                // Wenn Source leer
                if (dbPlayer.WorkstationSourceContainer.IsEmpty()) continue;

                Workstation workstation = dbPlayer.GetWorkstation();
                if (workstation == null) continue;

                // Verarbeiten...
                if(dbPlayer.WorkstationSourceContainer.GetItemAmount(workstation.SourceItemId) >= workstation.Source5MinAmount) // orüfe genug source items
                {
                    // Prüfen ob endcontainer passt
                    if (!dbPlayer.WorkstationEndContainer.CanInventoryItemAdded(workstation.EndItemId, workstation.End5MinAmount)) continue;

                    // Fuel Check first
                    if(workstation.FuelItemId != 0)
                    {
                        if (dbPlayer.WorkstationFuelContainer.GetItemAmount(workstation.FuelItemId) < workstation.Fuel5MinAmount) continue;

                        dbPlayer.WorkstationFuelContainer.RemoveItem(workstation.FuelItemId, workstation.Fuel5MinAmount);
                    }
                    dbPlayer.WorkstationSourceContainer.RemoveItem(workstation.SourceItemId, workstation.Source5MinAmount);
                    dbPlayer.WorkstationEndContainer.AddItem(workstation.EndItemId, workstation.End5MinAmount);                    
                }
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if(colShapeState == ColShapeState.Enter && colShape.HasData("workstation"))
            {
                Workstation workstation = Get(colShape.GetData("workstation"));
                dbPlayer.SendNewNotification($"Workstation {workstation.Name}, drücke E um diese für $2500 zu mieten!");
                if(workstation.FuelItemId == 0) dbPlayer.SendNewNotification($"Du kannst hier {ItemModelModule.Instance.Get(workstation.SourceItemId).Name} zu {ItemModelModule.Instance.Get(workstation.EndItemId).Name} verarbeiten!");
                else dbPlayer.SendNewNotification($"Du kannst hier {ItemModelModule.Instance.Get(workstation.SourceItemId).Name} mit {ItemModelModule.Instance.Get(workstation.FuelItemId).Name} zu {ItemModelModule.Instance.Get(workstation.EndItemId).Name} verarbeiten!");
                return true;
            }
            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle) return false;

            Workstation workstation = WorkstationModule.Instance.GetAll().Where(w => w.Value.NpcPosition.DistanceTo(dbPlayer.Player.Position) < 1.5f).FirstOrDefault().Value;
            if (workstation != null)
            {
                if(dbPlayer.TeamId != (int)teams.TEAM_CIVILIAN)
                {
                    dbPlayer.SendNewNotification($"Du scheinst mir zu unseriös zu sein... Arbeitest du schon etwas anderes?");
                    return true;
                }
                if(dbPlayer.WorkstationId == workstation.Id)
                {
                    dbPlayer.SendNewNotification($"Sie sind hier bereits eingemietet!");
                    return true;
                }

                if(workstation.RequiredLevel > 0 && workstation.RequiredLevel > dbPlayer.Level)
                {
                    dbPlayer.SendNewNotification($"Für diese Workstation benötigen Sie mind Level {workstation.RequiredLevel}!");
                    return true;
                }

                if(!dbPlayer.TakeMoney(2500))
                {
                    dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(2500));
                    return true;
                }
                dbPlayer.WorkstationEndContainer.ClearInventory();
                dbPlayer.WorkstationFuelContainer.ClearInventory();
                dbPlayer.WorkstationSourceContainer.ClearInventory();

                dbPlayer.SendNewNotification($"Sie haben sich in {workstation.Name} eingemietet und können diese nun benutzen!");
                dbPlayer.WorkstationId = workstation.Id;
                dbPlayer.SaveWorkstation();
                return true;
            }
            return false;
        }
    }

    public static class WorkstationPlayerExtension
    {
        public static void SaveWorkstation(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET workstation_id = '{dbPlayer.WorkstationId}' WHERE id = '{dbPlayer.Id}'");
        }

        public static Workstation GetWorkstation(this DbPlayer dbPlayer)
        {
            if (WorkstationModule.Instance.Contains(dbPlayer.WorkstationId)) 
            {
                return WorkstationModule.Instance.Get(dbPlayer.WorkstationId);
            }
            return null;
        }

        public static bool HasWorkstation(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            return dbPlayer.WorkstationId != 0;
        }
    }
}
