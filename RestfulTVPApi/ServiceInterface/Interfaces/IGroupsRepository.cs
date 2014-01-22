using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPApiModule.Objects;
using TVPApiModule.Objects.Responses;

namespace RestfulTVPApi.ServiceInterface
{
    public interface IGroupsRepository
    {
        GroupOperator[] GetGroupOperators(InitializationObject initObj, string scope);

        GroupRule[] GetGroupRules(InitializationObject initObj);

        FBConnectConfig FBConfig(InitializationObject initObj);

        FacebookResponseObject FBUserMerge(InitializationObject initObj, string sToken, string sFBID, string sUsername, string sPassword);

        FacebookResponseObject FBUserRegister(InitializationObject initObj, string sToken, bool bCreateNewDomain, bool bGetNewsletter);

        FacebookResponseObject GetFBUserData(InitializationObject initObj, string sToken);
    }
}