using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Data;
using DAL;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using System.Net;
using System.Web;
using System.ServiceModel;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BaseSubscription
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected BaseSubscription() { }
        protected BaseSubscription(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }
        protected Int32 m_nGroupID;

        public int GroupID
        {
            get
            {
                return m_nGroupID;
            }
            protected set
            {
                m_nGroupID = value;
            }
        }

        protected static readonly string BASE_SUBSCRIPTION_LOG_FILE = "BaseSubscription";

        #region public abstract
        public abstract Subscription GetSubscriptionData(string sSubscriptionCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive);
        public abstract Subscription GetSubscriptionDataByProductCode(string sProductCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive);
        public abstract Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count);
        public abstract Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nIsActive, int[] userTypesIDs);

        public abstract Subscription[] GetRelevantSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID);
        public abstract string[] GetRelevantSubscriptionsListSTR(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID);
        public abstract Subscription[] GetSubscriptionsShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract Int32[] GetMediaList(string sSubscriptionCode, int nFileTypeID, string sDevice);
        public abstract bool DoesMediasExists(string sSubscriptionCode, int nMediaID);
        public abstract string DoesMediasExistsInSubs(string sSubscriptionCodes, int nMediaID);


        public abstract Subscription[] GetSubscriptionsData(string[] oSubCodes, string sCountryCd, string sLanguageCode, string sDeviceName, SubscriptionOrderBy orderBy);

        public abstract Subscription[] GetSubscriptionsDataByProductCodes(List<string> productCodes, bool getAlsoUnactive);

        #endregion

        #region public virtual
        public virtual Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID)
        {
            return GetSubscriptionsContainingMedia(nMediaID, nFileTypeID, false);
        }

        public virtual Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked)
        {
            Subscription[] lResult = null;
            try
            {
                List<Subscription> lSubscription = new List<Subscription>();

                List<int> nChannels = GetMediaChannels(nMediaID);

                if (nChannels != null && nChannels.Count > 0)
                {
                    DataTable dt = PricingDAL.Get_SubscriptionsListByChannelAndFileType(m_nGroupID, nChannels, nFileTypeID);

                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            int nSubID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "SUBSCRIPTION_ID");

                            if (nSubID != 0)
                            {
                                Subscription sub = new Subscription();
                                sub.m_SubscriptionCode = nSubID.ToString();

                                lSubscription.Add(sub);
                            }
                        }
                    }
                }

                if (lSubscription.Count > 0)
                {
                    lResult = (from s in lSubscription orderby s.m_Priority select s).ToArray<Subscription>();
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsContainingMedia. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" File Type ID: ", nFileTypeID));
                sb.Append(String.Concat(" IsShrinked: ", isShrinked));
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }

            return lResult;
        }

        public virtual string GetSubscriptionsContainingMediaSTR(int nMediaID, int nFileTypeID, bool isShrinked)
        {
            string retVal = string.Empty;
            Dictionary<int, int[]> theList = null;

            try
            {

                theList = GetSubFileTypes();

                System.Collections.ArrayList retArray = new System.Collections.ArrayList();
                Int32 nCount = theList.Count;
                StringBuilder subStr = new StringBuilder();
                List<int> fileTypes = new List<int>();
                foreach (KeyValuePair<int, int[]> subFilePair in theList)
                {
                    string sSubCode = subFilePair.Key.ToString();
                    if (subFilePair.Value == null || subFilePair.Value.Length == 0 || Array.IndexOf(subFilePair.Value, nFileTypeID) > -1 && nFileTypeID != 0)
                    {
                        if (!string.IsNullOrEmpty(subStr.ToString()))
                        {
                            subStr.Append(",");
                        }
                        subStr.Append(sSubCode);
                    }

                }
                if (!string.IsNullOrEmpty(subStr.ToString()))
                {
                    retVal = DoesMediasExistsInSubs(subStr.ToString(), nMediaID);
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsContainingMediaSTR. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" FT ID: ", nFileTypeID));
                sb.Append(String.Concat(" IsShrinked: ", isShrinked.ToString().ToLower()));
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return retVal;
        }

        public virtual Subscription[] GetSubscriptionsContainingMedia(int nMediaID, int nFileTypeID, bool isShrinked, int index)
        {
            Subscription[] theList = null;
            Subscription[] ret = null;
            try
            {
                theList = GetRelevantSubscriptionsList(false, string.Empty, string.Empty, string.Empty, index, nMediaID, nFileTypeID);

                System.Collections.ArrayList retArray = new System.Collections.ArrayList();
                Int32 nCount = theList.Length;

                if (index < nCount)
                {
                    nCount = index;
                }
                for (int i = 0; i < nCount; i++)
                {
                    retArray.Add(theList[i]);
                }
                if (retArray.Count == 0)
                    return null;

                ret = new Subscription[retArray.Count];
                Array.Copy(retArray.ToArray(), ret, retArray.Count);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsContainingMedia. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" FT ID: ", nFileTypeID));
                sb.Append(String.Concat(" Is Shrinked: ", isShrinked.ToString().ToLower()));
                sb.Append(String.Concat(" Index: ", index));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            return ret;
        }

        public virtual IdsResponse GetSubscriptionIDsContainingMediaFile(int nMediaID, int nMediaFileID)
        {
            IdsResponse response = new IdsResponse();
            List<KeyValuePair<int, int>> lSubs = new List<KeyValuePair<int, int>>();
            try
            {
                int nFileTypeID = 0;
                nFileTypeID = Api.Module.GetMediaFileTypeID(m_nGroupID, nMediaFileID);


                //get from DB subscription List
                DataSet ds = PricingDAL.Get_SubscriptionsList(m_nGroupID, nFileTypeID);
                if (ds == null || ds.Tables == null || ds.Tables.Count == 0) //no data return 
                    return null;

                Dictionary<int, int> dSubs = new Dictionary<int, int>(); /*subscriptionID , orderNum*/
                Dictionary<int, List<int>> dChannelSubs = new Dictionary<int, List<int>>(); /*channelID , Subscriptions*/

                DataTable dtSubscriptions = ds.Tables[0];//Subscriptions
                DataTable dtChannelSubs = ds.Tables[1];//Channel+subscription
                // Build Dictionary with subscription , ordernum 
                foreach (DataRow dr in dtSubscriptions.Rows)
                {
                    int subID = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                    int orderNum = ODBCWrapper.Utils.GetIntSafeVal(dr["order_num"]);
                    dSubs.Add(subID, orderNum);
                }
                //Build Dictionary with channelID , List<Sbuscription>
                foreach (DataRow dr in dtChannelSubs.Rows)
                {
                    int channelID = ODBCWrapper.Utils.GetIntSafeVal(dr["channel_id"]);
                    int subID = ODBCWrapper.Utils.GetIntSafeVal(dr["subscription_id"]);
                    if (dChannelSubs.ContainsKey(channelID))
                    {
                        dChannelSubs[channelID].Add(subID);
                    }
                    else
                    {
                        dChannelSubs.Add(channelID, new List<int>(1) { subID });
                    }
                }

                if (nMediaFileID > 0 && nMediaID == 0)
                {
                    MeidaMaper[] mapper = Api.Module.MapMediaFiles(m_nGroupID, new int[] { nMediaFileID });
                    if (mapper != null && mapper.Length > 0 && mapper[0] != null)
                    {
                        nMediaID = mapper[0].m_nMediaID;
                    }
                }

                List<int> lchannels = Api.Module.ChannelsContainingMedia(m_nGroupID, dChannelSubs.Keys.ToList(), nMediaID, nFileTypeID);
                if (lchannels != null && lchannels.Count > 0)
                {
                    foreach (int channelItem in lchannels)
                    {
                        if (dChannelSubs.ContainsKey(channelItem))
                            foreach (int subItem in dChannelSubs[channelItem])
                            {
                                KeyValuePair<int, int> subPair = new KeyValuePair<int, int>(subItem, dSubs[subItem]); /*subscriptionid + priority*/
                                if (!lSubs.Contains(subPair))
                                {
                                    lSubs.Add(subPair);
                                }
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionIDsContainingMediaFile. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat("M ID: ", nMediaID));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }

            response.Ids = lSubs.OrderBy(x => x.Value).Select(y => y.Key).ToList();
            response.Status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()); 
            return response;
        }

        public virtual Subscription[] GetSubscriptionsContainingMediaFile(int nMediaID, int nMediaFileID)
        {
            List<Subscription> retList = new List<Subscription>();
            try
            {
                IdsResponse response = GetSubscriptionIDsContainingMediaFile(nMediaID, nMediaFileID);
                List<int> lSubs = response.Ids;
                Subscription oSub = null;
                if (lSubs != null && lSubs.Count > 0)
                {
                    foreach (int sub in lSubs)
                    {
                        oSub = new Subscription();
                        oSub.m_sCodes = new BundleCodeContainer[1];
                        oSub.m_sCodes[0] = new BundleCodeContainer();
                        oSub.m_sCodes[0].m_sCode = sub.ToString();
                        retList.Add(oSub);
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsContainingMediaFile. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat("Media ID: ", nMediaID));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return retList.ToArray<Subscription>();
        }


        #endregion

        protected virtual Dictionary<int, int[]> GetSubFileTypes()
        {
            List<int> subIDs = new List<int>();
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            Dictionary<int, int[]> retVal = new Dictionary<int, int[]>();

            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id from subscriptions with (nolock) where is_active=1 and status=1 and start_date<getdate() and (end_date is null or end_date>getdate()) and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nSubscriptionCode = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        subIDs.Add(nSubscriptionCode);
                    }
                }

                foreach (int subID in subIDs)
                {
                    Int32[] nFileTypes = GetSubscriptionFileTypes(subID, m_nGroupID);
                    if (!retVal.ContainsKey(subID))
                    {
                        retVal.Add(subID, nFileTypes);
                    }
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return retVal;
        }

        protected bool IsMediasExists(Subscription subscriptionData, int nMediaID)
        {
            if (subscriptionData != null && subscriptionData.m_sCodes != null && subscriptionData.m_sFileTypes != null)
            {
                string sSubscriptionCode = subscriptionData.m_SubscriptionCode;
                BundleCodeContainer[] subscriptionCodes = subscriptionData.m_sCodes;
                Int32[] subscriptionFileTypes = subscriptionData.m_sFileTypes;

                Int32[] nChannelList = null;

                if (subscriptionCodes != null && subscriptionCodes.Length > 0)
                {
                    nChannelList = new Int32[subscriptionCodes.Length];
                    for (int i = 0; i < subscriptionCodes.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(subscriptionCodes[i].m_sCode))
                            nChannelList[i] = int.Parse(subscriptionCodes[i].m_sCode);
                    }
                }

                return Api.Module.DoesMediaBelongToChannels(m_nGroupID, nChannelList, subscriptionFileTypes, nMediaID, true, "");
            }
            return false;
        }

        protected List<int> GetMediaChannels(int nMediaID)
        {
            List<int> nChannelsArr = Api.Module.GetMediaChannels(m_nGroupID, nMediaID);
            List<int> nChannels = null;
            if (nChannelsArr != null)
                nChannels = nChannelsArr.ToList();

            return nChannels;
        }

        protected LanguageContainer[] GetSubscriptionDescription(Int32 nSubscriptionID)
        {

            long lSubCode = (long)nSubscriptionID;
            LanguageContainer[] res = null;
            Dictionary<long, List<string[]>> dict = PricingDAL.Get_SubscriptionsDescription(m_nGroupID, new List<long>(1) { lSubCode });
            if (dict.ContainsKey(lSubCode))
            {
                List<string[]> lst = dict[lSubCode];
                res = new LanguageContainer[lst.Count];
                for (int i = 0; i < lst.Count; i++)
                {
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(lst[i][0], lst[i][1]);
                    res[i] = lc;
                }
            }

            return res;
        }

        protected static Int32[] GetSubscriptionFileTypes(Int32 nSubscriptionID, Int32 nGroupID)
        {

            long lSubCode = (long)nSubscriptionID;
            Dictionary<long, List<int>> subFileTypes = PricingDAL.Get_SubscriptionsFileTypes(nGroupID, new List<long>(1) { lSubCode });
            if (subFileTypes.ContainsKey(lSubCode))
                return subFileTypes[lSubCode].ToArray();
            return null;
        }

        protected static BundleCodeContainer[] GetSubscriptionsChannels(Int32 nSubscriptionID, Int32 nGroupID)
        {

            BundleCodeContainer[] res = null;
            long lSubCode = (long)nSubscriptionID;
            Dictionary<long, List<long>> subsChannels = PricingDAL.Get_SubscriptionsChannels(nGroupID, new List<long>(1) { lSubCode });
            if (subsChannels.ContainsKey(lSubCode))
            {
                List<long> lst = subsChannels[lSubCode];
                res = new BundleCodeContainer[lst.Count];
                for (int i = 0; i < lst.Count; i++)
                {
                    string sChannelID = lst[i] + "";
                    BundleCodeContainer bcc = new BundleCodeContainer();
                    bcc.Initialize(sChannelID, string.Empty);
                    res[i] = bcc;
                }
            }

            return res;
        }

        protected LanguageContainer[] GetSubscriptionName(Int32 nSubscriptionID)
        {
            long lSubCode = (long)nSubscriptionID;
            LanguageContainer[] res = null;
            Dictionary<long, List<string[]>> dict = PricingDAL.Get_SubscriptionsNames(m_nGroupID, new List<long>(1) { lSubCode });
            if (dict.ContainsKey(lSubCode))
            {
                List<string[]> lst = dict[lSubCode];
                res = new LanguageContainer[lst.Count];
                for (int i = 0; i < lst.Count; i++)
                {
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(lst[i][0], lst[i][1]);
                    res[i] = lc;
                }
            }

            return res;
        }

        protected ServiceObject[] GetSubscriptionServices(Int32 nSubscriptionID)
        {
            ServiceObject[] res = null;
            long lSubCode = (long)nSubscriptionID;
            DataTable dt = PricingDAL.Get_SubscriptionsServices(m_nGroupID, new List<long>(1) { lSubCode });
            ServiceObject service;
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new ServiceObject[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long serviceID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["service_id"]);
                    string desc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    if (serviceID == (int)eService.NPVR)
                    {
                        long quota = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["QUOTA_IN_MINUTES"]);
                        service = new NpvrServiceObject(serviceID, desc, quota);
                    }
                    else
                    {
                        service = new ServiceObject(serviceID, desc);
                    }
                    res[i] = service;
                }
            }
            return res;
        }

        protected List<SubscriptionCouponGroup> GetSubscriptionCouponsGroup(Int32 subscriptionId)
        {
            List<SubscriptionCouponGroup> sgList = Core.Pricing.Utils.GetSubscriptionCouponsGroup((long)subscriptionId, m_nGroupID, true);
            return sgList;
        }

    }
}
