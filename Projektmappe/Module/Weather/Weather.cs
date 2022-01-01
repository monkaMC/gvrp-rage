using System;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Weather
{
    public class Weather : Loadable<uint>
    {
        private uint Id { get; }
        public DayOfWeek DayOfWeek { get; }
        public uint Hour { get; }
        public GTANetworkAPI.Weather NWeather { get; }

        public Weather(MySqlDataReader reader) : base(reader)
        {
            var l_WeekString = reader.GetString("weekday");
            var l_WeatherId = reader.GetUInt32("weatherId");

            if (!Enum.TryParse(l_WeekString, out DayOfWeek dayOfWeek))
            {
                Logging.Logger.Crash(new ArgumentException($@"Unknown weekday in weather {reader.GetString("weekday")}"));
            }

            Id = reader.GetUInt32("id");
            DayOfWeek = dayOfWeek;
            Hour = reader.GetUInt32("hour");

            if (Enum.TryParse<GTANetworkAPI.Weather>("" + l_WeatherId, out var weather)) NWeather = weather;
            else NWeather = GTANetworkAPI.Weather.CLEAR;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}