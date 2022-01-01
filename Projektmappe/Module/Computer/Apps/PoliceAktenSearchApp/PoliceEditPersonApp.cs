using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Crime;
using GVRP.Module.Crime.PoliceAkten;
using GVRP.Module.Houses;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using Logger = GVRP.Module.Logging.Logger;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceEditPersonApp : SimpleApp
    {
        public PoliceEditPersonApp() : base("PoliceEditPersonApp")
        {
        }


        [RemoteEvent]
        public async void requestPersonData(Client p_Client, string p_Name)
        {
            var dbPlayer = p_Client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var foundPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.CustomData.CanAktenView = dbPlayer.CanAktenView();

            string note = "";

            if((dbPlayer.Team.IsCops() && dbPlayer.TeamRank >= 10) || foundPlayer.GovLevel.Length > 0)
            {
                note = "Sicherheitsstufe " + foundPlayer.GovLevel;
            }

            if(foundPlayer.ownHouse[0] > 0)
            {
                foundPlayer.CustomData.Address = "Haus " + foundPlayer.ownHouse[0];
            }
            else if(foundPlayer.IsTenant())
            {
                HouseRent tentant = foundPlayer.GetTenant();
                if (tentant != null) foundPlayer.CustomData.Address = "Mieter " + tentant.HouseId;
            }

            TriggerEvent(p_Client, "responsePersonData", NAPI.Util.ToJson(new CustomDataJson(foundPlayer.CustomData, note)));
        }


        [RemoteEvent]
        public async void requestAktenList(Client player, string searchQuery)
        {
            
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                var foundPlayer = Players.Players.Instance.FindPlayer(searchQuery);
                if (foundPlayer == null || !foundPlayer.IsValid()) return;

                if (!dbPlayer.CanAktenView())
                {
                    return;
                }

                TriggerEvent(player, "responseAktenList", NAPI.Util.ToJson(PoliceAktenModule.Instance.GetPlayerClientJsonAkten(foundPlayer)));
            
        }

        [RemoteEvent]
        public async void requestLicenses(Client player, string searchQuery)
        {
            
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                var foundPlayer = Players.Players.Instance.FindPlayer(searchQuery);
                if (foundPlayer == null || !foundPlayer.IsValid()) return;

                List<LicenseJson> licenseJsons = new List<LicenseJson>();

                licenseJsons.Add(new LicenseJson() { Name = "Motorradschein", Value = foundPlayer.Lic_Bike[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Führerschein", Value = foundPlayer.Lic_Car[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Bootsschein", Value = foundPlayer.Lic_Boot[0] });
                licenseJsons.Add(new LicenseJson() { Name = "LKW-Schein", Value = foundPlayer.Lic_LKW[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Flugschein A", Value = foundPlayer.Lic_PlaneA[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Flugschein B", Value = foundPlayer.Lic_PlaneB[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Pers. Beförderungsschein", Value = foundPlayer.Lic_Transfer[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Waffenschein", Value = foundPlayer.Lic_Gun[0] });
                licenseJsons.Add(new LicenseJson() { Name = "Erstehilfekurs", Value = foundPlayer.Lic_FirstAID[0] });

                TriggerEvent(player, "responseLicenses", NAPI.Util.ToJson(licenseJsons));
            
        }

        [RemoteEvent]
        public void requestAkte(Client player, string playername)
        {
            if (playername == null)
            {
                return;
            }
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            // GetValid Player by searchname
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(playername);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;


            if (!dbPlayer.CanAktenView())
            {
                return;
            }

            TriggerEvent(player, "responseAkte", NAPI.Util.ToJson(PoliceAktenModule.Instance.GetOpenAkteOrNew(foundPlayer)));
        }

        [RemoteEvent]
        public async void savePersonData(Client player, string playername, string address, string membership, string phone, string info)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;


            if (!dbPlayer.CanEditData())
            {
                dbPlayer.SendNewNotification("Keine Berechtigung!");
                return;
            }

            // GetValid Player by searchname
            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(playername);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            foundPlayer.UpdateCustomData(address, membership, phone, info);
        }


        [RemoteEvent]
        public async void requestOpenCrimes(Client p_Client, string p_Name)
        {
            var dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            var l_Crimes = dbPlayer.Crimes;
            List<CrimeJsonObject> l_List = new List<CrimeJsonObject>();

            try
            {
                foreach (var l_Reason in l_Crimes.ToList())
                {
                    l_List.Add(new CrimeJsonObject() { id = (int) l_Reason.Id, name = l_Reason.Name, description = l_Reason.Notice });
                }
                var l_Json = NAPI.Util.ToJson(l_List);
                TriggerEvent(p_Client, "responseOpenCrimes", l_Json);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public async void requestJailCosts(Client p_Client, string p_Name)
        {
            var dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            TriggerEvent(p_Client, "responseJailCosts", CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes));
        }

        [RemoteEvent]
        public async void requestJailTime(Client p_Client, string p_Name)
        {
            try
            {
                var dbPlayer = Players.Players.Instance.FindPlayer(p_Name);
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                TriggerEvent(p_Client, "responseJailTime", CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes));
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }
    }
    public class CustomDataJson
    {
        [JsonProperty(PropertyName = "address")]
        public string Address { get; set; }

        [JsonProperty(PropertyName = "membership")]
        public string Membership { get; set; }

        [JsonProperty(PropertyName = "phone")]
        public string Phone { get; set; }

        [JsonProperty(PropertyName = "info")]
        public string Info { get; set; }

        [JsonProperty(PropertyName = "canAktenView")]
        public bool CanAktenView { get; set; }

        [JsonProperty(PropertyName = "note")]
        public string Note { get; set; }


        public CustomDataJson(CustomData customData, string note = "")
        {
            Address = customData.Address;
            Membership = customData.Membership;
            Phone = customData.Phone;
            Info = customData.Info;
            CanAktenView = customData.CanAktenView;
            Note = note;
        }
    }

    public class LicenseJson
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }
    }
}

