using ApiObjects;
using CouchbaseManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;

namespace DalCB
{
    public class SocialDAL_Couchbase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly string CB_FEED_DESGIN = Utils.GetValFromConfig("cb_feed_design");

        CouchbaseManager.CouchbaseManager cbManager;
        private int m_nGroupID;
        public SocialDAL_Couchbase(int nGroupID)
        {
            m_nGroupID = nGroupID;
            cbManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.SOCIAL);
        }

        public bool GetUserSocialFeed(string sSiteGuid, int nSkip, int nNumOfRecords, out List<SocialActivityDoc> lResult)
        {
            lResult = new List<SocialActivityDoc>();
            bool bResult = false;
            try
            {
                ViewStaleState? staleState = null;
                var staleStateConfig = TVinciShared.WS_Utils.GetTcmConfigValue("FRIENDS_ACTIVITY_VIEW_STALE_STATE");
                if (!string.IsNullOrEmpty(staleStateConfig))
                {
                    ViewStaleState parsedStaleState = ViewStaleState.None;
                    if (Enum.TryParse(staleStateConfig, true, out parsedStaleState))
                        staleState = parsedStaleState;
                }

                long epochTime = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
                object[] startKey = new object[] { sSiteGuid, epochTime };
                object[] endKey = new object[] { sSiteGuid, 0 };

                List<string> retval;

                if (nNumOfRecords > 0)
                {
                    retval = cbManager.View<string>(new ViewManager(CB_FEED_DESGIN, "UserFeed")
                    {
                        startKey = startKey,
                        endKey = endKey,
                        isDescending = true,
                        skip = nSkip,
                        limit = nNumOfRecords,
                        staleState = staleState

                    });
                }
                else
                {
                    retval = cbManager.View<string>(new ViewManager(CB_FEED_DESGIN, "UserFeed")
                    {
                        startKey = startKey,
                        endKey = endKey,
                        isDescending = true,
                        staleState = staleState
                    });
                }

                if (retval != null)
                {
                    var cbRes = cbManager.GetValues<SocialActivityDoc>(retval, true, true);
                    if (cbRes != null)
                    {
                        lResult = cbRes.Values.ToList();
                    }
                }
                bResult = true;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserSocialFeed. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" SG: ", sSiteGuid));
                sb.Append(String.Concat(" Skip: ", nSkip));
                sb.Append(String.Concat(" Num Of Recs: ", nNumOfRecords));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bResult;
        }

        public bool GetUserSocialAction(string sSocialActionID, out SocialActivityDoc oRetval)
        {
            oRetval = null;
            bool bSuccess = false;
            try
            {
                oRetval = cbManager.GetJsonAsT<SocialActivityDoc>(sSocialActionID);
                bSuccess = true;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserSocialAction. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Action: ", sSocialActionID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bSuccess;
        }

        public List<SocialActivityDoc> GetUserSocialAction(List<string> lSocialActionIDs)
        {
            List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();
            try
            {

                IDictionary<string, object> dRetval = cbManager.GetValues<object>(lSocialActionIDs, true);

                if (dRetval != null && dRetval.Count > 0)
                {
                    string retObj;
                    SocialActivityDoc socialDoc;
                    foreach (string sKey in dRetval.Keys)
                    {
                        retObj = Convert.ToString(dRetval[sKey]);
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
                                log.Error("Error - " + string.Format("Deserialization of SocialActivityDoc failed. str obj={0}, ex={1}, stack={2}", retObj, ex.Message, ex.StackTrace), ex);
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Caught exception in GetUserSocialAction for multiple objects. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
            }

            return lRes;
        }

        public List<SocialActivityDoc> GetUserSocialAction(List<int> lSocialActionsTypes, string sSiteGuid, int eSocialPlatform)
        {
            List<SocialActivityDoc> lRes = new List<SocialActivityDoc>();
            List<List<object>> keys = new List<List<object>>();
            foreach (int socialActionsTypes in lSocialActionsTypes)
            {
                keys.Add(new List<object>() { sSiteGuid, eSocialPlatform, socialActionsTypes });
            }

            try
            {
                lRes = cbManager.View<SocialActivityDoc>(new ViewManager(CB_FEED_DESGIN, "UserSocialActions")
                {
                    keys = keys
                });
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Caught exception in GetUserSocialAction for UserSocialActions view. ex={0}; stack={1}", ex.Message, ex.StackTrace), ex);
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


                List<SocialActivityDoc> retval;
                if (nNumOfRecords > 0) 
                {
                    retval = cbManager.View<SocialActivityDoc>(new ViewManager(CB_FEED_DESGIN, "UserActions")
                    {
                        startKey = startKey,
                        endKey = endKey,
                        isDescending = true,
                        skip = nSkip,
                        limit = nNumOfRecords
                    });
                }
                else
                {
                    retval = cbManager.View<SocialActivityDoc>(new ViewManager(CB_FEED_DESGIN, "UserActions")
                    {
                        startKey = startKey,
                        endKey = endKey,
                        isDescending = true
                    });
                }

                if (retval != null)
                {
                    lUserActivities = retval.ToList();
                }

                bResult = true;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUserSocialAction. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" SG: ", sSiteGuid));
                sb.Append(String.Concat(" Skip: ", nSkip));
                sb.Append(String.Concat(" Num Of Recs: ", nNumOfRecords));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bResult;
        }

        public bool DeleteUserSocialAction(string sDocID)
        {
            bool bResult = false;
            try
            {
                bResult = cbManager.Remove(sDocID);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at DeleteUserSocialAction. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Doc ID: ", sDocID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bResult;
        }

        public bool GetFeedsByActorID(string sActorSiteGuid, int nNumOfDocs, out List<string> lDocIDs)
        {
            lDocIDs = new List<string>();
            bool bResult = false;
            try
            {
                var lFeeds = (nNumOfDocs > 0) ? 
                    cbManager.ViewIds(new ViewManager(CB_FEED_DESGIN, "FeedByActorId") { limit = nNumOfDocs }) :
                    cbManager.ViewIds(new ViewManager(CB_FEED_DESGIN, "FeedByActorId"));
                
                bResult = true;

                if (lFeeds != null)
                {
                    foreach (var feed in lFeeds)
                    {
                        lDocIDs.Add(feed);
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetFeedsByActorID. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Actor SG: ", sActorSiteGuid));
                sb.Append(String.Concat(" Num Of Docs: ", nNumOfDocs));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bResult;

        }

        public bool InsertUserSocialAction(string sDocID, object oDoc)
        {
            bool bRes = false;

            try
            {
                bRes = cbManager.SetJson<object>(sDocID, oDoc);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at InsertUserSocialAction. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Doc ID: ", sDocID));
                sb.Append(String.Concat(" Num Of Docs: ", oDoc != null ? oDoc.ToString() : "null"));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
            }

            return bRes;
        }

        public int GetAssetSocialActionCount(int assetId, eAssetType assetType, eUserAction actionType, DateTime startDate, DateTime endDate)
        {
            int res = 0;
            try
            {
                object[] startKey = new object[4] { assetId, 2, (int)actionType, Utils.DateTimeToUnixTimestamp(startDate) };
                object endKey = new object[4] { assetId, (int)assetType, (int)actionType, Utils.DateTimeToUnixTimestamp(endDate) };
                var view = cbManager.View<int>(new ViewManager(CB_FEED_DESGIN, "AssetStats")
                {
                    startKey = startKey,
                    endKey = endKey,
                    reduce = true
                });

                if (view.Count() > 0)
                {
                    res = view.First<int>();
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetAssetSocialActionCount. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Asset ID: ", assetId));
                sb.Append(String.Concat(" Asset Type: ", (int)assetType));
                sb.Append(String.Concat(" Action Type: ", (int)actionType));
                sb.Append(String.Concat(" SD: ", startDate.ToString()));
                sb.Append(String.Concat(" ED: ", endDate));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

            }

            return res;
        }

        public double GetRatesSum(int assetId, eAssetType assetType, DateTime startDate, DateTime endDate)
        {
            double res = 0d;
            try
            {
                object[] startKey = new object[4] { assetId, (int)assetType, (int)eUserAction.RATES, Utils.DateTimeToUnixTimestamp(startDate) };
                object endKey = new object[4] { assetId, (int)assetType, (int)eUserAction.RATES, Utils.DateTimeToUnixTimestamp(endDate) };
                var view = cbManager.View<double>(new ViewManager(CB_FEED_DESGIN, "AssetStatsRateSum")
                {
                    startKey = startKey,
                    endKey = endKey,
                    reduce = true
                });

                if (view.Count() > 0)
                {
                    res = view.First<double>();
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetRatesSum. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Asset ID: ", assetId));
                sb.Append(String.Concat(" Asset Type: ", assetType));
                sb.Append(String.Concat(" SD: ", startDate.ToString()));
                sb.Append(String.Concat(" ED: ", endDate));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

            }

            return res;
        }

        //public bool GetUserSocialAction(string sSiteGuid, int nNumOfRecords, int nSkip, out List<SocialActivityDoc> lUserActivities)
        //{
        //    bool bResult = false;
        //    lUserActivities = new List<SocialActivityDoc>();
        //    try
        //    {
        //        long epochTime = DalCB.Utils.DateTimeToUnixTimestamp(DateTime.UtcNow);
        //        object[] startKey = new object[] { sSiteGuid, epochTime };
        //        object[] endKey = new object[] { sSiteGuid, 0 };


        //        var retval = (nNumOfRecords > 0) ? m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActions", true).StartKey(startKey).EndKey(endKey).Descending(true).Skip(nSkip).Limit(nNumOfRecords)
        //                                       : m_oClient.GetView<SocialActivityDoc>(CB_FEED_DESGIN, "UserActions", true).StartKey(startKey).EndKey(endKey).Descending(true);
        //        if (retval != null)
        //        {
        //            lUserActivities = retval.ToList();
        //        }

        //        bResult = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        #region Logging
        //        StringBuilder sb = new StringBuilder("Exception at GetUserSocialAction. ");
        //        sb.Append(String.Concat(" Ex Msg: ", ex.Message));
        //        sb.Append(String.Concat(" SG: ", sSiteGuid));
        //        sb.Append(String.Concat(" Skip: ", nSkip));
        //        sb.Append(String.Concat(" Num Of Recs: ", nNumOfRecords));
        //        sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
        //        sb.Append(String.Concat(" ST: ", ex.StackTrace));
        //        #endregion
        //    }

        //    return bResult;
        //}

        private static T Deserialize<T>(string sJson) where T : class
        {
            T res = null;
            try
            {
                res = JsonConvert.DeserializeObject<T>(sJson);
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at Deserialize. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" JSON: ", sJson));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }

            return res;
        }

        /// <summary>
        /// Gets all users who performed a certain action a media on a certain platform
        /// </summary>
        /// <param name="p_nLimit">How many rows from the skipping/start point</param>
        /// <param name="p_nUserGUID">A user to ignore</param>
        /// <param name="p_nMediaID">The media that is the center of the function</param>
        /// <param name="p_nActionType">Get users who did -what- action?</param>
        /// <param name="p_nPlatform">Social platform</param>
        /// <param name="p_nSkip">Starting point</param>
        /// <returns></returns>
        public List<SocialActivityDoc> GetUsersActionedMedia(int p_nMediaID, int p_nUserGUID, int p_nActionType, int p_nPlatform, int p_nLimit, int p_nSkip)
        {
            List<SocialActivityDoc> lstResponse = new List<SocialActivityDoc>();

            // Get the rows from the view that have the correct key,
            // order the list from top to bottom,
            // get only rows that are from "skip" until "Limit"
            var lstRows = this.cbManager.View<SocialActivityDoc>(new ViewManager(CB_FEED_DESGIN, "MediaSocialActions")
            {
                startKey = new object[] { p_nMediaID, p_nPlatform, p_nActionType },
                endKey = new object[] { p_nMediaID, p_nPlatform, p_nActionType },
                isDescending = true,
                skip = p_nSkip,
                limit = p_nLimit
            });

            if (lstRows != null)
            {
                lstResponse = lstRows.ToList();
            }

            return (lstResponse);
        }
    }
}
