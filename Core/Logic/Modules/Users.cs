using APILogic.Notification;
using ApiObjects;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Web;
using ApiObjects.SSOAdapter;
using APILogic.Users;
using DAL;
using APILogic.Api.Managers;
using KeyValuePair = ApiObjects.KeyValuePair;
using ApiLogic.Users;
using ApiLogic.Users.Managers;

namespace Core.Users
{
    public interface IUserModule
    {
        UserResponseObject GetUserData(int groupId, long userId, string userIp);
        LongIdsResponse GetUserRoleIds(int groupId, long userId);
    }

    public class Module : IUserModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static int nMaxFailCount = 3;
        private static int nLockMinutes = 3;
        private const string USER_CLASS_NAME = "User";

        private static readonly Lazy<IUserModule> _lazy = new Lazy<IUserModule>(() => new Module(), LazyThreadSafetyMode.PublicationOnly);
        public static IUserModule Instance => _lazy.Value;

        private static void AddItemToContext(string key, string value)
        {
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                HttpContext.Current.Items[key] = value ?? "null";
            }
        }

        public static UserResponseObject CheckUserPassword(int nGroupID, string sUserName, string sPassword, bool bPreventDoubleLogins)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sUserName);

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

        public static SSOAdapterProfileInvoke Invoke(int groupId, string intent, List<KeyValuePair> keyValuePairs)
        {
            try
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, groupId);
                if (kUser != null)
                    return FlowManager.Invoke(groupId, intent, keyValuePairs, kUser);
            }
            catch (Exception ex)
            {
                //ToDo - Matan - add exception
                log.Error("", ex);
                throw;
            }
            return null;
        }

        public static UserResponseObject SignIn(int nGroupID, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, KeyValuePair[] KeyValuePairs)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sUserName);

                if (Utils.IsGroupIDContainedInConfig(nGroupID))
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

        public static UserResponseObject KalturaSignIn(int nGroupID, string sUserName, string sPassword, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sUserName);

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
                if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                AddItemToContext(Constants.USER_ID, sUserName);

                if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                AddItemToContext(Constants.USER_ID, sUserName);

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
                AddItemToContext(Constants.USER_ID, sSiteGUID);

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

        public static UserResponseObject SignOut(int nGroupID, string sSiteGUID, string sessionID, string sIP, string deviceID
            , bool bPreventDoubleLogins, List<KeyValuePair> keyValueList = null)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sSiteGUID);

                int nSiteGuid;
                if (!Int32.TryParse(sSiteGUID, out nSiteGuid))
                {
                    log.Debug("SignOut - Illegal Siteguid");
                    return null;
                }

                if (keyValueList == null)
                {
                    keyValueList = new List<KeyValuePair>();
                }

                if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                        return FlowManager.SignOut(kUser, nSiteGuid, nGroupID, sessionID, sIP, deviceID, keyValueList);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        public static UserResponseObject KalturaSignOut(int nGroupID, string sSiteGUID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<KeyValuePair> keyValueList)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sSiteGUID);

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

        public static ApiObjects.Response.Status AddUserFavorit(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, string sItemCode, string sExtraData)
        {
            try
            {
                ApiObjects.Response.Status response = new ApiObjects.Response.Status();

                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sUserGUID);

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

        public static GenericResponse<FavoritObject> InsertUserFavorite(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, string sItemCode, string sExtraData)
        {
            GenericResponse<FavoritObject> response = new GenericResponse<FavoritObject>();

            try
            {
                // add siteguid to logs/monitor
                HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";

                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    return t.InsertUserFavorite(sUserGUID, domainID, sDeviceUDID, sItemType, sItemCode, sExtraData, nGroupID);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return response;
        }

        public static bool AddChannelMediaToFavorites(int nGroupID, string sUserGUID, int domainID, string sDeviceUDID, string sItemType, string sChannelID, string sExtraData)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, sUserGUID);

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
                AddItemToContext(Constants.USER_ID, sSiteGUID);

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
            AddItemToContext(Constants.USER_ID, sSiteGUID);

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
            AddItemToContext(Constants.USER_ID, sUserGUID);

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

        public static ApiObjects.Response.Status RemoveUserFavorit(int nGroupID, string sUserGUID, long[] nMediaIDs)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, sUserGUID);

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
            AddItemToContext(Constants.USER_ID, sUserGUID);


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
            AddItemToContext(Constants.USER_ID, sUserGUID);

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

        UserResponseObject IUserModule.GetUserData(int groupId, long userId, string userIp)
        {
            return GetUserData(groupId, userId.ToString(), userIp);
        }

        public static UserResponseObject GetUserData(int nGroupID, string sSiteGUID, string sUserIp)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, sSiteGUID);

            if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                Utils.GetBaseImpl(ref kUser, nGroupID, -1, USER_CLASS_NAME, false);
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
            if (sSiteGUIDs != null && sSiteGUIDs.Length > 0 && HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var siteGuid in sSiteGUIDs)
                    sb.Append(String.Format("{0} ", siteGuid));

                AddItemToContext(Constants.USER_ID, sb.ToString());
            }

            if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                Utils.GetBaseImpl(ref kUser, nGroupID, -1, USER_CLASS_NAME, false);
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
                userResponseList = userResponseList.Where(y => y.m_RespStatus != ResponseStatus.UserDoesNotExist).ToList();
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
            UserResponseObject response = null;
            if (Utils.IsGroupIDContainedInConfig(nGroupID))
            {
                // old Core to PS flow
                BaseUsers t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t == null)
                {
                    return response;
                }

                response = t.AddNewUser(oBasicData, sDynamicData, sPassword);
            }
            else
            {
                // new Core to PS flow
                KalturaBaseUsers kUser = null;

                // get group ID + user type
                Utils.GetBaseImpl(ref kUser, nGroupID);
                if (kUser == null)
                {
                    return response;
                }

                response = FlowManager.AddNewUser(kUser, oBasicData, sDynamicData, sPassword, new List<KeyValuePair>());
            }

            return response;
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

        public static void Hit(int nGroupID, string sSiteGUID)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, sSiteGUID);

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
            AddItemToContext(Constants.USER_ID, sSiteGUID);

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
                var response = t.ChangeUserPassword(sUN, sOldPass, sPass, nGroupID);
                return response.Object;
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
                return t.ForgotPassword(sUN, null);
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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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

        public static GenericResponse<UserResponseObject> RenewUserPassword(int nGroupID, string sUserName, string sNewPassword)
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

        public static bool ResendWelcomeMail(int nGroupID, string sUserName, string sNewPassword)
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

        public static ApiObjects.Response.Status ResendActivationMail(int nGroupID, string sUserName, string sNewPassword)
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

        public static GenericResponse<UserResponseObject> ActivateAccount(int nGroupID, string sUserName, string sToken)
        {
            var response = new GenericResponse<UserResponseObject>();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.Object = t.ActivateAccount(sUserName, sToken);
                if (response.Object != null)
                {
                    // convert response status
                    response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus));
                }
            }
            return response;
        }

        public static UserResponseObject ActivateAccountByDomainMaster(int nGroupID, string sMasterUsername, string sUserName, string sToken)
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
            AddItemToContext(Constants.USER_ID, sSiteGUID);

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
                return Users.Utils.GetIPCountry2(nGroupID, sUserIP);
            }
            else
            {
                return null;
            }
        }

        public static User[] GetUsersLikedMedia(int nGroupID, Int32 nUserGuid, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, nUserGuid.ToString());

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
            AddItemToContext(Constants.USER_ID, nUserID.ToString());
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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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

        public static ApiObjects.Response.Status DeleteUserDynamicData(int groupId, long userId, string key)
        {
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, groupId);
            var response = t != null
                ? t.DeleteUserDynamicData(userId, key)
                : ApiObjects.Response.Status.Error;

            return response;
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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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
            AddItemToContext(Constants.USER_ID, sSiteGuid);

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

        public static PinCodeResponse GenerateLoginPIN(int nGroupID, string siteGuid, string secret, int? pinUsages = null, long? pinDuration = null)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, siteGuid);

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GenerateLoginPIN(siteGuid, nGroupID, secret, pinUsages, pinDuration);
            }
            else
            {
                return new PinCodeResponse();
            }
        }

        public static GenericResponse<UserResponseObject> LoginWithPIN(int groupID, string PIN, string sessionID, string ip, string deviceID, bool preventDoubleLogins, List<KeyValuePair> keyValueList, string secret)
        {
            BaseUsers baseUser = null;
            var response = new GenericResponse<UserResponseObject>();

            // get group ID + user implementation
            Utils.GetBaseImpl(ref baseUser, groupID);
            if (groupID == 0 || baseUser == null)
            {
                return response;
            }

            // validate pin
            response = baseUser.ValidateLoginWithPin(PIN, secret);

            if (response.Object == null)
            {
                response.Object = new UserResponseObject();
            }

            if (!response.IsOkStatusCode())
            {
                return response;
            }

            if (response.Object.m_user != null && !RolesPermissionsManager.Instance.IsPermittedPermission(groupID, response.Object.m_user.m_sSiteGUID, RolePermissions.LOGIN))
            {
                response.SetStatus(RolesPermissionsManager.GetSuspentionStatus(groupID, response.Object.m_user.m_domianID));
                return response;
            }

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
            Utils.GetBaseImpl(ref kUser, groupID, operatorId);
            if (kUser != null)
            {
                if (keyValueList == null)
                {
                    keyValueList = new List<KeyValuePair>();
                }
                keyValueList.Add(new KeyValuePair("pin", PIN));

                // execute Sign in
                response.Object = FlowManager.SignIn(int.Parse(response.Object.m_user.m_sSiteGUID), kUser, nMaxFailCount, nLockMinutes, groupID, sessionID, ip, deviceID, preventDoubleLogins, keyValueList, response.Object.m_user.m_oBasicData.m_sUserName, string.Empty);
                if (response.Object != null)
                {
                    // convert response status
                    response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus));

                    //check if pin should be expired
                    if (response.Object.m_RespStatus == ResponseStatus.OK ||
                        response.Object.m_RespStatus == ResponseStatus.UserWithNoDomain ||
                        response.Object.m_RespStatus == ResponseStatus.UserSuspended)
                    {
                        // expire the PIN
                        baseUser.ExpirePIN(groupID, PIN);
                        response.SetStatus(eResponseStatus.OK);
                    }
                }
            }

            return response;
        }

        public static PinCodeResponse SetLoginPIN(int nGroupID, string siteGuid, string PIN, string secret, int? pinUsages = null, long? pinDuration = null)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, siteGuid);

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetLoginPIN(siteGuid, PIN, nGroupID, secret, pinUsages, pinDuration);
            }
            else
            {
                return new PinCodeResponse();
            }
        }

        public static ApiObjects.Response.Status ClearLoginPIN(int nGroupID, string siteGuid, string pin)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, siteGuid);

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

        public static GenericResponse<UserResponseObject> LogIn(int groupID, string userName, string password, string sessionID, string ip, string deviceID, bool preventDoubleLogins, List<KeyValuePair> keyValueList)
        {
            var response = new GenericResponse<UserResponseObject>
            {
                Object = KalturaSignIn(groupID, userName, password, sessionID, ip, deviceID, preventDoubleLogins, keyValueList)
            };

            if (response.Object != null)
            {
                if (response.Object.m_user != null && !RolesPermissionsManager.Instance.IsPermittedPermission(groupID, response.Object.m_user.m_sSiteGUID, RolePermissions.LOGIN))
                {
                    response.SetStatus(RolesPermissionsManager.GetSuspentionStatus(groupID, response.Object.m_user.m_domianID));
                    return response;
                }

                // convert response status
                response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus, null, true, response.Object.ExternalCode, response.Object.ExternalMessage));

                if (response.IsOkStatusCode() && int.TryParse(response.Object.m_user.m_sSiteGUID, out int userID) && userID > 0)
                {
                    Utils.AddInitiateNotificationActionToQueue(groupID, eUserMessageAction.Login, userID, deviceID);
                }
                else
                {
                    log.ErrorFormat("LogIn: error for user: {0}, group: {1}, error: {2}", userName, groupID, response.Status.Code);
                }
            }



            return response;
        }

        public static GenericResponse<UserResponseObject> SignUp(int nGroupID, UserBasicData oBasicData, UserDynamicData dynamicData, string password, string affiliateCode)
        {
            var response = new GenericResponse<UserResponseObject>();

            // add username to logs/monitor
            if (oBasicData != null && !string.IsNullOrEmpty(oBasicData.m_sUserName))
            {
                AddItemToContext(Constants.USER_ID, oBasicData.m_sUserName);
            }

            if (oBasicData.RoleIds != null && oBasicData.RoleIds.Count > 0)
            {
                var roles = Api.Module.GetRoles(nGroupID, oBasicData.RoleIds);
                if (roles == null || roles.Status == null || roles.Status.Code == (int)eResponseStatus.Error || roles.Roles == null || roles.Roles.Count != oBasicData.RoleIds.Count)
                {
                    response.SetStatus(eResponseStatus.RoleDoesNotExists);
                    return response;
                }
            }

            response.Object = AddNewUser(nGroupID, oBasicData, dynamicData, password, affiliateCode);
            if (response.Object != null)
            {
                if (response.Object.m_RespStatus == ResponseStatus.OK || response.Object.m_RespStatus == ResponseStatus.UserWithNoDomain)
                {
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    // convert response status
                    response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus, null, false, response.Object.ExternalCode, response.Object.ExternalMessage));
                }
            }

            return response;
        }

        public static ApiObjects.Response.Status SendRenewalPasswordMail(int nGroupID, string userName, string templateName)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                UserResponseObject user = t.ForgotPassword(userName, templateName);
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
            var response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                var renewPasswordResponse = t.RenewPassword(userName, newPassword, nGroupID);
                if (renewPasswordResponse != null && renewPasswordResponse.Object != null)
                {
                    // convert response status
                    response = Utils.ConvertResponseStatusToResponseObject(renewPasswordResponse.Object.m_RespStatus, renewPasswordResponse.Status);
                }
            }
            return response;
        }

        public static GenericResponse<UserResponseObject> RenewPasswordWithToken(int groupId, string token, string newPassword)
        {
            var response = new GenericResponse<UserResponseObject>();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.RenewPasswordWithToken(token, newPassword);
            }
            return response;
        }

        public static ApiObjects.Response.Status ReplacePassword(int nGroupID, string userName, string oldPassword, string newPassword)
        {
            var response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                var changeUserPasswordResponse = t.ChangeUserPassword(userName, oldPassword, newPassword, nGroupID);
                if (changeUserPasswordResponse != null)
                {
                    // convert response status
                    response = Utils.ConvertResponseStatusToResponseObject(changeUserPasswordResponse.Object.m_RespStatus, changeUserPasswordResponse.Status);
                }
            }

            return response;
        }

        public static GenericResponse<UserResponseObject> CheckPasswordToken(int nGroupID, string token)
        {
            var response = new GenericResponse<UserResponseObject>();
            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.Object = t.CheckToken(token);
                if (response.Object != null)
                {
                    // convert response status
                    response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus));
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

                AddItemToContext(Constants.USER_ID, sb.ToString());
            }

            UsersResponse response = new UsersResponse();
            response.users = GetUsersData(nGroupID, sSiteGUIDs, userIP);
            if (response.users != null || response.users.Count > 0)
            {
                response.resp.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            else
            {
                response.resp.Set((int)eResponseStatus.Error, "Error");
            }

            return response;
        }

        public static GenericResponse<UserResponseObject> UpdateUser(int nGroupID, string siteGUID, UserBasicData basicData, UserDynamicData dynamicData)
        {
            var response = new GenericResponse<UserResponseObject>();

            if (basicData.RoleIds != null && basicData.RoleIds.Count > 0)
            {
                var roles = Api.Module.GetRoles(nGroupID, basicData.RoleIds);
                if (roles == null || roles.Status == null || roles.Status.Code == (int)eResponseStatus.Error || roles.Roles == null || roles.Roles.Count != basicData.RoleIds.Count)
                {
                    response.SetStatus(eResponseStatus.RoleDoesNotExists);
                    return response;
                }
            }

            response.Object = UpdateUserData(nGroupID, siteGUID, basicData, dynamicData);
            if (response.Object != null)
            {
                // convert response status
                response.SetStatus(Utils.ConvertResponseStatusToResponseObject(response.Object.m_RespStatus));
            }

            return response;
        }

        public static UserResponseObject UpdateUserData(int nGroupID, string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            // add siteguid to logs/monitor
            AddItemToContext(Constants.USER_ID, sSiteGUID);

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.UpdateUserData(sSiteGUID, oBasicData, sDynamicData);
            }

            return null;
        }

        public static FavoriteResponse FilterFavoriteMediaIds(int nGroupID, string userId, List<int> mediaIds, string udid, string mediaType, FavoriteOrderBy orderBy)
        {
            // add userId to logs/monitor
            AddItemToContext(Constants.USER_ID, userId);

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

        LongIdsResponse IUserModule.GetUserRoleIds(int groupId, long userId)
        {
            return GetUserRoleIds(groupId, userId.ToString());
        }

        public static LongIdsResponse GetUserRoleIds(int nGroupID, string userId)
        {
            // add userId to logs/monitor
            AddItemToContext(Constants.USER_ID, userId);

            LongIdsResponse response = new LongIdsResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserRoleIds(userId);
            }
            return response;
        }

        public static ApiObjects.Response.Status AddRoleToUser(int nGroupID, string userId, long roleId)
        {
            // add userId to logs/monitor
            AddItemToContext(Constants.USER_ID, userId);

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
            AddItemToContext(Constants.USER_ID, userId.ToString());

            if (!Utils.IsDeleteUserAllowedForGroup(nGroupID))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.NotAllowedToDelete, eResponseStatus.NotAllowedToDelete.ToString());
            }

            if (Utils.IsGroupIDContainedInConfig(nGroupID))
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
                OttUserCrudMessagePublisher.Instance.Delete(userId);
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

        public static GenericResponse<UserResponseObject> GetUserByExternalID(int nGroupID, string externalId, int operatorID)
        {
            var response = new GenericResponse<UserResponseObject>();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserByExternalID(externalId, operatorID);
            }
            return response;
        }

        public static GenericListResponse<UserResponseObject> GetUsersByEmail(int nGroupID, string email, int operatorID)
        {
            var response = new GenericListResponse<UserResponseObject>();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUsersByEmail(email, operatorID);
            }
            return response;
        }

        public static GenericResponse<UserResponseObject> GetUserByName(int nGroupID, string username)
        {
            var response = new GenericResponse<UserResponseObject>();

            BaseUsers t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.GetUserByName(username, nGroupID);
            }

            return response;
        }

        public static UserInterestResponseList GetUserInterests(int groupId, int userId)
        {
            UserInterestResponseList response = new UserInterestResponseList() { Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()) };

            try
            {
                return TopicInterestManager.GetUserInterests(groupId, userId);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error getting user interest. groupId: {0} User: {1}, exception {2} ", groupId, userId, ex);
            }

            return response;
        }

        public static UserResponseObject SignInWithUserId(int groupID, int userId, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins, List<ApiObjects.KeyValuePair> KeyValuePairs)
        {
            try
            {
                // add siteguid to logs/monitor
                AddItemToContext(Constants.USER_ID, userId.ToString());

                if (Utils.IsGroupIDContainedInConfig(groupID))
                {
                    // old Core to PS flow
                    BaseUsers t = null;
                    Utils.GetBaseImpl(ref t, groupID);
                    if (t != null)
                        return t.SignIn(userId, 3, 3, groupID, sessionID, sIP, deviceID, bPreventDoubleLogins);
                }
                else
                {
                    // new Core to PS flow
                    KalturaBaseUsers kUser = null;

                    // get group ID + user type
                    Utils.GetBaseImpl(ref kUser, groupID);
                    if (kUser != null)
                        return FlowManager.SignIn(userId, kUser, nMaxFailCount, nLockMinutes, groupID, sessionID, sIP, deviceID, bPreventDoubleLogins, KeyValuePairs);
                }
            }
            catch (Exception ex)
            {
                log.Error("", ex);
            }
            return null;
        }

        public static bool Purge()
        {
            bool result = Utils.Purge();
            return result;
        }

        public static SSOAdaptersResponse GetSSOAdapters(int groupId)
        {
            return SSOAdaptersManager.GetSSOAdapters(groupId);
        }

        public static SSOAdapterResponse InsertSSOAdapter(SSOAdapter adapterDetails, int updaterId)
        {
            return SSOAdaptersManager.InsertSSOAdapter(adapterDetails, updaterId);
        }

        public static SSOAdapterResponse UpdateSSOAdapter(SSOAdapter adapterDetails, int updaterId)
        {
            return SSOAdaptersManager.UpdateSSOAdapter(adapterDetails, updaterId);
        }

        public static ApiObjects.Response.Status DeleteSSOAdapter(int groupId, int ssoAdapterId, int updaterId)
        {
            return SSOAdaptersManager.DeleteSSOAdapter(groupId, ssoAdapterId, updaterId);
        }

        public static SSOAdapterResponse SetSSOAdapterSharedSecret(int groupId, int ssoAdapterId, string sharedSecret, int updaterId)
        {
            return SSOAdaptersManager.SetSSOAdapterSharedSecret(groupId, ssoAdapterId, sharedSecret, updaterId);
        }

        public static List<string> GetUserIdsByRoleIds(int groupId, HashSet<long> roleIds)
        {
            List<string> stringUserIds = null;

            try
            {
                List<long> longUserIds = UsersDal.GetUserIdsByRoleIds(groupId, roleIds);

                if (longUserIds != null && longUserIds.Count > 0)
                {
                    stringUserIds = new List<string>(longUserIds.Select(x => x.ToString()));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while Users.GetUserIdsByRoleIds, groupId: {0}, roleIds:{1}, ex:{2} ", groupId, string.Join(", ", roleIds), ex);
            }

            return stringUserIds;
        }

        public static ResponseStatus GetUserActivationState(int groupId, int userId)
        {
            ResponseStatus responseStatus = ResponseStatus.InternalError;
            try
            {
                var kalturaUser = new KalturaUsers(groupId);
                if (kalturaUser == null)
                {
                    return responseStatus;
                }
                string notUsed = null;
                bool notUsedBool = false;

                var userActivationStatus = kalturaUser.GetUserActivationState(ref notUsed, ref userId, ref notUsedBool);
                responseStatus = Utils.MapToResponseStatus(userActivationStatus);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserActivationState, groupId:{0}, userId:{1} ex:{2}", groupId, userId, ex);
            }

            return responseStatus;
        }
    }
}
