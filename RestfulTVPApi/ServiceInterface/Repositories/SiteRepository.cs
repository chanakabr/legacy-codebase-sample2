using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Manager;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceInterface
{
    public class SiteRepository : ISiteRepository
    {

        public Country[] GetCountriesList(InitializationObject initObj)
        {
            Country[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetCountriesList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetCountriesList();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public List<string> GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes)
        {
            List<string> response = new List<String>();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetAutoCompleteSearchList", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                int maxItems = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).SiteConfiguration.Data.Features.MovieFinder.MaxItems;
                
                List<string> lstResponse = MediaHelper.GetAutoCompleteList(groupID, initObj.Platform, iMediaTypes != null ? iMediaTypes.Cast<int>().ToArray() : new int[0],
                    prefixText, initObj.Locale.LocaleLanguage, 0, maxItems);

                foreach (String sTitle in lstResponse)
                {
                    if (sTitle.ToLower().StartsWith(prefixText.ToLower())) response.Add(sTitle);
                }
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }
    }
}