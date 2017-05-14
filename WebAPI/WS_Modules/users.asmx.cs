using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services;
using ApiObjects;
using KLogMonitor;
using Core.Users;
using ApiObjects.Response;
using ApiObjects.Billing;

namespace WS_Users
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://users.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class UsersService : System.Web.Services.WebService
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private int nMaxFailCount = 3;
        private int nLockMinutes = 3;

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject CheckUserPassword(string sWSUserName, string sWSPassword, string sUserName, string sPassword, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.CheckUserPassword(nGroupID, sUserName, sPassword, bPreventDoubleLogins);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject SignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, ApiObjects.KeyValuePair[] KeyValuePairs)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.SignIn(nGroupID, sUserName, sPassword, sessionID, sIP, deviceID, bPreventDoubleLogins, KeyValuePairs);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject KalturaSignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<ApiObjects.KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.KalturaSignIn(nGroupID, sUserName, sPassword, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject SignInWithToken(string sWSUserName, string sWSPassword, string sToken, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

                if (nGroupID != 0)
                {
                    return Core.Users.Module.SignInWithToken(nGroupID, sToken, sessionID, sIP, deviceID, bPreventDoubleLogins);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }


        [WebMethod]
        public virtual UserResponseObject SSOSignIn(string sWSUserName, string sWSPassword, string sUserName, string sPassword, int nSSOProviderID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

                if (nGroupID != 0)
                {
                    return Core.Users.Module.SSOSignIn(nGroupID, sUserName, sPassword, nSSOProviderID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }

        [WebMethod]
        public virtual UserResponseObject SSOCheckLogin(string sWSUserName, string sWSPassword, string sUserName, int nSSOProviderID)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.SSOCheckLogin(nGroupID, sUserName, nSSOProviderID);
                }
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject AutoSignIn(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                    return Core.Users.Module.AutoSignIn(nGroupID, sSiteGUID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }

        [WebMethod]
        public virtual UserResponseObject SignOut(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                    return Core.Users.Module.SignOut(nGroupID, sSiteGUID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }

        [WebMethod]
        public virtual UserResponseObject KalturaSignOut(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<ApiObjects.KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                {
                    log.Error("KalturaSignOut - Illegal Siteguid");
                    return null;
                }

                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetGroupID(sWSUserName, sWSPassword, "SignIn", ref kUser);
                if (nGroupID != 0 && kUser != null)
                    return FlowManager.SignOut(kUser, nSiteGuid, nGroupID, sessionID, sIP, deviceID, keyValueList);
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.Response.Status))]
        public virtual ApiObjects.Response.Status AddUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sItemCode, string sExtraData)
        {
            try
            {
                ApiObjects.Response.Status response = new ApiObjects.Response.Status();

                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

                
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.AddUserFavorit(nGroupID, sUserGUID, domainID, sDeviceUDID, sItemType, sItemCode, sExtraData);
                }
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
        }

        [WebMethod]
        public virtual bool AddChannelMediaToFavorites(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sChannelID, string sExtraData)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

                
                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                    return Core.Users.Module.AddChannelMediaToFavorites(nGroupID, sUserGUID, domainID, sDeviceUDID, sItemType, sChannelID, sExtraData);
                else
                    HttpContext.Current.Response.StatusCode = 404;
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.StatusCode = 404;
                log.Error("", ex);
            }
            return false;
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserState))]
        public virtual UserState GetUserState(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.GetUserState(nGroupID, sSiteGUID);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return UserState.Unknown;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                HttpContext.Current.Response.StatusCode = 404;
            }
            return UserState.Unknown;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserState))]
        public virtual UserState GetUserInstanceState(string sWSUserName, string sWSPassword, string sSiteGUID, string sessionID, string deviceID, string sIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserInstanceState(nGroupID, sSiteGUID, sessionID, sIP, deviceID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return UserState.Unknown;
            }
        }

        [WebMethod]
        public virtual bool WriteLog(string sWSUserName, string sWSPassword, string sUserGUID, string sLogMessage, string sWriter)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";


            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.WriteLog(nGroupID, sUserGUID, sLogMessage, sWriter);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        //[WebMethod]
        //public virtual void RemoveUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, Int32 nFavoritID)
        //{
        //    Users.BaseUsers t = null;
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        Core.Users.Module.RemoveUserFavorit(nFavoritID , sUserGUID, nGroupID);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //            HttpContext.Current.Response.StatusCode = 404;
        //    }
        //}

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.Response.Status))]
        public virtual ApiObjects.Response.Status RemoveUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, int[] nMediaIDs)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.RemoveUserFavorit(nGroupID, sUserGUID, nMediaIDs); ;
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
            }
        }

        [WebMethod]
        public virtual void RemoveChannelMediaUserFavorit(string sWSUserName, string sWSPassword, string sUserGUID, int[] nChannelIDs)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";


            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Core.Users.Module.RemoveChannelMediaUserFavorit(nGroupID, sUserGUID, nChannelIDs);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(FavoritObject))]
        public virtual FavoriteResponse GetUserFavorites(string sWSUserName, string sWSPassword, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, FavoriteOrderBy orderBy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserFavorites(nGroupID, sUserGUID, domainID, sDeviceUDID, sItemType, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new FavoriteResponse()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error"),
                    Favorites = null
                };
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject GetUserByFacebookID(string sWSUserName, string sWSPassword, string sFacebookID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserByFacebookID(nGroupID, sFacebookID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject GetUserByUsername(string sWSUserName, string sWSPassword, string sUsername)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserByUsername(nGroupID, sUsername);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject GetUserDataByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid, int operatorID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserDataByCoGuid(nGroupID, sCoGuid, operatorID);
            }
            else
            {
                {
                    log.Debug("blocked - " + sWSUserName + " || " + sWSPassword);
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return null;
            }
        }

       

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject GetUserData(string sWSUserName, string sWSPassword, string sSiteGUID, string sUserIp)
        {
            // add siteguid to logs/monitor
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";
            }

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserData(nGroupID, sSiteGUID, sUserIp);
            }
            else
            {
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual List<UserResponseObject> GetUsersData(string sWSUserName, string sWSPassword, string[] sSiteGUIDs, string userIp)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUsersData(nGroupID, sSiteGUIDs, userIp);
            }
            else
            {
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        public virtual List<UserBasicData> SearchUsers_MT(string sWSUserName, string sWSPassword, string sTerms, string sFields, bool bIsExact)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SearchUsers_MT(nGroupID, sTerms, sFields, bIsExact);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        public virtual List<UserBasicData> SearchUsers(string sWSUserName, string sWSPassword, string[] sTerms, string[] sFields, bool bIsExact)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SearchUsers(nGroupID, sTerms, sFields, bIsExact);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject AddNewUser(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
            {
                // old Core to PS flow
                
                Utils.GetGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Users.Module.AddNewUser(nGroupID, oBasicData, sDynamicData, sPassword, sAffiliateCode);
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetGroupID(sWSUserName, sWSPassword, "SignIn", ref kUser);
                if (nGroupID != 0 && kUser != null)
                {
                    return FlowManager.AddNewUser(kUser, oBasicData, sDynamicData, sPassword, new List<KeyValuePair>());
                }
                else
                {
                    HttpContext.Current.Response.StatusCode = 404;
                    return null;
                }
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject AddNewKalturaUser(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode, List<ApiObjects.KeyValuePair> keyValueList)
        {
            KalturaBaseUsers kUser = null;

            // get operatorId if exists
            int operatorId = -1;
            if (keyValueList != null)
            {
                var keyValueOperatorId = keyValueList.FirstOrDefault(x => x.key == "operator");
                if (keyValueOperatorId != null)
                    operatorId = Convert.ToInt32(keyValueOperatorId.value);
            }
            // get group ID + user type
            int nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SignIn", ref kUser);
            if (nGroupID != 0 && kUser != null)
            {
                return FlowManager.AddNewUser(kUser, oBasicData, sDynamicData, sPassword, keyValueList);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        /*
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject AddNewUserST(string sWSUserName, string sWSPassword, string sBasicDataXML, string sDynamicDataXML , string sPassword)
        {
            Users.BaseUsers t = null;
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "AddNewUserST", ref t);
            if (nGroupID != 0)
            {
                return Core.Users.Module.AddNewUser(nGroupID, sBasicDataXML, sDynamicDataXML, sPassword);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        */
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject SetUserData(string sWSUserName, string sWSPassword, string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetUserData(nGroupID, sSiteGUID, oBasicData, sDynamicData);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        [WebMethod]
        public virtual void Hit(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Core.Users.Module.Hit(nGroupID, sSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }
        [WebMethod]
        public virtual void Logout(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Core.Users.Module.Logout(nGroupID, sSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }
        /*
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject SetUserDataST(string sWSUserName, string sWSPassword, string sSiteGUID, string sBasicDataXML, string sDynamicDataXML)
        {
            Users.BaseUsers t = null;
            Int32 nGroupID = GetGroupID(sWSUserName, sWSPassword, "SetUserDataST", ref t);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetUserData(nGroupID, sSiteGUID, sBasicDataXML, sDynamicDataXML);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        */
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject ChangeUserPassword(string sWSUserName, string sWSPassword, string sUN, string sOldPass, string sPass)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ChangeUserPassword(nGroupID, sUN, sOldPass, sPass);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status UpdateUserPassword(string sWSUserName, string sWSPassword, int userId, string password)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.UpdateUserPassword(nGroupID, userId, password);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject ForgotPassword(string sWSUserName, string sWSPassword, string sUN)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ForgotPassword(nGroupID, sUN);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject ChangePassword(string sWSUserName, string sWSPassword, string sUN)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ChangePassword(nGroupID, sUN);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        public virtual ResponseStatus SendChangedPinMail(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserRuleID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SendChangedPinMail(nGroupID, sSiteGuid, nUserRuleID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return ResponseStatus.ErrorOnSendingMail;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject CheckTemporaryToken(string sWSUserName, string sWSPassword, string sToken)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {


                return Core.Users.Module.CheckTemporaryToken(nGroupID, sToken);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject RenewUserPassword(string sWSUserName, string sWSPassword, string sUserName,
            string sNewPassword)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.RenewUserPassword(nGroupID, sUserName, sNewPassword);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual bool ResendWelcomeMail(string sWSUserName, string sWSPassword, string sUserName,
            string sNewPassword)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ResendWelcomeMail(nGroupID, sUserName, sNewPassword);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual ApiObjects.Response.Status ResendActivationMail(string sWSUserName, string sWSPassword, string sUserName,
            string sNewPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ResendActivationMail(nGroupID, sUserName, sNewPassword);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponse ActivateAccount(string sWSUserName, string sWSPassword, string sUserName,
            string sToken)
        {
            UserResponse response = new UserResponse();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ActivateAccount(nGroupID, sUserName, sToken);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual UserResponseObject ActivateAccountByDomainMaster(string sWSUserName, string sWSPassword, string sMasterUsername,
            string sUserName, string sToken)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ActivateAccountByDomainMaster(nGroupID, sMasterUsername, sUserName, sToken);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool SendPasswordMail(string sWSUserName, string sWSPassword, string sUserName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SendPasswordMail(nGroupID, sUserName);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual string GetUserToken(string sWSUserName, string sWSPassword, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserToken(nGroupID, sSiteGUID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual bool DoesUserNameExists(string sWSUserName, string sWSPassword, string sUserName)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.DoesUserNameExists(nGroupID, sUserName);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Users.Country))]
        [System.Xml.Serialization.XmlInclude(typeof(State))]
        public virtual Core.Users.Country[] GetCountryList(string sWSUserName, string sWSPassword)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Utils.GetCountryList();
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Users.Country))]
        [System.Xml.Serialization.XmlInclude(typeof(State))]
        public virtual State[] GetStateList(string sWSUserName, string sWSPassword, Int32 nCountryID)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Utils.GetStateList(nCountryID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Users.Country))]
        public virtual Core.Users.Country GetIPToCountry(string sWSUserName, string sWSPassword, string sUserIP)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Utils.GetIPCountry2(sUserIP);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        public virtual User[] GetUsersLikedMedia(string sWSUserName, string sWSPassword, Int32 nUserGuid, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserGuid.ToString();

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUsersLikedMedia(nGroupID, nUserGuid, nMediaID, nPlatform, bOnlyFriends, nStartIndex, nNumberOfItems);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status IsUserActivated(string sWSUserName, string sWSPassword, Int32 userId)

        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId.ToString();
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                response = Core.Users.Module.IsUserActivated(nGroupID, userId);
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserOfflineObject))]
        public virtual UserOfflineObject[] GetAllUserOfflineAssets(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetAllUserOfflineAssets(nGroupID, sSiteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }
        //[WebMethod]
        //[System.Xml.Serialization.XmlInclude(typeof(UserOfflineObject))]
        //public virtual UserOfflineObject[] GetUserOfflineAssetsByFileType(string sWSUserName, string sWSPassword, string sSiteGuid, string sFileType)
        //{
        //    Users.BaseUsers t = null;
        //    Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
        //    if (nGroupID != 0)
        //    {
        //        return Core.Users.Module.GetUserOfflineItemsByFileType(nGroupID, sSiteGuid, sFileType);
        //    }
        //    else
        //    {
        //        if (nGroupID == 0)
        //            HttpContext.Current.Response.StatusCode = 404;
        //        return null;
        //    }
        //}
        [WebMethod]
        public virtual bool AddUserOfflineAsset(string sWSUserName, string sWSPassword, string sSiteGuid, string sMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.AddUserOfflineAsset(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        [WebMethod]
        public virtual bool RemoveUserOfflineAsset(string sWSUserName, string sWSPassword, string sSiteGuid, string sMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.RemoveUserOfflineAsset(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }
        [WebMethod]
        public virtual bool ClearUserOfflineAssets(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ClearUserOfflineAssets(nGroupID, sSiteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        public virtual bool SetUserDynamicData(string sWSUserName, string sWSPassword, string sSiteGuid, string sType, string sValue)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetUserDynamicData(nGroupID, sSiteGuid, sType, sValue);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return false;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserGroupRuleResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(UserGroupRuleResponseStatus))]
        public virtual UserGroupRuleResponse CheckParentalPINToken(string sWSUserName, string sWSPassword, string sToken)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Users.Module.CheckParentalPINToken(nGroupID, sToken);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        public virtual UserGroupRuleResponse ChangeParentalPInCodeByToken(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            UserGroupRuleResponse response = null;
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response = Core.Users.Module.ChangeParentalPInCodeByToken(nGroupID, sSiteGuid, nUserRuleID, sChangePinToken, sCode);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;

                response = new UserGroupRuleResponse() { ResponseStatus = UserGroupRuleResponseStatus.Error };
            }
            return response;
        }



        [WebMethod]
        public virtual bool AddItemToList(string sWSUserName, string sWSPassword, UserItemList userItemList)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            bool result = false;
            if (nGroupID != 0)
            {
                result = Core.Users.Module.AddItemToList(nGroupID, userItemList);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return result;
        }

        [WebMethod]
        public virtual bool RemoveItemFromList(string sWSUserName, string sWSPassword, UserItemList userItemList)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            bool result = false;
            if (nGroupID != 0)
            {
                result = Core.Users.Module.RemoveItemFromList(nGroupID, userItemList);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return result;
        }

        [WebMethod]
        public virtual bool UpdateItemInList(string sWSUserName, string sWSPassword, UserItemList userItemList)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            bool result = false;
            if (nGroupID != 0)
            {
                result = Core.Users.Module.UpdateItemInList(nGroupID, userItemList);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return result;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(List<UserItemList>))]
        [System.Xml.Serialization.XmlInclude(typeof(UserItemList))]
        public virtual UserItemListsResponse GetItemFromList(string sWSUserName, string sWSPassword, UserItemList userItemList)
        {
            UserItemListsResponse response = new UserItemListsResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Users.Module.GetItemFromList(nGroupID, userItemList);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UsersItemsListsResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(UserItemsList))]
        [System.Xml.Serialization.XmlInclude(typeof(Item))]
        public virtual UsersItemsListsResponse GetItemsFromUsersLists(string sWSUserName, string sWSPassword, List<string> userIds, ListType listType, ListItemType itemType)
        {
            UsersItemsListsResponse response = new UsersItemsListsResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Users.Module.GetItemsFromUsersLists(nGroupID, userIds, listType, itemType);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(UserItemList))]

        public virtual List<ApiObjects.KeyValuePair> IsItemExistsInList(string sWSUserName, string sWSPassword, UserItemList userItemList)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Users.Module.IsItemExistsInList(nGroupID, userItemList);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            return null;
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Core.Users.Country))]
        [System.Xml.Serialization.XmlInclude(typeof(State))]
        public virtual UserType[] GetGroupUserTypes(string sWSUserName, string sWSPassword)
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetGroupUserTypes(nGroupID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return null;
            }
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        public virtual ResponseStatus SetUserTypeByUserID(string sWSUserName, string sWSPassword, string sSiteGuid, int nUserTypeID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetUserTypeByUserID(nGroupID, sSiteGuid, nUserTypeID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return ResponseStatus.ErrorOnSendingMail;
            }
        }

        [WebMethod]
        public virtual int GetUserType(string sWSUserName, string sWSPassword, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUserType(nGroupID, sSiteGuid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return 0;
            }
        }

        [WebMethod]
        public virtual PinCodeResponse GenerateLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string secret)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GenerateLoginPIN(nGroupID, siteGuid, secret);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new PinCodeResponse();
            }
        }

        [WebMethod]
        public virtual UserResponse LoginWithPIN(string sWSUserName, string sWSPassword, string PIN, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins,
            List<ApiObjects.KeyValuePair> keyValueList, string secret)
        {
            UserResponse response = new UserResponse();

            // get group ID + user implementation
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.LoginWithPIN(nGroupID, PIN, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList, secret);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual PinCodeResponse SetLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string PIN, string secret)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetLoginPIN(nGroupID, siteGuid, PIN, secret);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new PinCodeResponse();
            }
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status ClearLoginPIN(string sWSUserName, string sWSPassword, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ClearLoginPIN(nGroupID, siteGuid, pin);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        [WebMethod]
        public virtual UserResponse LogIn(string sWSUserName, string sWSPassword, string userName, string password, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins,
            List<ApiObjects.KeyValuePair> keyValueList)
        {
            UserResponse response = new UserResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response.user = KalturaSignIn(sWSUserName, sWSPassword, userName, password, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList);
                if (response.user != null)
                {
                    // convert response status
                    response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);
                    int userID;

                    if (response.resp.Code == (int)ApiObjects.Response.eResponseStatus.OK && int.TryParse(response.user.m_user.m_sSiteGUID, out userID) && userID > 0)
                    {
                        Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.Login, userID, deviceID);
                    }
                    else
                        log.ErrorFormat("LogIn: error while signing in out: user: {0}, group: {1}, error: {2}", userName, nGroupID, response.resp.Code);
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;

        }


        [WebMethod]
        public virtual UserResponse SignUp(string sWSUserName, string sWSPassword, UserBasicData oBasicData, UserDynamicData dynamicData, string password, string affiliateCode)
        {
            UserResponse response = new UserResponse();

            // add username to logs/monitor
            if (oBasicData != null && !string.IsNullOrEmpty(oBasicData.m_sUserName))
            {
                HttpContext.Current.Items[Constants.USER_ID] = oBasicData.m_sUserName;
            }

            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                response.user = AddNewUser(sWSUserName, sWSPassword, oBasicData, dynamicData, password, affiliateCode);

                if (response.user != null)
                {
                    if (response.user.m_RespStatus == ResponseStatus.OK || response.user.m_RespStatus == ResponseStatus.UserWithNoDomain)
                    {
                        response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        // convert response status
                        response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);
                    }
                }
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status SendRenewalPasswordMail(string sWSUserName, string sWSPassword, string userName)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SendRenewalPasswordMail(nGroupID, userName);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status RenewPassword(string sWSUserName, string sWSPassword, string userName, string newPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.RenewPassword(nGroupID, userName, newPassword);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status ReplacePassword(string sWSUserName, string sWSPassword, string userName, string oldPassword, string newPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.ReplacePassword(nGroupID, userName, oldPassword, newPassword);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual UserResponse CheckPasswordToken(string sWSUserName, string sWSPassword, string token)
        {
            UserResponse response = new UserResponse();
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.CheckPasswordToken(nGroupID, token);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual UsersResponse GetUsers(string sWSUserName, string sWSPassword, string[] sSiteGUIDs, string userIP)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.GetUsers(nGroupID, sSiteGUIDs, userIP);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                UsersResponse response = new UsersResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public virtual UserResponse SetUser(string sWSUserName, string sWSPassword, string siteGUID, UserBasicData basicData, UserDynamicData dynamicData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID != null ? siteGUID : "null";


            UserResponse response = new UserResponse();
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Users.Module.SetUser(nGroupID, siteGUID, basicData, dynamicData);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
                response = new UserResponse();
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = ApiObjects.Response.eResponseStatus.Error.ToString();
                return response;
            }
        }

        [WebMethod]
        public virtual FavoriteResponse FilterFavoriteMediaIds(string sWSUserName, string sWSPassword, string userId, List<int> mediaIds, string udid, string mediaType, FavoriteOrderBy orderBy)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            FavoriteResponse response = new FavoriteResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.FilterFavoriteMediaIds(nGroupID, userId, mediaIds, udid, mediaType, orderBy);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual LongIdsResponse GetUserRoleIds(string sWSUserName, string sWSPassword, string userId)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            LongIdsResponse response = new LongIdsResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.GetUserRoleIds(nGroupID, userId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status AddRoleToUser(string sWSUserName, string sWSPassword, string userId, long roleId)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.AddRoleToUser(nGroupID, userId, roleId);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status DeleteUser(string sWSUserName, string sWSPassword, int userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                response = Core.Users.Module.DeleteUser(nGroupID, userId);
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            if (response.Code == (int)eResponseStatus.OK)
            {
                Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.DeleteUser, userId, string.Empty);
            }
            else
                log.ErrorFormat("DeleteUser: error while deleting user: user: {0}, group: {1}, error: {2}", userId, nGroupID, response.Code);

            return response;
        }

        [WebMethod]
        public ApiObjects.Response.Status ChangeUsers(string sWSUserName, string sWSPassword, string userId, string userIdToChange, string udid)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.ChangeUsers(nGroupID, userId, userIdToChange, udid);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public void AddInitiateNotificationAction(string sWSUserName, string sWSPassword, eUserMessageAction userAction, int userId, string udid, string pushToken = "")
        {
            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.AnonymousPushRegistration, userId, udid, pushToken);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public virtual ApiObjects.Response.Status ResendActivationToken(string sWSUserName, string sWSPassword, string username)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.ResendActivationToken(nGroupID, username);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual ApiObjects.Response.Status DeleteItemFromUsersList(string sWSUserName, string sWSPassword, string userId, Item item)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.DeleteItemFromUsersList(nGroupID, userId, item);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual UsersListItemResponse AddItemToUsersList(string sWSUserName, string sWSPassword, string userId, Item item)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.AddItemToUsersList(nGroupID, userId, item);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual UsersListItemResponse GetItemFromUsersList(string sWSUserName, string sWSPassword, string userId, Item item)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.GetItemFromUsersList(nGroupID, userId, item);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public virtual UserResponse GetUserByExternalID(string sWSUserName, string sWSPassword, string externalId, int operatorID)
        {
            UserResponse response = new UserResponse()
            {
                resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.GetUserByExternalID(nGroupID, externalId, operatorID);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;               
            }
            return response;
        }
        [WebMethod]
        public virtual UserResponse GetUserByName(string sWSUserName, string sWSPassword, string username)
        {
            UserResponse response = new UserResponse()
            {
                resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Users.Module.GetUserByName(nGroupID, username);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }
    }
}
