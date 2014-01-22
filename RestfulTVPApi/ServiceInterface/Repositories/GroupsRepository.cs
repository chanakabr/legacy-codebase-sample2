using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Helper;
using TVPApiModule.Objects;
using TVPApiModule.Services;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public class GroupsRepository : IGroupsRepository
    {
        public GroupOperator[] GetGroupOperators(InitializationObject initObj, string scope)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupOperators", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                TVPApiModule.Services.ApiApiService _service = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform);

                return _service.GetGroupOperators(scope);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public GroupRule[] GetGroupRules(InitializationObject initObj)
        {
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                ApiApiService _service = new ApiApiService(groupID, initObj.Platform);

                return _service.GetGroupRules();
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FBConnectConfig FBConfig(InitializationObject initObj)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBConfig", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                FacebookConfig fbConfig = _service.GetFBConfig("0");

                FBConnectConfig retVal = new FBConnectConfig
                {
                    appId = fbConfig.sFBKey,
                    scope = fbConfig.sFBPermissions,
                    apiUser = initObj.ApiUser,
                    apiPass = initObj.ApiPass
                };

                return retVal;
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserMerge", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());
            
            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.FBUserMerge(sToken, sFBID, sUsername, sPassword);
            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "FBUserRegister", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                var oExtra = new List<TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair>() { new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "news", value = bGetNewsletter ? "1" : "0" }, new TVPPro.SiteManager.TvinciPlatform.Social.KeyValuePair() { key = "domain", value = bCreateNewDomain ? "1" : "0" } };
                
                //Ofir - why its was UserHostAddress in ip param?
                return _service.FBUserRegister(sToken, "0", oExtra, SiteHelper.GetClientIP());

            }
            else
            {
                throw new UnknownGroupException();
            }
        }

        public FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken)
        {
            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetFBUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                ApiSocialService _service = new ApiSocialService(groupId, initObj.Platform);

                return _service.GetFBUserData(sToken, "0");
            }
            else
            {
                throw new UnknownGroupException();
            }
        }
    }
}