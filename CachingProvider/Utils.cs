using KLogMonitor;
using System;
using System.Collections.Generic;
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
    }
}
