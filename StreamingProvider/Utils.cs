using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StreamingProvider
{
    public class Utils
    {
        static public string GetValueFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }
    }
}
