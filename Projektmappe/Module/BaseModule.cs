using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module
{
    public enum Key
    {
        E,
        I,
        L,
        K,
        B,
        F1,
        F2,
        O,
        J
    }

    public enum ColShapeState
    {
        Enter,
        Exit
    }

    public abstract class BaseModule
    {
        private bool loaded = false;
        private DateTime loadTime = DateTime.Now;

        private StringBuilder currentLog;

        public void Log(string log)
        {
            currentLog?.AppendLine(log);
        }
        
        public virtual bool OnClientConnected(Client client)
        {
            return true;
        }

        public virtual void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
        }

        public virtual void OnVehicleSpawn(SxVehicle sxvehicle)
        {
        }

        public virtual void OnServerBeforeRestart()
        {
        }

        public virtual void OnPlayerFirstSpawnAfterSync(DbPlayer dbPlayer)
        {
        }

        public virtual void OnPlayerSpawn(DbPlayer dbPlayer)
        {
        }

        public virtual void OnPlayerConnected(DbPlayer dbPlayer)
        {
        }

        public virtual void OnPlayerLoggedIn(DbPlayer dbPlayer)
        {

        }

        public virtual void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
        }

        public virtual bool OnPlayerDeathBefore(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            return false;
        }

        public virtual void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
        }

        public virtual void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
        }

        public virtual void OnPlayerExitVehicle(DbPlayer dbPlayer, Vehicle vehicle)
        {
        }

        public virtual void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {
        }

        public virtual bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            return false;
        }
        
        public virtual bool OnChatCommand(DbPlayer dbPlayer, string command, string[] args)
        {
            return false;
        }

        public virtual bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            return false;
        }

        protected virtual bool OnLoad()
        {
            Logging.Logger.Print("Loading Module " + this.ToString());
            return true;
        }

        public virtual void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
        }

        public bool IsLoaded()
        {
            return loaded;
        }

        public virtual void OnMinuteUpdate()
        {
        }

        public virtual async Task OnMinuteUpdateAsync()
        {
        }

        public virtual void OnTwoMinutesUpdate()
        {
        }

        public virtual void OnFiveMinuteUpdate()
        {
        }
        
        public virtual void OnFifteenMinuteUpdate()
        {

        }

        public virtual void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null)
            {
                return;
            }
        }

        public virtual void OnVehicleMinuteUpdate(SxVehicle sxVehicle) //TODO
        {
            if (sxVehicle == null)
            {
                return;
            }
        }

        public virtual void OnTenSecUpdate()
        {
        }

        public virtual async Task OnTenSecUpdateAsync()
        {
        }

        public virtual void OnFiveSecUpdate()
        {
        }

        public virtual int GetOrder()
        {
            return 0;
        }
        
        public virtual void OnDailyReset()
        {
        }

        protected bool UpdateSetting(string key, string value)
        {
            Settings.Setting  setting = Settings.SettingsModule.Instance.GetAll().ToList().Where(s => s.Value.Key.ToLower() == key.ToLower()).FirstOrDefault().Value;

            if (setting == null) return false;

            setting.Value = value;

            Settings.SettingsModule.Instance.SaveSetting(setting);
            return false;
        }

        public virtual bool Load(bool reload = false)
        {
            if (loaded && !reload) return true;
            var requiredModules = RequiredModules();
            if (requiredModules != null)
            {
                foreach (var requiredModule in requiredModules)
                {
                    Modules.Instance.Load(requiredModule, reload);
                }
            }

            currentLog = new StringBuilder();

            loaded = OnLoad();
            return loaded;
        }

        public virtual Type[] RequiredModules()
        {
            return null;
        }
    }
}