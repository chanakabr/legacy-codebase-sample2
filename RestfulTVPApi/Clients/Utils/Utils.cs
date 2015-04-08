using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Clients.Utils
{
    public class Utils
    {
        public static bool GetClientCredentials(RestfulTVPApi.Objects.Enums.Client clientType, int groupId, out string userName, out string password)
        {
            bool res = false;
            userName = null;
            password = null;

            userName = string.Format("{0}_{1}", clientType.ToString(), groupId);
            password = "11111";
            res = true;


            return res;
        }




    }
}