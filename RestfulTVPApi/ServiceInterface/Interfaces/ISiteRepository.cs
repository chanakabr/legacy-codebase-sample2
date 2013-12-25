using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace RestfulTVPApi.ServiceInterface
{
    public interface ISiteRepository
    {
        Country[] GetCountriesList(InitializationObject initObj);

        List<string> GetAutoCompleteSearchList(InitializationObject initObj, string prefixText, int?[] iMediaTypes);
    }
}