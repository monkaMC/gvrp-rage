using System;
using GTANetworkAPI;
using GVRP.Module.GTAN;
using GVRP.Module.Jobs;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerJob
    {
        public static Random rnd = new Random();
        
        public static Job GetJob(this DbPlayer iPlayer)
        {
            return JobsModule.Instance.GetJob(iPlayer.job[0]);
        }

        public static void SetPlayerCurrentJobSkill(this DbPlayer dbPlayer)
        {
            //      if (dbPlayer.job[0] == 0) return;
            //      if (dbPlayer.job_skills[0] != "")
            //{
            //      string[] Items = dbPlayer.job_skills[0].Split(',');
            //foreach (string item in Items)
            //      {
            //     string[] parts = item.Split(':');
            //      if (parts[0] == Convert.ToString(dbPlayer.job[0]))
            //            {
            //    dbPlayer.jobskill[0] = Convert.ToInt32(parts[1]);
            //    return;
            // }
            //        //    }
            //    }
        }

        //Todo: to DbPlayer attribute
        public static void SetJobStatus(this Client player, int status)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            iPlayer.SetData("jobstate", status);
        }
        
        public static void 
            Increase(this DbPlayer iPlayer, int multiplier = 1, int pureinc = 0)
        {
            if (iPlayer.job[0] <= 0) return;
            var rand = rnd.Next(10, 30);

            rand = rand * multiplier;
            if (pureinc > 0)
            {
                rand = pureinc;
            }

            if (iPlayer.jobskill[0] >= 5000) return;
            if (iPlayer.jobskill[0] + rand < 5000)
            {
                if (iPlayer.uni_workaholic[0] > 0)
                {
                    var multiplierx = 100 + (iPlayer.uni_workaholic[0] * 2);
                    rand = Convert.ToInt32((rand * (multiplierx)) / 100);
                }

                iPlayer.jobskill[0] += rand;
                var xJob = JobsModule.Instance.GetJob(iPlayer.job[0]);
                iPlayer.SendNewNotification(
                    "Skill erhoeht! Beruf: " + xJob.Name);
            }
            else
            {
                iPlayer.jobskill[0] = 5000;
                var xJob = JobsModule.Instance.GetJob(iPlayer.job[0]);
                iPlayer.SendNewNotification(
                    "Skill erhoeht! Beruf: " + xJob.Name);
            }
        }
    }
}