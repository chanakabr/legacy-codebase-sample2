using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using DAL;
using Logger;
using ApiObjects;
using ApiObjects.Statistics;

namespace Users
{
    public abstract class BaseUsers
    {
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
        public abstract UserResponseObject GetUserData(string sSiteGUID);
        public abstract List<UserBasicData> GetUsersBasicData(long[] nSiteGUIDs);
        public abstract List<UserResponseObject> GetUsersData(string[] sSiteGUIDs);
        public abstract List<UserBasicData> SearchUsers(string[] sTerms, string[] sFields, bool bIsExact);
        public abstract UserResponseObject AddNewUser(UserBasicData oBasicData, UserDynamicData sDynamicData, string sPassword);
        public abstract UserResponseObject AddNewUser(string sBasicDataXML, string sDynamicDataXML, string sPassword);
        public abstract bool DoesUserNameExists(string sUserName);
        public abstract UserGroupRuleResponse CheckParentalPINToken(string sChangePinToken);
        public abstract UserGroupRuleResponse ChangeParentalPInCodeByToken(string sSiteGuid, int nUserRuleID, string sChangePinToken, string sCode);
        public abstract DomainResponseObject AddNewDomain(string sUN, int nUserID, int nGroupID);

        public virtual bool WriteToLog(string sSiteGUID, string sMessage, string sWriter)
        {
            return false;
        }

        public virtual bool AddUserFavorit(string sUserGUID, int domainID, string sDeviceUDID,
            string sItemType, string sItemCode, string sExtraData, Int32 nGroupID)
        {
            try
            {
                bool saveRes = false;
                FavoritObject f = new FavoritObject();
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

                saveRes &= WriteFavoriteToES(view);

                return saveRes;
            }
            catch (Exception ex)
            {
                return false;
            }
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
                          Logger.Logger.Log("WriteFavoriteToES", string.Format("Was unable to insert record to ES. index={0};type={1};doc={2}", index, ElasticSearch.Common.Utils.ES_STATS_TYPE, sJsonView), "Users");
                  }

                return bRes;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("WriteFavoriteToES", string.Format("Failed ex={0}, index={1};type={2}", ex.Message, index, ElasticSearch.Common.Utils.ES_STATS_TYPE), "Users");
                return false;
            }
        }



        public virtual bool AddChannelMediaToFavorites(string sUserGUID, int domainID, string sDeviceUDID,
          string sItemType, string sChannelID, string sExtraData, Int32 nGroupID)
        {
            FavoritObject f = new FavoritObject();
            f.Initialize(0, sUserGUID, domainID, sDeviceUDID, sItemType, sChannelID, sExtraData, DateTime.UtcNow, nGroupID, 1);
            return f.Save(nGroupID);
        }

        public virtual void RemoveUserFavorit(Int32 nFavoritID, string sUserGUID, Int32 nGroupID)
        {
            FavoritObject.RemoveFavorit(sUserGUID, nGroupID, nFavoritID);
        }

        public virtual void RemoveUserFavorit(int[] nMediaIDs, string sUserGUID, int nGroupID)
        {
            FavoritObject.RemoveFavorit(sUserGUID, nGroupID, nMediaIDs);
        }

        public virtual void RemoveChannelMediaUserFavorit(int[] nChannelIDs, string sUserGUID, int nGroupID)
        {
            FavoritObject.RemoveChannelMediaFavorit(sUserGUID, nGroupID, nChannelIDs);
        }

        public virtual FavoritObject[] GetUserFavorites(string sSiteGUID, string sDeviceUDID, string sItemType, int nGroupID, int domainID)
        {
            return FavoritObject.GetFavorites(nGroupID, sSiteGUID, domainID, sDeviceUDID, sItemType);
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
        public abstract UserActivationState GetUserActivationStatus(ref string sUserName, ref Int32 nUserID);
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
        static protected Int32 m_nActivationMustHours;
        static protected Int32 m_nTokenValidityHours;
        static protected Int32 m_nChangePinTokenValidityHours;

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
                uro = GetUserData(sSiteGUID);
            }

            if (uro.m_RespStatus != ResponseStatus.OK || uro.m_user == null || uro.m_user.m_oDynamicData == null)
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
                    bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetByteSafeVal(drUserType,"is_default"));
                    UserType userType = new UserType(nUserTypeID, sUserTypeDesc, isDefault);
                    userTypesList.Add(userType);
                }
            }
            return userTypesList;
        }


        public virtual UserType[] GetGroupUserTypes(Int32 nGroupID)
        {
            DataTable dtUserData = UsersDal.GetUserTypeData(nGroupID, null);
            List<UserType> userTypesList = GetUserTypesList(dtUserData);
            return userTypesList.ToArray();
        }


        // if no UserBasicData object is present send null instead
        public virtual bool IsActivationNeeded(UserBasicData oBasicData)
        {
            if (!m_bIsActivationNeeded.HasValue)
            {
                m_bIsActivationNeeded = UsersDal.GetIsActivationNeeded(m_nGroupID);

                //DataTable dt = UsersDal.GetIsActivationNeeded(m_nGroupID);
                //if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                //    m_bIsActivationNeeded = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["IS_ACTIVATION_NEEDED"]) != 0;
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

                bool result = UsersDal.Insert_ItemList(nSiteGuid, dItems, (int)userItemList.listType, (int)userItemList.itemType, nGroupID);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("AddItemToList", "exception =  " + ex.Message, "BaseUsers");
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
                Logger.Logger.Log("RemoveItemFromList", "exception =  " + ex.Message, "BaseUsers");
                return false;
            }
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
                Logger.Logger.Log("UpdateItemInList", "exception =  " + ex.Message, "BaseUsers");
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
                Logger.Logger.Log("GetItemFromList", "exception =  " + ex.Message, "BaseUsers");
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
                Logger.Logger.Log("IsItemExists", "exception =  " + ex.Message, "BaseUsers");
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
    }
}
