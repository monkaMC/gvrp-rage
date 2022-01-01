using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.FraktionUebersichtApp.Apps
{
    public class Frakmember
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "rang")]
        public uint Rang { get; set; }
        [JsonProperty(PropertyName = "title")]
        public String Title { get; set; }
        [JsonProperty(PropertyName = "payday")]
        public int Payday { get; set; }
        [JsonProperty(PropertyName = "rights_storage")]
        public bool Storage { get; set; }
        [JsonProperty(PropertyName = "rights_manage")]
        public bool Manage { get; set; }
        [JsonProperty(PropertyName = "rights_bank")]
        public bool Bank { get; set; }



    }

    public class FrakMembersObject
    {
        [JsonProperty(PropertyName = "list")]
        public List<Frakmember> Frakmembers { get; set; }
        [JsonProperty(PropertyName = "manage")]
        public bool ManagePermission { get; set; }
    }
}
