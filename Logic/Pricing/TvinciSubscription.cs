using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Data;
using DAL;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Pricing;

namespace Core.Pricing
{
    public class TvinciSubscription : BaseSubscription
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciSubscription(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override Subscription GetSubscriptionData(string sSubscriptionCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            Subscription tmpSubscription = new Subscription();
            try
            {
                int nSubscriptionCode = 0;
                int? nIsActive = 1;

                if (!bGetAlsoUnActive)
                {
                    /*
                     * In this case (get only active subscription) we can used an optimized flow which reduces the number of calls to the DB.
                     */
                    Subscription[] subs = GetSubscriptionsData(new string[1] { sSubscriptionCode }, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, SubscriptionOrderBy.StartDateAsc);
                    if (subs != null && subs.Length > 0)
                        return subs[0];
                    return null;
                }
                else
                {
                    nIsActive = null;
                }

                if (!Int32.TryParse(sSubscriptionCode, out nSubscriptionCode) || nSubscriptionCode == 0)
                {
                    return null;
                }
                DataSet dsSubscriptions = PricingDAL.Get_SubscriptionData(m_nGroupID, nIsActive, nSubscriptionCode);
                List<Subscription> subscriptionsList = CreateSubscriptionsListFromDataSet(false, dsSubscriptions, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (subscriptionsList != null && subscriptionsList.Count > 0)
                {
                    tmpSubscription = subscriptionsList[0];
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" SC: ", sSubscriptionCode));
                sb.Append(String.Concat(" C Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Code: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" GetAlsoUnactive: ", bGetAlsoUnActive.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }

            return tmpSubscription;
        }

        public override Subscription GetSubscriptionDataByProductCode(string sProductCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bGetAlsoUnActive)
        {
            Subscription tmpSubscription = null;
            try
            {
                int? nIsActive = 1;

                if (bGetAlsoUnActive)
                {
                    nIsActive = null;
                }


                DataSet dsSubscriptions = PricingDAL.Get_SubscriptionData(m_nGroupID, nIsActive, null, sProductCode, null);
                List<Subscription> subscriptionsList = CreateSubscriptionsListFromDataSet(false, dsSubscriptions, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (subscriptionsList != null && subscriptionsList.Count > 0)
                {
                    tmpSubscription = subscriptionsList[0];
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionDataByProductCode. ");
                sb.Append(String.Concat(" Prd Cd: ", sProductCode));
                sb.Append(String.Concat(" C Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" GetAlsoUnactive: ", bGetAlsoUnActive.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return tmpSubscription;
        }

        public override Subscription[] GetSubscriptionsShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetTvinciSubscriptionsList(true, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override Subscription[] GetRelevantSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            return GetTvinciRelevantSubscriptionsList(bShrink, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, count, mediaID, nFileTypeID);
        }

        public override string[] GetRelevantSubscriptionsListSTR(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            return GetTvinciRelevantSubscriptionsListSTR(bShrink, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, count, mediaID, nFileTypeID);
        }

        protected string[] GetTvinciRelevantSubscriptionsListSTR(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            DataSet dsSubscriptions = PricingDAL.Get_SubscriptionData(m_nGroupID, null);
            List<Subscription> subscriptionsList = CreateSubscriptionsListFromDataSet(false, dsSubscriptions, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            string[] tmp = null;
            try
            {
                if (subscriptionsList != null && subscriptionsList.Count > 0)
                {
                    Int32 nCount = subscriptionsList.Count;
                    if (count > 0 && nCount > count)
                    {
                        nCount = count;
                    }
                    if (nCount > 0)
                        tmp = new string[nCount];
                    Int32 nIndex = 0;
                    int counter = 0;
                    int i = 0;
                    bool isValid = false;
                    while (counter != nCount)
                    {
                        isValid = false;
                        log.Debug("Check Index: " + i.ToString());
                        Int32 nSubscriptionCode = 0;
                        int.TryParse(subscriptionsList[i].m_SubscriptionCode, out nSubscriptionCode);

                        Int32[] nFileTypes = GetSubscriptionFileTypes(nSubscriptionCode, m_nGroupID);
                        if (!bShrink)
                        {
                            if (IsMediasExists(subscriptionsList[i], mediaID))
                            {
                                if (nFileTypes == null || nFileTypes.Length == 0 || Array.IndexOf(nFileTypes, nFileTypeID) > -1 && nFileTypeID != 0)
                                {
                                    isValid = true;
                                    counter++;
                                }
                            }
                        }
                        else
                        {
                            if (IsMediasExists(subscriptionsList[i], mediaID))
                            {
                                if (nFileTypes == null || nFileTypes.Length == 0 || Array.IndexOf(nFileTypes, nFileTypeID) > -1 && nFileTypeID != 0)
                                {
                                    isValid = true;
                                    counter++;
                                }
                            }
                        }
                        if (isValid && nSubscriptionCode > 0)
                        {
                            tmp[nIndex] = nSubscriptionCode.ToString();
                            nIndex++;
                        }
                        i++;
                    }
                    if (nIndex < nCount)
                    {
                        string[] tmp1 = new string[nIndex];
                        Array.Copy(tmp, tmp1, nIndex);
                        tmp = tmp1;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetTvinciRelevantSubscriptionsListSTR. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Shrink: ", bShrink.ToString().ToLower()));
                sb.Append(String.Concat(" C Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Count: ", count));
                sb.Append(String.Concat(" M ID: ", mediaID));
                sb.Append(String.Concat(" FT ID: ", nFileTypeID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return tmp;
        }

        protected Subscription[] GetTvinciRelevantSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count, int mediaID, int nFileTypeID)
        {
            int nIsActive = 1;
            Subscription[] arrSubscriptions = null;
            Subscription tmpSubscription = null;

            try
            {
                DataSet dsSubscriptions = PricingDAL.Get_SubscriptionData(m_nGroupID, nIsActive, null, null, null, 0);

                if (dsSubscriptions != null && dsSubscriptions.Tables.Count > 0)
                {
                    Dictionary<int, List<UserType>> dictSubUserTypes = GetSubscriptionsUserTypes(dsSubscriptions);
                    DataTable dtSubscription = dsSubscriptions.Tables[0];

                    int nCount = dtSubscription.DefaultView.Count;
                    if (count > 0 && nCount > count)
                    {
                        nCount = count;
                    }
                    if (nCount > 0)
                    {
                        arrSubscriptions = new Subscription[nCount];
                    }

                    int nIndex = 0;
                    int counter = 0;
                    int i = 0;

                    while (counter != nCount)
                    {
                        if (i < dtSubscription.Rows.Count)
                        {
                            DataRow subscriptionRow = dtSubscription.Rows[i];
                            int nSubscriptionCode = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["ID"]);
                            Int32[] nFileTypes = GetSubscriptionFileTypes(nSubscriptionCode, m_nGroupID);
                            UserType[] arrUserTypes = null;
                            if (dictSubUserTypes != null && dictSubUserTypes.ContainsKey(nSubscriptionCode))
                            {
                                arrUserTypes = dictSubUserTypes[nSubscriptionCode].ToArray();
                            }

                            tmpSubscription = CreateSubscriptionObject(bShrink, subscriptionRow, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nFileTypes, GetSubscriptionDescription(nSubscriptionCode),
                                                                       GetSubscriptionsChannels(nSubscriptionCode, m_nGroupID), GetSubscriptionName(nSubscriptionCode), 
                                                                       GetSubscriptionServices(nSubscriptionCode), arrUserTypes,
                                                                       GetSubscriptionCouponsGroup(nSubscriptionCode));

                            if ((nFileTypes == null || nFileTypes.Length == 0 || Array.IndexOf(nFileTypes, nFileTypeID) > -1 && nFileTypeID != 0) && IsMediasExists(tmpSubscription, mediaID))
                            {
                                counter++;
                                arrSubscriptions[nIndex] = tmpSubscription;
                                nIndex++;
                            }
                        }
                        else
                        {
                            break;
                        }
                        i++;
                    }

                    if (nIndex < nCount)
                    {
                        Subscription[] tmp1 = new Subscription[nIndex];
                        Array.Copy(arrSubscriptions, tmp1, nIndex);
                        arrSubscriptions = tmp1;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetTvinciRelevantSubscriptionsList. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Shrink: ", bShrink));
                sb.Append(String.Concat(" C Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Count: ", count));
                sb.Append(String.Concat(" M ID: ", mediaID));
                sb.Append(String.Concat(" FT ID: ", nFileTypeID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            return arrSubscriptions;
        }

        protected Subscription[] GetTvinciSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nIsActive, List<int> userTypesIDsList, int? topRows)
        {
            Subscription[] arrSubscriptions = null;
            try
            {
                DataSet dsSubscriptions = PricingDAL.Get_SubscriptionData(m_nGroupID, nIsActive, null, null, userTypesIDsList, topRows);

                List<Subscription> subscriptionsList = CreateSubscriptionsListFromDataSet(bShrink, dsSubscriptions, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (subscriptionsList != null && subscriptionsList.Count > 0)
                {
                    arrSubscriptions = subscriptionsList.FindAll(sub => sub.m_SubscriptionCode != null).ToArray();
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetTvinciSubscriptionsList. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Shrink: ", bShrink.ToString().ToLower()));
                sb.Append(String.Concat(" C Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" IsActive: ", nIsActive));
                sb.Append(String.Concat(" Top Rows: ", topRows ?? Int32.MinValue));
                if (userTypesIDsList != null && userTypesIDsList.Count > 0)
                {
                    sb.Append(String.Concat(" User Types Lst: "));
                    for (int i = 0; i < userTypesIDsList.Count; i++)
                    {
                        sb.Append(String.Concat(userTypesIDsList[i], "; "));
                    }
                }
                else
                {
                    sb.Append("User Types Lst is empty. ");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            return arrSubscriptions;
        }

        public Subscription[] GetTvinciSubscriptionsList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            int? topRows = null;
            int nIsActive = 1;
            return GetTvinciSubscriptionsList(bShrink, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nIsActive, null, topRows);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetTvinciSubscriptionsList(false, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int count)
        {
            int nIsActive = 1;
            return GetTvinciSubscriptionsList(false, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nIsActive, null, count);
        }

        public override Subscription[] GetSubscriptionsList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, int nIsActive, int[] userTypesIDs)
        {
            List<int> userTypesIDsList = null;
            if (userTypesIDs != null)
            {
                userTypesIDsList = userTypesIDs.ToList();
            }
            return GetTvinciSubscriptionsList(false, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nIsActive, userTypesIDsList, null);
        }

        public override int[] GetMediaList(string sSubscriptionCode, Int32 nFileTypeID, string sDevice)
        {
            List<int> ret = null;
            try
            {
                int nSubCode = 0;
                bool bTry = int.TryParse(sSubscriptionCode, out nSubCode);
                int[] m_nFileTypes = new int[1] { nFileTypeID };

                ret = Api.Module.GetSubscriptionMediaIds(m_nGroupID, nSubCode, m_nFileTypes, sDevice);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetMediaList. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Sub Cd: ", sSubscriptionCode));
                sb.Append(String.Concat(" FT ID: ", nFileTypeID));
                sb.Append(String.Concat(" D: ", sDevice));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            return ret.ToArray();
        }

        public override string DoesMediasExistsInSubs(string sSubscriptionCodesStr, int nMediaID)
        {
            StringBuilder retVal = new StringBuilder();
            StringBuilder channelsSB = new StringBuilder();
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.DataSetSelectQuery channelsSelectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select channel_id from subscriptions_channels with (nolock) where is_active=1 and status=1 and ";
                selectQuery += "subscription_id in (" + sSubscriptionCodesStr.ToString() + ")";
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            string channelID = selectQuery.Table("query").DefaultView[i].Row["channel_id"].ToString();
                            if (!string.IsNullOrEmpty(channelsSB.ToString()))
                            {
                                channelsSB.Append(",");
                            }
                            channelsSB.Append(channelID);
                        }
                    }
                }

                channelsSelectQuery = new ODBCWrapper.DataSetSelectQuery();
                channelsSelectQuery += "select cm.id, sc.subscription_id, s.id from TVinci.dbo.channels_media cm,Pricing.dbo.subscriptions s, Pricing.dbo.subscriptions_channels sc where cm.status=1 and ";
                channelsSelectQuery += "cm.channel_id in (" + channelsSB.ToString() + ") and sc.channel_id = cm.channel_id and s.id=sc.subscription_id and sc.status = 1 and sc.is_active = 1 and s.status = 1 and s.is_active = 1";
                channelsSelectQuery += " and ";
                channelsSelectQuery += ODBCWrapper.Parameter.NEW_PARAM("cm.MEDIA_ID", "=", nMediaID);
                if (channelsSelectQuery.Execute("query", true) != null)
                {
                    int mediaCount = channelsSelectQuery.Table("query").DefaultView.Count;
                    if (mediaCount > 0)
                    {
                        for (int j = 0; j < mediaCount; j++)
                        {
                            string sub_id = channelsSelectQuery.Table("query").DefaultView[j].Row["subscription_id"].ToString();
                            if (!string.IsNullOrEmpty(retVal.ToString()))
                            {
                                retVal.Append(",");
                            }
                            retVal.Append(sub_id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at DoesMediasExistsInSubs.");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Sub Code Str: ", sSubscriptionCodesStr));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (channelsSelectQuery != null)
                {
                    channelsSelectQuery.Finish();
                }
            }

            return retVal.ToString();
        }

        public override bool DoesMediasExists(string sSubscriptionCode, int nMediaID)
        {

            Int32[] nChannelList = null;
            try
            {
                Subscription s = GetSubscriptionData(sSubscriptionCode, String.Empty, String.Empty, String.Empty, false);
                BundleCodeContainer[] sc = s.m_sCodes;
                if (sc != null)
                {
                    nChannelList = new Int32[sc.Length];
                    for (int i = 0; i < sc.Length; i++)
                    {
                        if (sc[i].m_sCode != "" && sc[i].m_sCode != null)
                            nChannelList[i] = int.Parse(sc[i].m_sCode);
                    }
                }

                return Api.Module.DoesMediaBelongToChannels(m_nGroupID, nChannelList, s.m_sFileTypes, nMediaID, true, "");
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at DoesMediasExists. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sb.Append(String.Concat(" M ID: ", nMediaID));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
        }

        private Dictionary<int, List<UserType>> GetSubscriptionsUserTypes(DataSet dsSubscriptions)
        {
            Dictionary<int, List<UserType>> retDict = new Dictionary<int, List<UserType>>();
            List<int> userTypesIDsList = new List<int>();

            try
            {
                if (dsSubscriptions != null && dsSubscriptions.Tables.Count > 0 && dsSubscriptions.Tables[0] != null && dsSubscriptions.Tables[1] != null)
                {
                    DataTable dtSubscriptions = dsSubscriptions.Tables[0];
                    DataTable dtSubscriptionUserTypes = dsSubscriptions.Tables[1];
                    DataTable dtDistinctUserTypes = dtSubscriptionUserTypes.DefaultView.ToTable(true, "user_type_id");

                    if (dtDistinctUserTypes.Rows.Count > 0)
                    {
                        foreach (DataRow userTypeRow in dtDistinctUserTypes.Rows)
                        {
                            int userTypeID = ODBCWrapper.Utils.GetIntSafeVal(userTypeRow["user_type_id"]);
                            userTypesIDsList.Add(userTypeID);
                        }

                        if (userTypesIDsList != null && userTypesIDsList.Count > 0)
                        {
                            DataTable dtUserTypes = UsersDal.GetUserTypeDataByIDs(m_nGroupID, userTypesIDsList);
                            if (dtUserTypes != null && dtUserTypes.Rows.Count > 0)
                            {
                                List<UserType> userTypesList = GetUserTypesList(dtUserTypes);
                                if (userTypesList != null && userTypesList.Count > 0)
                                {
                                    foreach (DataRow subUserTypeRow in dtSubscriptionUserTypes.Rows)
                                    {
                                        int subID = ODBCWrapper.Utils.GetIntSafeVal(subUserTypeRow["subscription_id"]);
                                        int subUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(subUserTypeRow["user_type_id"]);
                                        if (!retDict.ContainsKey(subID))
                                        {
                                            retDict.Add(subID, new List<UserType>());
                                        }
                                        retDict[subID].Add(userTypesList.Find(x => x.ID == subUserTypeID));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("GetSubscriptionsUserTypes - Error on GetSubscriptionsUserTypes:" + ex.ToString(), ex);
            }

            return retDict;
        }

        private List<UserType> GetUserTypesList(DataTable dtUserTypes)
        {
            List<UserType> userTypesList = new List<UserType>();

            foreach (DataRow drUserType in dtUserTypes.Rows)
            {
                int? nUserTypeID = ODBCWrapper.Utils.GetIntSafeVal(drUserType["id"]);
                string sUserTypeDesc = ODBCWrapper.Utils.GetSafeStr(drUserType["description"]);
                bool isDefault = Convert.ToBoolean(ODBCWrapper.Utils.GetByteSafeVal(drUserType, "is_default"));
                UserType userType = new UserType(nUserTypeID, sUserTypeDesc, isDefault);
                userTypesList.Add(userType);
            }
            return userTypesList;
        }

        private Subscription CreateSubscriptionObject(bool bShrink, DataRow subscriptionRow, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME,
                                                      int[] nFileTypes, LanguageContainer[] subscriptionDescription, BundleCodeContainer[] subscriptionChannels,
                                                      LanguageContainer[] subscriptionName, ServiceObject[] services, UserType[] userTypes,
                                                      List<SubscriptionCouponGroup> couponsGroup)
        {
            Subscription retSubscription = new Subscription();

            int nSubscriptionCode = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["ID"]);
            int nDlmID = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["device_limit_id"]);
            string sPriceCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["PRICE_CODE"]);
            string priority = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["order_num"]);
            string sSubPriceCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["SUB_PRICE_CODE"]);
            string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["USAGE_MODULE_CODE"]);
            string sSubscriptionUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["SUB_USAGE_MODULE_CODE"]);
            string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["DISCOUNT_MODULE_CODE"]);
            string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["COUPON_GROUP_CODE"]);
            string sName = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["NAME"]);
            int gracePeriodMinutes = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["GRACE_PERIOD_MINUTES"]);
            string adsParam = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["ADS_PARAM"]);

            int adsPolicyInt = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["ADS_POLICY"]);
            AdsPolicy? adsPolicy = null;
            if (adsPolicyInt > 0)
            {
                adsPolicy = (AdsPolicy)adsPolicyInt;
            }

            int nIsRecurring = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["IS_RECURRING"]);
            bool bIsRecurring = false;
            if (nIsRecurring == 1)
            {
                bIsRecurring = true;
            }
            int nNumOfPeriods = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["NUM_OF_REC_PERIODS"]);

            DateTime dStart = new DateTime(2000, 1, 1);
            DateTime dEnd = new DateTime(2099, 1, 1);
            if (subscriptionRow["START_DATE"] != null && subscriptionRow["START_DATE"] != DBNull.Value)
            {
                dStart = (DateTime)(subscriptionRow["START_DATE"]);
            }

            if (subscriptionRow["END_DATE"] != null && subscriptionRow["END_DATE"] != DBNull.Value)
            {
                dEnd = (DateTime)(subscriptionRow["END_DATE"]);
            }

            string sExtDiscount = string.Empty;
            if (subscriptionRow["Ext_discount_module"] != null && subscriptionRow["Ext_discount_module"] != DBNull.Value)
            {
                sExtDiscount = (subscriptionRow["Ext_discount_module"]).ToString();
            }

            string sProductCode = ODBCWrapper.Utils.GetSafeStr(subscriptionRow["Product_Code"]);
            int nSubscriptionGeoCommerceID = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["geo_commerce_block_id"]);
            long lPreviewModuleID = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["preview_module_id"]);

            if (bShrink)
            {
                retSubscription.Initialize(sPriceCode, string.Empty, string.Empty, string.Empty, subscriptionDescription, m_nGroupID, nSubscriptionCode.ToString(),
                                           subscriptionChannels, dStart, dEnd, nFileTypes, bIsRecurring, nNumOfPeriods, subscriptionName, sSubPriceCode, sSubscriptionUsageModuleCode, sName,
                                           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, priority, sProductCode, sExtDiscount, userTypes, services, lPreviewModuleID, nSubscriptionGeoCommerceID, nDlmID,
                                           gracePeriodMinutes, adsPolicy, adsParam, couponsGroup);


            }
            else
            {
                retSubscription.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, subscriptionDescription, m_nGroupID, nSubscriptionCode.ToString(),
                                           subscriptionChannels, dStart, dEnd, nFileTypes, bIsRecurring, nNumOfPeriods, subscriptionName, sSubPriceCode, sSubscriptionUsageModuleCode, sName,
                                           sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, priority, sProductCode, sExtDiscount, userTypes, services, lPreviewModuleID, nSubscriptionGeoCommerceID, nDlmID,
                                           gracePeriodMinutes, adsPolicy, adsParam, couponsGroup);
            }


            return retSubscription;
        }


        private List<Subscription> CreateSubscriptionsListFromDataSet(bool bShrink, DataSet dsSubscriptions, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            List<Subscription> retList = new List<Subscription>();
            Subscription tmpSubscription = new Subscription();

            if (dsSubscriptions != null && dsSubscriptions.Tables.Count > 0)
            {
                int nCount = dsSubscriptions.Tables[0].DefaultView.Count;

                if (nCount > 0)
                {
                    DataTable dtSubscription = dsSubscriptions.Tables[0];
                    Dictionary<int, List<UserType>> dictSubUserTypes = GetSubscriptionsUserTypes(dsSubscriptions);
                    foreach (DataRow subscriptionRow in dtSubscription.Rows)
                    {
                        int nSubscriptionCode = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["ID"]);
                        int dlmID = ODBCWrapper.Utils.GetIntSafeVal(subscriptionRow["device_limit_id"]);
                        Int32[] nFileTypes = GetSubscriptionFileTypes(nSubscriptionCode, m_nGroupID);
                        UserType[] arrUserTypes = null;
                        if (dictSubUserTypes != null && dictSubUserTypes.ContainsKey(nSubscriptionCode))
                        {
                            arrUserTypes = dictSubUserTypes[nSubscriptionCode].ToArray();
                        }
                        tmpSubscription = CreateSubscriptionObject(bShrink, subscriptionRow, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nFileTypes, GetSubscriptionDescription(nSubscriptionCode),
                                                                   GetSubscriptionsChannels(nSubscriptionCode, m_nGroupID), GetSubscriptionName(nSubscriptionCode), 
                                                                   GetSubscriptionServices(nSubscriptionCode), arrUserTypes,
                                                                   GetSubscriptionCouponsGroup(nSubscriptionCode));
                        retList.Add(tmpSubscription);
                    }

                }
            }
            return retList;
        }

        public override Subscription[] GetSubscriptionsData(string[] oSubCodes, string sCountryCd, string sLanguageCode, string sDeviceName, SubscriptionOrderBy orderBy)
        {
            Subscription[] res = null;
            try
            {
                if (oSubCodes != null && oSubCodes.Length > 0)
                {
                    List<long> lSubCodes = new List<long>(oSubCodes.Length);
                    for (int i = 0; i < oSubCodes.Length; i++)
                    {
                        long temp = 0;
                        if (Int64.TryParse(oSubCodes[i], out temp) && temp > 0)
                        {
                            lSubCodes.Add(temp);
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("Failed to parse: {0} into long", oSubCodes[i]));
                        }
                    } // end for

                    if (lSubCodes.Count > 0)
                    {
                        DataSet ds = PricingDAL.Get_SubscriptionsData(m_nGroupID, lSubCodes);
                        if (IsSubsDataSetValid(ds))
                        {
                            Dictionary<long, List<int>> subsFileTypesMapping = ExtractSubscriptionsFileTypes(ds);
                            Dictionary<long, List<LanguageContainer>> subsDescriptionsMapping = ExtractSubscriptionsDescriptions(ds);
                            Dictionary<long, List<BundleCodeContainer>> subsChannelsMapping = ExtractSubscriptionsChannels(ds);
                            Dictionary<long, List<LanguageContainer>> subsNamesMapping = ExtractSubscriptionsNames(ds);
                            Dictionary<long, List<ServiceObject>> subsServicesMapping = ExtractSubscriptionsServices(ds);
                            Dictionary<long, List<SubscriptionCouponGroup>> subsCouponsGroup = ExtractSubscriptionsCouponGroup(ds);

                            res = CreateSubscriptions(ds, subsFileTypesMapping, subsDescriptionsMapping, subsChannelsMapping, subsNamesMapping, subsServicesMapping,
                                sCountryCd, sLanguageCode, sDeviceName, subsCouponsGroup).ToArray();
                        }
                        else
                        {
                            log.Error("Error - " + GetGetSubscriptionsDataErrMsg("Subs DataSet is not valid", lSubCodes));
                        }
                    }
                    else
                    {
                        log.Error("Error - " + GetGetSubscriptionsDataErrMsg("SubCodes is empty", lSubCodes));
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionsData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLanguageCode));
                sb.Append(String.Concat(" D Name: ", sDeviceName));
                if (oSubCodes != null && oSubCodes.Length > 0)
                {
                    sb.Append(" Sub Codes: ");
                    for (int i = 0; i < oSubCodes.Length; i++)
                    {
                        sb.Append(String.Concat(oSubCodes[i], "; "));
                    }
                }
                else
                {
                    sb.Append("Sub Codes is empty. ");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }

            return res;
        }

        private Dictionary<long, List<SubscriptionCouponGroup>> ExtractSubscriptionsCouponGroup(DataSet ds)
        {
            Dictionary<long, List<SubscriptionCouponGroup>> res = new Dictionary<long, List<SubscriptionCouponGroup>>();
            DataTable dt = ds.Tables[7];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                SubscriptionCouponGroup scg = new SubscriptionCouponGroup();
                foreach (DataRow dr in dt.Rows)
                {
                    long subID = ODBCWrapper.Utils.GetLongSafeVal(dr, "subscription_id");
                    long couponGroupID = ODBCWrapper.Utils.GetLongSafeVal(dr, "COUPON_GROUP_ID");
                    DateTime startDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "START_DATE");
                    DateTime endDate = ODBCWrapper.Utils.GetDateSafeVal(dr, "END_DATE");

                    CouponsGroup couponGroupData = null;
                    if (couponGroupID > 0)
                    {
                        BaseCoupons c = null;
                        Utils.GetBaseImpl(ref c, m_nGroupID);
                        if (c != null)
                        {
                            couponGroupData = c.GetCouponGroupData(couponGroupID.ToString());
                        }
                    }
                    scg.Initialize(startDate, endDate, couponGroupData);

                    if (res.ContainsKey(subID))
                    {
                        res[subID].Add(scg);
                    }
                    else
                    {
                        List<SubscriptionCouponGroup> sgList = new List<SubscriptionCouponGroup>();
                        sgList.Add(scg);
                        res.Add(subID, sgList);
                    }
                }
            }
            return res;
        }

        public override Subscription[] GetSubscriptionsDataByProductCodes(List<string> productCodes, bool getAlsoUnactive)
        {
            Subscription[] subscriptions = null;
            if (productCodes != null && productCodes.Count > 0)
            {
                string[] subscriptionsCodes = DAL.PricingDAL.Get_SubscriptionsFromProductCodes(productCodes.Distinct().ToList(), m_nGroupID).Keys.ToArray();
                if (subscriptionsCodes != null && subscriptionsCodes.Length > 0)
                {
                    subscriptions = GetSubscriptionsData(subscriptionsCodes, string.Empty, string.Empty, string.Empty, SubscriptionOrderBy.StartDateAsc);
                }
            }

            return subscriptions;
        }            

        private Dictionary<long, List<ServiceObject>> ExtractSubscriptionsServices(DataSet ds)
        {
            Dictionary<long, List<ServiceObject>> res = new Dictionary<long, List<ServiceObject>>();
            DataTable dt = ds.Tables[6];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                ServiceObject service;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long subID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                    long serviceID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["service_id"]);
                    string desc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    long quota = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["QUOTA_IN_MINUTES"]);

                    if (serviceID == (int)eService.NPVR)
                    {
                        service = new NpvrServiceObject(serviceID, desc, quota);
                    }
                    else
                    {
                        service = new ServiceObject(serviceID, desc);
                    }

                    if (res.ContainsKey(subID))
                    {
                        res[subID].Add(service);
                    }
                    else
                    {
                        List<ServiceObject> services = new List<ServiceObject>();
                        services.Add(service);
                        res.Add(subID, services);
                    }
                }
            }
            return res;
        }

        private List<Subscription> CreateSubscriptions(DataSet ds, Dictionary<long, List<int>> subsFileTypesMapping,
            Dictionary<long, List<LanguageContainer>> subsDescriptionsMapping, Dictionary<long, List<BundleCodeContainer>> subsChannelsMapping,
            Dictionary<long, List<LanguageContainer>> subsNamesMapping, Dictionary<long, List<ServiceObject>> subsServicesMapping, string sCountryCd, string sLanguageCode, string sDeviceName,
             Dictionary<long, List<SubscriptionCouponGroup>> subsCouponsGroup)
        {
            List<Subscription> res = null;
            DataTable subsTable = ds.Tables[0];
            if (subsTable != null && subsTable.Rows != null && subsTable.Rows.Count > 0)
            {
                res = new List<Subscription>(subsTable.Rows.Count);
                Dictionary<int, List<UserType>> dictSubUserTypes = GetSubscriptionsUserTypes(ds);

                for (int i = 0; i < subsTable.Rows.Count; i++)
                {
                    long lSubCode = ODBCWrapper.Utils.GetLongSafeVal(subsTable.Rows[i]["ID"]);

                    // get sub file types
                    int[] nFileTypes = null;
                    if (subsFileTypesMapping.ContainsKey(lSubCode))
                    {
                        nFileTypes = subsFileTypesMapping[lSubCode].ToArray();
                    }

                    // get sub user types
                    UserType[] arrUserTypes = null;
                    if (dictSubUserTypes != null && dictSubUserTypes.ContainsKey((int)lSubCode))
                    {
                        arrUserTypes = dictSubUserTypes[(int)lSubCode].ToArray();
                    }

                    // get sub descriptions
                    LanguageContainer[] descs = null;
                    if (subsDescriptionsMapping.ContainsKey(lSubCode))
                    {
                        descs = subsDescriptionsMapping[lSubCode].ToArray();
                    }

                    // get sub channels
                    BundleCodeContainer[] bcc = null;
                    if (subsChannelsMapping.ContainsKey(lSubCode))
                    {
                        bcc = subsChannelsMapping[lSubCode].ToArray();
                    }

                    // get sub names
                    LanguageContainer[] names = null;
                    if (subsNamesMapping.ContainsKey(lSubCode))
                    {
                        names = subsNamesMapping[lSubCode].ToArray();
                    }

                    // get sub services
                    ServiceObject[] services = null;
                    if (subsServicesMapping.ContainsKey(lSubCode))
                    {
                        services = subsServicesMapping[lSubCode].ToArray();
                    }

                    List<SubscriptionCouponGroup> couponsGroup = null;
                    if (subsCouponsGroup.ContainsKey(lSubCode))
                    {
                        couponsGroup = subsCouponsGroup[lSubCode].ToList();
                    }

                    res.Add(CreateSubscriptionObject(false, subsTable.Rows[i], sCountryCd, sLanguageCode, sDeviceName, nFileTypes, descs,
                        bcc, names, services, arrUserTypes, couponsGroup));


                }

            }
            else
            {
                res = new List<Subscription>(0);
            }

            return res;
        }

        private Dictionary<long, List<LanguageContainer>> ExtractSubscriptionsNames(DataSet ds)
        {
            Dictionary<long, List<LanguageContainer>> res = new Dictionary<long, List<LanguageContainer>>();
            DataTable dt = ds.Tables[5];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["SUBSCRIPTION_ID"]);
                    string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                    string sDesc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(sLanguageCode, sDesc);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(lc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<LanguageContainer>() { lc });
                    }
                }
            }

