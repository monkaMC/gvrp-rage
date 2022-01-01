using System;
using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Weather
{
    public class WeatherModule : SqlModule<WeatherModule, Weather, uint>
    {
        public bool Blackout = false;
        public float WaterHeight = 0.0f;

        protected override string GetQuery()
        {
            return "SELECT * FROM `weather` ORDER by weekday, hour;";
        }

        protected override bool OnLoad()
        {
            Blackout = false;
            WaterHeight = 0.0f;
            if (Configuration.Instance.DevMode)
            {
                NAPI.World.SetTime(15, 0, 0);
            }
            else
            {
                var now = DateTime.Now;
                NAPI.World.SetTime(now.Hour, now.Minute, now.Second);
            }
            return base.OnLoad();
        }

        public override void OnMinuteUpdate()
        {
            var l_Time = DateTime.Now;
            DayOfWeek l_Day = l_Time.DayOfWeek;
            var l_Hour = (uint) l_Time.Hour;

            foreach (KeyValuePair<uint, Weather> kvp in GetAll())
            {
                if (!kvp.Value.DayOfWeek.Equals(l_Day))
                    continue;

                if (kvp.Value.Hour != l_Hour)
                    continue;

                if(!Main.WeatherOverride) Main.m_CurrentWeather = kvp.Value.NWeather;
                break;
            }

            NAPI.World.SetTime((int)l_Hour, l_Time.Minute, l_Time.Second);
            if (!Configuration.Instance.DevMode) NAPI.World.SetWeather(Main.m_CurrentWeather);
        }
        
        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("setBlackout", Blackout);
            //dbPlayer.Player.TriggerEvent("modifyWater", WaterHeight);
        }

        public void SetBlackout(bool blackout)
        {
            Blackout = blackout;
            foreach(DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
            {
                iPlayer.Player.TriggerEvent("setBlackout", Blackout);
            }
        }

        public void SetWaterHeight(float height = 0)
        {
            WaterHeight = height;
            foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers())
            {
                iPlayer.Player.TriggerEvent("modifyWater", iPlayer.Player.Position.X, iPlayer.Player.Position.Y, height);
                iPlayer.Player.SendNative(0xC443FD757C3BA637, iPlayer.Player.Position.X, iPlayer.Player.Position.Y, 500, height);
            }
        }
    }
}  