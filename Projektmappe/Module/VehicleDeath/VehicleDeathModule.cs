using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Items;
using GVRP.Module.Meth;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.VehicleDeath
{

    public class VehicleDeathModule : Module<VehicleDeathModule>
    {
        private string VehicleBackupDB = "container_vehicle_backups";

        public int GetVehiclesRepairPrice(SxVehicle sxVehicle)
        {
            int price = sxVehicle.Data.Price / 1000;
            if (price <= 500) price = 500; // min
            if (price >= 50000) price = 50000; // max
            return price;
        }

        public void CreateVehicleBackupInventory(SxVehicle sxVehicle)
        {
            if (sxVehicle == null || sxVehicle.Container == null || (!sxVehicle.IsPlayerVehicle() && !sxVehicle.IsTeamVehicle()) || sxVehicle.databaseId == 0) return;

            string saveQuery = GetContainerInsertionQuery(sxVehicle.Container);
            if (saveQuery != "")
                MySQLHandler.ExecuteAsync(saveQuery, true);

            Logging.Logger.Debug(saveQuery);

            sxVehicle.Container.ClearInventory(); // bam in your face
        }

        public void RemoveOccupantsOnDeath(SxVehicle xVeh)
        {
            try
            {
                if (xVeh.Visitors.Count > 0)
                {
                    foreach (DbPlayer iPlayer in xVeh.Visitors)
                    {
                        if (iPlayer.DimensionType[0] == DimensionType.Camper && iPlayer.Player.Dimension != 0)
                        {
                            try
                            {
                                if (xVeh.Visitors.Contains(iPlayer)) xVeh.Visitors.Remove(iPlayer);
                                iPlayer.Player.SetPosition(new Vector3(xVeh.entity.Position.X + 3.0f,
                                    xVeh.entity.Position.Y,
                                    xVeh.entity.Position.Z));
                            }
                            catch (Exception e)
                            {
                                Logging.Logger.Crash(e);
                            }
                            finally
                            {
                                // Reset Cooking on Exit
                                if (iPlayer.HasData("cooking"))
                                {
                                    iPlayer.ResetData("cooking");
                                }
                                if (MethModule.CookingPlayers.Contains(iPlayer)) MethModule.CookingPlayers.Remove(iPlayer);

                                iPlayer.DimensionType[0] = DimensionType.World;
                                iPlayer.Dimension[0] = 0;
                                iPlayer.Player.Dimension = 0;
                                iPlayer.Player.SetPosition((Vector3)iPlayer.GetData("CamperEnterPos"));
                                iPlayer.ResetData("CamperEnterPos");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        private string GetContainerInsertionQuery(Container container)
        {
            try
            {
                string slotsValuesQuery = "";

                for (int i = 0; i < container.MaxSlots; i++)
                {
                    slotsValuesQuery += $"'{NAPI.Util.ToJson(container.ConvertToSaving().ContainsKey(i) ? container.ConvertToSaving()[i] : new List<SaveItem>())}',";
                }

                return $"INSERT INTO `{VehicleBackupDB}` VALUES ('', '{container.Id}', '', '{(int)container.Type}', '{container.MaxWeight}', '{container.MaxSlots}', {slotsValuesQuery.Substring(0, slotsValuesQuery.Length - 1)});";
            }
            catch
            {
                return "";
            }
        }
    }
}
