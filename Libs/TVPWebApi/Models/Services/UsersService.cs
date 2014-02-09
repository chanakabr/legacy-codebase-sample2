using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Net;
using TVPApi;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using System.Configuration;

namespace TVPWebApi.Models
{
    public class UsersService : IUsersService
    {

        public UserResponseObject GetUserData(InitializationObject initObj, string siteGuid)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserData(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject SetUserData(InitializationObject initObj, string siteGuid, UserBasicData userBasicData, UserDynamicData userDynamicData)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserData", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData b = new TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData();

                b.m_sFirstName = userBasicData.first_name;
                b.m_sAffiliateCode = userBasicData.affiliate_code;
                b.m_sLastName = userBasicData.last_name;
                b.m_sUserName = userBasicData.user_name;
                b.m_sFacebookID = userBasicData.facebook_id;
                b.m_bIsFacebookImagePermitted = userBasicData.is_facebook_image_permitted;
                b.m_sFacebookImage = userBasicData.facebook_image;
                b.m_sEmail = userBasicData.email;
                b.m_sAddress = userBasicData.address;
                b.m_sCity = userBasicData.city;
                b.m_sZip = userBasicData.zip;
                b.m_sPhone = userBasicData.phone;

                if (userBasicData.country != null)
                {
                    b.m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country();

                    b.m_Country.m_sCountryCode = userBasicData.country.country_code;
                    b.m_Country.m_sCountryName = userBasicData.country.country_name;
                    b.m_Country.m_nObjecrtID = userBasicData.country.object_id;
                }

                if (userBasicData.state != null)
                {
                    b.m_State = new TVPPro.SiteManager.TvinciPlatform.Users.State();

                    if (userBasicData.state.country != null)
                    {
                        b.m_State.m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country();

                        b.m_State.m_Country.m_sCountryCode = userBasicData.state.country.country_code;
                        b.m_State.m_Country.m_sCountryName = userBasicData.state.country.country_name;
                        b.m_State.m_Country.m_nObjecrtID = userBasicData.state.country.object_id;
                    }

                    b.m_State.m_nObjecrtID = userBasicData.state.object_id;
                    b.m_State.m_sStateCode = userBasicData.state.state_code;
                    b.m_State.m_sStateName = userBasicData.state.state_name;
                }

                TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData d = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();

                if (userDynamicData.user_data != null)
                {
                    d.m_sUserData = userDynamicData.user_data.Select(x => new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer()
                    {
                        m_sDataType = x.data_type,
                        m_sValue = x.value
                    }).ToArray();
                }

                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SetUserData(siteGuid, b, d);
            }
            else
            {
                throw new HttpResponseException(HttpStatusCode.Unauthorized);
            }

            return response;
        }

        public PermittedSubscriptionContainer[] GetUserPermitedSubscriptions(InitializationObject initObj, string siteGuid)
        {
            PermittedSubscriptionContainer[] permitedSubscriptions = new PermittedSubscriptionContainer[] { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermitedSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermitedSubscriptions(siteGuid);

                if (permitted != null)
                    permitedSubscriptions = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permitedSubscriptions;
        }

        public PermittedSubscriptionContainer[] GetUserExpiredSubscriptions(InitializationObject initObj, string siteGuid, int totalItems)
        {
            PermittedSubscriptionContainer[] items = null;

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredSubscriptions", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                items = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredSubscriptions(siteGuid, totalItems);
                
                if (items != null)
                    items = items.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            if (items == null)
            {
                items = new PermittedSubscriptionContainer[0];
            }

            return items;
        }

        public PermittedMediaContainer[] GetUserPermittedItems(InitializationObject initObj, string siteGuid)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserPermittedItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserPermittedItems(siteGuid);

                if (permitted != null)
                    permittedMediaContainer = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permittedMediaContainer;
        }

