using System;
using System.Globalization;

namespace CachingProvider
{
    public class Utils
    {
        private static DateTime GetTruncDateTimeUtc()
        {
            DateTime truncDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return truncDateTimeUtc;
        }
        
        public static long GetUtcUnixTimestampNow()
        {
            TimeSpan ts = DateTime.UtcNow - GetTruncDateTimeUtc();
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }
        
        public static long DateTimeToUtcUnixTimestampSeconds(DateTime dateTime)
        {
            return (long)(dateTime - GetTruncDateTimeUtc()).TotalSeconds;
        }
    }
}
