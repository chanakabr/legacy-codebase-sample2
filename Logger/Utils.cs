using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Logger
{
    public class Utils
    {
        public static string GetTcmConfigValue(string sKey)
        {           
            string result = string.Empty;
            try
            {
                result = TCMClient.Settings.Instance.GetValue<string>(sKey);
            }
            catch (Exception ex)
            {
                result = string.Empty;
                Logger.Log("Logger", "Key=" + sKey + "," + ex.Message, "Tcm");
            }
            return result;
        }
    }
}
