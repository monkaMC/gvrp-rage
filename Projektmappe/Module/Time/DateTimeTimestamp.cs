using System;

namespace GVRP.Module.Time
{
    public static class DateTimeTimestamp
    {
        public static int GetTimestamp(this DateTime date)
        {
            return (int) date.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}