        public PermittedMediaContainer[] GetUserExpiredItems(InitializationObject initObj, string siteGuid, int totalItems)
        {
            PermittedMediaContainer[] permittedMediaContainer = { };

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetUserExpiredItems", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupId > 0)
            {
                var permitted = new TVPApiModule.Services.ApiConditionalAccessService(groupId, initObj.Platform).GetUserExpiredItems(siteGuid, totalItems);

                if (permitted != null)
                    permittedMediaContainer = permitted.OrderByDescending(r => r.m_dPurchaseDate.Date).ThenByDescending(r => r.m_dPurchaseDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return permittedMediaContainer;
        }

        public UserResponseObject SignUp(InitializationObject initObj, UserBasicData userBasicData, UserDynamicData userDynamicData, string sPassword, string sAffiliateCode)
        {
            UserResponseObject response = new UserResponseObject();

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SignUp", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData b = new TVPPro.SiteManager.TvinciPlatform.Users.UserBasicData();

                b.m_sFirstName = userBasicData.first_name;
                b.m_sAffiliateCode = userBasicData.affiliate_code;
                b.m_sLastName = userBasicData.last_name;
                b.m_sUserName = userBasicData.user_name;
                b.m_sFacebookID = userBasicData.facebook_id;
                b.m_bIsFacebookImagePermitted = userBasicData.is_facebook_image_permitted;
                b.m_sFacebookImage = userBasicData.facebook_image;
                b.m_sEmail = userBasicData.email;
                b.m_sAddress = userBasicData.address;
                b.m_sCity = userBasicData.city;
                b.m_sZip = userBasicData.zip;
                b.m_sPhone = userBasicData.phone;

                if (userBasicData.country != null)
                {
                    b.m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country();

                    b.m_Country.m_sCountryCode = userBasicData.country.country_code;
                    b.m_Country.m_sCountryName = userBasicData.country.country_name;
                    b.m_Country.m_nObjecrtID = userBasicData.country.object_id;
                }

                if (userBasicData.state != null)
                {
                    b.m_State = new TVPPro.SiteManager.TvinciPlatform.Users.State();

                    if (userBasicData.state.country != null)
                    {
                        b.m_State.m_Country = new TVPPro.SiteManager.TvinciPlatform.Users.Country();

                        b.m_State.m_Country.m_sCountryCode = userBasicData.state.country.country_code;
                        b.m_State.m_Country.m_sCountryName = userBasicData.state.country.country_name;
                        b.m_State.m_Country.m_nObjecrtID = userBasicData.state.country.object_id;
                    }

                    b.m_State.m_nObjecrtID = userBasicData.state.object_id;
                    b.m_State.m_sStateCode = userBasicData.state.state_code;
                    b.m_State.m_sStateName = userBasicData.state.state_name;
                }

                TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData d = new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicData();

                if (userDynamicData.user_data != null)
                {
                    d.m_sUserData = userDynamicData.user_data.Select(x => new TVPPro.SiteManager.TvinciPlatform.Users.UserDynamicDataContainer()
                    {
                        m_sDataType = x.data_type,
                        m_sValue = x.value
                    }).ToArray();
                }

                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).SignUp(b, d, sPassword, sAffiliateCode);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject GetUserByUserName(InitializationObject initObj, string sUserName)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByUserName", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByUsername(sUserName);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public UserResponseObject GetUserByFacebookID(InitializationObject initObj, string sFacebookID)
        {
            UserResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserByFacebookID", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserByFacebookID(sFacebookID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public FavoritObject[] GetUserFavorites(InitializationObject initObj, string siteGuid)
        {
            FavoritObject[] favoritesObj = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                favoritesObj = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).GetUserFavorites(siteGuid, string.Empty, initObj.DomainID, string.Empty);
                
                favoritesObj = favoritesObj.OrderByDescending(r => r.m_dUpdateDate.Date).ThenByDescending(r => r.m_dUpdateDate.TimeOfDay).ToArray();
            }
            else
            {
                throw new UnknownGroupException();
            }

            return favoritesObj;
        }

        public bool AddUserFavorite(InitializationObject initObj, string siteGuid, int mediaID, int mediaType, int extraVal)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                string isOfflineSync = ConfigurationManager.AppSettings[string.Concat(groupID, "_OfflineFavoriteSync")];

                retVal = new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).AddUserFavorite(siteGuid, initObj.DomainID, initObj.UDID, mediaType.ToString(), mediaID.ToString(), extraVal.ToString());

                if (!string.IsNullOrEmpty(isOfflineSync))
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).AddUserOfflineMedia(siteGuid, mediaID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public bool RemoveUserFavorite(InitializationObject initObj, string siteGuid, int mediaID)
        {
            bool retVal = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserFavorites", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                string isOfflineSync = ConfigurationManager.AppSettings[string.Concat(groupID, "_OfflineFavoriteSync")];

                new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RemoveUserFavorite(siteGuid, new int[] { mediaID });
                
                retVal = true;

                if (!string.IsNullOrEmpty(isOfflineSync))
                    new TVPApiModule.Services.ApiUsersService(groupID, initObj.Platform).RemoveUserOfflineMedia(siteGuid, mediaID);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return retVal;
        }

        public TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] GetUserGroupRules(InitializationObject initObj, string siteGuid)
        {
            TVPPro.SiteManager.TvinciPlatform.api.GroupRule[] response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetUserGroupRules", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).GetUserGroupRules(siteGuid);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool SetUserGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetUserGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).SetUserGroupRule(siteGuid, ruleID, PIN, isActive);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

        public bool CheckGroupRule(InitializationObject initObj, string siteGuid, int ruleID, string PIN)
        {
            bool response = false;
            
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "CheckGroupRule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                response = new TVPApiModule.Services.ApiApiService(groupID, initObj.Platform).CheckParentalPIN(siteGuid, ruleID, PIN);
            }
            else
            {
                throw new UnknownGroupException();
            }

            return response;
        }

    }
}