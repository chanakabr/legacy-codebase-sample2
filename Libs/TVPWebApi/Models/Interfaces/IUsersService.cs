using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;

namespace TVPWebApi.Models
{
    public interface IUsersService
    {
        UserResponseObject GetUserData(InitializationObject initObj, string siteGuid);

        UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, UserBasicData userBasicData, UserDynamicData userDynamicData);

        PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid);

        PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int iTotalItems);

        PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid);

        PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int iTotalItems);

        UserResponseObject SignUp(InitializationObject initObj, UserBasicData userBasicData, UserDynamicData userDynamicData, string sPassword, string sAffiliateCode);

        FavoritObject[] GetUserFavorites(InitializationObject initObj, string siteGuid);

        bool AddUserFavorite(InitializationObject initObj, string siteGuid, int mediaID, int mediaType, int extraVal);

        bool RemoveUserFavorite(InitializationObject initObj, string siteGuid, int mediaID);

        TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid);

        bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive);

        bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN);
    }
}