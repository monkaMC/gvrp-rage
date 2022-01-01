using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.PlayerTask
{
    public static class PlayerTaskExtension
    {
        public static void LoadTasks(this DbPlayer iPlayer)
        {

     
        }

        public static void AddTask(this DbPlayer iPlayer, PlayerTaskTypeId type, string data = "")
        {

        }

        public static void RemoveTask(this DbPlayer dbPlayer, uint taskId)
        {
            MySQLHandler.ExecuteAsync($"DELETE FROM `tasks` WHERE `id` = '{taskId}'");
            dbPlayer.PlayerTasks?.Remove(taskId);
        }

        public static void CheckTasks(this DbPlayer iPlayer)
        {

            
        }

        public static bool CheckTaskExists(this DbPlayer iPlayer, PlayerTaskTypeId type)
        {
            return iPlayer.PlayerTasks?.FirstOrDefault(task => task.Value.Type.Id == type).Value != null;
        }
    }
}