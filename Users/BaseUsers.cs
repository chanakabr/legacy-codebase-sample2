using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Statistics;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Users
{
    public abstract class BaseUsers
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ROLE_ALREADY_ASSIGNED_TO_USER_ERROR = "Role already assigned to user";

        public const int PIN_NUMBER_OF_DIGITS = 10;
        public const int PIN_MIN_NUMBER_OF_DIGITS = 8;
        public const int PIN_MAX_NUMBER_OF_DIGITS = 10;

        protected BaseUsers() { }
        protected BaseUsers(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
            //m_bIsInitialized = false;
            //IsActivationNeeded(null);
            Initialize();
        }

        ////Domain
        //public abstract Domain AddDomain(string domainName, string domainDescription, Int32 masterUserGuid, Int32 nGroupID);
        //public abstract Domain SetDomainInfo(Int32 domainID, string domainName, Int32 nGroupID, string domainDescription);
        public abstract Domain AddUserToDomain(Int32 nGroupID, Int32 domainID, Int32 userGuid, bool isMaster);
        //public abstract Domain RemoveUserFromDomain(Int32 nGroupID, Int32 domainID, Int32 userGUID);
        //public abstract Domain GetDomainInfo(Int32 domainID, Int32 nGroupID);


        public abstract UserResponseObject CheckUserPassword(string sUN, string sPass, Int32 nMaxFailCount, Int32 nLockMinutes, Int32 nGroupID, bool bPreventDoubleLogins);
        public abstract UserResponseObject SignIn(string sUN, string sPass, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        public abstract UserResponseObject SignIn(int siteGuid, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        public abstract UserResponseObject SignInWithToken(string sToken, int nMaxFailCount, int nLockMinutes, int nGroupID, string sessionID, string sIP, string deviceID, bool bPreventDoubleLogins);
        public abstract UserState GetUserState(int siteGuid);
        public abstract UserState GetUserInstanceState(int siteGuid, string sessionID, string sIP, string deviceID);
        public abstract UserResponseObject SignOut(int siteGuid, string sessionID, string sIP, string deviceID);
        public abstract UserResponseObject GetUserByFacebookID(string sFacebookID, Int32 nGroupID);
        public abstract UserResponseObject GetUserByCoGuid(string sCoGuid, int operatorID);
        public abstract UserResponseObject GetUserByUsername(string sUsername, Int32 nGroupID);
        public abstract UserResponseObject GetUserData(string sSiteGUID, bool shouldSaveInCache = true);
        public abstract List<UserBasicData> GetUsersBasicData(long[] nSiteGUIDs);
        public abstract List<UserResponseObject> GetUsersData(string[] sSiteGUIDs);
        public abstract List<UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact);
        public abstract UserResponseObject AddNewUser(UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword);
        public abstract UserResponseObject AddNewUser(string sBasicDataXML, string sDynamicDataXML, string sPassword);
        public abstract bool DoesUserNameExists(string sUserName);
        public abstract UserGroupRuleResponse CheckParentalPINToken(string sChangePinToken);
        public abstract UserGroupRuleResponse ChangeParentalPInCodeByToken(string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode);
        public abstract DomainResponseObject AddNewDomain(string sUN, int nUserID, int nGroupID);
        public abstract ApiObjects.Response.Status DeleteUser(int userId);
        public abstract ApiObjects.Response.Status ChangeUsers(string userId, string userIdToChange, string udid, int groupId);

        public virtual bool WriteToLog(string sSiteGUID, string sMessage, string sWriter)
        {
            return false;
        }

        public virtual ApiObjects.Response.Status AddUserFavorit(string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sItemCode, string sExtraData, Int32 nGroupID)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            try
            {
                bool saveRes = false;
                FavoritObject f = new FavoritObject();

                if (!IsAddUserFavoriteParamValid(nGroupID, sUserGUID, sItemType, sItemCode, out status))
                {
                    return status;
                }

                f.Initialize(0, sUserGUID, domainID, sDeviceUDID, sItemType, sItemCode, sExtraData, DateTime.UtcNow, nGroupID);

                saveRes = f.Save(nGroupID);

                //insert favorites record to ES
                //add channel favorites to ES
                MediaView view = new MediaView()
                {
                    GroupID = nGroupID,
                    MediaID = ODBCWrapper.Utils.GetIntSafeVal(sItemCode),
                    Location = 0,
                    MediaType = sItemType,
                    Action = "favoraite",
                    Date = DateTime.UtcNow
                };

                if (saveRes)
                {
                    WriteFavoriteToES(view); //saving to the ES onlt if save Succeeded  
                }

                return status;
            }
            catch (Exception ex)
            {
                log.Error("", ex);
                status.Code = (int)eResponseStatus.Error;
                status.Message = "";
                return status;
            }
        }

        private bool IsAddUserFavoriteParamValid(int nGroupID, string sUserGUID, string sItemType, string sItemCode, out ApiObjects.Response.Status status)
        {
            //check if userID exist
            if (!IsUserValid(nGroupID, sUserGUID, out status))
            {
                return false;
            }

            if (sItemType.Trim().Length == 0)
            {
                status.Code = (int)eResponseStatus.Error;
                status.Message = "Item Type is empty ";
                return false;
            }

            if (sItemCode.Trim().Length == 0)
            {
                status.Code = (int)eResponseStatus.Error;
                status.Message = "Item Code is empty ";
                return false;
            }

            return true;
        }

        private bool IsARemoveUserFavoriteParamValid(int nGroupID, string sUserGUID, int[] mediaIDs, out ApiObjects.Response.Status status)
        {
            //check if userID exist
            if (!IsUserValid(nGroupID, sUserGUID, out status))
            {
                return false;
            }

            if (mediaIDs == null)
            {
                status.Code = (int)eResponseStatus.Error;
                status.Message = "Error";
                return false;
            }

            return true;
        }


        private bool WriteFavoriteToES(ApiObjects.Statistics.MediaView oMediaView)
        {
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(oMediaView.GroupID);
            try
            {
                bool bRes = false;
                ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi();

                string sJsonView = Newtonsoft.Json.JsonConvert.SerializeObject(oMediaView);

                if (oESApi.IndexExists(index) && !string.IsNullOrEmpty(sJsonView))
                {
                    Guid guid = Guid.NewGuid();

                    bRes = oESApi.InsertRecord(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), sJsonView);

                    if (!bRes)
                        log.Debug("WriteFavoriteToES - " + string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}", index, ElasticSearch.Common.Utils.ES_STATS_TYPE, sJsonView));
                }

                return bRes;
            }
            catch (Exception ex)
            {
                log.Error("WriteFavoriteToES - " + string.Format("Failed ex={0}, index={1};type={2}", ex.Message, index, ElasticSearch.Common.Utils.ES_STATS_TYPE), ex);
                return false;
            }
        }

        public virtual bool AddChannelMediaToFavorites(string sUserGUID, int domainID, string sDeviceUDID,
          string sItemType, string sChannelID, string sExtraData, Int32 nGroupID)
        {
            FavoritObject f = new FavoritObject();
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            if (!IsAddUserFavoriteParamValid(nGroupID, sUserGUID, sItemType, sChannelID, out status))
            {
                return false;
            }

            f.Initialize(0, sUserGUID, domainID, sDeviceUDID, sItemType, sChannelID, sExtraData, DateTime.UtcNow, nGroupID, 1);
            return f.Save(nGroupID);
        }

        public virtual void RemoveUserFavorit(Int32 nFavoritID, string sUserGUID, Int32 nGroupID)
        {
            FavoritObject.RemoveFavorit(sUserGUID, nGroupID, nFavoritID);
        }

        public virtual ApiObjects.Response.Status RemoveUserFavorit(int[] nMediaIDs, string sUserGUID, int nGroupID)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();

            if (IsARemoveUserFavoriteParamValid(nGroupID, sUserGUID, nMediaIDs, out status))
            {
                FavoritObject.RemoveFavorit(sUserGUID, nGroupID, nMediaIDs);
            }
            else
            {
                status.Code = (int)eResponseStatus.Error;
                status.Message = "Error";
            }

            return status;
        }

        public virtual void RemoveChannelMediaUserFavorit(int[] nChannelIDs, string sUserGUID, int nGroupID)
        {
            FavoritObject.RemoveChannelMediaFavorit(sUserGUID, nGroupID, nChannelIDs);
        }

        public virtual FavoriteResponse GetUserFavorites(string sSiteGUID, string sDeviceUDID, string sItemType, int nGroupID, int domainID, FavoriteOrderBy orderBy)
        {
            FavoriteResponse response = new FavoriteResponse();
            ApiObjects.Response.Status status = new ApiObjects.Response.Status();
            //check if userID exist
            if (IsUserValid(nGroupID, sSiteGUID, out status))
            {
                return FavoritObject.GetFavorites(nGroupID, sSiteGUID, domainID, sDeviceUDID, sItemType, orderBy);
            }
            else
            {
                status.Code = (int)eResponseStatus.Error;
                status.Message = "Error";
                return new FavoriteResponse() { Status = status, Favorites = new FavoritObject[0] };
            }
        }

        public virtual string GetUniqueTitle(UserBasicData oBasicData, UserDynamicData sDynamicData)
        {
            //Take nickname when needed
            if (m_nGroupID == 109 || m_nGroupID == 112)
            {
                string retVal = string.Empty;
                if (sDynamicData != null && sDynamicData.m_sUserData != null)
                {
                    foreach (UserDynamicDataContainer dynamicData in sDynamicData.m_sUserData)
                    {
                        if (dynamicData != null && dynamicData.m_sDataType.Equals("NickName"))
                        {
                            retVal = dynamicData.m_sValue;
                        }
                    }
                }
                return retVal;
            }
            else
            {
                return oBasicData.m_sFirstName;
            }
        }

        public virtual User[] GetUsersLikedMedia(Int32 nUserGUID, Int32 nMediaID, Int32 nPlatform, bool bOnlyFriends, Int32 nStartIndex, Int32 nNumberOfItems)
        {
            User[] ret = null;

            Int32 nTopNum = nStartIndex + nNumberOfItems;

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select distinct top " + nTopNum.ToString() + " user_site_guid from users_social_actions where is_active=1 and status=1";
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_id", "=", nMediaID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("user_site_guid", "<>", nUserGUID);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_action", "=", 1);
            selectQuery += " and ";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("social_platform", "=", nPlatform);
            selectQuery.SetConnectionKey("main_connection");
            if (selectQuery.Execute("query", true) != null)
            {
                Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                if (nTopNum > nCount)
                    nTopNum = nCount;
                if (nCount > 0)
                {
                    ret = new User[nTopNum];

                }
                for (int i = nStartIndex; i < nTopNum; i++)
                {
                    string sSiteGuid = selectQuery.Table("query").DefaultView[i].Row["user_site_guid"].ToString();

                    ret[i] = new User();
                    ret[i] = GetUserData(sSiteGuid).m_user;
                }
            }
            selectQuery.Finish();
            selectQuery = null;

            return ret;
        }

        public abstract UserResponseObject CheckToken(string sToken);
        public abstract bool ResendWelcomeMail(string sUN);
        public abstract bool ResendActivationMail(string sUN);
        public abstract UserResponseObject RenewPassword(string sUN, string sPass, int nGroupID);
        public abstract UserResponseObject ActivateAccount(string sUN, string sToken);
        public abstract UserResponseObject ActivateAccountByDomainMaster(string sMasterUN, string sUN, string sToken);
        public abstract bool IsUserActivated(ref string sUserName, ref Int32 nUserID);
        public abstract UserActivationState GetUserActivationStatus(ref string sUserName, ref Int32 nUserID, ref bool isGracePeriod);
        public abstract bool SendPasswordMail(string sUN);

        public virtual bool IsUserActivated(Int32 nUserID)
        {
            Int32 nAS = DAL.UsersDal.IsUserActivated(m_nGroupID, nUserID);

            return (nAS == 1);
        }

        public abstract void Initialize();
        public abstract UserResponseObject SetUserData(string sSiteGUID, UserBasicData oBasicData, UserDynamicData sDynamicData);
        public abstract UserResponseObject SetUserData(string sSiteGUID, string sBasicDataXML, string sDynamicDataXML);
        public abstract UserResponseObject ChangeUserPassword(string sUN, string sOldPass, string sPass, Int32 nGroupID);
        public abstract UserResponseObject ForgotPassword(string sUN);
        public abstract UserResponseObject ChangePassword(string sUN);
        public abstract ResponseStatus SendChangedPinMail(string sSiteGuid, int nRuleID);
        public abstract string GetUserToken(string sSiteGUID, Int32 nGroupID);
        public abstract void Hit(string sSiteGUID);
        public abstract void Logout(string sSiteGUID);

        protected Int32 m_nGroupID;
        protected Int32 m_nActivationMustHours;
        protected Int32 m_nTokenValidityHours;
        protected Int32 m_nChangePinTokenValidityHours;

        protected string m_sWelcomeMailTemplate;
        protected string m_sWelcomeFacebookMailTemplate;
        protected string m_sForgotPasswordMail;
        protected string m_sChangePasswordMail;
        protected string m_sChangedPinMail;
        protected string m_sActivationMail;
        protected string m_sMailFromName;
        protected string m_sMailFromAdd;
        protected string m_sMailServer;
        protected string m_sMailServerUN;
        protected string m_sMailServerPass;
        protected string m_sWelcomeMailSubject;
        protected string m_sWelcomeFacebookMailSubject;
        protected string m_sForgotPassMailSubject;
        protected string m_sChangePassMailSubject;
        protected string m_sChangedPinMailSubject;
        protected string m_sSendPasswordMailTemplate;
        protected string m_sSendPasswordMailSubject;

        protected int m_sMailSSL = 0;
        protected int m_sMailPort = 0;

        //static protected bool m_bIsInitialized;
        protected bool? m_bIsActivationNeeded; // Visibility reduced && type changed to nullable bool
        //due to MCORP-1685. Use IsActivationNeeded declared in this abstract class in order to access it

        protected BaseNewsLetterImpl m_newsLetterImpl;

        protected BaseMailImpl m_mailImpl;

        #region Offline user Media Asset
        /// <summary>
        /// Get usesr all offline items
        /// </summary>
        /// <returns></returns>
        public abstract UserOfflineObject[] GetUserOfflineMedia(Int32 nGroupID, string sSiteGuid);
        /// <summary>
        /// Get usesr offline items
        /// </summary>
        /// <returns></returns>
        //public abstract UserOfflineObject[] GetUserOfflineItemsByFileType(Int32 nGroupID, string sSiteGuid, string sFileType);
        /// <summary>
        /// Add user offline items
        /// </summary>
        /// <returns></returns>
        public abstract bool AddUserOfflineItems(Int32 nGroupID, string sSiteGuid, string sMediaID);
        /// <summary>
        /// Remove user offline items
        /// </summary>
        /// <returns></returns>
        public abstract bool RemoveUserOfflineItems(Int32 nGroupID, string sSiteGuid, string sMediaID);
        /// <summary>
        /// Clear user off line items
        /// </summary>
        /// <returns></returns>
        public abstract bool ClearUserOfflineItems(Int32 nGroupID, string sSiteGuid);
        #endregion

        public virtual bool SetUserDynamicData(string sSiteGUID, List<KeyValuePair> lKeyValue, UserResponseObject uro)
        {

            if (uro == null)
            {
                uro = GetUserData(sSiteGUID, false);
            }

            if (uro.m_RespStatus != ResponseStatus.OK || uro.m_user == null || uro.m_user.m_oDynamicData == null
                || uro.m_user.m_eSuspendState == DomainSuspentionStatus.Suspended)
                return false;

            if (uro.m_user.m_oDynamicData.m_sUserData == null)
            {
                uro.m_user.m_oDynamicData.m_sUserData = new UserDynamicDataContainer[0];
            }

            bool hasChanged = false; //indicates if there is a need to update the dynamic data           
            List<UserDynamicDataContainer> newPairs = new List<UserDynamicDataContainer>();

            foreach (KeyValuePair pair in lKeyValue)
            {
                bool exists = false;//indicates if the pair exists inside the current dynamic data or not
                for (int i = 0; i < uro.m_user.m_oDynamicData.m_sUserData.Length && !exists; i++)
                {
                    if (uro.m_user.m_oDynamicData.m_sUserData[i].m_sDataType == pair.key)
                    {
                        exists = true;
                        if (uro.m_user.m_oDynamicData.m_sUserData[i].m_sValue != pair.value) //change the value only if it has changed
                        {
                            uro.m_user.m_oDynamicData.m_sUserData[i].m_sValue = pair.value;
                            hasChanged = true;
                        }
                    }
                }
                if (!exists)
                {
                    UserDynamicDataContainer ud = new UserDynamicDataContainer();
                    ud.m_sDataType = pair.key;
                    ud.m_sValue = pair.value;
                    newPairs.Add(ud);
                }
            }

            if (hasChanged && newPairs.Count == 0)
            {
                uro.m_user.UpdateDynamicData(uro.m_user.m_oDynamicData, m_nGroupID);
            }

            //else 
            if (newPairs.Count > 0)
            {
                UserDynamicData newUdd = new UserDynamicData();
                newUdd.m_sUserData = new UserDynamicDataContainer[uro.m_user.m_oDynamicData.m_sUserData.Length + newPairs.Count];

                int preLength = uro.m_user.m_oDynamicData.m_sUserData.Length;
                for (int i = 0; i < preLength; i++)//copy all elments that are not new
                {
                    newUdd.m_sUserData[i] = uro.m_user.m_oDynamicData.m_sUserData[i];
                }
                for (int j = 0; j < newPairs.Count; j++)//add the new pairs
                {
                    newUdd.m_sUserData[j + preLength] = newPairs[j];
                }

                uro.m_user.UpdateDynamicData(newUdd, m_nGroupID);
            }

            #region Old Code
            //for (int i = 0; i < uro.m_user.m_oDynamicData.m_sUserData.Length; i++)
            //   {
            //       if (uro.m_user.m_oDynamicData.m_sUserData[i].m_sDataType == sType)
            //       {
            //           index = i;
            //           break;
            //       }
            //   }

            //   if (index != -1)
            //   {
            //       uro.m_user.m_oDynamicData.m_sUserData[index].m_sValue = sValue;
            //       uro.m_user.Update(uro.m_user.m_oBasicData, uro.m_user.m_oDynamicData, m_nGroupID);
            //   }
            //   else
            //   {
            //       UserDynamicData newUdd = new UserDynamicData();
            //       newUdd.m_sUserData = new UserDynamicDataContainer[uro.m_user.m_oDynamicData.m_sUserData.Length + 1];
            //       for (int i = 0; i < uro.m_user.m_oDynamicData.m_sUserData.Length; i++)
            //       {
            //           newUdd.m_sUserData[i] = uro.m_user.m_oDynamicData.m_sUserData[i];
            //       }

            //       UserDynamicDataContainer ud = new UserDynamicDataContainer();
            //       ud.m_sDataType = sType;
            //       ud.m_sValue = sValue;
            //       newUdd.m_sUserData[uro.m_user.m_oDynamicData.m_sUserData.Length] = ud;

            //       uro.m_user.Update(uro.m_user.m_oBasicData, newUdd, m_nGroupID); 
            #endregion

            return true;
        }


        private List<UserType> GetUserTypesList(DataTable dtUserTypes)
        {
            List<UserType> userTypesList = new List<UserType>();

            if (dtUserTypes != null && dtUserTypes.Rows.Count > 0)
            {
                foreach (DataRow drUserType in dtUserTypes.Rows)
                {
                    int? nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(drUserType["id"]);
                    string sUserTypeDesc = ODBCWrapper.Utils.GetSafeStr(drUserType["description"]);
                    bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetByteSafeVal(drUserType, "is_default"));
                    UserType userType = new UserType(nUserTypeID, sUserTypeDesc, isDefault);
                    userTypesList.Add(userType);
                }
            }
            return userTypesList;
        }


        public virtual UserType[] GetGroupUserTypes(Int32 nGroupID)
        {
            List<UserType> userTypesList;
            string key = string.Format("users_GetGroupUserTypes_{0}", m_nGroupID);

            bool bRes = UsersCache.GetItem<List<UserType>>(key, out  userTypesList);
            if (!bRes)
            {
                DataTable dtUserData = UsersDal.GetUserTypeData(nGroupID, null);
                userTypesList = GetUserTypesList(dtUserData);
                UsersCache.AddItem(key, userTypesList);
            }
            return userTypesList.ToArray();
        }


        // if no UserBasicData object is present send null instead
        public virtual bool IsActivationNeeded(UserBasicData oBasicData)
        {
            if (!m_bIsActivationNeeded.HasValue)
            {
                m_bIsActivationNeeded = UsersDal.GetIsActivationNeeded(m_nGroupID);
            }

            return (m_bIsActivationNeeded.HasValue ? m_bIsActivationNeeded.Value : true);
        }

        public virtual bool AddItemToList(UserItemList userItemList, int nGroupID)
        {
            try
            {
                int nSiteGuid = 0;
                if (userItemList == null || string.IsNullOrEmpty(userItemList.siteGuid) || userItemList.itemObj == null || userItemList.itemObj.Count() == 0)
                    return false;

                try
                {
                    nSiteGuid = int.Parse(userItemList.siteGuid);
                }
                catch
                {
                }

                List<KeyValuePair<int, int>> dItems = new List<KeyValuePair<int, int>>();
                int nOrderNum = 0;

                foreach (ItemObj itemObj in userItemList.itemObj)
                {
                    nOrderNum = itemObj.orderNum.HasValue ? itemObj.orderNum.Value : 0;
                    dItems.Add(new KeyValuePair<int, int>(itemObj.item, nOrderNum));
                }

                bool result = UsersDal.Insert_ItemList(nSiteGuid, dItems, (int)userItemList.listType, (int)userItemList.itemType, nGroupID);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("AddItemToList - exception =  " + ex.Message, ex);
                return false;
            }
        }

        public virtual bool RemoveItemFromList(UserItemList userItemList, int nGroupID)
        {
            try
            {
                int nSiteGuid = 0;
                if (userItemList == null || string.IsNullOrEmpty(userItemList.siteGuid) || userItemList.itemObj == null || userItemList.itemObj.Count() == 0)
                    return false;

                try
                {
                    nSiteGuid = int.Parse(userItemList.siteGuid);
                }
                catch
                {
                }

                Dictionary<int, List<int>> dItems = new Dictionary<int, List<int>>();
                List<int> orderNum = null;
                int nOrderNum = 0;

                foreach (ItemObj itemObj in userItemList.itemObj)
                {
                    if (itemObj.orderNum != null)
                    {
                        nOrderNum = itemObj.orderNum.Value;
                    }
                    orderNum = new List<int>();
                    orderNum.Add(nOrderNum);
                    dItems.Add(itemObj.item, orderNum);
                }
                bool result = UsersDal.Remove_ItemFromList(nSiteGuid, dItems, (int)userItemList.listType, (int)userItemList.itemType, nGroupID);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("RemoveItemFromList - exception =  " + ex.Message, ex);
                return false;
            }
        }

        public virtual ApiObjects.Response.Status DeleteItemFromUsersList(int itemId, ListType listType, ItemType itemType, string userId, int groupId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                if (itemId == 0 || string.IsNullOrEmpty(userId) || listType == ListType.All || itemType == ItemType.All)
                    return response;

                int spRes = UsersDal.DeleteItemFromUserList(itemId, (int)listType, (int)itemType, userId, groupId);
                if (spRes == -1)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ItemNotFound, "Item was not found in list");
                }
                else if (spRes > -1)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("DeleteItemFromUsersList - exception =  " + ex.Message, ex);
            }

            return response;
        }

        public virtual bool UpdateItemInList(UserItemList userItemList, int nGroupID)
        {
            try
            {
                int nSiteGuid = 0;
                if (userItemList == null || string.IsNullOrEmpty(userItemList.siteGuid) || userItemList.itemObj == null || userItemList.itemObj.Count() == 0)
                    return false;

                try
                {
                    nSiteGuid = int.Parse(userItemList.siteGuid);
                }
                catch
                {
                }

                Dictionary<int, List<int>> dItems = new Dictionary<int, List<int>>();
                List<int> orderNum = null;
                int nOrderNum = 0;

                foreach (ItemObj itemObj in userItemList.itemObj)
                {
                    if (itemObj.orderNum != null)
                    {
                        nOrderNum = itemObj.orderNum.Value;
                    }
                    orderNum = new List<int>();
                    orderNum.Add(nOrderNum);
                    dItems.Add(itemObj.item, orderNum);
                }

                bool result = UsersDal.Update_ItemInList(nSiteGuid, dItems, (int)userItemList.listType, (int)userItemList.itemType, nGroupID);
                return result;
            }
            catch (Exception ex)
            {
                log.Error("UpdateItemInList - exception =  " + ex.Message, ex);
                return false;
            }
        }

        public virtual List<UserItemList> GetItemFromList(UserItemList userItemList, int nGroupID)
        {
            try
            {
                List<UserItemList> luserItemList = new List<UserItemList>();
                int nSiteGuid = 0;
                if (userItemList == null || string.IsNullOrEmpty(userItemList.siteGuid))
                    return null;

                try
                {
                    nSiteGuid = int.Parse(userItemList.siteGuid);
                }
                catch
                {
                    return null;
                }

                DataTable dt = UsersDal.GetItemFromList(nSiteGuid, (int)userItemList.listType, (int)userItemList.itemType, nGroupID);
                if (dt != null && dt.DefaultView.Count > 0)
                {
                    UserItemList userItem = new UserItemList();
                    foreach (DataRow dr in dt.Rows)
                    {
                        ListType elistType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(dr["list_type"]);
                        ItemType eitemType = (ItemType)ODBCWrapper.Utils.GetIntSafeVal(dr["item_type"]);

                        if (elistType == userItem.listType && eitemType == userItem.itemType)
                            continue;

                        DataRow[] dRows = dt.Select("list_type=" + (int)elistType + " AND item_type=" + (int)eitemType);

                        bool firstTime = true;
                        userItem = new UserItemList();
                        ItemObj itemObj = null;
                        foreach (DataRow item in dRows)
                        {
                            if (firstTime)
                            {
                                userItem.siteGuid = ODBCWrapper.Utils.GetSafeStr(item["user_id"]);
                                userItem.listType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(item["list_type"]);
                                userItem.itemType = (ItemType)ODBCWrapper.Utils.GetIntSafeVal(item["item_type"]);
                                userItem.itemObj = new List<ItemObj>();
                                firstTime = false;
                            }
                            itemObj = new ItemObj();
                            itemObj.item = ODBCWrapper.Utils.GetIntSafeVal(item["item_id"]);
                            itemObj.orderNum = ODBCWrapper.Utils.GetIntSafeVal(item["order_num"]);
                            userItem.itemObj.Add(itemObj);
                        }
                        luserItemList.Add(userItem);
                    }
                }
                return luserItemList;
            }
            catch (Exception ex)
            {
                log.Error("GetItemFromList - exception =  " + ex.Message, ex);
                return null;
            }
        }

        public virtual UsersItemsListsResponse GetItemsFromUsersLists(int groupId, List<string> userIds, ListType listType, ItemType itemType)
        {
            UsersItemsListsResponse response = new UsersItemsListsResponse();

            try
            {
                // check if user ids supplied
                if (userIds == null || userIds.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "No user ids supplied");
                    return response;
                }

                // parse user ids to ints
                List<int> ids = new List<int>();
                int id;
                foreach (var userId in userIds)
                {
                    if (int.TryParse(userId, out id))
                    {
                        ids.Add(id);
                    }
                    else
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidUser, "user id must be int");
                        return response;
                    }
                }

                // get items lists
                DataTable dt = UsersDal.GetItemsFromUsersLists(ids, (int)listType, (int)itemType, groupId);
                if (dt != null && dt.DefaultView.Count > 0)
                {
                    // build all the lists using dictionary
                    Dictionary<ListType, UserItemsList> listsDict = new Dictionary<ListType,UserItemsList>();
                    foreach (DataRow dr in dt.Rows)
                    {
                        listType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(dr["list_type"]);
                        if (!listsDict.ContainsKey(listType))
                        {
                            listsDict.Add(listType, new UserItemsList() 
                            {
                                ListType = listType,
                                ItemsList = new List<Item>()
                            });
                        }

                        listsDict[listType].ItemsList.Add(new Item()
                        {
                            ItemType = (ItemType)ODBCWrapper.Utils.GetIntSafeVal(dr["item_type"]),
                            ItemId = ODBCWrapper.Utils.GetIntSafeVal(dr["item_id"]),
                            OrderIndex = ODBCWrapper.Utils.GetIntSafeVal(dr["order_num"]),
                            UserId = ODBCWrapper.Utils.GetSafeStr(dr["user_id"]),
                            ListType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(dr["list_type"])
                        });
                    }

                    // copy the lists to the response
                    response.UsersItemsLists = new List<UserItemsList>();
                    foreach (var list in listsDict)
                    {
                        response.UsersItemsLists.Add(list.Value);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                log.Error("GetItemFromList - exception =  " + ex.Message, ex);
                return null;
            }
        }


        /*Return for each item true/false if it exists in the list of the user  */
        public List<ApiObjects.KeyValuePair> IsItemExistsInList(UserItemList userItemList, int nGroupID)
        {
            try
            {
                List<ApiObjects.KeyValuePair> itemsExists = new List<ApiObjects.KeyValuePair>();
                if (userItemList == null || userItemList.itemObj == null || userItemList.itemObj.Count == 0 || nGroupID == 0 || string.IsNullOrEmpty(userItemList.siteGuid))
                    return null;
                List<int> lItems = new List<int>();

                foreach (ItemObj itemObj in userItemList.itemObj)
                {
                    lItems.Add(itemObj.item);
                }
                DataTable dt = UsersDal.IsItemExists(lItems, nGroupID, userItemList.siteGuid);
                if (dt != null && dt.DefaultView.Count > 0)
                {
                    ApiObjects.KeyValuePair itemObj = null;
                    foreach (DataRow dr in dt.Rows)
                    {
                        string itemId = ODBCWrapper.Utils.GetSafeStr(dr["itemId"]);
                        string itemExists = ODBCWrapper.Utils.GetSafeStr(dr["itemExists"]);
                        itemObj = new ApiObjects.KeyValuePair(itemId, itemExists);
                        itemsExists.Add(itemObj);
                    }
                }
                return itemsExists;
            }
            catch (Exception ex)
            {
                log.Error("IsItemExists - exception =  " + ex.Message, ex);
                return null;
            }
        }

        public virtual ResponseStatus SetUserTypeByUserID(string sSiteGUID, int nUserTypeID)
        {
            User u = new User();
            ResponseStatus ret = u.SetUserTypeByUserID(m_nGroupID, sSiteGUID, nUserTypeID);
            return ret;
        }

        public virtual int GetUserType(string sSiteGUID)
        {
            long lSiteGuid = 0;
            int nUserTypeID = 0;

            if (long.TryParse(sSiteGUID, out lSiteGuid))
            {
                DataTable dtUserType = UsersDal.GetUserType(m_nGroupID, lSiteGuid);

                if (dtUserType != null && dtUserType.Rows.Count > 0)
                {
                    foreach (DataRow drUserType in dtUserType.Rows)
                    {
                        nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(drUserType["User_Type"]);
                    }
                }
            }

            return nUserTypeID;
        }

        /// <summary>
        /// generates a login pin code for the given user (only if login with pin is enabled). The generated pin code is unique, 8-10 digits length and not start with 0.
        /// </summary>
        /// <param name="siteGuid">user's siteGuid</param>
        /// <param name="groupID">group id</param>
        /// <param name="secret">security parameter</param>
        /// <returns>PinCodeResponse containing the generated pin code </returns>
        public virtual PinCodeResponse GenerateLoginPIN(string siteGuid, int groupID, string secret)
        {
            PinCodeResponse response = new PinCodeResponse();
            try
            {
                // check that user exists 
                UserResponseObject user = GetUserData(siteGuid);
                if (user != null)
                {
                    if (user.m_user != null && (user.m_RespStatus == ResponseStatus.OK || user.m_RespStatus == ResponseStatus.UserNotIndDomain))
                    {
                        // check if security question is forced + login via pin is allowed
                        bool loginViaPin = false;
                        bool securityQuestion = false;
                        UsersDal.Get_LoginSettings(groupID, out securityQuestion, out loginViaPin);

                        if (loginViaPin && (!securityQuestion || (securityQuestion && !string.IsNullOrEmpty(secret))))
                        {
                            // generate login pin code
                            string pinCode = GenerateNewPIN(groupID);

                            DataTable dt = DAL.UsersDal.Insert_LoginPIN(siteGuid, pinCode, groupID, DateTime.UtcNow, secret);
                            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                            {
                                DataRow dr = dt.Rows[0];
                                response.pinCode = ODBCWrapper.Utils.GetSafeStr(dr, "pinCode");
                                response.expiredDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "expired_date");
                                response.siteGuid = ODBCWrapper.Utils.GetSafeStr(dr, "user_id");

                                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "New login pin was generated for the user");
                            }
                        }
                        // login via pin is not allowed
                        else if (!loginViaPin)
                        {
                            response.resp = new ApiObjects.Response.Status((int)eResponseStatus.LoginViaPinNotAllowed, "Login via pin is not allowed");
                        }
                        // security must be provided
                        else 
                        {
                            response.resp = new ApiObjects.Response.Status((int)eResponseStatus.MissingSecurityParameter, "Missing security parameter");
                        }
                    }
                    // invalid user - return error
                    else
                    {
                        // convert response status
                        response.resp = Utils.ConvertResponseStatusToResponseObject(user.m_RespStatus);
                    }
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }   
            }
            catch (Exception ex)
            {
                response = new PinCodeResponse();
                log.Error("GenerateLoginPIN - " + string.Format("Failed ex={0}, siteGuid={1}, groupID ={2}, ", ex.Message, siteGuid, groupID), ex);
            }
            return response;
        }




        private static IEnumerable<int> Digits(bool first)
        {
            int firstNumber = first ? 1 : 0;
            Random random = new Random();
            while (true)
                yield return random.Next(firstNumber, 10);
        }

        static string AddUniqueCode(int length)
        {
            while (true)
            {
                string firstChar = string.Join(null, Digits(true).Take(1)); //The PIN should not begin with “0” 
                string code = string.Join(null, Digits(false).Take(length - 1));
                code = string.Concat(firstChar, code);
                return code;
            }
        }

        private string GenerateNewPIN(int groupID)
        {
            string sNewPIN = string.Empty;

            bool codeExsits = true;

            while (codeExsits)
            {
                // Create new PIN               
                try
                {
                    int length = TVinciShared.WS_Utils.GetTcmIntValue("PIN_NUMBER_OF_DIGITS");
                    if (length == 0)
                    {
                        length = PIN_NUMBER_OF_DIGITS; //default number of digits
                    }

                    //The generated PIN should always be 10 digits (number only)
                    sNewPIN = AddUniqueCode(length);
                    //The PIN should be unique - if exsits create NEW one
                    codeExsits = UsersDal.PinCodeExsits(groupID, sNewPIN, DateTime.UtcNow);
                }
                finally
                {
                }
            }
            return sNewPIN;
        }

        private bool isValidPIN(string PIN, out ApiObjects.Response.Status response)
        {
            int minlength = TVinciShared.WS_Utils.GetTcmIntValue("PIN_MIN_NUMBER_OF_DIGITS");
            int maxlength = TVinciShared.WS_Utils.GetTcmIntValue("PIN_MAX_NUMBER_OF_DIGITS");
            if (minlength == 0)
            {
                minlength = PIN_MIN_NUMBER_OF_DIGITS; //default number of digits
            }
            if (maxlength == 0)
            {
                maxlength = PIN_MAX_NUMBER_OF_DIGITS; //default number of digits
            }
            //Allow the operator to set a minNum to MaxNum digits PIN
            if (string.IsNullOrEmpty(PIN) || PIN.Length < minlength || PIN.Length > maxlength)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.PinNotInTheRightLength, "Pin must be between" + minlength.ToString() + " - " + maxlength.ToString() + " digit");
                return false;
            }
            response = new ApiObjects.Response.Status();
            return true;
        }

        private bool IsUserValid(int groupID, string siteGuid, out ApiObjects.Response.Status response)
        {
            int userId = 0;
            bool parse = int.TryParse(siteGuid, out userId);
            bool isUserValid = false;
            UserActivationState activStatus = new UserActivationState();

            if (parse)
            {
                string userName = string.Empty;
                bool isGracePeriod = false;
                activStatus = (UserActivationState)DAL.UsersDal.GetUserActivationState(groupID, 0, ref userName, ref userId, ref isGracePeriod);
            }
            if (userId <= 0)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.WrongPasswordOrUserName, "User not valid");
            }
            else
            {
                switch (activStatus)
                {
                    case UserActivationState.Activated:
                    case UserActivationState.UserWIthNoDomain:
                    case UserActivationState.UserRemovedFromDomain:
                    case UserActivationState.NotActivated:
                    case UserActivationState.NotActivatedByMaster:
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "User valid");
                        isUserValid = true;
                        break;
                    case UserActivationState.Error:
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "User not valid");
                        break;
                    case UserActivationState.UserDoesNotExist:
                        response = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, "User not valid");
                        break;
                    case UserActivationState.UserSuspended:
                        response = new ApiObjects.Response.Status((int)eResponseStatus.UserSuspended, "User not valid");
                        break;
                    default:
                        response = new ApiObjects.Response.Status((int)eResponseStatus.Error, "User not valid");
                        break;
                }
            }
            return isUserValid;
        }

        /*
         * Get: groupID , PIN , secret
         * return UserResponse
         * login to system if pin code is valid + secret code check only if force by group == > group must enable loin by PIN
         */
        public UserResponse ValidateLoginWithPin(string PIN, string secret)
        {
            UserResponse response = new UserResponse();
            try
            {
                //Try to get users by PIN from DB 

                bool security = false;
                bool loginViaPin = false;
                DateTime expiredPIN = DateTime.MaxValue;
                DataRow dr = UsersDal.GetUserByPIN(m_nGroupID, PIN, secret, out security, out loginViaPin, out expiredPIN);

                if (!loginViaPin)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.LoginViaPinNotAllowed, "Login via pin is not allowed");
                    return response;
                }
                if (dr == null)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.PinNotExists, "Pin code not exists");
                    return response;
                }
                // check secret 
                bool isSecret = true;
                if (security)
                {
                    isSecret = ODBCWrapper.Utils.ExtractBoolean(dr, "secretValid");
                }
                if (isSecret)
                {
                    int userId = ODBCWrapper.Utils.GetIntSafeVal(dr, "user_id");//, up.pinCode, up.
                    DateTime expiredDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "expired_date");
                    if (DateTime.UtcNow >= expiredDate) // pincode is expired
                    {
                        response.resp = new ApiObjects.Response.Status((int)eResponseStatus.PinExpired, "Pin code expired at " + expiredDate.ToString());
                    }
                    else
                    {
                        response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "valid pin and user");
                        response.user = new UserResponseObject();
                        response.user.m_user = new User(m_nGroupID, userId);
                    }
                }
                else
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.SecretIsWrong, "Problems with the secret code");
                }
            }
            catch (Exception ex)
            {
                response = new UserResponse();
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.PinNotExists, "Pin code not exists");
                log.ErrorFormat("SignInWithPIN - Failed ex={0}, PIN={1}, groupID ={2}, ", ex.Message, PIN, m_nGroupID);
            }
            return response;
        }

        public void ExpirePIN(int groupID, string PIN)
        {
            bool expirePIN = UsersDal.ExpirePIN(groupID, PIN);
        }

        /// <summary>
        /// sets the supplied pin code for the given user (only if login with pin is enabled). The pin code should be unique, 8-10 digits length and not start with 0.
        /// </summary>
        /// <param name="siteGuid">user's site guid</param>
        /// <param name="pinCode">pin code</param>
        /// <param name="groupID">group id</param>
        /// <param name="secret">must be supplied if security is forced</param>
        /// <returns></returns>
        public PinCodeResponse SetLoginPIN(string siteGuid, string pinCode, int groupID, string secret)
        {
            PinCodeResponse response = new PinCodeResponse();
            try
            {
                string userName = string.Empty;

                // check if user is valid 
                ApiObjects.Response.Status status = null;
                if (!IsUserValid(groupID, siteGuid, out status))
                {
                    response.resp = status;
                    return response;
                }

                // validate pin code
                if (!isValidPIN(pinCode, out status))
                {
                    response.resp = status;
                    return response;
                }

                // check if security question is forced + login via pin is allowed
                bool loginViaPin = false;
                bool securityQuestion = false;
                UsersDal.Get_LoginSettings(groupID, out securityQuestion, out loginViaPin);

                if (loginViaPin && (!securityQuestion || (securityQuestion && !string.IsNullOrEmpty(secret))))
                {
                    // the pin code must be unique (among all other active pin codes) - if not return error
                    if (UsersDal.PinCodeExsits(groupID, pinCode, DateTime.UtcNow))
                    {                        
                        response.resp = new ApiObjects.Response.Status((int)eResponseStatus.PinAlreadyExists, "Pin code already exists - try new pin code");
                    }
                    // insert new PIN to user with expired date 
                    else
                    {
                        DataTable dt = DAL.UsersDal.Insert_LoginPIN(siteGuid, pinCode, groupID, DateTime.UtcNow, secret);

                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            response.expiredDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[0]["expired_date"]);
                            response.pinCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["pinCode"]);
                            response.siteGuid = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["user_id"]);

                            response.resp = new ApiObjects.Response.Status((int)eResponseStatus.OK, "login pin code was set for user");
                        }
                        else
                        {
                            response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, "failed to set pin code for user");
                        }
                    }
                }
                // login via pin is not allowed
                else if (!loginViaPin)
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.LoginViaPinNotAllowed, "login via pin is not allowed");
                }
                // security parameter must be provided
                else 
                {
                    response.resp = new ApiObjects.Response.Status((int)eResponseStatus.MissingSecurityParameter, "missing security parameter");
                }
            }
            catch (Exception ex)
            {
                response.resp = new ApiObjects.Response.Status((int)eResponseStatus.Error, ex.Message);
                log.Error("SetLoginPIN - " + string.Format("Failed ex={0}, siteGuid={1}, PIN={2}, groupID ={3}, ", ex.Message, siteGuid, pinCode, groupID), ex);
            }
            return response;
        }

        /// <summary>
        /// Clears users pin codes (sets them to be expired). If a specific pin code is supplied clears only this one, if not, clears all the user's pin codes
        /// </summary>
        /// <param name="siteGuid">user's siteGuid</param>
        /// <param name="pinCode">pin code - if supplied is cleared</param>
        /// <param name="groupID">group id</param>
        /// <returns>status for the action</returns>
        public ApiObjects.Response.Status ClearLoginPIN(string siteGuid, string pinCode, int groupID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            try
            {
                // check if user is valid 
                string userName = string.Empty;
                if (!IsUserValid(groupID, siteGuid, out response))
                {
                    return response;
                }

                // if pin code is not supplied - clear all the user's pin codes
                if (string.IsNullOrEmpty(pinCode))
                {
                    if (UsersDal.ExpirePINsByUserID(groupID, siteGuid))
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Cleared login pin");
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "No pin code exists for user");
                    }
                }
                // if a pin code supplied clear only this pin code 
                else
                {
                    if (UsersDal.UpdateLoginPinStatusByPinCode(groupID, siteGuid, pinCode))
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "Cleared login pin");
                    }
                    else
                    {
                        response = new ApiObjects.Response.Status((int)eResponseStatus.PinNotExists, "The supplied pin code does not exist for the user");
                    }
                }
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, ex.Message);
                log.Error("ClearLoginPIN - " + string.Format("Failed ex={0}, siteGuid={1}, groupID ={2}, ", ex.Message, siteGuid, groupID), ex);
            }
            return response;
        }

        /// <summary>
        /// Filters the list of supplied media ids and returns only the favorites for the user
        /// </summary>
        /// <param name="sWSUserName"></param>
        /// <param name="sWSPassword"></param>
        /// <param name="userId"></param>
        /// <param name="mediaIds"></param>
        /// <returns></returns>
        public FavoriteResponse FilterFavoriteMediaIds(int groupId, string userId, List<int> mediaIds, string udid, string mediaType, FavoriteOrderBy orderBy)
        {
            FavoriteResponse response = new FavoriteResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()); 

            try
            {
                // check if user is valid 
                ApiObjects.Response.Status status = null;
                if (!IsUserValid(groupId, userId, out status))
                {
                    response.Status = status;
                    return response;
                }

                // get the favorites ids
                DataTable dt = UsersDal.Get_FavoriteMediaIds(userId, mediaIds, udid, mediaType, (int)orderBy);
                if (dt != null)
                {
                    if (dt.Rows != null && dt.Rows.Count > 0)
                    {
                        response.Favorites = new FavoritObject[dt.Rows.Count];
                        FavoritObject favorite;
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            favorite = new FavoritObject()
                            {
                                m_sItemCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["ID"]),
                                m_sExtraData = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["EXTRA_DATA"]),
                                m_dCreateDate = ODBCWrapper.Utils.GetDateSafeVal(dt.Rows[i]["CREATE_DATE"])
                            };
                            response.Favorites[i] = favorite;
                        }
                    }
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("FilterFavoriteMediaIds failed, ex = {0}, userId = {1}, ", ex.Message, userId), ex);
            }
            return response;
        }

        public LongIdsResponse GetUserRoleIds(int groupId, string userId)
        {
            LongIdsResponse response = new LongIdsResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                response.Ids = UsersDal.Get_UserRoleIds(groupId, userId);
                if (response.Ids != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUserRoleIds failed, ex = {0}, userId = {1}, ", ex.Message, userId), ex);
            }
            return response;
        }

        public ApiObjects.Response.Status AddRoleToUser(int groupId, string userId, long roleId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status();
            response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                int rowCount = UsersDal.Insert_UserRole(groupId, userId, roleId, false);
                if (rowCount == -1)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.RoleAlreadyAssignedToUser, ROLE_ALREADY_ASSIGNED_TO_USER_ERROR);
                }
                else if (rowCount > 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("AddRoleToUser failed, ex = {0}, userId = {1}, roleId = {2} ", ex.Message, userId), ex);
            }
            return response;
        }

        public abstract ApiObjects.Response.Status ResendActivationToken(string username);

        public UsersListItemResponse AddItemToUsersList(int itemId, ListType listType, ItemType itemType, int order, string userId, int groupId)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                int parsedUserId = 0;
                if (!int.TryParse(userId, out parsedUserId) || itemId == 0 || string.IsNullOrEmpty(userId) || listType == ListType.All || itemType == ItemType.All)
                    return response;

                DataTable dt = UsersDal.InsertItemToUserList(parsedUserId, order, itemId, (int)listType, (int)itemType, groupId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    response.Item = new Item()
                    {
                        ItemId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["item_id"]),
                        ItemType = (ItemType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["item_type"]),
                        ListType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["list_type"]),
                        OrderIndex = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["order_num"]),
                        UserId = userId
                    };
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                log.Error("AddItemToUsersList - exception =  " + ex.Message, ex);
            }

            return response;
        }

        public UsersListItemResponse GetItemFromUsersList(int itemId, ListType listType, ItemType itemType, string userId, int groupId)
        {
            UsersListItemResponse response = new UsersListItemResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            try
            {
                int parsedUserId = 0;
                if (!int.TryParse(userId, out parsedUserId) || itemId == 0 || string.IsNullOrEmpty(userId))
                    return response;

                DataTable dt = UsersDal.GetItemFromUserList(parsedUserId, itemId, (int)listType, (int)itemType, groupId);
                if (dt != null && dt.Rows.Count > 0)
                {
                    response.Item = new Item()
                    {
                        ItemId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["item_id"]),
                        ItemType = (ItemType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["item_type"]),
                        ListType = (ListType)ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["list_type"]),
                        OrderIndex = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["order_num"]),
                        UserId = userId
                    };
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ItemNotFound, "Item was not found in list");
                }
            }
            catch (Exception ex)
            {
                log.Error("GetItemFromUsersList - exception =  " + ex.Message, ex);
            }

            return response;
        }
    }
}
