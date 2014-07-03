using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DalCB
{
    public class Utils
    {
        public static string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }
    }
}
