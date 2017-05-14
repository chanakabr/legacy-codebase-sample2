using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Core.Users;
using ApiObjects;
using System.Reflection;
using ApiObjects.Response;

namespace Core.Users
{
    public class Module
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static int nMaxFailCount = 3;
        private static int nLockMinutes = 3;

        
        public static UserResponseObject CheckUserPassword(int nGroupID, string sUserName, string sPassword, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    return t.CheckUserPassword(sUserName, sPassword, 3, 3, nGroupID, bPreventDoubleLogins);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject SignIn(int nGroupID, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, ApiObjects.KeyValuePair[] KeyValuePairs)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
                {
                    // old Core to PS flow
                    BaseUsers t = null;
                    Utils.GetBaseImpl(ref t, nGroupID);
                    if (t != null)
                        return t.SignIn(sUserName, sPassword, 3, 3, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                }
                else
                {
                    // new Core to PS flow
                    KalturaBaseUsers kUser = null;

                    // get group ID + user type
                    Utils.GetBaseImpl(ref kUser, nGroupID);
                    if (kUser != null)
                        return FlowManager.SignIn(0, kUser, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, KeyValuePairs.ToList(), sUserName, sPassword);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject KalturaSignIn(int nGroupID, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<ApiObjects.KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

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
                Utils.GetBaseImpl(ref kUser, nGroupID, operatorId);
                if (kUser != null)
                    return FlowManager.SignIn(0, kUser, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList, sUserName, sPassword);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject SignInWithToken(int nGroupID, string sToken, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
                {
                    BaseUsers t = null;
                    Utils.GetBaseImpl(ref t, nGroupID);
                    if (t != null)
                    {
                        return t.SignInWithToken(sToken, 3, 3, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                    }
                }
                else
                {
                    // new Core to PS flow
                    KalturaBaseUsers kUser = null;

                    // get group ID + user type
                    Utils.GetBaseImpl(ref kUser, nGroupID);
                    if (kUser != null)
                    {
                        // add the token to the key-value list
                        List<KeyValuePair> keyValuePair = new List<KeyValuePair>();
                        keyValuePair.Add(new KeyValuePair() { key = "token", value = sToken });
                        return FlowManager.SignIn(0, kUser, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValuePair);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }


        
        public static UserResponseObject SSOSignIn(int nGroupID, string sUserName, string sPassword, int nSSOProviderID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
                {
                    // old Core to PS flow
                    BaseUsers t = null;
                    Utils.GetBaseImpl(ref t, nGroupID);
                    if (t != null)
                    {
                        ISSOProvider sso = ((SSOUsers)t).GetSSOImplementation(nSSOProviderID);
                        if (sso != null)
                        {
                            //Operator ID might have been changed to default ID by implementation in case it was 0
                            nSSOProviderID = ((SSOUsers)sso).OperatorId;
                            return sso.SignIn(sUserName, sPassword, nSSOProviderID, 3, 3, sessionID, sIP, deviceID, bPreventDoubleLogins);
                        }
                        else
                            return new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };
                    }
                }
                else
                {
                    // new Core to PS flow
                    KalturaBaseUsers kUser = null;

                    // get group ID + user type
                    Utils.GetBaseImpl(ref kUser, nGroupID, nSSOProviderID);
                    if (kUser != null)
                    {
                        // add the provider ID to the key-value list
                        List<KeyValuePair> keyValuePair = new List<KeyValuePair>();
                        keyValuePair.Add(new KeyValuePair() { key = "operator", value = nSSOProviderID.ToString() });
                        return FlowManager.SignIn(0, kUser, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValuePair, sUserName, sPassword);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject SSOCheckLogin(int nGroupID, string sUserName, int nSSOProviderID)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserName != null ? sUserName : "null";

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    ISSOProvider sso = ((SSOUsers)t).GetSSOImplementation(nSSOProviderID);
                    if (sso != null)
                        return sso.CheckLogin(sUserName, nSSOProviderID);
                    else
                        return new UserResponseObject() { m_RespStatus = ResponseStatus.WrongPasswordOrUserName };
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject AutoSignIn(int nGroupID, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                    return null;

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                    return t.SignIn(nSiteGuid, 3, 3, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject SignOut(int nGroupID, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                {
                    log.Debug("SignOut - Illegal Siteguid");
                    return null;
                }

                if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
                {
                    // old Core to PS flow
                    BaseUsers t = null;
                    Utils.GetBaseImpl(ref t, nGroupID);
                    if (t != null)
                        return t.SignOut(nSiteGuid, sessionID, sIP, deviceID);
                }
                else
                {
                    // new Core to PS flow
                    KalturaBaseUsers kUser = null;

                    // get group ID + user type
                    Utils.GetBaseImpl(ref kUser, nGroupID);
                    if (kUser != null)
                        return FlowManager.SignOut(kUser, nSiteGuid, nGroupID, sessionID, sIP, deviceID, new List<KeyValuePair>());
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static UserResponseObject KalturaSignOut(int nGroupID, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<ApiObjects.KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                {
                    log.Error("KalturaSignOut - Illegal Siteguid");
                    return null;
                }

                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser != null)
                    return FlowManager.SignOut(kUser, nSiteGuid, nGroupID, sessionID, sIP, deviceID, keyValueList);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        
        public static ApiObjects.Response.Status AddUserFavorit(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sItemCode, string sExtraData)
        {
            try
            {
                ApiObjects.Response.Status response = new ApiObjects.Response.Status();

                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    return t.AddUserFavorit(sUserGUID, domainID, sDeviceUDID, sItemType, sItemCode, sExtraData, nGroupID);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
        }

        
        public static bool AddChannelMediaToFavorites(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sChannelID, string sExtraData)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                    return t.AddChannelMediaToFavorites(sUserGUID, domainID, sDeviceUDID, sItemType, sChannelID, sExtraData, nGroupID);
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return false;
        }


        
        public static UserState GetUserState(int nGroupID, string sSiteGUID)
        {
            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                    return UserState.Unknown;
                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    return t.GetUserState(nSiteGuid);
                }
                else
                {
                    return UserState.Unknown;
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return UserState.Unknown;
        }

        
        public static UserState GetUserInstanceState(int nGroupID, string sSiteGUID, string sessionID, string deviceID, string sIP)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            int nSiteGuid;
            if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                return UserState.Unknown;
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserInstanceState(nSiteGuid, sessionID, sIP, deviceID);
            }
            else
            {
                return UserState.Unknown;
            }
        }

        
        public static bool WriteLog(int nGroupID, string sUserGUID, string sLogMessage, string sWriter)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";


            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.WriteToLog(sUserGUID, sLogMessage, sWriter);
            }
            else
            {
                return false;
            }
        }

        
        public static ApiObjects.Response.Status RemoveUserFavorit(int nGroupID, string sUserGUID, int[] nMediaIDs)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RemoveUserFavorit(nMediaIDs, sUserGUID, nGroupID); ;
            }
            else
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
            }
        }


        public static void RemoveChannelMediaUserFavorit(int nGroupID, string sUserGUID, int[] nChannelIDs)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";


            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                t.RemoveChannelMediaUserFavorit(nChannelIDs, sUserGUID, nGroupID);
            }
        }
        
        public static FavoriteResponse GetUserFavorites(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, FavoriteOrderBy orderBy)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserFavorites(sUserGUID, sDeviceUDID, sItemType, nGroupID, domainID, orderBy);
            }
            else
            {
                return new FavoriteResponse()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error"),
                    Favorites = null
                };
            }
        }

        
        public static UserResponseObject GetUserByFacebookID(int nGroupID, string sFacebookID)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserByFacebookID(sFacebookID, nGroupID);
            }
            else
            {
                return null;
            }
        }

        
        public static UserResponseObject GetUserByUsername(int nGroupID, string sUsername)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserByUsername(sUsername, nGroupID);
            }
            else
            {
                return null;
            }
        }


        
        public static UserResponseObject GetUserDataByCoGuid(int nGroupID, string sCoGuid, int operatorID)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserByCoGuid(sCoGuid, operatorID);
            }
            else
            {
                log.Debug("blocked - " + nGroupID);
                return null;
            }
        }



        
        public static UserResponseObject GetUserData(int nGroupID, string sSiteGUID, string sUserIp)
        {
            // add siteguid to logs/monitor
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";
            }

            if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
            {
                // old Core to PS flow
                BaseUsers user = null;
                Utils.GetBaseImpl(ref user, nGroupID);
                if (user != null)
                {
                    return user.GetUserData(sSiteGUID);
                }
                else
                {
                    log.Debug("blocked - " + nGroupID);
                    return null;
                }
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser != null)
                {
                    return FlowManager.GetUserData(kUser, sSiteGUID, new List<KeyValuePair>(), sUserIp);
                }
                else
                {
                    return null;
                }
            }
        }

        
        public static List<UserResponseObject> GetUsersData(int nGroupID, string[] sSiteGUIDs, string userIp)
        {
            List<UserResponseObject> userResponseList = null;
            // add siteguid to logs/monitor
            if (sSiteGUIDs != null && sSiteGUIDs.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var siteGuid in sSiteGUIDs)
                    sb.Append(String.Format("{0} ", siteGuid));

                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[Constants.USER_ID] = sb.ToString();
                }
            }

            if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
            {
                BaseUsers user = null;
                Utils.GetBaseImpl(ref user, nGroupID);
                if (user != null)
                {
                    userResponseList = user.GetUsersData(sSiteGUIDs);
                }
                else
                {
                    log.Debug("blocked - " + nGroupID);
                    return null;
                }
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser != null)
                {
                    userResponseList = FlowManager.GetUsersData(kUser, sSiteGUIDs.ToList(), new List<KeyValuePair>(), userIp);
                }
                else
                {
                    return null;
                }
            }

            if (userResponseList != null)
            {
                userResponseList = userResponseList.Select(x => x).Where(y => y.m_RespStatus != ResponseStatus.UserDoesNotExist).ToList();
            }
            return userResponseList;
        }

        
        public static List<UserBasicData> SearchUsers_MT(int nGroupID, string sTerms, string sFields, bool bIsExact)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                string[] terms = sTerms.Split(';');
                string[] fields = sFields.Split(';');

                return t.SearchUsers(terms, fields, bIsExact);
            }
            else
            {
                return null;
            }
        }

        
        public static List<UserBasicData> SearchUsers(int nGroupID, string[] sTerms, string[] sFields, bool bIsExact)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SearchUsers(sTerms, sFields, bIsExact);
            }
            else
            {
                return null;
            }
        }

        
        public static UserResponseObject AddNewUser(int nGroupID, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode)
        {
            if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
            {
                // old Core to PS flow
                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    return t.AddNewUser(oBasicData, sDynamicData, sPassword);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser != null)
                {
                    return FlowManager.AddNewUser(kUser, oBasicData, sDynamicData, sPassword, new List<KeyValuePair>());
                }
                else
                {
                    return null;
                }
            }
        }

        
        public static UserResponseObject AddNewKalturaUser(int nGroupID, UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword, string sAffiliateCode, List<ApiObjects.KeyValuePair> keyValueList)
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
            Utils.GetBaseImpl(ref kUser, nGroupID, operatorId);
            if (kUser != null)
            {
                return FlowManager.AddNewUser(kUser, oBasicData, sDynamicData, sPassword, keyValueList);
            }
            else
            {
                return null;
            }
        }
        
        public static UserResponseObject SetUserData(int nGroupID, string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetUserData(sSiteGUID, oBasicData, sDynamicData);
            }
            else
            {
                return null;
            }
        }
        
        public static void Hit(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                t.Hit(sSiteGUID);
            }
        }
        
        public static void Logout(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                t.Logout(sSiteGUID);
            }
        }
        
        public static UserResponseObject ChangeUserPassword(int nGroupID, string sUN, string sOldPass, string sPass)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChangeUserPassword(sUN, sOldPass, sPass, nGroupID);
            }
            else
            {
                return null;
            }
        }

        
        public static ApiObjects.Response.Status UpdateUserPassword(int nGroupID, int userId, string password)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.UpdateUserPassword(userId, password);
            }
            else
            {
                return null;
            }
        }

        
        public static UserResponseObject ForgotPassword(int nGroupID, string sUN)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ForgotPassword(sUN);
            }
            else
            {
                return null;
            }
        }

        
        public static UserResponseObject ChangePassword(int nGroupID, string sUN)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChangePassword(sUN);
            }
            else
            {
                return null;
            }
        }

        
        public static ResponseStatus SendChangedPinMail(int nGroupID, string sSiteGuid, int nUserRuleID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SendChangedPinMail(sSiteGuid, nUserRuleID);
            }
            else
            {
                return ResponseStatus.ErrorOnSendingMail;
            }
        }

        
        public static UserResponseObject CheckTemporaryToken(int nGroupID, string sToken)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {


                return t.CheckToken(sToken);
            }
            else
            {
                return null;
            }
        }

        
        public static UserResponseObject RenewUserPassword(int nGroupID, string sUserName,
            string sNewPassword)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RenewPassword(sUserName, sNewPassword, nGroupID);
            }
            else
            {
                return null;
            }
        }

        
        public static bool ResendWelcomeMail(int nGroupID, string sUserName,
            string sNewPassword)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ResendWelcomeMail(sUserName);
            }
            else
            {
                return false;
            }
        }

        
        public static ApiObjects.Response.Status ResendActivationMail(int nGroupID, string sUserName,
            string sNewPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                if (t.ResendActivationMail(sUserName))
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
            }

            return response;
        }

        
        public static UserResponse ActivateAccount(int nGroupID, string sUserName,
            string sToken)
        {
            UserResponse response = new UserResponse();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.user = t.ActivateAccount(sUserName, sToken);
                if (response.user != null)
                {
                    // convert response status
                    response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);
                }
            }
            return response;
        }

        
        public static UserResponseObject ActivateAccountByDomainMaster(int nGroupID, string sMasterUsername,
            string sUserName, string sToken)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ActivateAccountByDomainMaster(sMasterUsername, sUserName, sToken);
            }
            else
            {
                return null;
            }
        }

        
        public static bool SendPasswordMail(int nGroupID, string sUserName)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SendPasswordMail(sUserName);
            }
            else
            {
                return false;
            }
        }

        
        public static string GetUserToken(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGUID != null ? sSiteGUID : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserToken(sSiteGUID, nGroupID);
            }
            else
            {
                return null;
            }
        }

        
        public static bool DoesUserNameExists(int nGroupID, string sUserName)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.DoesUserNameExists(sUserName);
            }
            else
            {
                return false;
            }
        }

        
        public static Users.Country[] GetCountryList(int nGroupID)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return Users.Utils.GetCountryList();
            }
            else
            {
                return null;
            }
        }

        
        public static State[] GetStateList(int nGroupID, Int32 nCountryID)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return Users.Utils.GetStateList(nCountryID);
            }
            else
            {
                return null;
            }
        }

        
        public static Users.Country GetIPToCountry(int nGroupID, string sUserIP)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return Users.Utils.GetIPCountry2(sUserIP);
            }
            else
            {
                return null;
            }
        }

        
        public static User[] GetUsersLikedMedia(int nGroupID, Int32 nUserGuid, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserGuid.ToString();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUsersLikedMedia(nUserGuid, nMediaID, nPlatform, bOnlyFriends, nStartIndex, nNumberOfItems);
            }
            else
            {
                return null;
            }
        }
        
        public static ApiObjects.Response.Status IsUserActivated(int nGroupID, Int32 nUserID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserID.ToString();
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.IsUserActivated(nUserID);
            }

            return response;
        }
        
        public static UserOfflineObject[] GetAllUserOfflineAssets(int nGroupID, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserOfflineMedia(nGroupID, sSiteGuid);
            }
            else
            {
                return null;
            }
        }
        
        public static bool AddUserOfflineAsset(int nGroupID, string sSiteGuid, string sMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.AddUserOfflineItems(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                return false;
            }
        }
        
        public static bool RemoveUserOfflineAsset(int nGroupID, string sSiteGuid, string sMediaID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RemoveUserOfflineItems(nGroupID, sSiteGuid, sMediaID);
            }
            else
            {
                return false;
            }
        }
        
        public static bool ClearUserOfflineAssets(int nGroupID, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ClearUserOfflineItems(nGroupID, sSiteGuid);
            }
            else
            {
                return false;
            }
        }

        
        public static bool SetUserDynamicData(int nGroupID, string sSiteGuid, string sType, string sValue)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                ApiObjects.KeyValuePair kvp = new ApiObjects.KeyValuePair(sType, sValue);
                List<ApiObjects.KeyValuePair> list = new List<ApiObjects.KeyValuePair>() { kvp };
                return t.SetUserDynamicData(sSiteGuid, list, null);
            }
            else
            {
                return false;
            }
        }

        
        public static UserGroupRuleResponse CheckParentalPINToken(int nGroupID, string sToken)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            return t.CheckParentalPINToken(sToken);
        }

        
        public static UserGroupRuleResponse ChangeParentalPInCodeByToken(int nGroupID, string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            UserGroupRuleResponse response = null;
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            response = t.ChangeParentalPInCodeByToken(sSiteGuid, nUserRuleID, sChangePinToken, sCode);
            
            return response;
        }



        
        public static bool AddItemToList(int nGroupID, UserItemList userItemList)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            bool result = false;
            if (t != null)
            {
                result = t.AddItemToList(userItemList, nGroupID);
            }
            return result;
        }

        
        public static bool RemoveItemFromList(int nGroupID, UserItemList userItemList)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            bool result = false;
            if (t != null)
            {
                result = t.RemoveItemFromList(userItemList, nGroupID);
            }
            return result;
        }

        
        public static bool UpdateItemInList(int nGroupID, UserItemList userItemList)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            bool result = false;
            if (t != null)
            {
                result = t.UpdateItemInList(userItemList, nGroupID);
            }
            return result;
        }

        
        public static UserItemListsResponse GetItemFromList(int nGroupID, UserItemList userItemList)
        {
            UserItemListsResponse response = new UserItemListsResponse();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            if (t != null)
            {
                response.UserItemLists = t.GetItemFromList(userItemList, nGroupID);
                if (response.UserItemLists != null)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static UsersItemsListsResponse GetItemsFromUsersLists(int nGroupID, List<string> userIds, ListType listType, ListItemType itemType)
        {
            UsersItemsListsResponse response = new UsersItemsListsResponse();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            if (t != null)
            {
                response = t.GetItemsFromUsersLists(nGroupID, userIds, listType, itemType);
                if (response != null)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                else
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            else
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
            return response;
        }

        
        public static List<ApiObjects.KeyValuePair> IsItemExistsInList(int nGroupID, UserItemList userItemList)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            if (t != null)
            {
                List<ApiObjects.KeyValuePair> itemExists = t.IsItemExistsInList(userItemList, nGroupID);
                return itemExists;
            }
            return null;
        }


        
        public static UserType[] GetGroupUserTypes(int nGroupID)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetGroupUserTypes(nGroupID);
            }
            else
            {
                return null;
            }
        }


        
        public static ResponseStatus SetUserTypeByUserID(int nGroupID, string sSiteGuid, int nUserTypeID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetUserTypeByUserID(sSiteGuid, nUserTypeID);
            }
            else
            {
                return ResponseStatus.ErrorOnSendingMail;
            }
        }

        
        public static int GetUserType(int nGroupID, string sSiteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sSiteGuid != null ? sSiteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetUserType(sSiteGuid);
            }
            else
            {
                return 0;
            }
        }

        
        public static PinCodeResponse GenerateLoginPIN(int nGroupID, string siteGuid, string secret)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GenerateLoginPIN(siteGuid, nGroupID, secret);
            }
            else
            {
                return new PinCodeResponse();
            }
        }

        
        public static UserResponse LoginWithPIN(int nGroupID, string PIN, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins,
            List<ApiObjects.KeyValuePair> keyValueList, string secret)
        {
            BaseUsers baseUser = null;
            UserResponse response = new UserResponse();

            // get group ID + user implementation
            Utils.GetBaseImpl(ref baseUser, nGroupID);
            if (nGroupID != 0 && baseUser != null)
            {
                // validate pin
                response = baseUser.ValidateLoginWithPin(PIN, secret);

                if (response == null || response.resp == null)
                {
                    response = new UserResponse();
                    return response;
                }

                if (response.resp.Code == (int)ApiObjects.Response.eResponseStatus.OK)
                {
                    // get Kaltura user implementation
                    KalturaBaseUsers kUser = null;

                    // get operatorId if exists
                    int operatorId = -1;
                    if (keyValueList != null)
                    {
                        var keyValueOperatorId = keyValueList.FirstOrDefault(x => x.key == "operator");
                        if (keyValueOperatorId != null)
                            operatorId = Convert.ToInt32(keyValueOperatorId.value);
                    }

                    //  get user type
                    Utils.GetBaseImpl(ref kUser, nGroupID, operatorId);
                    if (kUser != null)
                    {
                        // execute Sign in
                        response.user = FlowManager.SignIn(int.Parse(response.user.m_user.m_sSiteGUID), kUser, nMaxFailCount, nLockMinutes, nGroupID, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList, response.user.m_user.m_oBasicData.m_sUserName, string.Empty);
                        if (response.user != null)
                        {
                            // convert response status
                            response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);

                            //check if pin should be expired
                            if (response.user.m_RespStatus == ResponseStatus.OK ||
                                response.user.m_RespStatus == ResponseStatus.UserWithNoDomain ||
                                response.user.m_RespStatus == ResponseStatus.UserSuspended)
                            {
                                // expire the PIN
                                baseUser.ExpirePIN(nGroupID, PIN);
                                response.resp.Code = (int)ResponseStatus.OK;
                                response.resp.Message = ResponseStatus.OK.ToString();
                            }
                        }
                    }
                }
            }
            return response;
        }

        
        public static PinCodeResponse SetLoginPIN(int nGroupID, string siteGuid, string PIN, string secret)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetLoginPIN(siteGuid, PIN, nGroupID, secret);
            }
            else
            {
                return new PinCodeResponse();
            }
        }

        
        public static ApiObjects.Response.Status ClearLoginPIN(int nGroupID, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ClearLoginPIN(siteGuid, pin, nGroupID);
            }
            else
            {
                return new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
            }
        }

        
        public static UserResponse LogIn(int nGroupID, string userName, string password, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins,
            List<ApiObjects.KeyValuePair> keyValueList)
        {
            UserResponse response = new UserResponse();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.user = KalturaSignIn(nGroupID, userName, password, sessionID, sIP, deviceID, bPreventDoubleLogins, keyValueList);
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
            return response;

        }


        
        public static UserResponse SignUp(int nGroupID, UserBasicData oBasicData, UserDynamicData dynamicData, string password, string affiliateCode)
        {
            UserResponse response = new UserResponse();

            // add username to logs/monitor
            if (oBasicData != null && !string.IsNullOrEmpty(oBasicData.m_sUserName))
            {
                HttpContext.Current.Items[Constants.USER_ID] = oBasicData.m_sUserName;
            }

            response.user = AddNewUser(nGroupID, oBasicData, dynamicData, password, affiliateCode);

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
            return response;
        }

        
        public static ApiObjects.Response.Status SendRenewalPasswordMail(int nGroupID, string userName)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                UserResponseObject user = t.ForgotPassword(userName);
                if (user != null)
                {
                    // convert response status
                    response = Utils.ConvertResponseStatusToResponseObject(user.m_RespStatus);
                }
            }
            return response;
        }

        
        public static ApiObjects.Response.Status RenewPassword(int nGroupID, string userName, string newPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                UserResponseObject user = t.RenewPassword(userName, newPassword, nGroupID);
                if (user != null)
                {
                    // convert response status
                    response = Utils.ConvertResponseStatusToResponseObject(user.m_RespStatus);
                }
            }
            return response;
        }

        
        public static ApiObjects.Response.Status ReplacePassword(int nGroupID, string userName, string oldPassword, string newPassword)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                UserResponseObject user = t.ChangeUserPassword(userName, oldPassword, newPassword, nGroupID);
                if (user != null)
                {
                    // convert response status
                    response = Utils.ConvertResponseStatusToResponseObject(user.m_RespStatus);
                }
            }

            return response;
        }

        
        public static UserResponse CheckPasswordToken(int nGroupID, string token)
        {
            UserResponse response = new UserResponse();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.user = t.CheckToken(token);
                if (response.user != null)
                {
                    // convert response status
                    response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);
                }
            }
            return response;
        }

        
        public static UsersResponse GetUsers(int nGroupID, string[] sSiteGUIDs, string userIP)
        {
            // add siteguid to logs/monitor
            if (sSiteGUIDs != null && sSiteGUIDs.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var siteGuid in sSiteGUIDs)
                    sb.Append(String.Format("{0} ", siteGuid));

                if (HttpContext.Current != null && HttpContext.Current.Items != null)
                {
                    HttpContext.Current.Items[Constants.USER_ID] = sb.ToString();
                }
            }

            UsersResponse response = new UsersResponse();
            response.users = GetUsersData(nGroupID, sSiteGUIDs, userIP);
            if (response.users != null || response.users.Count > 0)
            {
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.OK;
                response.resp.Message = ApiObjects.Response.eResponseStatus.OK.ToString();
            }
            else
            {
                response.resp.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                response.resp.Message = "Error";
            }

            return response;
        }

        
        public static UserResponse SetUser(int nGroupID, string siteGUID, UserBasicData basicData, UserDynamicData dynamicData)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGUID != null ? siteGUID : "null";


            UserResponse response = new UserResponse();
            response.user = SetUserData(nGroupID, siteGUID, basicData, dynamicData);
            if (response.user != null)
            {
                // convert response status
                response.resp = Utils.ConvertResponseStatusToResponseObject(response.user.m_RespStatus);
            }
            return response;
        }

        
        public static FavoriteResponse FilterFavoriteMediaIds(int nGroupID, string userId, List<int> mediaIds, string udid, string mediaType, FavoriteOrderBy orderBy)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            FavoriteResponse response = new FavoriteResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.FilterFavoriteMediaIds(nGroupID, userId, mediaIds, udid, mediaType, orderBy);
            }
            return response;
        }

        
        public static LongIdsResponse GetUserRoleIds(int nGroupID, string userId)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            LongIdsResponse response = new LongIdsResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserRoleIds(nGroupID, userId);
            }
            return response;
        }

        
        public static ApiObjects.Response.Status AddRoleToUser(int nGroupID, string userId, long roleId)
        {
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != null ? userId : "null";

            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.AddRoleToUser(nGroupID, userId, roleId);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status DeleteUser(int nGroupID, int userId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            // add userId to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userId != 0 ? userId : 0;
            if (!Utils.IsDeleteUserAllowedForGroup(nGroupID))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.NotAllowedToDelete, eResponseStatus.NotAllowedToDelete.ToString());
            }

            if (Utils.IsGroupIDContainedInConfig(nGroupID, "EXCLUDE_PS_DLL_IMPLEMENTATION", ';'))
            {
                // old Core to PS flow
                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                    response = t.DeleteUser(userId);
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser != null)
                    response = FlowManager.DeleteUser(userId, kUser);
            }

            if (response.Code == (int)eResponseStatus.OK)
            {
                Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.DeleteUser, userId, string.Empty);
            }
            else
                log.ErrorFormat("DeleteUser: error while deleting user: user: {0}, group: {1}, error: {2}", userId, nGroupID, response.Code);

            return response;
        }

        
        public static ApiObjects.Response.Status ChangeUsers(int nGroupID, string userId, string userIdToChange, string udid)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.ChangeUsers(userId, userIdToChange, udid, nGroupID);
            }

            return response;
        }


        public static void AddInitiateNotificationAction(int nGroupID, eUserMessageAction userAction, int userId, string udid, string pushToken = "")
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                Utils.AddInitiateNotificationActionToQueue(nGroupID, eUserMessageAction.AnonymousPushRegistration, userId, udid, pushToken);
            }
        }

        
        public static ApiObjects.Response.Status ResendActivationToken(int nGroupID, string username)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.ResendActivationToken(username);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status DeleteItemFromUsersList(int nGroupID, string userId, Item item)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.DeleteItemFromUsersList(item.ItemId, item.ListType, item.ItemType, userId, nGroupID);
            }

            return response;
        }

        
        public static UsersListItemResponse AddItemToUsersList(int nGroupID, string userId, Item item)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.AddItemToUsersList(item.ItemId, item.ListType, item.ItemType, item.OrderIndex.HasValue ? item.OrderIndex.Value : 0, userId, nGroupID);
            }

            return response;
        }

        
        public static UsersListItemResponse GetItemFromUsersList(int nGroupID, string userId, Item item)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetItemFromUsersList(item.ItemId, item.ListType, item.ItemType, userId, nGroupID);
            }

            return response;
        }

        
        public static UserResponse GetUserByExternalID(int nGroupID, string externalId, int operatorID)
        {
            UserResponse response = new UserResponse()
            {
                resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserByExternalID(externalId, operatorID);
            }
            return response;
        }
        
        public static UserResponse GetUserByName(int nGroupID, string username)
        {
            UserResponse response = new UserResponse()
            {
                resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserByName(username, nGroupID);
            }
            return response;
        }
    }
}
