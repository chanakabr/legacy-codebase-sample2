using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Globalization;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;

namespace NotificationInterface
{
    public class Utils
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
       


        static public string GetWSURL(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static string ExtractDate(DateTime date, string format)
        {
            try
            {
                string result = date.ToString(format);
                return result;
            }
            catch (FormatException ex)
            {
                log.Error("", ex);
                return string.Empty;
            }
            catch (Exception exp)
            {
                log.Error("", exp);
                return string.Empty;
            }
        }

       
    }
}


