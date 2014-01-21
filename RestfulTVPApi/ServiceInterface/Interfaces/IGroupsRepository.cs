using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Notification;
using TVPPro.SiteManager.TvinciPlatform.Social;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IGroupsRepository
    {
        TVPApiModule.Objects.Responses.GroupOperator[] GetGroupOperators(InitializationObject initObj, string scope);

        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetGroupRules(InitializationObject initObj);

        FBConnectConfig FBConfig(InitializationObject initObj);

        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter);

        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken);
    }
}