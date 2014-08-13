using ApiObjects;
using Couchbase;
using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Couchbase.Extensions;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DalCB
{
    public class SocialDAL_Couchbase
    {
        private static readonly string sEndMaxValue = @"\uefff";
        private static readonly string LOGGER_FILENAME = "SocialDal";
        private static readonly string CB_FEED_DESGIN = Utils.GetValFromConfig("cb_feed_design");

        CouchbaseClient m_oClient;
        private int m_nGroupID;
        public SocialDAL_Couchbase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oClient = CouchbaseManager.CouchbaseManager.GetInstance(eCouchbaseBucket.SOCIAL);
        }

        public bool GetUserSocialFeed(string sSiteGuid, int nSkip, int nNumOfRecords, out List<SocialActivityDoc> lResult)
        {
            lResult = new List<SocialActivityDoc>();
            bool bResult = false;
            try
            {
                long epochTime = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                object[] startKey = new object[] { sSiteGuid, epochTime };
                object[] endKey = new object[] { sSiteGuid, 0 };


                var retval = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserFeed", true).StartKey(startKey).EndKey(endKey).Descending(true).Skip(nSkip).Limit(nNumOfRecords)
                                               : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserFeed", true).StartKey(startKey).EndKey(endKey).Descending(true);
                if (retval != null)
                {
                    lResult = retval.ToList();
                }
                bResult = true;
            }
            catch (Couchbase.Exceptions.ViewException vEx) { 
            }

            return bResult;
        }

        public bool GetUserSocialAction(string sSocialActionID, out SocialActivityDoc oRetval)
        {
            oRetval = null;
            bool bSuccess = false;
            try
            {
                oRetval = m_oClient.GetJson<SocialActivityDoc>(sSocialActionID);
                bSuccess = true;
            }
            catch (Exception ex)
            {
            }

            return bSuccess;
        }

        public List<SocialActivityDoc> GetUserSocialAction(List<string> lSocialActionIDs)
        {
            List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();
            try
            {

                IDictionary<string, object> dRetval = m_oClient.Get(lSocialActionIDs);

                if (dRetval != null && dRetval.Count > 0)
                {
                    string retObj;
                    SocialActivityDoc socialDoc;
                    foreach (string sKey in dRetval.Keys)
                    {
                        retObj = dRetval[sKey] as string;
                        if (!string.IsNullOrEmpty(retObj))
                        {
                            try
                            {
                                socialDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<SocialActivityDoc>(retObj);
                                if (socialDoc != null)
                                {
                                    lRes.Add(socialDoc);
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Logger.Log("Error", string.Format("Deserialization of SocialActivityDoc failed. str obj={0}, ex={1}, stack={2}", retObj, ex.Message, ex.StackTrace), LOGGER_FILENAME);
                            }
                            
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Caught exception in GetUserSocialAction for multiple objects. ex={0}; stack={1}", ex.Message, ex.StackTrace), LOGGER_FILENAME);
            }

            return lRes;
        }

        //returns user social actions by descending date
        public bool GetUserSocialAction(string sSiteGuid, int nNumOfRecords, int nSkip, out List<SocialActivityDoc> lUserActivities)
        {
            bool bResult = false;
            lUserActivities = new List<SocialActivityDoc>();
            try
            {
                long epochTime = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                object[] startKey = new object[] { sSiteGuid, epochTime };
                object[] endKey = new object[] { sSiteGuid, 0 };


                var retval = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActions", true).StartKey(startKey).EndKey(endKey).Descending(true).Skip(nSkip).Limit(nNumOfRecords)
                                               : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActions", true).StartKey(startKey).EndKey(endKey).Descending(true);
                if (retval != null)
                {
                    lUserActivities = retval.ToList();
                }

                bResult = true;
            }
            catch (Couchbase.Exceptions.ViewException vEx)
            {
            }

            return bResult;
        }

        public bool DeleteUserSocialAction(string sDocID)
        {
            bool bResult = false;
                try
                {
                    bResult = m_oClient.Remove(sDocID);
                }catch{}

            return bResult;
        }

        public bool GetFeedsByActorID(string sActorSiteGuid, int nNumOfDocs, out List<string> lDocIDs)
        {
            lDocIDs = new List<string>();
            bool bResult = false;
            try
            {
                var lFeeds = (nNumOfDocs > 0) ? m_oClient.GetView(CB_FEED_DESGIN, "FeedByActorId").Limit(nNumOfDocs) : m_oClient.GetView(CB_FEED_DESGIN, "FeedByActorId");
                bResult = true;

                if (lFeeds != null)
                {
                    foreach (var feed in lFeeds)
                    {
                        lDocIDs.Add(feed.ItemId);
                    }
                }
            }catch{}

            return bResult;

        }

        public bool InsertUserSocialAction(string sDocID, object oDoc)
        {
            bool bRes = false;

            try
            {
                bRes = m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Set, sDocID, oDoc);
            }
            catch (Exception ex) { }

            return bRes;
        }

        private static T Deserialize<T>(string sJson) where T : class
        {
            T res = null;
            try
            {
                res = JsonConvert.DeserializeObject<T>(sJson);
            }
            catch { }

            return res;
        }
    }

}
