using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVRP.Module.Settings
{
    public static class Settings
    {
        public static string DailyReset = "dailyreset";
    }


    class SettingsModule : SqlModule<SettingsModule, Setting, uint>
    {


        protected override string GetQuery()
        {
            return "SELECT * FROM `module_settings`;";
        }

        protected override void OnLoaded()
        {
            base.OnLoaded();
        }

        public void SaveSetting(Setting setting)
        {
            MySQLHandler.ExecuteAsync($"UPDATE module_settings SET `value` = '{setting.Value}' WHERE `id` = '{setting.Id}'");
            return;
        }

        public Setting GetSetting(string key)
        {
            return Instance.GetAll().ToList().Where(s => s.Value.Key.ToLower() == key.ToLower()).FirstOrDefault().Value;
        }

        public bool CanDailyReset()
        {
            Setting setting = Instance.GetAll().ToList().Where(s => s.Value.Key.ToLower() == Settings.DailyReset.ToLower()).FirstOrDefault().Value;
            if (setting == null) return false;

            // check auf gestern
            if (setting.Updated.Day == DateTime.Now.AddDays(-1).Day)
            {
                return true;
            }
            return false;
        }
    }
}
