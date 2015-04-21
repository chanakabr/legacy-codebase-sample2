using RestfulTVPApi.Api;
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

            userName = string.Format("{0}_{1}", clientType.ToString().ToLower(), groupId);
            password = "11111";
            res = true;


            return res;
        }

        public static int ConvertLocaleLanguageToInt(int groupId, string language)
        {
            int result = 0;

            if (!string.IsNullOrEmpty(language))
            {
                LanguageObj[] groupLanguages = ClientsManager.ApiClient().GetGroupLanguages(groupId);
                if (groupLanguages != null)
                {
                    var lang = groupLanguages.Where(l => l.Code == language).FirstOrDefault();
                    result = lang != null ? lang.ID : 0;
                }
            }
            return result;
        }


    }
}