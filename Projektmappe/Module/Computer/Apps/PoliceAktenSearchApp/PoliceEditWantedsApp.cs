using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.Players;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceEditWantedsApp : SimpleApp
    {
        public PoliceEditWantedsApp() : base("PoliceEditWantedsApp")
        {
        }

        [RemoteEvent]
        public void requestWantedCategories(Client p_Client)
        {
            var l_CrimeCategories = CrimeCategoryModule.Instance.GetAll();
            List<CategoryObject> l_List = new List<CategoryObject>();

            foreach (var l_Category in l_CrimeCategories)
            {
                l_List.Add(new CategoryObject() { id = (int)l_Category.Value.Id, name = l_Category.Value.Name });
            }

            var l_Json = NAPI.Util.ToJson(l_List);
            TriggerEvent(p_Client, "responseCategories", l_Json);
        }

        [RemoteEvent]
        public void requestCategoryReasons(Client p_Client, int p_ID)
        {
            var l_CrimeReasons = CrimeReasonModule.Instance.GetAll();
            List<ReasonObject> l_List = new List<ReasonObject>();

            foreach (var l_Reason in l_CrimeReasons)
            {
                if (l_Reason.Value.Category.Id != p_ID)
                    continue;

                l_List.Add(new ReasonObject() { id = (int)l_Reason.Value.Id, name = l_Reason.Value.Name });
            }

            var l_Json = NAPI.Util.ToJson(l_List);
            TriggerEvent(p_Client, "responseCategoryReasons", l_Json);
        }


        [RemoteEvent]
        public void requestPlayerWanteds(Client p_Client, string p_Name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                var l_Crimes = dbPlayer.Crimes;
                List<CrimeJsonObject> l_List = new List<CrimeJsonObject>();

                foreach (var l_Reason in l_Crimes)
                {
                    l_List.Add(new CrimeJsonObject() { id = (int)l_Reason.Id, name = l_Reason.Name, description = l_Reason.Description });
                }

                var l_Json = NAPI.Util.ToJson(l_List);

                TriggerEvent(p_Client, "responsePlayerWanteds", l_Json);
            }));
        }

        [RemoteEvent]
        public void removeAllCrimes(Client p_Client, string name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var suspect = Players.Players.Instance.FindPlayer(name);
                if (suspect == null) return;
                suspect.RemoveAllCrimes(p_Client.Name);
            }));
        }

        [RemoteEvent]
        public void removePlayerCrime(Client p_Client, string name, int crime)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var suspect = Players.Players.Instance.FindPlayer(name);
                if (suspect == null) return;

                CrimePlayerReason crimePlayerReason = suspect.Crimes.Where(cpr => cpr.Id == (uint)crime).FirstOrDefault();
                if (crimePlayerReason != null)
                {
                    suspect.RemoveCrime(crimePlayerReason, p_Client.Name);
                }
            }));
        }

        [RemoteEvent]
        public void addPlayerWanteds(Client player, string name, string crimes)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsACop() || dbPlayer.TeamId == (int)teams.TEAM_SWAT) return;

            List<uint> crimesList = JsonConvert.DeserializeObject<List<uint>>(crimes);

            var suspect = Players.Players.Instance.FindPlayer(name);
            if (suspect == null || suspect.IsACop() || crimesList == null || suspect.isInjured()) return;
            foreach(uint crime in crimesList)
            {
                suspect.AddCrime(dbPlayer, CrimeReasonModule.Instance.Get((uint)crime));
            }
            
            Teams.TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} hat die Akte von {suspect.GetName()} bearbeitet!");
        }
    }

    public class CrimeJsonObject
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string description { get; set; }
    }
    public class ReasonObject
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }

    public class CategoryObject
    {
        [JsonProperty(PropertyName = "id")]
        public int id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }
}
