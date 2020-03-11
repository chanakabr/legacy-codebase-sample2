using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Ingest.Utils
{
    public class Utils
    {
        public static string GetBusinessModuleName(IngestModule module)
        {
            if (module is IngestMultiPricePlan)
                return "multi price plan";
            if (module is IngestPricePlan)
                return "price plan";
            if (module is IngestPPV)
                return "ppv";

            return string.Empty;
        }

        public static DateTime ConvertFromUnixTimestamp(long timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static long ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return (long)diff.TotalSeconds;
        }

        public static long GetCurrentUtcTimeInUnixTimestamp()
        {
            return ConvertToUnixTimestamp(DateTime.UtcNow);
        }
    }
}