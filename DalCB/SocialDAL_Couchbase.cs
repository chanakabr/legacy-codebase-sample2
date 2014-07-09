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

        public List<SocialActivityDoc> GetUserSocialFeed(string sSiteGuid, int nSkip, int nNumOfRecords)
        {
            List<SocialActivityDoc> lRes = null;

            try
            {
                long epochTime = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                object[] startKey = new object[] { sSiteGuid, epochTime };
                object[] endKey = new object[] { sSiteGuid, 0 };


                lRes = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserFeed", true).StartKey(startKey).EndKey(endKey).Descending(true).Skip(nSkip).Limit(nNumOfRecords).ToList()
                                               : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserFeed", true).StartKey(startKey).EndKey(endKey).Descending(true).ToList();
            }
            catch (Couchbase.Exceptions.ViewException vEx) { }

            return lRes;
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
                    object retObj;
                    SocialActivityDoc socialDoc;
                    foreach (string sKey in dRetval.Keys)
                    {
                        retObj = dRetval[sKey];
                        if (retObj != null)
                        {
                            socialDoc = retObj as SocialActivityDoc;

                            if (socialDoc != null)
                            {
                                lRes.Add(socialDoc);
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

        public bool DeleteUserSocialAction(string sDocID)
        {
            bool bResult = m_oClient.Remove(sDocID);

            return bResult;
        }

        //public List<SocialActivityDoc> GetUserSocialAction(int nNumOfRecords, string sSiteGuid, int nSocialPlatform, List<int> socialActions)
        //{
        //    List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();

        //    if (socialActions == null || socialActions.Count <= 0 || string.IsNullOrEmpty(sSiteGuid))
        //    {
        //        return lRes;
        //    }

        //    List<List<object>> lKeys = new List<List<object>>();

        //    foreach (int nAction in socialActions)
        //    {
        //        lKeys.Add(new List<object>() { nSocialPlatform, nAction, sSiteGuid });
        //    }

        //    try
        //    {
        //        lRes = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserAction", true).Keys(lKeys).Limit(nNumOfRecords).ToList()
        //                                           : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserAction", true).Keys(lKeys).ToList();
        //    }
        //    catch (Couchbase.Exceptions.ViewException vEx) { }

        //    return (lRes.Count <= nNumOfRecords || nNumOfRecords == 0) ? lRes : lRes.GetRange(0, nNumOfRecords);
        //}

        //public List<SocialActivityDoc> GetUserSocialActionOnAsset(int nNumOfRecords, string sSiteGuid, int nSocialPlatform, List<int> socialActions, int nAssetID)
        //{
        //    List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();

        //    if (socialActions == null || socialActions.Count <= 0 || string.IsNullOrEmpty(sSiteGuid))
        //    {
        //        return lRes;
        //    }

        //    List<List<object>> lKeys = new List<List<object>>();

        //    foreach (int nAction in socialActions)
        //    {
        //        lKeys.Add(new List<object>() { nAssetID, sSiteGuid, nSocialPlatform, nAction });
        //    }

        //    try
        //    {
        //        lRes = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActionOnMedia", true).Keys(lKeys).Limit(nNumOfRecords).ToList()
        //                                           : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActionOnMedia", true).Keys(lKeys).ToList();
        //    }
        //    catch (Couchbase.Exceptions.ViewException vEx) { }

        //    return (lRes.Count <= nNumOfRecords || nNumOfRecords == 0) ? lRes : lRes.GetRange(0, nNumOfRecords);
        //}

        //public List<SocialActivityDoc> GetUsersSocialActions(int nNumOfRecords, List<string> lSiteGuids, int nSocialPlatform, List<int> lSocialAction)
        //{
        //    List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();

        //    List<List<object>> lKeys = new List<List<object>>();

        //    foreach (string siteGuid in lSiteGuids)
        //    {
        //        foreach (int socialAction in lSocialAction)
        //        {
        //            lKeys.Add(new List<object>() { nSocialPlatform, socialAction, siteGuid });
        //        }
        //    }

        //    if (lKeys.Count > 0)
        //    {
        //        try
        //        {
        //            lRes = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserAction", true).Keys(lKeys).Limit(nNumOfRecords).ToList()
        //                                               : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserAction", true).Keys(lKeys).ToList();
        //        }
        //        catch (Couchbase.Exceptions.ViewException vEx) { }
        //    }

        //    return (lRes.Count <= nNumOfRecords || nNumOfRecords == 0) ? lRes : lRes.GetRange(0, nNumOfRecords);
        //}

        //public List<SocialActivityDoc> GetUsersSocialActionsOnAsset(int nNumOfRecords, List<string> lSiteGuids, int nSocialPlatform, List<int> lSocialAction, int nAssetID)
        //{
        //    List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();

        //    List<List<object>> lKeys = new List<List<object>>();
        //    foreach (string siteGuid in lSiteGuids)
        //    {
        //        foreach (int socialAction in lSocialAction)
        //        {
        //            lKeys.Add(new List<object>() { nAssetID, socialAction, siteGuid });
        //        }
        //    }

        //    if (lKeys.Count > 0)
        //    {
        //        try
        //        {
        //            lRes = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActionOnMedia", true).Keys(lKeys).Limit(nNumOfRecords).ToList()
        //                                               : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActionOnMedia", true).Keys(lKeys).ToList();
        //        }
        //        catch (Couchbase.Exceptions.ViewException vEx) { }
        //    }

        //    return (lRes.Count <= nNumOfRecords || nNumOfRecords == 0) ? lRes : lRes.GetRange(0, nNumOfRecords);

        //}

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

        //public bool UpdateUserSocialAction(string sDocID, string sJsonDoc)
        //{
        //    bool bRes = false;

        //    try
        //    {
        //        bRes = m_oClient.StoreJson(Enyim.Caching.Memcached.StoreMode.Replace, sDocID, sJsonDoc);
        //    }
        //    catch (Exception ex) { }

        //    return bRes;
        //}

        //public List<SocialActivityDoc> GetGroupDailyFeed(int nLimit)
        //{
        //    List<SocialActivityDoc> lResult = new List<SocialActivityDoc>();
        //    try
        //    {
        //        var rows = (nLimit > 0) ? m_oClient.GetView<SocialActivityDoc>("dev_Action", "24HourFeed", true).StartKey(m_nGroupID).Limit(nLimit).ToArray()
        //                                : m_oClient.GetView<SocialActivityDoc>("dev_Action", "24HourFeed", true).StartKey(m_nGroupID).ToArray();

        //        foreach (SocialActivityDoc doc in rows)
        //        {
        //            lResult.Add(doc);
        //        }
        //    }
        //    catch (Exception ex) { }

        //    return lResult;
        //}

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
