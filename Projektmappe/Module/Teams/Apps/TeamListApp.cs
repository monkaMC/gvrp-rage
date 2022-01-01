using System.Collections.Generic;
using GVRP.Module.ClientUI.Apps;
using GTANetworkAPI;
using GVRP.Module.Players;
using Newtonsoft.Json;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams.Permission;
using System.Linq;

namespace GVRP.Module.Teams.Apps
{
    public class TeamListApp : SimpleApp
    {
        public TeamListApp() : base("TeamListApp")
        {
        }

        public class TeamMember
        {
            [JsonProperty(PropertyName = "id")] public uint Id { get; }
            [JsonProperty(PropertyName = "name")] public string Name { get; }
            [JsonProperty(PropertyName = "number")] public uint Number { get; }
            [JsonProperty(PropertyName = "rank")] public uint Rank { get; }

            [JsonProperty(PropertyName = "inventory")]
            public bool Inventory { get; }

            [JsonProperty(PropertyName = "bank")] public bool Bank { get; }
            [JsonProperty(PropertyName = "manage")]public int Manage { get; }

            public TeamMember(uint id, string name, uint rank, bool inventory, bool bank, int manage, uint number)
            {
                Id = id;
                Name = name;
                Rank = rank;
                Inventory = inventory;
                Bank = bank;
                Manage = manage;
                Number = number;
            }
        }

        public void SendTeamMembers(DbPlayer dbPlayer)
        {    
            var membersManageObject = new MembersManageObject { TeamMemberList = TeamListFunctions.GetTeamMembersForList(dbPlayer), ManagePermission = dbPlayer.TeamRankPermission.Manage };
            var membersJson = JsonConvert.SerializeObject(membersManageObject);


            if (!string.IsNullOrEmpty(membersJson))
            {
                TriggerEvent(dbPlayer.Player, "responseTeamMembers", membersJson);
            }
        }

        [RemoteEvent]
        public void requestTeamMembers(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.TeamId == (uint) TeamList.Zivilist) return;
            SendTeamMembers(dbPlayer);
        }
    }

    class MembersManageObject
    {
        // Auto-implemented properties.
        public List<TeamListApp.TeamMember> TeamMemberList { get; set; }
        public int ManagePermission { get; set; }
    }
}