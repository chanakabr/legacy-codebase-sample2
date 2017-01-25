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
                log.Error("GetDoubleValueFromTcm - Key=" + key + "," + ex.Message, ex);
            }

            return result;
        }

        public static T GetTcmGenericValue<T>(string sKey)
        {
            T result = default(T);
            try
            {
                result = TCMClient.Settings.Instance.GetValue<T>(sKey);
                if (result == null)
                    throw new NullReferenceException("missing key");
            }
            catch (Exception ex)
            {
                result = default(T);
                log.Error("TvinciShared.Ws_Utils - Key=" + sKey + "," + ex.Message, ex);
            }
            return result;
        }

        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

    }
}
