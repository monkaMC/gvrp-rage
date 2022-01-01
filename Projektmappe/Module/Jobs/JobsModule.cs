using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Module.GTAN;
using GVRP.Module.Players;
using GVRP.Module.Spawners;

namespace GVRP.Module.Jobs
{
    public sealed class JobsModule : Module<JobsModule>
    {
        private Dictionary<int, Job> jobs;

        protected override bool OnLoad()
        {
            if (jobs != null)
            {
                foreach (var job in jobs)
                {
                    job.Value.Marker?.Delete();
                    PlayerNotifications.Instance.Remove(job.Value.NotificationId);
                }
            }

            jobs = new Dictionary<int, Job>();
            Add(1, 1, JobContent.Weapondealer.Name, JobContent.Weapondealer.Description,
                JobContent.Weapondealer.Help, false, new Vector3(1383.317f, -2078.667f, 51.999f), 0.0f,
                PedHash.ExArmy01, false, true);
            Add(2, 1, JobContent.Plagiat.Name, JobContent.Plagiat.Description, JobContent.Plagiat.Help, false,
                new Vector3(-1126.433, 2694.437, 18.800), 32.588f, PedHash.Busker01SMO);
            Add(8, 3, JobContent.Mech.Name, JobContent.Mech.Description, JobContent.Mech.Help, true,
                new Vector3(483.179, -1309.349, 29.216), 256.472f, PedHash.Autoshop02SMM);
            Add(11, 3, JobContent.Makler.Name, JobContent.Makler.Description, JobContent.Makler.Help, true,
                new Vector3(-581.1449, -330.7596, 35.15263), 78.42422f, PedHash.Andreas);
            Add(14, 5, JobContent.Anwalt.Name, JobContent.Anwalt.Description, JobContent.Anwalt.Help, true,
                new Vector3(67.87844, -959.4167, 29.80383), 162.4345f, PedHash.DaveNorton, true);

            return true;
        }

        public bool Add(int id, int level, string name, string description, string helps, bool legal, Vector3 pos,
            float heading, PedHash skin, bool disablegang = false, bool disablezivi = false, bool disabled = false)
        {
            if (jobs.ContainsKey(id))
                return false;

            var job = new Job
            {
                Id = id,
                Level = level,
                Name = name,
                Description = description,
                Legal = legal,
                Position = pos,
                Heading = heading,
                Skin = skin,
                Helps = helps,
                disablegang = disablegang,
                disablezivi = disablezivi,
                disabled = disabled
            };

            OnJobSpawn(job);

            jobs.Add(id, job);

            return true;
        }

        public Dictionary<int, Job> GetAll()
        {
            return jobs;
        }

        public Job GetJob(int jobid)
        {
            return !jobs.ContainsKey(jobid) ? null : jobs[jobid];
        }

        public string GetJobName(int jobid)
        {
            return !jobs.ContainsKey(jobid) ? "Arbeitslos" : jobs[jobid].Name;
        }

        public Job GetThisJob(Vector3 pos)
        {
            return (from kvp in jobs where kvp.Value.Position.DistanceTo(pos) <= 2.0f select kvp.Value)
                .FirstOrDefault();
        }

        public List<Job> GetJobByType(bool legal = true)
        {
            var jobsLegal = new List<Job>();
            foreach (var kvp in jobs)
            {
                if (kvp.Value.disabled) continue;
                if (kvp.Value.Legal == legal && legal)
                {
                    jobsLegal.Add(kvp.Value);
                }
                else if (kvp.Value.Legal != legal && !legal)
                {
                    jobsLegal.Add(kvp.Value);
                }
            }

            return jobsLegal;
        }

        public void OnJobSpawn(Job job)
        {
            if (job == null || job.disabled) return;
            job.Marker = Markers.CreateDefaultMarker(job.Position);
            job.NotificationId = PlayerNotifications.Instance.Add(job.Position, job.Name,
                job.Description + " Level " + job.Level +
                " /job fuer mehr Informationen!").Id;
        }
    }
}