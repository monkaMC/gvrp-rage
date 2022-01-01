using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Business.Apps
{
    public class BusinessListApp : SimpleApp
    {
        public BusinessListApp() : base("BusinessListApp")
        {
        }

        internal class BusinessMember
        {
            [JsonProperty(PropertyName = "id")]
            public uint Id { get; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; }

            [JsonProperty(PropertyName = "bank")]
            public bool Bank { get; }

            [JsonProperty(PropertyName = "manage")]
            public bool Manage { get; }

            [JsonProperty(PropertyName = "salary")]
            public int Salary { get; }

            [JsonProperty(PropertyName = "owner")]
            public bool Owner { get; }

            [JsonProperty(PropertyName = "number")]
            public int Number { get; }

            [JsonProperty(PropertyName = "raffinery")]
            public bool Raffinery { get; }

            [JsonProperty(PropertyName = "fuelstation")]
            public bool Fuelstation { get; }

            [JsonProperty(PropertyName = "nightclub")]
            public bool NightClub { get; }
            [JsonProperty(PropertyName = "tattoo")]
            public bool Tattoo { get; }

            public BusinessMember(uint id, string name, bool bank, bool manage, bool owner, int salary, int number, bool raffinery, bool fuelstation, bool nightclub, bool tattoo)
            {
                Id = id;
                Name = name;
                Bank = bank;
                Manage = manage;
                Salary = salary;
                Owner = owner;
                Number = number;
                Raffinery = raffinery;
                Fuelstation = fuelstation;
                NightClub = nightclub;
                Tattoo = tattoo;
            }
        }

        public void SendBusinessMembers(DbPlayer dbPlayer)
        {
            var members = new List<BusinessMember>();
            var business = dbPlayer.ActiveBusiness;

            if (business.GetMembers().Count <= 0) return;
            if (business.GetMember(dbPlayer.Id) == null) return;

            foreach (var member in business.GetMembers().ToList())
            {
                if (member.Value == null) continue;
                var findplayer = Players.Players.Instance.FindPlayerById(member.Value.PlayerId);
                if (findplayer == null || !findplayer.IsValid() || findplayer.IsInAdminDuty() || findplayer.IsInGuideDuty())continue;                
                
                var businessMember = member.Value;
                var currentDbPlayer = Players.Players.Instance.GetByDbId(member.Key);
                if (currentDbPlayer == null || !currentDbPlayer.IsValid()) continue;

                members.Add(new BusinessMember(currentDbPlayer.Id, currentDbPlayer.GetName(), businessMember.Money, businessMember.Manage, businessMember.Owner, businessMember.Salary, (int)currentDbPlayer.handy[0], businessMember.Raffinery, businessMember.Fuelstation, businessMember.NightClub, businessMember.Tattoo));
            }

            int manage = 0;
            if (business.GetMember(dbPlayer.Id).Owner) manage = 2;
            else if (business.GetMember(dbPlayer.Id).Manage) manage = 1;

            var membersManageObject = new MembersManageObject { BusinessMemberList = members, ManagePermission = manage };
            var membersJson = JsonConvert.SerializeObject(membersManageObject);
                
            if (!string.IsNullOrEmpty(membersJson))
            {
                TriggerEvent(dbPlayer.Player, "responseBusinessMembers", membersJson);
            }
        }

        [RemoteEvent]
        public void requestBusinessMembers(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;
            SendBusinessMembers(dbPlayer);
        }

        [RemoteEvent]
        public void requestBusinessMOTD(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsMemberOfBusiness()) return;

            TriggerEvent(dbPlayer.Player, "responseBusinessMOTD", dbPlayer.ActiveBusiness.MessageOfTheDay);
        }

        [RemoteEvent]
        public void leaveBusiness(Client p_Player)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                return;
                
            l_DbPlayer.RemoveBusinessMembership(l_DbPlayer.ActiveBusiness);
            l_DbPlayer.UpdateApps();
        }

        [RemoteEvent]
        public void saveBusinessMOTD(Client p_Player, string motd)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                return;
            var l_edit_Member = l_DbPlayer.ActiveBusiness.GetMember((uint)l_DbPlayer.Id);
            
            if (l_edit_Member == null || (!l_edit_Member.Owner && !l_edit_Member.Manage))
            {
                l_DbPlayer.SendNewNotification("Dazu bist du nicht berechtigt.", title:"Business", notificationType:PlayerNotification.NotificationType.BUSINESS);
                return;
            }

            l_DbPlayer.ActiveBusiness.ChangeMotd(motd);
        }

        [RemoteEvent]
        public void editBusinessMember(Client p_Player, int p_MemberID, bool p_Bank, bool p_Manage, int p_Salary, bool raffinery, bool fuelstation, bool nightclub, bool tattoo)
        {
            try { 
                DbPlayer l_DbPlayer = p_Player.GetPlayer();
                if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                    return;

                //user who gets edited
                var l_Member = l_DbPlayer.ActiveBusiness.GetMember((uint)p_MemberID);
                if (l_Member == null)
                    return;

                //user who edits
                var l_edit_Member = l_DbPlayer.ActiveBusiness.GetMember((uint)p_Player.GetPlayer().Id);
                if (l_edit_Member == null)
                    return;

                //If owner gets edited by someone else then himself OR edit player has no permission
                if ((l_Member.Owner && !l_edit_Member.Owner) || !l_edit_Member.Manage) {
                    l_DbPlayer.SendNewNotification("Dazu bist du nicht berechtigt.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                    return;
                }

                if (l_edit_Member.Owner || l_edit_Member.Manage)
                {
                    l_Member.Raffinery = raffinery;
                    l_Member.Fuelstation = fuelstation;
                    l_Member.NightClub = nightclub;
                    l_Member.Tattoo = tattoo;
                }
                if (l_edit_Member.Owner)
                {
                    l_Member.Money = p_Bank;
                    l_Member.Manage = p_Manage;
                }

                if (l_edit_Member.Owner || l_edit_Member.Money)
                {
                    if (p_Salary < 0) return;


                    l_Member.Salary = p_Salary;
                }

                l_DbPlayer.SaveBusinessMembership(l_Member);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void kickBusinessMember(Client p_Player, int p_PlayerID)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                DbPlayer l_DbPlayer = p_Player.GetPlayer();
                if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsMemberOfBusiness())
                    return;

                //user who gets kicked
                var l_Member = l_DbPlayer.ActiveBusiness.GetMember((uint)p_PlayerID);
                DbPlayer MemberPlayer = Players.Players.Instance.GetByDbId(l_Member.PlayerId);

                if (l_Member == null || MemberPlayer == null || !MemberPlayer.IsValid())
                    return;

                //user who kicks
                DbPlayer editPlayer = p_Player.GetPlayer();
                if (editPlayer == null || !editPlayer.IsValid() || !editPlayer.IsMemberOfBusiness()) return;
                var l_edit_Member = l_DbPlayer.ActiveBusiness.GetMember((uint)editPlayer.Id);

                //If owner gets kicked OR edit player has no permission
                if ((l_Member.Owner) || !l_edit_Member.Manage)
                {
                    l_DbPlayer.SendNewNotification("Dazu bist du nicht berechtigt.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                    return;
                }

                DbPlayer kickedPlayer = Players.Players.Instance.FindPlayer(p_PlayerID);
                if (kickedPlayer == null || !kickedPlayer.IsValid()) return;

                kickedPlayer.RemoveBusinessMembership(l_DbPlayer.ActiveBusiness);
                kickedPlayer.UpdateApps();
            }));
        }

        [RemoteEvent]
        public void addPlayerToBusiness(Client p_Player, string p_Name)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    DbPlayer l_InvitingPlayer = p_Player.GetPlayer();
                    if (l_InvitingPlayer == null || !l_InvitingPlayer.IsValid())
                        return;

                    if (l_InvitingPlayer.ActiveBusiness == null)
                        return;
                    if (String.IsNullOrEmpty(p_Name)) return;
                    DbPlayer l_DbPlayer = Players.Players.Instance.FindPlayer(p_Name);
                    if (l_DbPlayer == null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{p_Name} wurde nicht gefunden.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    if (l_DbPlayer.ActiveBusiness != null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{p_Name} ist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    ComponentManager.Get<ConfirmationWindow>().Show()(l_DbPlayer, new ConfirmationObject($"{l_InvitingPlayer.ActiveBusiness.Name}", $"Möchtest du die Einladung von {l_InvitingPlayer.GetName()} annehmen?", "addBusinessMemberConfirm", l_InvitingPlayer.GetName(), l_InvitingPlayer.ActiveBusiness.Name));
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }

        [RemoteEvent]
        public void addBusinessMemberConfirm(Client p_Player, string p_InvitingPersonName, string p_BusinessName)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                try
                {
                    var l_DbPlayer = p_Player.GetPlayer();
                    if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                        return;

                    var l_InvitingPlayer = Players.Players.Instance.FindPlayer(p_InvitingPersonName).Player.GetPlayer();
                    if (l_InvitingPlayer == null || !l_InvitingPlayer.IsValid())
                        return;

                    var l_ManagePerm = l_InvitingPlayer.GetActiveBusinessMember();
                    if (l_ManagePerm == null)
                        return;

                    if (l_ManagePerm.Manage == false && l_ManagePerm.Owner == false)
                    {
                        l_DbPlayer.SendNewNotification($"{l_InvitingPlayer.GetName()} ist nicht berechtigt, Personen in das Business einzuladen.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        l_InvitingPlayer.SendNewNotification("Du bist nicht berechtig, Personen in das Business einzuladen.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    if (l_DbPlayer.ActiveBusiness != null)
                    {
                        l_InvitingPlayer.SendNewNotification($"{p_Player.Name} ist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        l_DbPlayer.SendNewNotification("Du bist bereits in einem Business.", title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                        return;
                    }

                    l_DbPlayer.SendNewNotification($"Willkommen im Business " + l_InvitingPlayer.ActiveBusiness.Name, title: "Business", notificationType: PlayerNotification.NotificationType.BUSINESS);
                    l_DbPlayer.AddBusinessMembership(l_InvitingPlayer.ActiveBusiness);
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }));
        }
    }
    class MembersManageObject
    {
        public List<BusinessListApp.BusinessMember> BusinessMemberList { get; set; }
        public int ManagePermission { get; set; }
    }
}