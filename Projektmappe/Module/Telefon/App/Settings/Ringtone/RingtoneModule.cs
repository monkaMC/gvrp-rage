using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Telefon.App.Settings.Ringtone
{
    public class RingtoneModule : SqlModule<RingtoneModule, Ringtone, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `phone_ringtones`;";
        }

        protected override void OnItemLoaded(Ringtone ringtone)
        {
            return;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.ringtone = Instance.Get(reader.GetUInt32("klingeltonId"));
            dbPlayer.phoneSetting = new PhoneSetting(false, false, false);
            dbPlayer.playerWhoHearRingtone = new List<DbPlayer>();
        }

        public String getJsonRingtonesForPlayer(DbPlayer dbPlayer)
        {
            bool staffMember = dbPlayer.Rank.Id == 0 ? false : true;
            List<Ringtone> liste = new List<Ringtone>();
            foreach (var item in this.GetAll().Values)
            {
                if ((item.isTeamOnly && staffMember) || !item.isTeamOnly) liste.Add(item);
            }

            return JsonConvert.SerializeObject(liste);
        }
    }
}
