using ApiObjects.MediaMarks;
using CouchbaseManager;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;

namespace DAL
{
    public class SocialDAL
    {
        private static readonly string CB_MEDIA_MARK_DESGIN = ODBCWrapper.Utils.GetTcmConfigValue("cb_media_mark_design");

        private int m_nGroupID;
        public SocialDAL(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public DataTable GetGroupFBNamespace()
        {
            ODBCWrapper.StoredProcedure spFBNamespace = new ODBCWrapper.StoredProcedure("GetFBNamespace");
            spFBNamespace.SetConnectionKey("MAIN_CONNECTION_STRING");
            spFBNamespace.AddParameter("@GroupID", m_nGroupID);

            DataSet ds = spFBNamespace.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetFBFriends(List<long> lFBFriendsList)
        {
            ODBCWrapper.StoredProcedure spFBFriendsGuid = new ODBCWrapper.StoredProcedure("GetFBFriends");
            spFBFriendsGuid.SetConnectionKey("users_connection");
            spFBFriendsGuid.AddIDListParameter<string>("@FBFriendList", lFBFriendsList.Select(f => f.ToString()).ToList(), "STR");
            spFBFriendsGuid.AddParameter("@GroupID", m_nGroupID);

            DataSet ds = spFBFriendsGuid.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public int UpdateUserStatus(int nSiteGuid, int nStatus, int nIsActive)
        {
            ODBCWrapper.StoredProcedure spUpdateUserStatus = new ODBCWrapper.StoredProcedure("UpdateUserStatus");
            spUpdateUserStatus.SetConnectionKey("users_connection");
            spUpdateUserStatus.AddParameter("@UserSiteGuid", nSiteGuid);
            spUpdateUserStatus.AddParameter("@GroupID", m_nGroupID);
            spUpdateUserStatus.AddParameter("@Status", nStatus);
            spUpdateUserStatus.AddParameter("@IsActive", nIsActive);

            int retVal = spUpdateUserStatus.ExecuteReturnValue<int>();

            return retVal;
        }

        public DataTable GetUserSocialPrivacy(int nSiteGuid, int nSocialPlatform, int nSocialAction)
        {
            ODBCWrapper.StoredProcedure spGetUserPrivacy = new ODBCWrapper.StoredProcedure("GetUserPrivacy");
            spGetUserPrivacy.SetConnectionKey("users_connection");
            spGetUserPrivacy.AddParameter("@UserSiteGuid", nSiteGuid);
            spGetUserPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
            spGetUserPrivacy.AddParameter("@ActionID", nSocialAction);

            DataSet ds = spGetUserPrivacy.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }


        //public int InsertUserSocialPrivacy(int nSiteGuid, int nSocialPlatform, int nPrivacy)
        //{
        //    ODBCWrapper.StoredProcedure spInsertUserPrivacy = new ODBCWrapper.StoredProcedure("InsertUserSocialPrivacy");
        //    spInsertUserPrivacy.SetConnectionKey("users_connection");
        //    spInsertUserPrivacy.AddParameter("@UserSiteGuid", nSiteGuid);
        //    spInsertUserPrivacy.AddParameter("@GroupID", m_nGroupID);
        //    spInsertUserPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
        //    spInsertUserPrivacy.AddParameter("@Privacy", nPrivacy);

        //    int retVal = spInsertUserPrivacy.ExecuteReturnValue<int>();

        //    return retVal;
        //}

        public int SetUserSocialPrivacy(int nSiteGuid, int nSocialPlatform, int nSocialAction, int nPrivacy)
        {
            ODBCWrapper.StoredProcedure spUpdateUserPrivacy = new ODBCWrapper.StoredProcedure("UpdateUserSocialPrivacy");
            spUpdateUserPrivacy.SetConnectionKey("users_connection");
            spUpdateUserPrivacy.AddParameter("@UserSiteGuid", nSiteGuid);
            spUpdateUserPrivacy.AddParameter("@GroupID", m_nGroupID);
            spUpdateUserPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
            spUpdateUserPrivacy.AddParameter("@ActionID", nSocialAction);
            spUpdateUserPrivacy.AddParameter("@Privacy", nPrivacy);

            int retVal = spUpdateUserPrivacy.ExecuteReturnValue<int>();

            return retVal;
        }

        public DataTable GetMediaFBObjectID(int nMediaID)
        {
            ODBCWrapper.StoredProcedure spGetFBObjectID = new ODBCWrapper.StoredProcedure("GetFBObjectID");
            spGetFBObjectID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetFBObjectID.AddParameter("@MediaID", nMediaID);

            DataSet ds = spGetFBObjectID.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetUsersLikedMedia(int nNumOfResults, int nSiteGuid, int nMediaID, int nSocialAction, int nSocialPlatform)
        {
            ODBCWrapper.StoredProcedure spGetUsersLikedMedia = new ODBCWrapper.StoredProcedure("GetUsersLikedMedia");
            spGetUsersLikedMedia.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUsersLikedMedia.AddParameter("@NumOfResults", nNumOfResults);
            spGetUsersLikedMedia.AddParameter("@UserSiteGuid", nSiteGuid);
            spGetUsersLikedMedia.AddParameter("@MediaID", nMediaID);
            spGetUsersLikedMedia.AddParameter("@SocialAction", nSocialAction);
            spGetUsersLikedMedia.AddParameter("@SocialPlatform", nSocialPlatform);

            DataSet ds = spGetUsersLikedMedia.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public List<MediaMarkLog> GetAllFriendsWatchedMedia(List<int> lFriendsList, int nMediaID = 0)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);

            List<MediaMarkLog> lRes = new List<MediaMarkLog>();

            if (nMediaID == 0)
            {

                List<UserMediaMark> mediaMarksList = CatalogDAL.GetMediaMarksLastDateByUsers(lFriendsList);
                foreach (UserMediaMark mediaMark in mediaMarksList)
                {
                    lRes.Add(new MediaMarkLog() { LastMark = mediaMark });
                }
            }
            else
            {
                List<string> docKeysList = new List<string>();

                foreach (int nUserID in lFriendsList)
                {
                    docKeysList.Add(UtilsDal.getUserMediaMarkDocKey(nUserID, nMediaID));
                }

                IDictionary<string, MediaMarkLog> res = cbManager.GetValues<MediaMarkLog>(docKeysList, true, true);

                foreach (string sKey in res.Keys)
                {
                    lRes.Add(res[sKey]);
                }
            }
            return lRes;
        }

        public List<MediaMarkLog> GetFriendsWhoWatchedMedia(int nMediaID, List<int> lFriendsGuidList)
        {
            var cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.MEDIAMARK);
            List<string> docKeysList = new List<string>();
            List<MediaMarkLog> lRes = new List<MediaMarkLog>();

            foreach (int userID in lFriendsGuidList)
            {
                docKeysList.Add(DAL.UtilsDal.getUserMediaMarkDocKey(userID, nMediaID));
            }

            IDictionary<string, object> res = cbManager.GetValues<object>(docKeysList, true);

            foreach (string sKey in res.Keys)
            {
                lRes.Add(JsonConvert.DeserializeObject<MediaMarkLog>(res[sKey].ToString()));
            }
            return lRes;
        }

        public Dictionary<string, int> GetFriendsWatchedCount(List<int> lFriendsGuidList)
        {
            Dictionary<string, int> retDict = CatalogDAL.GetMediaMarkUserCount(lFriendsGuidList);
            return retDict;
        }

        public DataTable GetUserSocialActionId(int nUserSiteGuid, int nMediaID, int nSocialPlatform, int nSocialAction, int nProgramID)
        {
            ODBCWrapper.StoredProcedure spGetUserSocialActionId = new ODBCWrapper.StoredProcedure("GetUserSocialActionId");
            spGetUserSocialActionId.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUserSocialActionId.AddParameter("@UserSiteGuid", nUserSiteGuid);
            spGetUserSocialActionId.AddParameter("@GroupID", m_nGroupID);
            spGetUserSocialActionId.AddParameter("@MediaID", nMediaID);
            spGetUserSocialActionId.AddParameter("@ProgramID", nProgramID);
            spGetUserSocialActionId.AddParameter("@SocialPlatform", nSocialPlatform);
            spGetUserSocialActionId.AddParameter("@SocialAction", nSocialAction);

            DataSet ds = spGetUserSocialActionId.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetUserSocialAction(int nUserSiteGuid, int nSocialPlatform, List<int> lSocialAction, int nAssetType = 0, int nMediaID = 0)
        {
            ODBCWrapper.StoredProcedure spGetUserSocialAction = new ODBCWrapper.StoredProcedure("GetUserSocialAction");
            spGetUserSocialAction.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUserSocialAction.AddParameter("@UserSiteGuid", nUserSiteGuid);
            spGetUserSocialAction.AddParameter("@GroupID", m_nGroupID);
            spGetUserSocialAction.AddIDListParameter<int>("@SocialAction", lSocialAction, "Id");
            spGetUserSocialAction.AddParameter("@MediaID", nMediaID);
            spGetUserSocialAction.AddParameter("@SocialPlatform", nSocialPlatform);
            spGetUserSocialAction.AddParameter("@AssetType", nAssetType);

            DataSet ds = spGetUserSocialAction.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public int UpdateUserSocialAction(int nUserSiteGuid, int nMediaID, int nSocialPlatform, int nSocialAction, int nActive, DateTime dUpdateTime)
        {
            ODBCWrapper.StoredProcedure spUpdateUserSocialAction = new ODBCWrapper.StoredProcedure("UpdateUserSocialAction");
            spUpdateUserSocialAction.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateUserSocialAction.AddParameter("@UserSiteGuid", nUserSiteGuid);
            spUpdateUserSocialAction.AddParameter("@GroupID", m_nGroupID);
            spUpdateUserSocialAction.AddParameter("@MediaID", nMediaID);
            spUpdateUserSocialAction.AddParameter("@SocialPlatform", nSocialPlatform);
            spUpdateUserSocialAction.AddParameter("@SocialAction", nSocialAction);
            spUpdateUserSocialAction.AddParameter("@Active", nActive);
            spUpdateUserSocialAction.AddParameter("@UpdateTime", dUpdateTime);


            int retVal = spUpdateUserSocialAction.ExecuteReturnValue<int>();

            return retVal;
        }

        public DataTable GetFBConfig(string sConnStr = "")
        {
            ODBCWrapper.StoredProcedure spGetFBConfig = new ODBCWrapper.StoredProcedure("GetFBConfig");
            if (string.IsNullOrEmpty(sConnStr))
            {
                spGetFBConfig.SetConnectionKey("MAIN_CONNECTION_STRING");
            }
            else
            {
                spGetFBConfig.SetConnectionKey(sConnStr);
            }
            spGetFBConfig.AddParameter("@GroupID", m_nGroupID);

            DataSet ds = spGetFBConfig.ExecuteDataSet();
            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetTwitterConfig(string sConnStr = "")
        {

            return null;
        }

        public DataTable GetMediaLinks(int nMediaID, int nIsActive, int nStatus)
        {
            ODBCWrapper.StoredProcedure spGetMediaLinks = new ODBCWrapper.StoredProcedure("GetMediaLinks");
            spGetMediaLinks.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetMediaLinks.AddParameter("@GroupID", m_nGroupID);
            spGetMediaLinks.AddParameter("@MediaID", nMediaID);
            spGetMediaLinks.AddParameter("@IsActive", nIsActive);
            spGetMediaLinks.AddParameter("@Status", nStatus);

            DataSet ds = spGetMediaLinks.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public int IncrementMediaLikeCounter(int nMediaID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaCounter = new ODBCWrapper.StoredProcedure("IncrementMediaLikeCounter");
            spUpdateMediaCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaCounter.AddParameter("@MediaID", nMediaID);

            int retVal = spUpdateMediaCounter.ExecuteReturnValue<int>();

            return retVal;
        }

        public int DecrementMediaLikeCounter(int nMediaID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaCounter = new ODBCWrapper.StoredProcedure("DecrementMediaLikeCounter");
            spUpdateMediaCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaCounter.AddParameter("@MediaID", nMediaID);

            int retVal = spUpdateMediaCounter.ExecuteReturnValue<int>();

            return retVal;
        }

        public int IncrementProgramLikeCounter(int nProgramID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaCounter = new ODBCWrapper.StoredProcedure("IncrementProgramLikeCounter");
            spUpdateMediaCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaCounter.AddParameter("@ProgramID", nProgramID);

            int retVal = spUpdateMediaCounter.ExecuteReturnValue<int>();

            return retVal;
        }

        public int DecrementProgramLikeCounter(int nProgramID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaCounter = new ODBCWrapper.StoredProcedure("DecrementProgramLikeCounter");
            spUpdateMediaCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaCounter.AddParameter("@ProgramID", nProgramID);

            int retVal = spUpdateMediaCounter.ExecuteReturnValue<int>();

            return retVal;
        }


        public DataTable GetMediaLikeCounter(int nMediaID)
        {
            ODBCWrapper.StoredProcedure spGetMediaLikeCounter = new ODBCWrapper.StoredProcedure("GetMediaLikeCounter");
            spGetMediaLikeCounter.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetMediaLikeCounter.AddParameter("@MediaID", nMediaID);

            DataSet ds = spGetMediaLikeCounter.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetUsersSocialActions(int nNumOfRecords, List<int> lSiteGuidList, int nSocialPlatform, List<int> lSocialActions, int nAssetType)
        {
            ODBCWrapper.StoredProcedure spGetUsersSocialActions = new ODBCWrapper.StoredProcedure("GetUsersSocialActions");
            spGetUsersSocialActions.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUsersSocialActions.AddParameter("@NumOfRecords", nNumOfRecords);
            spGetUsersSocialActions.AddParameter("@GroupID", m_nGroupID);
            spGetUsersSocialActions.AddIDListParameter<int>("@SiteGuidList", lSiteGuidList, "Id");
            spGetUsersSocialActions.AddParameter("@SocialPlatform", nSocialPlatform);
            spGetUsersSocialActions.AddIDListParameter<int>("@SocialAction", lSocialActions, "Id");
            spGetUsersSocialActions.AddParameter("@AssetType", nAssetType);

            DataSet ds = spGetUsersSocialActions.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public DataTable GetUsersSocialActionsOnAsset(int nNumOfRecords, List<int> lSiteGuidList, int nSocialPlatform, List<int> lSocialActions, int nMediaID, int nProgramID)
        {
            ODBCWrapper.StoredProcedure spGetUsersSocialActions = new ODBCWrapper.StoredProcedure("GetUsersSocialActionsOnMedia");
            spGetUsersSocialActions.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetUsersSocialActions.AddParameter("@NumOfRecords", nNumOfRecords);
            spGetUsersSocialActions.AddParameter("@GroupID", m_nGroupID);
            spGetUsersSocialActions.AddIDListParameter("@SiteGuidList", lSiteGuidList, "Id");
            spGetUsersSocialActions.AddParameter("@SocialPlatform", nSocialPlatform);
            spGetUsersSocialActions.AddIDListParameter("@SocialAction", lSocialActions, "Id");
            spGetUsersSocialActions.AddParameter("@MediaID", nMediaID);
            spGetUsersSocialActions.AddParameter("@ProgramID", nProgramID);

            DataSet ds = spGetUsersSocialActions.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        public int InsertUserSocialAction(int nSiteGuid, string sDeviceUDID, int nMediaID, int nProgramID, int nSocialAction, int nSocialPlatform, int nAssetType, string sFBActionID, int nInternalPrivacy, int nRateValue = 0)
        {

            ODBCWrapper.StoredProcedure spInsertUserSocialAction = new ODBCWrapper.StoredProcedure("InsertUserSocialAction");
            spInsertUserSocialAction.SetConnectionKey("MAIN_CONNECTION_STRING");
            spInsertUserSocialAction.AddParameter("@MediaID", nMediaID);
            spInsertUserSocialAction.AddParameter("@DeviceUDID", sDeviceUDID);
            spInsertUserSocialAction.AddParameter("@UserSiteGuid", nSiteGuid);
            spInsertUserSocialAction.AddParameter("@SocialAction", nSocialAction);
            spInsertUserSocialAction.AddParameter("@SocialPlatform", nSocialPlatform);
            spInsertUserSocialAction.AddParameter("@GroupID", m_nGroupID);
            spInsertUserSocialAction.AddParameter("@FBActionID", sFBActionID);
            spInsertUserSocialAction.AddParameter("@AssetType", nAssetType);
            spInsertUserSocialAction.AddParameter("@InternalPrivacy", nInternalPrivacy);
            spInsertUserSocialAction.AddParameter("@ProgramID", nProgramID);
            spInsertUserSocialAction.AddParameter("@RateValue", nRateValue);
            int retVal = spInsertUserSocialAction.ExecuteReturnValue<int>();

            return retVal;
        }

        public int DeleteUserSocialAction(int nSiteGuid, int nMediaID, int nProgramID, int nSocialAction, int nSocialPlatform, int nAssetType)
        {

            ODBCWrapper.StoredProcedure spInsertUserSocialAction = new ODBCWrapper.StoredProcedure("DeleteUserSocialAction");
            spInsertUserSocialAction.SetConnectionKey("MAIN_CONNECTION_STRING");
            spInsertUserSocialAction.AddParameter("@MediaID", nMediaID);
            spInsertUserSocialAction.AddParameter("@UserSiteGuid", nSiteGuid);
            spInsertUserSocialAction.AddParameter("@SocialAction", nSocialAction);
            spInsertUserSocialAction.AddParameter("@SocialPlatform", nSocialPlatform);
            spInsertUserSocialAction.AddParameter("@GroupID", m_nGroupID);
            spInsertUserSocialAction.AddParameter("@AssetType", nAssetType);
            spInsertUserSocialAction.AddParameter("@ProgramID", nProgramID);

            int retVal = spInsertUserSocialAction.ExecuteReturnValue<int>();

            return retVal;
        }

        public int SetMediaFBObjectID(int nMediaID, string sMediaFBObjectID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaFBObjID = new ODBCWrapper.StoredProcedure("UpdateMediaFBObjectID");
            spUpdateMediaFBObjID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaFBObjID.AddParameter("@MediaID", nMediaID);
            spUpdateMediaFBObjID.AddParameter("@FBObjectID", sMediaFBObjectID);

            int retVal = spUpdateMediaFBObjID.ExecuteReturnValue<int>();

            return retVal;
        }

        public int SetProgramFBObjectID(int nProgramID, string sProgramFBObjectID)
        {
            ODBCWrapper.StoredProcedure spUpdateMediaFBObjID = new ODBCWrapper.StoredProcedure("SetProgramFBObjID");
            spUpdateMediaFBObjID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spUpdateMediaFBObjID.AddParameter("@ProgramID", nProgramID);
            spUpdateMediaFBObjID.AddParameter("@FbObjectID", sProgramFBObjectID);

            int retVal = spUpdateMediaFBObjID.ExecuteReturnValue<int>();

            return retVal;
        }

        public DataTable GetMediaActionCount(string sGroupIDs, int nSocialAction)
        {
            ODBCWrapper.StoredProcedure spMediaAcountCount = new ODBCWrapper.StoredProcedure("GetMediaActionAggregate");
            spMediaAcountCount.SetConnectionKey("MAIN_CONNECTION_STRING");
            spMediaAcountCount.AddParameter("@GroupIDs", sGroupIDs);
            spMediaAcountCount.AddParameter("@SocialAction", nSocialAction);

            DataSet ds = spMediaAcountCount.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];

            return null;
        }

        //public DataTable GetGroupDefaultActionPrivacy(int nActionID)
        //{
        //    ODBCWrapper.StoredProcedure spDefaultGroupActionPrivacy = new ODBCWrapper.StoredProcedure("GetGroupDefaultActionPrivacy");
        //    spDefaultGroupActionPrivacy.SetConnectionKey("MAIN_CONNECTION_STRING");
        //    spDefaultGroupActionPrivacy.AddParameter("@GroupID", m_nGroupID);
        //    spDefaultGroupActionPrivacy.AddParameter("@ActionID", nActionID);

        //    DataSet ds = spDefaultGroupActionPrivacy.ExecuteDataSet();

        //    if (ds != null)
        //        return ds.Tables[0];

        //    return null;
        //}

        //public int UpdateGroupDefaultActionPrivacy(int nActionID, int nActionPrivacy)
        //{
        //    ODBCWrapper.StoredProcedure spDefaultGroupActionPrivacy = new ODBCWrapper.StoredProcedure("GetGroupDefaultActionPrivacy");
        //    spDefaultGroupActionPrivacy.SetConnectionKey("MAIN_CONNECTION_STRING");
        //    spDefaultGroupActionPrivacy.AddParameter("@GroupID", m_nGroupID);
        //    spDefaultGroupActionPrivacy.AddParameter("@ActionID", nActionID);
        //    spDefaultGroupActionPrivacy.AddParameter("@ActionPrivacy", nActionPrivacy);
        //    int retVal = spDefaultGroupActionPrivacy.ExecuteReturnValue<int>();

        //    return retVal;
        //}

        //public int SetUserFBActionPrivacy(string sUserSiteGuid, int nSocialPlatform, int nSocialAction, int nActionPrivacy)
        //{

        //    ODBCWrapper.StoredProcedure spUpdateUserActionPrivacy = new ODBCWrapper.StoredProcedure("SetUserFBActionPrivacy");
        //    spUpdateUserActionPrivacy.SetConnectionKey("users_connection");
        //    spUpdateUserActionPrivacy.AddParameter("@UserSiteGuid", sUserSiteGuid);
        //    spUpdateUserActionPrivacy.AddParameter("@GroupID", m_nGroupID);
        //    spUpdateUserActionPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
        //    spUpdateUserActionPrivacy.AddParameter("@Action", nSocialAction);
        //    spUpdateUserActionPrivacy.AddParameter("@FBPrivacy", nActionPrivacy);

        //    int retVal = spUpdateUserActionPrivacy.ExecuteReturnValue<int>();

        //    return retVal;
        //}

        public int SetUserInternalActionPrivacy(string sUserSiteGuid, int nSocialPlatform, int nSocialAction, int nActionPrivacy)
        {
            ODBCWrapper.StoredProcedure spUpdateUserActionPrivacy = new ODBCWrapper.StoredProcedure("SetUserInternalActionPrivacy");
            spUpdateUserActionPrivacy.SetConnectionKey("users_connection");
            spUpdateUserActionPrivacy.AddParameter("@UserSiteGuid", sUserSiteGuid);
            spUpdateUserActionPrivacy.AddParameter("@GroupID", m_nGroupID);
            spUpdateUserActionPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
            spUpdateUserActionPrivacy.AddParameter("@ActionID", nSocialAction);
            spUpdateUserActionPrivacy.AddParameter("@Share", nActionPrivacy);

            int retVal = spUpdateUserActionPrivacy.ExecuteReturnValue<int>();

            return retVal;
        }

        public int SetUserExternalActionShare(string sUserSiteGuid, int nSocialPlatform, int nSocialAction, int nActionPrivacy)
        {
            ODBCWrapper.StoredProcedure spUpdateUserActionPrivacy = new ODBCWrapper.StoredProcedure("SetUserExternalActionShare");
            spUpdateUserActionPrivacy.SetConnectionKey("users_connection");
            spUpdateUserActionPrivacy.AddParameter("@UserSiteGuid", sUserSiteGuid);
            spUpdateUserActionPrivacy.AddParameter("@GroupID", m_nGroupID);
            spUpdateUserActionPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
            spUpdateUserActionPrivacy.AddParameter("@ActionID", nSocialAction);
            spUpdateUserActionPrivacy.AddParameter("@Share", nActionPrivacy);

            int retVal = spUpdateUserActionPrivacy.ExecuteReturnValue<int>();

            return retVal;
        }

        //public DataTable GetUserSocialActionPrivacy(string sUserSiteGuid, int nSocialPlatform, int nSocialAction)
        //{
        //    ODBCWrapper.StoredProcedure spGetUserActionPrivacy = new ODBCWrapper.StoredProcedure("GetUserSocialActionPrivacy");
        //    spGetUserActionPrivacy.SetConnectionKey("users_connection");
        //    spGetUserActionPrivacy.AddParameter("@UserSiteGuid", sUserSiteGuid);
        //    spGetUserActionPrivacy.AddParameter("@GroupID", m_nGroupID);
        //    spGetUserActionPrivacy.AddParameter("@SocialPlatform", nSocialPlatform);
        //    spGetUserActionPrivacy.AddParameter("@Action", nSocialAction);

        //    DataSet ds = spGetUserActionPrivacy.ExecuteDataSet();

        //    if (ds != null)
        //        return ds.Tables[0];

        //    return null;

        //}

        public DataTable GetSocialFeedMediaTags(int mediaId)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetSocialFeedMediaTags");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@mediaId", mediaId);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];

            return null;
        }
    }
}


