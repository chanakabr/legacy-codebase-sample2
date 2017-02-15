using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CachingProvider
{
    public class Utils
    {

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static double GetDoubleValueFromTcm(string key)
        {
            double result = 0;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<double>(key);
            }
            catch (Exception ex)
            {
                result = 0;
                log.Error(string.Format("CachingProvider.Utils - GetDoubleValueFromTcm, key: {0}", key), ex);
            }

            return result;
        }

        public static T GetTcmGenericValue<T>(string key)
        {
            T result = default(T);
            try
            {
                result = TCMClient.Settings.Instance.GetValue<T>(key);
                if (result == null)
                    throw new NullReferenceException("missing key");
            }
            catch (Exception ex)
            {
                result = default(T);
                log.Error(string.Format("CachingProvider.Utils - GetTcmGenericValue, key: {0}", key), ex);
            }
            return result;
        }

        public static string GetTcmConfigValue(string key)
        {
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(key);
                if (string.IsNullOrEmpty(result))
                {
                    throw new Exception("missing key");
                }
            }
            catch (Exception ex)
            {
                result = string.Empty;
                log.Error(string.Format("CachingProvider.Utils - GetTcmConfigValue, key: {0}", key), ex);
            }
            return result;
        }

        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (long)(dateTime - new DateTime(1970, 1, 1).ToUniversalTime()).TotalSeconds;
        }

    }
}