            return res;
        }

        private Dictionary<long, List<BundleCodeContainer>> ExtractSubscriptionsChannels(DataSet ds)
        {
            Dictionary<long, List<BundleCodeContainer>> res = new Dictionary<long, List<BundleCodeContainer>>();
            DataTable dt = ds.Tables[4];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["SUBSCRIPTION_ID"]);
                    long lChannelID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["CHANNEL_ID"]);
                    BundleCodeContainer bcc = new BundleCodeContainer();
                    bcc.Initialize(lChannelID + "", string.Empty);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(bcc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<BundleCodeContainer>() { bcc });
                    }
                }
            }

            return res;
        }

        private Dictionary<long, List<LanguageContainer>> ExtractSubscriptionsDescriptions(DataSet ds)
        {
            Dictionary<long, List<LanguageContainer>> res = new Dictionary<long, List<LanguageContainer>>();
            DataTable dt = ds.Tables[3];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["subscription_id"]);
                    string sLanguageCode = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["language_code3"]);
                    string sDesc = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i]["description"]);
                    LanguageContainer lc = new LanguageContainer();
                    lc.Initialize(sLanguageCode, sDesc);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(lc);
                    }
                    else
                    {
                        res.Add(lSubID, new List<LanguageContainer>() { lc });
                    }
                }
            }

            return res;
        }

        private Dictionary<long, List<int>> ExtractSubscriptionsFileTypes(DataSet ds)
        {
            Dictionary<long, List<int>> res = new Dictionary<long, List<int>>();
            DataTable dt = ds.Tables[2];
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lSubID = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[i]["SUBSCRIPTION_ID"]);
                    int nFileTypeID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i]["FILE_TYPE_ID"]);
                    if (res.ContainsKey(lSubID))
                    {
                        res[lSubID].Add(nFileTypeID);
                    }
                    else
                    {
                        res.Add(lSubID, new List<int>() { nFileTypeID });
                    }
                }
            }

            return res;

        }



        private string GetGetSubscriptionsDataErrMsg(string sMsg, List<long> lSubCodes)
        {
            StringBuilder sb = new StringBuilder(String.Concat(sMsg, ". "));
            if (lSubCodes != null && lSubCodes.Count > 0)
            {
                sb.Append("SubCodes: ");
                for (int i = 0; i < lSubCodes.Count; i++)
                {
                    sb.Append(String.Concat(lSubCodes, ","));
                }
            }
            else
            {
                sb.Append("SubCodes is null or empty.");
            }

            return sb.ToString();
        }

        private bool IsSubsDataSetValid(DataSet ds)
        {
            return ds != null && ds.Tables != null && ds.Tables.Count == 8;
        }

    }
}
