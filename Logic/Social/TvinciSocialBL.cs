using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Web.Script.Serialization;
using DAL;
using ApiObjects;
using Tvinci.Core.DAL;
using TVinciShared;
using CachingManager;
using ApiObjects.MediaMarks;
using EpgBL;
using DalCB;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects.Social;
using Core.Users;
using Core.Catalog.Response;
using Core.Catalog.Request;
using Core.Catalog;

namespace Core.Social
{
    public class TvinciSocialBL : BaseSocialBL
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        SocialDAL m_oSocialSQL;
        DalCB.SocialDAL_Couchbase m_oSocialCouchbase;

        public TvinciSocialBL(int nGroupID)
            : base(nGroupID)
        {
            m_oSocialCouchbase = new SocialDAL_Couchbase(m_nGroupID);
            m_oSocialSQL = new SocialDAL(m_nGroupID);
        }

        public override string GetGroupFBNamespace()
        {
            string sRes = string.Empty;

            DataTable dt = m_oSocialSQL.GetGroupFBNamespace();

            if (dt != null && dt.DefaultView.Count > 0)
            {

                sRes = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "fb_namespace");
            }

            return sRes;
        }

        public override bool InsertUserSocialAction(SocialActivityDoc oSocialDoc, out string sDBRecordID)
        {
            sDBRecordID = string.Empty;

            if (oSocialDoc == null)
            {
                return false;
            }

            #region Couchbase
            eSocialActionPrivacy eInternalPrivacy = GetUserInternalActionShare(oSocialDoc.DocOwnerSiteGuid, (ApiObjects.SocialPlatform)oSocialDoc.SocialPlatform, (eUserAction)oSocialDoc.ActivityVerb.ActionType);
            string id = Utils.CreateSocialActionId(oSocialDoc.DocOwnerSiteGuid, oSocialDoc.SocialPlatform, oSocialDoc.ActivityVerb.ActionType, oSocialDoc.ActivityObject.AssetID, (int)oSocialDoc.ActivityObject.AssetType);
            oSocialDoc.id = id;
            oSocialDoc.CreateDate = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
            oSocialDoc.LastUpdate = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
            oSocialDoc.PermitSharing = (eInternalPrivacy == eSocialActionPrivacy.DONT_ALLOW) ? false : true;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(oSocialDoc);

            bool bCouchRes = false;

            if (!string.IsNullOrEmpty(json))
            {
                bCouchRes = m_oSocialCouchbase.InsertUserSocialAction(id, oSocialDoc);
                if (bCouchRes)
                {
                    sDBRecordID = id;

                    if (oSocialDoc.PermitSharing)
                    {
                        string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskSocialFeed");
                        string routingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeySocialFeedUpdate");
                        Guid guid = Guid.NewGuid();

                        ApiObjects.BaseCeleryData data = new ApiObjects.BaseCeleryData(guid.ToString(), task, m_nGroupID.ToString(), oSocialDoc.ActivitySubject.ActorSiteGuid, id);
                        QueueWrapper.BaseQueue queue = new QueueWrapper.Queues.QueueObjects.SocialQueue();
                        bool bIsUpdateIndexSucceeded = queue.Enqueue(data, string.Concat(routingKey, "\\", m_nGroupID));

                        if (!bIsUpdateIndexSucceeded)
                        {
                            log.Error("Error - " + string.Format("Failed to in queue user feed. SiteGuid={0};ObjectID={1};actionID={2};action={3};DBActionID={4}", oSocialDoc.DocOwnerSiteGuid, oSocialDoc.ActivityObject.ObjectID, oSocialDoc.ActivityVerb.SocialActionID, oSocialDoc.ActivityVerb.ActionName, sDBRecordID));
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Unable to insert record into couchbase. json={0}", json));
                }
            }

            #endregion


            return bCouchRes;
        }

        public override bool DeleteUserSocialAction(string sSiteGuid, int nAssetID, eAssetType assetType, eUserAction userAction, ApiObjects.SocialPlatform eSocialPlatform)
        {
            string sDBRecordID = string.Empty;
            string id = Utils.CreateSocialActionId(sSiteGuid, (int)eSocialPlatform, (int)userAction, nAssetID, (int)assetType);
            SocialActivityDoc doc;
            bool bSuccess = m_oSocialCouchbase.GetUserSocialAction(id, out doc);

            if (bSuccess && doc != null)
            {
                doc.IsActive = false;
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                bSuccess = m_oSocialCouchbase.InsertUserSocialAction(id, doc);

                if (bSuccess)
                {
                    sDBRecordID = id;

                    if (doc.PermitSharing)
                    {
                        string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskSocialFeed");
                        string routingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeySocialFeedUpdate");
                        Guid guid = Guid.NewGuid();

                        ApiObjects.BaseCeleryData data = new ApiObjects.BaseCeleryData(guid.ToString(), task, m_nGroupID.ToString(), doc.ActivitySubject.ActorSiteGuid, id);
                        QueueWrapper.BaseQueue queue = new QueueWrapper.Queues.QueueObjects.SocialQueue();
                        bool bIsUpdateIndexSucceeded = queue.Enqueue(data, string.Concat(routingKey, "\\", m_nGroupID));

                        if (!bIsUpdateIndexSucceeded)
                        {
                            log.Error("Error - " + string.Format("Failed to enqueue user feed. SiteGuid={0};ObjectID={1};actionID={2};action={3};DBActionID={4}", doc.DocOwnerSiteGuid, doc.ActivityObject.ObjectID, doc.ActivityVerb.SocialActionID, doc.ActivityVerb.ActionName, sDBRecordID));
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Unable to delete record from couchbase. json={0}", json));
                }
            }

            return bSuccess;
        }

        public override bool UpdateUserActivityFeed(List<string> lOwnerSiteGuids, string sNewActivityID)
        {
            SocialActivityDoc doc;
            bool bSuccess = m_oSocialCouchbase.GetUserSocialAction(sNewActivityID, out doc);

            if (bSuccess && doc != null)
            {
                //if document is active then update all friends
                if (doc.IsActive)
                {
                    string sOldDocID = doc.id;
                    doc.DocType = "user_feed";

                    UpdateDocAssetDetails(ref doc);
                    UpdateDocActorDetails(ref doc);

                    foreach (string sOwnerSiteGuid in lOwnerSiteGuids)
                    {
                        doc.id = string.Format("{0}::{1}", sOwnerSiteGuid, sOldDocID);

                        doc.DocOwnerSiteGuid = sOwnerSiteGuid;
                        bSuccess &= m_oSocialCouchbase.InsertUserSocialAction(doc.id, doc);
                        log.DebugFormat("{0} friend social action. docId = {1}", bSuccess ? "Successfully inserted" : "Failed to insert", doc.id);
                    }
                }
                //document is not active, delete from all user's friends feed
                else
                {
                    string docID;
                    foreach (string sOwnerSiteGuid in lOwnerSiteGuids)
                    {
                        docID = string.Concat(sOwnerSiteGuid, "::", doc.id);
                        bSuccess &= m_oSocialCouchbase.DeleteUserSocialAction(docID);
                        log.DebugFormat("{0} friend social action. docId = {1}", bSuccess ? "Successfully deleted" : "Failed to delete", doc.id);
                    }
                }
            }

            return bSuccess;
        }

        public override bool InsertFriendsActivitiesToUserActivityFeed(string sSiteGuiid, List<SocialActivityDoc> lFriendsActivities)
        {
            SocialActivityDoc doc;
            bool bSuccess = true;

            for (int i = 0; i < lFriendsActivities.Count; i++)
            {
                doc = lFriendsActivities[i];

                if (doc != null && doc.IsActive)
                {
                    string sOldDocID = doc.id;
                    doc.DocType = "user_feed";

                    UpdateDocAssetDetails(ref doc);
                    UpdateDocActorDetails(ref doc);

                    doc.id = string.Format("{0}::{1}", sSiteGuiid, sOldDocID);
                    doc.DocOwnerSiteGuid = sSiteGuiid;
                    bSuccess &= m_oSocialCouchbase.InsertUserSocialAction(doc.id, doc);
                }
            }

            return bSuccess;
        }

        public override bool GetFeedIDsByActorID(string sActorSiteGuid, int nNumOfDocs, out List<string> lDocIDs)
        {
            bool bResult = m_oSocialCouchbase.GetFeedsByActorID(sActorSiteGuid, nNumOfDocs, out lDocIDs);

            return bResult;
        }

        public override bool DeleteActivityFromUserFeed(string sFeedDocID)
        {
            return m_oSocialCouchbase.DeleteUserSocialAction(sFeedDocID);
        }

        public override bool GetUserActivityFeed(string sSiteGuid, int nNumOfRecords, int nStartIndex, string sPicDimensions, out List<SocialActivityDoc> lResult)
        {
            bool bResult = m_oSocialCouchbase.GetUserSocialFeed(sSiteGuid, nStartIndex, nNumOfRecords, out lResult);

            if (lResult != null && lResult.Count > 0)
            {
                for (int i = 0; i < lResult.Count; i++)
                {
                    if (lResult[i] == null)
                    {
                        lResult.RemoveAt(i);
                    }
                    else
                    {
                        lResult[i].ActivityObject.PicUrl = Utils.ChangePicUrlDim(lResult[i].ActivityObject.PicUrl, sPicDimensions);
                    }
                }
            }

            return bResult;
        }

        public override bool GetUserActivityFeedIds(string sSiteGuid, int nNumOfRecords, int nStartIndex, out List<string> lResult)
        {
            List<SocialActivityDoc> lRetval;
            bool bResult = m_oSocialCouchbase.GetUserSocialFeed(sSiteGuid, nStartIndex, nNumOfRecords, out lRetval);

            if (bResult && lRetval != null && lRetval.Count > 0)
            {
                lResult = lRetval.Select(item => item.id).ToList();
            }
            else
            {
                lResult = new List<string>();
            }

            return bResult;
        }

        public override bool GetUserSocialAction(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eAssetType assetType, eUserAction eUserAction, int nAssetID, out SocialActivityDoc oSocialActionDoc)
        {
            oSocialActionDoc = null;
            List<string> idList = new List<string>();

            int nAssetType = (int)assetType;

            string id = Utils.CreateSocialActionId(sSiteGuid, (int)eSocialPlatform, (int)eUserAction, nAssetID, (int)assetType);

            bool bSuccess = m_oSocialCouchbase.GetUserSocialAction(id, out oSocialActionDoc);

            return bSuccess;
        }

        public override List<SocialActivityDoc> GetUserSocialAction(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eAssetType assetType, List<int> lSocialActions, List<int> lAssetIDs)
        {
            List<SocialActivityDoc> res;
            List<string> idList = new List<string>();

            int nAssetType = (int)assetType;

            string id;
            if ((lAssetIDs != null && lAssetIDs.Count > 0) && !lAssetIDs.Contains(0))
            {
                foreach (int nAction in lSocialActions)
                {
                    foreach (int nAssetID in lAssetIDs)
                    {
                        id = Utils.CreateSocialActionId(sSiteGuid, (int)eSocialPlatform, nAction, nAssetID, nAssetType);
                        idList.Add(id);
                    }
                }
                res = m_oSocialCouchbase.GetUserSocialAction(idList);
            }
            else
            {
                res = m_oSocialCouchbase.GetUserSocialAction(lSocialActions, sSiteGuid, (int)eSocialPlatform);
            }

            if (res != null && res.Count > 0)
            {
                res = res.Where(x => x.IsActive == true).ToList<SocialActivityDoc>();
            }

            return res;

        }

        public override bool GetUserSocialAction(string sSiteGuid, int nNumOfRecords, int nSkip, out List<SocialActivityDoc> lActions)
        {
            bool bResult = m_oSocialCouchbase.GetUserSocialAction(sSiteGuid, nNumOfRecords, nSkip, out lActions);

            return bResult;
        }

        public override List<SocialActivityDoc> GetFriendsSocialActions(List<string> lFriendIds, ApiObjects.SocialPlatform eSocialPlatform, eAssetType assetType, List<int> lSocialActions, List<int> lAssetIDs, string userId)
        {
            List<string> idList = new List<string>();

            int nAssetType = (int)assetType;

            string id;

            foreach (var sFriendId in lFriendIds)
            {
                foreach (int nAssetID in lAssetIDs)
                {
                    foreach (int nAction in lSocialActions)
                    {
                        id = Utils.CreateSocialFriendActionId(userId ,sFriendId, (int)eSocialPlatform, nAction, nAssetID, nAssetType);

                        idList.Add(id);
                    }
                }
            }

            List<SocialActivityDoc> doc = m_oSocialCouchbase.GetUserSocialAction(idList);

            log.Debug("return from CB  " + string.Format("doc count = {0}", doc != null ? doc.Count : 0));
            return doc;
        }

        public override List<FBUser> GetFBFriendsFromDB(List<long> lFBFriendList)
        {
            List<FBUser> lRes = new List<FBUser>();

            DataTable dt = m_oSocialSQL.GetFBFriends(lFBFriendList);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                lRes = dt.AsEnumerable()
                    .Select(user => new FBUser()
                    {
                        m_sSiteGuid = ODBCWrapper.Utils.GetSafeStr(user, "id"),
                        uid = ODBCWrapper.Utils.GetSafeStr(user, "facebook_id"),
                        first_name = ODBCWrapper.Utils.GetSafeStr(user, "first_name"),
                        last_name = ODBCWrapper.Utils.GetSafeStr(user, "last_name"),
                        email = ODBCWrapper.Utils.GetSafeStr(user, "email_add"),
                        name = ODBCWrapper.Utils.GetSafeStr(user, "username")
                    }
                    ).ToList();
            }

            return lRes;
        }

        public override string GetMediaFBObjectID(int nMediaID)
        {
            string oRes = string.Empty;

            DataTable dt = m_oSocialSQL.GetMediaFBObjectID(nMediaID);

            if (dt != null && dt.DefaultView.Count > 0)
            {

                oRes = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "FB_OBJECT_ID");
            }

            return oRes;
        }

        public override string GetProgramFBObjectID(int nProgID)
        {
            string oRes = string.Empty;

            DataTable dt = EpgDal.GetEpgProgramInfo(m_nGroupID, nProgID);

            if (dt != null && dt.DefaultView.Count > 0)
            {

                oRes = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "FB_OBJECT_ID");
            }

            return oRes;
        }

        public override ApiObjects.EPGChannelProgrammeObject GetProgramInfo(int nProgID)
        {
            ApiObjects.EPGChannelProgrammeObject result = null;

            DataTable dtProgInfo = EpgDal.GetEpgProgramInfo(m_nGroupID, nProgID);

            if (dtProgInfo != null && dtProgInfo.Rows.Count > 0)
            {
                DataTable dtTags = EpgDal.GetEpgProgramTags(m_nGroupID, nProgID);
                result = createProgramObject(dtProgInfo.Rows[0], dtTags);
            }

            return result;
        }

        public override int GetAssetMediaID(int nAssetID, eAssetType assetType)
        {
            int nRes;

            switch (assetType)
            {
                case eAssetType.PROGRAM:
                    nRes = GetProgramMediaID(nAssetID);
                    break;
                case eAssetType.MEDIA:
                    nRes = nAssetID;
                    break;
                default:
                    nRes = 0;
                    break;
            }

            return nRes;
        }

        public override int GetProgramMediaID(int nProgID)
        {
            int nRes = 0;

            DataTable dt = EpgDal.GetEpgProgramInfo(m_nGroupID, nProgID);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nRes = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "MEDIA_ID");
            }

            return nRes;
        }

        public override string GetMediaName(int nProgID)
        {
            string sRes = string.Empty;

            DataTable dt = EpgDal.GetEpgProgramInfo(m_nGroupID, nProgID);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                sRes = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "channel_name");
            }

            return sRes;
        }

        private ApiObjects.EPGChannelProgrammeObject createProgramObject(DataRow drProgramInfo, DataTable dtProgramTags)
        {
            long program_id = ODBCWrapper.Utils.GetLongSafeVal(drProgramInfo, "id");
            string EPG_CHANNEL_ID = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "EPG_CHANNEL_ID");
            string EPG_IDENTIFIER = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "EPG_IDENTIFIER");
            string NAME = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "NAME");
            string DESCRIPTION = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "DESCRIPTION");
            DateTime oStartDate = ODBCWrapper.Utils.GetDateSafeVal(drProgramInfo, "START_DATE");
            string START_DATE = oStartDate.ToString("dd/MM/yyyy HH:mm:ss");
            DateTime oEndtDate = ODBCWrapper.Utils.GetDateSafeVal(drProgramInfo, "END_DATE");
            string END_DATE = oEndtDate.ToString("dd/MM/yyyy HH:mm:ss");
            long pic_id = ODBCWrapper.Utils.GetLongSafeVal(drProgramInfo, "PIC_ID");
            string sPic_base_url = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "pic_base_url");
            string sPic_remote_base_url = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "pics_remote_base_url");
            string PIC_URL = PageUtils.GetPicURL(pic_id, sPic_base_url, sPic_remote_base_url, "");
            string STATUS = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "STATUS");
            string IS_ACTIVE = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "IS_ACTIVE");
            string GROUP_ID = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "GROUP_ID");
            string UPDATER_ID = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "UPDATER_ID");
            string UPDATE_DATE = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "UPDATE_DATE");
            string PUBLISH_DATE = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "PUBLISH_DATE");
            string CREATE_DATE = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "CREATE_DATE");
            string MEDIA_ID = ODBCWrapper.Utils.GetSafeStr(drProgramInfo, "MEDIA_ID");

            List<ApiObjects.EPGDictionary> EPG_ResponseTag = new List<ApiObjects.EPGDictionary>();

            if (dtProgramTags != null)
            {
                foreach (DataRowView drTag in dtProgramTags.DefaultView)
                {
                    ApiObjects.EPGDictionary tagItem = new ApiObjects.EPGDictionary();
                    tagItem.Key = ODBCWrapper.Utils.GetSafeStr(drTag, "Name");
                    tagItem.Value = ODBCWrapper.Utils.GetSafeStr(drTag, "Value");
                    EPG_ResponseTag.Add(tagItem);
                }
            }

            ApiObjects.EPGChannelProgrammeObject item = new ApiObjects.EPGChannelProgrammeObject();
            item.Initialize(program_id, EPG_CHANNEL_ID, EPG_IDENTIFIER, NAME, DESCRIPTION, START_DATE, END_DATE, PIC_URL, STATUS, IS_ACTIVE, GROUP_ID, UPDATER_ID, UPDATE_DATE, PUBLISH_DATE, CREATE_DATE, EPG_ResponseTag, null, MEDIA_ID, 0);
            return item;

        }

        public override FacebookConfig GetFBConfig(string sConnStr = "")
        {
            FacebookConfig fbConfig = null;

            DataTable dt = m_oSocialSQL.GetFBConfig(sConnStr);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                fbConfig = dt.AsEnumerable()
                    .Select(config => new FacebookConfig()
                    {
                        sFBKey = ODBCWrapper.Utils.GetSafeStr(config, "fb_app_id"),
                        sFBSecret = ODBCWrapper.Utils.GetSafeStr(config, "fb_app_secret"),
                        sFBCallback = ODBCWrapper.Utils.GetSafeStr(config, "fb_callback"),
                        sFBPermissions = ODBCWrapper.Utils.GetSafeStr(config, "fb_permissions"),
                        sFBRedirect = ODBCWrapper.Utils.GetSafeStr(config, "fb_redirect"),
                        nFBMinFriends = ODBCWrapper.Utils.GetIntSafeVal(config, "fb_min_friends"),
                        sFBToken = ODBCWrapper.Utils.GetSafeStr(config, "fb_app_token")
                    }
                    ).First();
            }

            return fbConfig;
        }

        public override string GetMediaLinkPostParameters(int nMediaID, ref Dictionary<string, string> dParams)
        {
            string sLink = string.Empty;
            string sParams = string.Empty;

            DataTable dt = m_oSocialSQL.GetMediaLinks(nMediaID, 1, 1);

            if (dt != null)
            {
                int nCount = dt.DefaultView.Count;

                if (nCount > 0)
                {
                    int nMediaGroupID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "group_id");
                    string sName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "name");
                    string sDesc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "description");
                    string sBaseUrl = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "base_url");

                    string sBasePicsURL = PageUtils.GetBasePicURL(nMediaGroupID);
                    sBasePicsURL += ImageUtils.GetTNName(sBaseUrl, "tn");

                    AddParameter("name", sName, ref sParams);
                    AddParameter("description", sDesc, ref sParams);
                    AddParameter("picture", sBasePicsURL, ref sParams);

                    sLink = (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "fb_redirect"))) ? ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "fb_redirect") : "www.tvinci.com";

                    AddParameter("caption", sLink, ref sParams);
                    AddParameter("link", sLink, ref sParams);

                    dParams.Add("name", sName);
                    dParams.Add("description", sDesc);
                    dParams.Add("picture", sBasePicsURL);
                    dParams.Add("caption", sLink);
                    dParams.Add("link", sLink);
                }
            }

            return sParams;
        }

        public override bool IncrementAssetLikeCounter(int nAssetID, eAssetType assetType)
        {
            bool bRes = false;

            switch (assetType)
            {
                case eAssetType.PROGRAM:
                    bRes = IncrememntProgramLikeCounter(nAssetID);
                    break;
                case eAssetType.MEDIA:
                    bRes = m_oSocialSQL.IncrementMediaLikeCounter(nAssetID) == 0 ? true : false;
                    break;
                default:
                    break;
            }

            return bRes;
        }

        private bool IncrememntProgramLikeCounter(int nAssetId)
        {
            bool bResult = false;
            m_oSocialSQL.IncrementProgramLikeCounter(nAssetId);
            TvinciEpgBL tvinciBl = new TvinciEpgBL(m_nGroupID);

            ulong cas;
            int numOfRetries = 5;
            string sAssetId = nAssetId.ToString();
            for (int i = 0; i < numOfRetries && !bResult; i++)
            {
                EpgCB epg = tvinciBl.GetEpgCB((ulong)nAssetId, out cas);

                if (epg != null && cas > 0)
                {
                    epg.Statistics.Likes++;

                    bResult = tvinciBl.UpdateEpg(epg, cas);

                }
            }

            return bResult;
        }

        public override bool DecrementAssetLikeCounter(int nAssetID, eAssetType assetType)
        {
            bool bRes = false;

            switch (assetType)
            {
                case eAssetType.PROGRAM:
                    bRes = DecrementProgramLikeCounter(nAssetID);
                    break;
                case eAssetType.MEDIA:
                    bRes = m_oSocialSQL.DecrementMediaLikeCounter(nAssetID) == 0 ? true : false;
                    break;
                default:
                    break;
            }

            return bRes;
        }

        private bool DecrementProgramLikeCounter(int nAssetId)
        {
            bool bResult = false;
            m_oSocialSQL.DecrementProgramLikeCounter(nAssetId);
            EpgBL.TvinciEpgBL tvinciBl = new EpgBL.TvinciEpgBL(m_nGroupID);

            ulong cas;
            string sAssetId = nAssetId.ToString();

            int numOfRetries = 5;
            for (int i = 0; i < numOfRetries && !bResult; i++)
            {
                EpgCB epg = tvinciBl.GetEpgCB((ulong)nAssetId, out cas);

                if (epg != null && cas > 0)
                {
                    if (epg.Statistics.Likes > 0)
                    {
                        epg.Statistics.Likes--;
                        bResult = tvinciBl.UpdateEpg(epg, cas);
                    }
                    else
                    {
                        bResult = true;
                    }
                }
            }

            return bResult;
        }

        public override bool RateAsset(string sSiteGuid, int nAssetID, eAssetType assetType, int nRateVal)
        {
            RateMediaObject oRateMedia = Core.Api.Module.RateMedia(m_nGroupID, nAssetID, sSiteGuid, nRateVal);

            return (oRateMedia == null) ? false : true;
        }

        public override bool SetAssetFBObjectID(int nAssetID, eAssetType assetType, string sAssetFBObjectID)
        {
            bool bRes = false;

            if (assetType == eAssetType.UNKNOWN || string.IsNullOrEmpty(sAssetFBObjectID))
                return bRes;

            if (assetType == eAssetType.PROGRAM)
            {
                //set fb object id in epg_channel_schedule
                bRes = (m_oSocialSQL.SetProgramFBObjectID(nAssetID, sAssetFBObjectID) == 0) ? true : false;
            }
            else if (assetType == eAssetType.MEDIA)
            {
                //set fb object id in media
                bRes = (m_oSocialSQL.SetMediaFBObjectID(nAssetID, sAssetFBObjectID) == 0) ? true : false;
            }

            return bRes;
        }

        private void AddParameter(string name, string val, ref string parameters)
        {
            if (!string.IsNullOrEmpty(parameters))
            {
                parameters += "&";
            }

            parameters += name + "=" + val;
        }

        public override void GetFBObjectID(int nAssetID, eAssetType assetType, ref string sObjectID)
        {
            switch (assetType)
            {
                case eAssetType.MEDIA:
                    sObjectID = GetMediaFBObjectID(nAssetID);
                    break;
                case eAssetType.PROGRAM:
                    sObjectID = GetProgramFBObjectID(nAssetID);
                    break;
                default:
                    break;
            }
        }

        public override bool RemoveUser(int nGroupID, string sSiteGuid)
        {
            return (m_oSocialSQL.UpdateUserStatus(int.Parse(sSiteGuid), 2, 2) == 0) ? true : false;
        }

        public override string[] GetUsersLikedMedia(int nUserGUID, int nMediaID, int nPlatform, int nStartIndex, int nNumberOfItems)
        {
            List<SocialActivityDoc> lstLikes = m_oSocialCouchbase.GetUsersActionedMedia(
                nMediaID, nUserGUID,
                (int)ApiObjects.eUserAction.LIKE, nPlatform,
                nNumberOfItems, nStartIndex);

            // Convert the class-array to a string array, extracting the matching property
            string[] arrUsersLikedMedia = lstLikes.Select(doc => doc.DocOwnerSiteGuid).ToArray();

            return arrUsersLikedMedia;
        }

        public override List<ApiObjects.KeyValuePair> GetFriendsWatchCount(List<int> lFriendsSiteGuid, int nLimit)
        {
            List<ApiObjects.KeyValuePair> lResults = new List<ApiObjects.KeyValuePair>();

            Dictionary<string, int> dictMediaUsersCount = m_oSocialSQL.GetFriendsWatchedCount(lFriendsSiteGuid);

            if (dictMediaUsersCount != null && dictMediaUsersCount.Keys.Count > 0)
            {
                List<KeyValuePair<string, int>> SortedList = dictMediaUsersCount.OrderByDescending(x => x.Value).ToList();
                foreach (KeyValuePair<string, int> pair in SortedList)
                {
                    string sUsersCount = pair.Value.ToString();
                    lResults.Add(new ApiObjects.KeyValuePair(pair.Key.ToString(), sUsersCount));
                }
            }
            return (nLimit > 0) ? lResults.GetRange(0, nLimit) : lResults;
        }

        public override List<string> GetFriendsWhoWatchedMedia(int nMediaID, List<int> lFriendsGuid, int nLimit)
        {
            List<string> lResults = new List<string>();

            List<int> lGroups = TVinciShared.PageUtils.GetGroupListByParent(m_nGroupID);

            if (lGroups == null || lGroups.Count <= 0)
            {
                return lResults;
            }

            List<MediaMarkLog> mediaMarksList = m_oSocialSQL.GetFriendsWhoWatchedMedia(nMediaID, lFriendsGuid);


            if (mediaMarksList != null && mediaMarksList.Count > 0)
            {
                lResults = mediaMarksList.Select(x => x.LastMark.UserID.ToString()).Distinct().ToList();
            }

            return (nLimit > 0) ? lResults.GetRange(0, nLimit) : lResults;
        }

        public override eSocialPrivacy GetUserSocialPrivacy(int nSiteGUID, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            eSocialPrivacy ePrivacy = eSocialPrivacy.UNKNOWN;
            DataTable dt = m_oSocialSQL.GetUserSocialPrivacy(nSiteGUID, (int)eSocialPlatform, (int)eAction);

            if (dt != null && dt.DefaultView.Count > 0)
            {
                int nRetval = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "EXTERNAL_PRIVACY");
                ePrivacy = (eSocialPrivacy)nRetval;
            }

            return ePrivacy;
        }

        public override bool SetUserSocialPrivacy(int nSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction, eSocialPrivacy eNewPrivacy)
        {
            bool bRes = false;

            if (eNewPrivacy == eSocialPrivacy.UNKNOWN || eSocialPlatform == ApiObjects.SocialPlatform.UNKNOWN)
                return bRes;

            eSocialPrivacy eCurrentPrivacy = GetUserSocialPrivacy(nSiteGuid, eSocialPlatform, eAction);

            //Currently no privacy is defined in DB or  value in DB is different than new privacy --> insert/update
            if (eCurrentPrivacy == eSocialPrivacy.UNKNOWN || eCurrentPrivacy != eNewPrivacy)
            {
                bRes = (m_oSocialSQL.SetUserSocialPrivacy(nSiteGuid, (int)eSocialPlatform, (int)eAction, (int)eNewPrivacy) == 0) ? true : false;

            }
            else
            {
                bRes = true;
            }

            return bRes;
        }

        public override eSocialActionPrivacy GetUserExternalActionShare(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            //default value for external action = DONT_ALLOW
            eSocialActionPrivacy eAllow = eSocialActionPrivacy.DONT_ALLOW;
            int nSiteGuid;
            if (int.TryParse(sSiteGuid, out nSiteGuid))
            {
                DataTable dt = m_oSocialSQL.GetUserSocialPrivacy(nSiteGuid, (int)eSocialPlatform, (int)eAction);

                if (dt != null && dt.DefaultView.Count > 0)
                {
                    int nRetval = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "EXTERNAL_SHARE");
                    eAllow = (eSocialActionPrivacy)nRetval;
                }
            }

            return eAllow;
        }

        public override eSocialActionPrivacy GetUserInternalActionShare(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction)
        {
            //default value for internal action = ALLOW
            eSocialActionPrivacy eAllow = eSocialActionPrivacy.ALLOW;
            int nSiteGuid;
            if (int.TryParse(sSiteGuid, out nSiteGuid))
            {
                DataTable dt = m_oSocialSQL.GetUserSocialPrivacy(nSiteGuid, (int)eSocialPlatform, (int)eAction);

                if (dt != null && dt.DefaultView.Count > 0)
                {
                    int nRetval = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "INTERNAL_SHARE");
                    eAllow = (eSocialActionPrivacy)nRetval;
                }
            }

            return eAllow;

        }

        public override bool SetUserInternalActionShare(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy ePrivacy)
        {
            bool bRes = false;

            if (ePrivacy != eSocialActionPrivacy.UNKNOWN && eAction != eUserAction.UNKNOWN && eSocialPlatform != ApiObjects.SocialPlatform.UNKNOWN)
            {
                bRes = (m_oSocialSQL.SetUserInternalActionPrivacy(sSiteGuid, (int)eSocialPlatform, (int)eAction, (int)ePrivacy) == 0) ? true : false;
            }

            return bRes;
        }

        public override bool SetUserExternalActionShare(string sSiteGuid, ApiObjects.SocialPlatform eSocialPlatform, eUserAction eAction, eSocialActionPrivacy ePrivacy)
        {
            bool bRes = false;

            if (ePrivacy != eSocialActionPrivacy.UNKNOWN && eAction != eUserAction.UNKNOWN && eSocialPlatform != ApiObjects.SocialPlatform.UNKNOWN)
            {
                bRes = (m_oSocialSQL.SetUserExternalActionShare(sSiteGuid, (int)eSocialPlatform, (int)eAction, (int)ePrivacy) == 0) ? true : false;
            }

            return bRes;
        }       

        public override List<FriendWatchedObject> GetAllFriendsWatchedMedia(int nMediaID, List<string> lFriendsSiteGuid)
        {
            List<FriendWatchedObject> lRes = new List<FriendWatchedObject>();
            List<MediaMarkLog> mediaMarksList = m_oSocialSQL.GetAllFriendsWatchedMedia(Utils.EnumerableStringToInt(lFriendsSiteGuid), nMediaID);

            if (mediaMarksList != null && mediaMarksList.Count > 0)
            {
                lRes = mediaMarksList.AsEnumerable()
                    .Select(umm => new FriendWatchedObject()
                    {
                        MediaID = umm.LastMark.AssetID,
                        SiteGuid = umm.LastMark.UserID,
                        UpdateDate = umm.LastMark.CreatedAt
                    }
                    ).OrderByDescending(x => x.UpdateDate).ToList();
            }
            return lRes;
        }

        public override int GetAssetLikeCounter(int nAssetID, eAssetType assetType)
        {
            int nLikeCounter = 0;

            DataTable dt = null;

            if (assetType == eAssetType.PROGRAM)
            {
                dt = EpgDal.GetEpgProgramInfo(m_nGroupID, nAssetID);
            }
            else if (assetType == eAssetType.MEDIA)
            {
                dt = m_oSocialSQL.GetMediaLikeCounter(nAssetID);
            }

            if (dt != null && dt.DefaultView.Count > 0)
            {
                nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "like_counter");
            }

            return nLikeCounter;
        }

        private List<SocialActionDoc> socialActionDatatableToActionDoc(DataTable dt)
        {
            List<SocialActionDoc> lSocialActions = new List<SocialActionDoc>();
            if (dt != null && dt.DefaultView.Count > 0)
            {
                lSocialActions = dt.AsEnumerable().Select(umm => new SocialActionDoc()
                {
                    ActionType = ODBCWrapper.Utils.GetIntSafeVal(umm, "social_action"),
                    FBActionID = ODBCWrapper.Utils.GetSafeStr(umm, "fb_action_id"),
                    id = ODBCWrapper.Utils.GetSafeStr(umm, "id"),
                    SiteGuid = ODBCWrapper.Utils.GetSafeStr(umm, "user_site_guid"),
                    GroupID = ODBCWrapper.Utils.GetIntSafeVal(umm, "group_id"),
                    SocialPlatform = ODBCWrapper.Utils.GetIntSafeVal(umm, "social_platform"),
                    CreateDate = DateUtils.DateTimeToUnixTimestamp(ODBCWrapper.Utils.GetDateSafeVal(umm, "create_date")),
                    LastUpdate = DateUtils.DateTimeToUnixTimestamp(ODBCWrapper.Utils.GetDateSafeVal(umm, "update_date")),
                    MediaID = ODBCWrapper.Utils.GetIntSafeVal(umm, "media_id"),
                    AssetType = (eAssetType)ODBCWrapper.Utils.GetIntSafeVal(umm, "asset_type"),
                    nProgramID = ODBCWrapper.Utils.GetIntSafeVal(umm, "program_id"),
                    nRateValue = ODBCWrapper.Utils.GetIntSafeVal(umm, "rate_value")
                }
                ).ToList();
            }

            return lSocialActions;
        }

        protected void UpdateDocAssetDetails(ref SocialActivityDoc doc)
        {
            if (doc == null)
            {
                log.Debug("Info - " + string.Format("doc is null ={0} ", string.Empty));
                return;
            }
            if (doc.ActivityObject.AssetType == eAssetType.MEDIA)
            {
                MediaObj mediaObj = GetMediaDetails(doc.ActivityObject.AssetID);

                if (mediaObj != null)
                {
                    doc.ActivityObject.AssetName = mediaObj.m_sName;
                    doc.ActivityObject.PicUrl = string.Empty;
                    if (mediaObj.m_lPicture != null && mediaObj.m_lPicture.Count > 0 && mediaObj.m_lPicture[0] != null)
                    {
                        doc.ActivityObject.PicUrl = mediaObj.m_lPicture[0].m_sURL;
                    }
                }
            }
            else if (doc.ActivityObject.AssetType == eAssetType.PROGRAM)
            {
                ProgramObj programObj = GetEpgDetails(doc.ActivityObject.AssetID);
                if (programObj != null && programObj.m_oProgram != null)
                {
                    doc.ActivityObject.AssetName = programObj.m_oProgram.NAME;
                    doc.ActivityObject.PicUrl = programObj.m_oProgram.PIC_URL;
                }
            }
        }

        protected void UpdateDocActorDetails(ref SocialActivityDoc doc)
        {
            UserResponseObject retval = Utils.GetUserDataByID(doc.ActivitySubject.ActorSiteGuid, m_nGroupID);

            if (retval != null && retval.m_user != null && retval.m_user.m_oBasicData != null)
            {
                doc.ActivitySubject.ActorPicUrl = retval.m_user.m_oBasicData.m_sFacebookImage;
                doc.ActivitySubject.ActorTvinciUsername = string.Format("{0} {1}", retval.m_user.m_oBasicData.m_sFirstName, retval.m_user.m_oBasicData.m_sLastName);
            }
        }

        protected MediaObj GetMediaDetails(int nMediaID)
        {
            MediaObj oMedia = null;
            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = Utils.GetWSURL("CatalogSignatureKey");
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            Filter filter = new Filter();
            filter.m_bOnlyActiveMedia = false;
            filter.m_bUseFinalDate = false;
            filter.m_bUseStartDate = false;
            filter.m_nLanguage = 0; // consider as main language 
            filter.m_sDeviceId = "";
            filter.m_sPlatform = "";


            MediasProtocolRequest mediaProtocolRequest = new MediasProtocolRequest();
            mediaProtocolRequest.m_oFilter = filter;
            mediaProtocolRequest.m_nGroupID = m_nGroupID;

            mediaProtocolRequest.m_sSignature = sSignature;
            mediaProtocolRequest.m_sSignString = sSignString;
            mediaProtocolRequest.m_sUserIP = "";
            mediaProtocolRequest.m_lMediasIds = new List<int> { nMediaID };
            mediaProtocolRequest.m_nPageIndex = 0;
            mediaProtocolRequest.m_nPageSize = 0;

            MediaResponse oMediaResponse = null;

            try
            {
                oMediaResponse = mediaProtocolRequest.GetMediasByIDs(mediaProtocolRequest);
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Caught exception when calling GetMediasByIDs. nMediaID={0};Ex={1};Callstack={2}", nMediaID, ex.Message, ex.StackTrace), ex);

            }

            if (oMediaResponse != null && oMediaResponse.m_lObj != null && oMediaResponse.m_lObj.Count > 0)
            {
                oMedia = oMediaResponse.m_lObj[0] as MediaObj;
            }

            return oMedia;
        }

        protected ProgramObj GetEpgDetails(int nEpgID)
        {
            ProgramObj oRes = null;

            string sSignString = Guid.NewGuid().ToString();
            string sSignatureString = Utils.GetWSURL("CatalogSignatureKey");
            string sSignature = TVinciShared.WS_Utils.GetCatalogSignature(sSignString, sSignatureString);

            Filter filter = new Filter();
            filter.m_bOnlyActiveMedia = false;
            filter.m_bUseFinalDate = false;
            filter.m_bUseStartDate = false;
            filter.m_nLanguage = 2;
            filter.m_sDeviceId = "";
            filter.m_sPlatform = "";

            List<int> lEpgIDs = new List<int> { nEpgID };

            EpgProgramDetailsRequest request = new EpgProgramDetailsRequest()
            {
                m_nGroupID = this.m_nGroupID,
                m_oFilter = filter,
                m_lProgramsIds = lEpgIDs,
                m_sSignature = sSignature,
                m_sSignString = sSignString
            };

            EpgProgramResponse response = null;

            try
            {
                response = request.GetProgramsByIDs(request);
            }
            catch (Exception ex)
            {
                log.Debug("Error - " + string.Format("Caught exception when calling GetEpgDetails. nEpgID={0};Ex={1};Callstack={2}", nEpgID, ex.Message, ex.StackTrace), ex);
            }

            if (response != null && response.m_lObj != null && response.m_lObj.Count > 0)
            {
                oRes = response.m_lObj[0] as ProgramObj;
            }


            return oRes;
        }

        public override ApiObjects.Social.SocialPrivacySettingsResponse SetUserSocialPrivacySettings(string siteGUID, int groupID, ApiObjects.Social.SocialPrivacySettings settings)
        {
            ApiObjects.Social.SocialPrivacySettingsResponse response = new ApiObjects.Social.SocialPrivacySettingsResponse();
            try
            {
                bool res = false;
                response.settings = settings;

                if (settings != null)
                {                    
                    DataTable dt = Utils.InitSocialPrivacySettings();
                    FillSocialPrivacySettings(ref dt, siteGUID, groupID, settings);

                    if (dt != null)
                    {
                        res = (m_oSocialSQL.SetSocialPrivacySettings(dt) ? true : false);
                    }
                }
                if (res)
                {
                    ApiObjects.Response.Status status = response.Status;
                    // get current settings  - to return all to client
                    response.settings = UserSocialPrivacySettings(siteGUID, groupID, out status);
                    response.Status = status;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("SetUserSocialPrivacySettings: Error while set user privacy settings . siteGuid = {0}, settings= {1} ex = {2}",
                   siteGUID, settings.ToString(), ex);
                return response;
            }
            return response;
        }

        internal static void FillSocialPrivacySettings(ref DataTable dt, string siteGUID, int groupId, ApiObjects.Social.SocialPrivacySettings settings)
        {
            try
            {
                DataRow row;
                if (settings != null)
                {
                    if (settings.InternalPrivacy != null && settings.InternalPrivacy.Count > 0)
                    {
                        foreach (SocialActionPrivacy internalPrivacy in settings.InternalPrivacy)
                        {
                            row = dt.NewRow();
                            row["site_guid"] = siteGUID;
                            row["group_id"] = groupId;
                            row["action_id"] = (int)internalPrivacy.Action;
                            row["social_platform"] = 999;// this is internal platform
                            row["internal_share"] = 0;

                            if (internalPrivacy.Privacy == eSocialActionPrivacy.UNKNOWN && internalPrivacy.Action == eUserAction.WATCHES)
                            {
                                row["internal_share"] = (int)eSocialActionPrivacy.DONT_ALLOW;
                            }
                            else if (internalPrivacy.Privacy == eSocialActionPrivacy.UNKNOWN)
                            {
                                row["internal_share"] = (int)eSocialActionPrivacy.ALLOW;
                            }
                            else
                            {
                                row["internal_share"] = (int)internalPrivacy.Privacy;
                            }
                            row["external_share"] = 0;
                            row["external_privacy"] = 0;
                            dt.Rows.Add(row);
                        }
                    }
                  
                    if (settings.SocialNetworks != null && settings.SocialNetworks.Count > 0)
                    {
                        foreach (SocialNetwork network in settings.SocialNetworks)
                        {
                            foreach (SocialActionPrivacy externalPrivacy in network.SocialAction)
                            {
                                row = dt.NewRow();
                                row["site_guid"] = siteGUID;
                                row["group_id"] = groupId;
                                row["social_platform"] = (int)network.Network;
                                row["internal_share"] = 0;
                                row["external_privacy"] = (int)network.SocialPrivacy;
                                row["action_id"] = (int)externalPrivacy.Action;
                                if (externalPrivacy.Privacy == eSocialActionPrivacy.UNKNOWN)
                                {
                                    row["external_share"] = (int)eSocialActionPrivacy.DONT_ALLOW;
                                }
                                else
                                {
                                    row["external_share"] = (int)externalPrivacy.Privacy;
                                }
                                dt.Rows.Add(row);
                            }
                        }
                    }                   
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("FillSocialPrivacySettings faild ex={0}", ex);
                dt = null;
            }
        }


        private static void FillDeafultSocialActionPrivacy(ref DataTable dt, string siteGUID, int groupId, ApiObjects.Social.SocialPrivacySettings settings, List<eUserAction> missingActions, int network = 999)
        {
            DataRow row;

            if (missingActions != null && missingActions.Count > 0)
            {
                foreach (eUserAction action in missingActions)
                {
                    row = dt.NewRow();
                    row["site_guid"] = siteGUID;
                    row["group_id"] = groupId;
                    row["action_id"] = (int)action;
                    row["social_platform"] = network;
                    row["external_privacy"] = 0;
                    if (network == 999) // internal
                    {
                        row["internal_share"] = 0;
                        if (action == eUserAction.WATCHES)
                        {
                            row["internal_share"] = (int)eSocialActionPrivacy.DONT_ALLOW;
                        }
                        else
                        {
                            row["internal_share"] = (int)eSocialActionPrivacy.ALLOW;
                        }
                        row["external_share"] = 0;

                    }
                    else // external 
                    {
                        row["internal_share"] = 0;
                        row["external_share"] = (int)eSocialActionPrivacy.DONT_ALLOW;
                    }
                    dt.Rows.Add(row);
                }
            }
        }
        
        public override ApiObjects.Social.SocialPrivacySettingsResponse GetUserSocialPrivacySettings(string siteGUID, int groupID)
        {
            ApiObjects.Social.SocialPrivacySettingsResponse response = new SocialPrivacySettingsResponse();
            try
            {
                // get current settings  - to return all to client
                ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                response.settings = UserSocialPrivacySettings(siteGUID, groupID, out status);
                response.Status = status;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetUserSocialPrivacySettings: Error while get user privacy settings . siteGuid = {0} ex = {2}", siteGUID, ex);
                return response;
            }
            return response;
        }

        private SocialPrivacySettings UserSocialPrivacySettings(string siteGUID, int groupID, out ApiObjects.Response.Status status)
        {
            SocialPrivacySettings settings = new SocialPrivacySettings();
            status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
            List<eUserAction> actions = new List<eUserAction>() { eUserAction.LIKE, eUserAction.WATCHES, eUserAction.SHARE, eUserAction.RATES };
            try
            {
                DataTable dtSettings = m_oSocialSQL.GetSocialPrivacySettings(siteGUID, groupID);
                if (dtSettings == null || dtSettings.Rows == null || dtSettings.Rows.Count == 0)
                {
                    settings = SocialPrivacySettings.SetDefultPrivacySettings();
                }
                else
                {
                    int privacy = 0;
                    int socialPrivacy = 0;
                    //int network = 0;
                    int action = 0;
                    SocialActionPrivacy actionPrivecy;
                    DataRow[] drows = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") == 999).ToArray();
                    foreach (DataRow dr in drows)
                    {
                        actionPrivecy = new SocialActionPrivacy();
                        privacy = ODBCWrapper.Utils.GetIntSafeVal(dr, "internal_share");
                        action = ODBCWrapper.Utils.GetIntSafeVal(dr, "action_id");

                        if (Enum.IsDefined(typeof(eSocialActionPrivacy), privacy) && Enum.IsDefined(typeof(eUserAction), action))
                        {
                            actionPrivecy.Action = (eUserAction)action;
                            actionPrivecy.Privacy = (eSocialActionPrivacy)privacy;
                            settings.InternalPrivacy.Add(actionPrivecy);
                        }
                    }
                    // complete default privacy for actions 
                    List<eUserAction> missingActions = actions.Where(a => settings.InternalPrivacy.All(i => i.Action != a)).ToList();
                    if (missingActions != null && missingActions.Count() > 0)
                    {
                        settings.InternalPrivacy.AddRange(SocialPrivacySettings.SetDefaultInternalPrivacy(missingActions));
                    }
                    settings.InternalPrivacy = settings.InternalPrivacy.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();

                    List<int> networks = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") != 999 && x.Field<int>("social_platform") != 0).Select(x => x.Field<int>("social_platform")).Distinct().ToList();
                    SocialNetwork sn;
                    foreach (int network in networks)
                    {
                        drows = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") == network).ToArray();
                        sn = new SocialNetwork();

                        if (Enum.IsDefined(typeof(SocialPlatform), network))
                        {
                            sn.Network = (SocialPlatform)network;
                        }
                        socialPrivacy = ODBCWrapper.Utils.GetIntSafeVal(drows[0], "external_privacy");

                        if (Enum.IsDefined(typeof(eSocialPrivacy), socialPrivacy))
                        {
                            sn.SocialPrivacy = (eSocialPrivacy)socialPrivacy;
                        }
                        sn.SocialAction = new List<SocialActionPrivacy>();

                        foreach (DataRow dr in drows)
                        {
                            actionPrivecy = new SocialActionPrivacy();
                            privacy = ODBCWrapper.Utils.GetIntSafeVal(dr, "external_share");
                            action = ODBCWrapper.Utils.GetIntSafeVal(dr, "action_id");

                            if (Enum.IsDefined(typeof(eSocialActionPrivacy), privacy) && Enum.IsDefined(typeof(eUserAction), action))
                            {
                                actionPrivecy.Action = (eUserAction)action;
                                actionPrivecy.Privacy = (eSocialActionPrivacy)privacy;
                                if (sn.SocialAction == null)
                                {
                                    sn.SocialAction = new List<SocialActionPrivacy>();
                                }
                                sn.SocialAction.Add(actionPrivecy);
                            }
                        }

                        missingActions = actions.Where(a => sn.SocialAction.All(i => i.Action != a)).ToList();
                        if (missingActions != null && missingActions.Count() > 0)
                        {
                            sn.SocialAction.AddRange(SocialPrivacySettings.SetDefaultNetworkPlatformPrivacy(missingActions));
                        }
                        sn.SocialAction = sn.SocialAction.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();
                        settings.SocialNetworks.Add(sn);
                    }
                    if (networks == null || networks.Count() == 0)
                    {
                        settings.SocialNetworks.AddRange(SocialPrivacySettings.SetDefultNetworkPrivacySettings());
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UserSocialPrivacySettings: Error while get user privacy settings . siteGuid = {0} ex = {2}", siteGUID, ex);
                status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return null;
            }
            return settings;
        }

        public override ApiObjects.Response.Status InsertUserSocialAction(SocialActivityDoc oSocialDoc, SocialPrivacySettings privacySettings, out string sDBRecordID)
        {
            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            sDBRecordID = string.Empty;

            if (oSocialDoc == null)
            {
                status.Message = "social doc is empty";
                return status;
            }

            #region Couchbase
            string id = Utils.CreateSocialActionId(oSocialDoc.DocOwnerSiteGuid, oSocialDoc.SocialPlatform, oSocialDoc.ActivityVerb.ActionType, oSocialDoc.ActivityObject.AssetID, (int)oSocialDoc.ActivityObject.AssetType);
            oSocialDoc.id = id;
            oSocialDoc.CreateDate = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
            oSocialDoc.LastUpdate = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
            eSocialActionPrivacy socialActionPrivacy = privacySettings.InternalPrivacy.Where(x => (int)x.Action == oSocialDoc.ActivityVerb.ActionType).Select(x => x.Privacy).FirstOrDefault();
            oSocialDoc.PermitSharing = (socialActionPrivacy == eSocialActionPrivacy.DONT_ALLOW) ? false : true;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(oSocialDoc);

            bool bCouchRes = false;

            if (!string.IsNullOrEmpty(json))
            {
                bCouchRes = m_oSocialCouchbase.InsertUserSocialAction(id, oSocialDoc);
                if (bCouchRes)
                {
                    sDBRecordID = id;

                    if (oSocialDoc.PermitSharing)
                    {
                        string task = TVinciShared.WS_Utils.GetTcmConfigValue("taskSocialFeed");
                        string routingKey = TVinciShared.WS_Utils.GetTcmConfigValue("routingKeySocialFeedUpdate");
                        Guid guid = Guid.NewGuid();

                        ApiObjects.BaseCeleryData data = new ApiObjects.BaseCeleryData(guid.ToString(), task, m_nGroupID.ToString(), oSocialDoc.ActivitySubject.ActorSiteGuid, id);
                        QueueWrapper.BaseQueue queue = new QueueWrapper.Queues.QueueObjects.SocialQueue();
                        bool bIsUpdateIndexSucceeded = queue.Enqueue(data, string.Concat(routingKey, "\\", m_nGroupID));

                        if (!bIsUpdateIndexSucceeded)
                        {
                            log.Error("Error - " + string.Format("Failed to in queue user feed. SiteGuid={0};ObjectID={1};actionID={2};action={3};DBActionID={4}", oSocialDoc.DocOwnerSiteGuid, oSocialDoc.ActivityObject.ObjectID, oSocialDoc.ActivityVerb.SocialActionID, oSocialDoc.ActivityVerb.ActionName, sDBRecordID));
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Concat("Unable to insert record into couchbase. json={0}", json));
                }
            }

            #endregion
            status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            return status;
        }


        public override bool GetUserSocialAction(string id, out SocialActivityDoc oSocialActionDoc)
        {
            oSocialActionDoc = null;

            bool bSuccess = m_oSocialCouchbase.GetUserSocialAction(id, out oSocialActionDoc);

            return bSuccess;
        }
    }

    public class SocialActionDoc
    {
        public string id { get; set; }
        public string SiteGuid { get; set; }
        public int GroupID { get; set; }
        public int MediaID { get; set; }
        public string ObjectID { get; set; }
        public int ActionType { get; set; }
        public int SocialPlatform { get; set; }
        public string FBID { get; set; }
        public string FBActionID { get; set; }
        public eAssetType AssetType { get; set; }
        public int nProgramID { get; set; }
        public int nRateValue { get; set; }
        public string Type
        {
            get
            {
                return "action";
            }
        }

        public long CreateDate { get; set; }
        public long LastUpdate { get; set; }
    }
}
