using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using ApiObjects;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Pricing;
using System.Data;
using System.Xml;
using DAL;

namespace Core.Pricing
{
    public class Utils
    {
        private static string PRICING_CONNECTION = "PRICING_CONNECTION";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static string GetValFromConfig(string sKey)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(sKey);
        }

        public static TimeSpan GetEndDateTimeSpan(Int32 nVal)
        {
            DateTime dEnd = DateTime.UtcNow;
            DateTime dStart = DateTime.UtcNow;
            if (nVal == 1111111)
                dEnd = dEnd.AddMonths(1);
            else if (nVal == 2222222)
                dEnd = dEnd.AddMonths(2);
            else if (nVal == 3333333)
                dEnd = dEnd.AddMonths(3);
            else if (nVal == 4444444)
                dEnd = dEnd.AddMonths(4);
            else if (nVal == 5555555)
                dEnd = dEnd.AddMonths(5);
            else if (nVal == 6666666)
                dEnd = dEnd.AddMonths(6);
            else if (nVal == 9999999)
                dEnd = dEnd.AddMonths(9);
            else if (nVal == 11111111)
                dEnd = dEnd.AddYears(1);
            else if (nVal == 22222222)
                dEnd = dEnd.AddYears(2);
            else if (nVal == 33333333)
                dEnd = dEnd.AddYears(3);
            else if (nVal == 44444444)
                dEnd = dEnd.AddYears(4);
            else if (nVal == 55555555)
                dEnd = dEnd.AddYears(5);
            else
                dEnd = dEnd.AddMinutes(nVal);
            return dEnd - dStart;
        }

        public static int GetGroupID(string sWSUserName, string sWSPassword)
        {
            Credentials wsc = new Credentials(sWSUserName, sWSPassword);
            int nGroupID = TvinciCache.WSCredentials.GetGroupID(eWSModules.PRICING, wsc);

            return nGroupID;
        }

        internal static int GetModuleImplID(int nGroupID, ePricingModules ePricingModule)
        {
            int nImplID = TvinciCache.ModulesImplementation.GetModuleID(eWSModules.PRICING, nGroupID, (int)ePricingModule, PRICING_CONNECTION);

            return nImplID;
        }

        internal static void GetBaseImpl(ref BaseDiscount t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Discount);

            if (nImplID == 1)
                t = new TvinciDiscount(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCampaign t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Campaign);

            if (nImplID == 1)
                t = new TvinciCampaign(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseUsageModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.UsageModule);

            if (nImplID == 1)
                t = new TvinciUsageModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePPVModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.PPV);

            if (nImplID == 1)
                t = new TvinciPPVModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseSubscription t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Subscription);

            if (nImplID == 1)
                t = new TvinciSubscription(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCollection t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Collection);

            if (nImplID == 1)
                t = new TvinciCollection(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePricing t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Pricing);

            if (nImplID == 1)
                t = new TvinciPricing(nGroupID);
        }

        internal static void GetBaseImpl(ref BaseCoupons t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Coupons);

            if (nImplID == 1)
                t = new TvinciCoupons(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePrePaidModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.PrePaid);

            if (nImplID == 1)
                t = new TvinciPrePaidModule(nGroupID);
        }

        internal static void GetBaseImpl(ref BasePreviewModule t, Int32 nGroupID)
        {
            int nImplID = GetModuleImplID(nGroupID, ePricingModules.Preview);

            if (nImplID == 1)
                t = new TvinciPreviewModule(nGroupID);
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePricing t)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static BasePricing GetBasePricing(int groupID, string sFunctionName)
        {
            BasePricing t = null;
            if (groupID != 0)
                GetBaseImpl(ref t, groupID);
            else
                log.Debug("WS ignored - " + string.Format("groupID:{0}, func:{1}", groupID, sFunctionName));

            return t;
        }

     
        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCampaign t)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCoupons t)
        {
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                Utils.GetBaseImpl(ref t, nGroupID);
            }
            else
            {
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            }
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseDiscount t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseUsageModule t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));

            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePPVModule t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseSubscription t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BaseCollection t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePrePaidModule t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static Int32 GetGroupID(string sWSUserName, string sWSPassword, string sFunctionName, ref BasePreviewModule t)
        {
            int nGroupID = 0;
            nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
                GetBaseImpl(ref t, nGroupID);
            else
                log.Debug("WS ignored - " + string.Format("user:{0}, pass:{1}, func:{2}", sWSPassword, sWSPassword, sFunctionName));
            return nGroupID;
        }

        public static void GetWSCredentials(int nGroupID, eWSModules eWSModule, string sFunctionName, ref string sUN, ref string sPass)
        {
            Credentials uc = TvinciCache.WSCredentials.GetWSCredentials(eWSModules.PRICING, nGroupID, eWSModule);
            sUN = uc.m_sUsername;
            sPass = uc.m_sPassword;
        }

        internal static Tuple<PriceCode, bool> GetPriceCodeByCountryAndCurrency(Dictionary<string, object> funcParams)
        {
            bool res = false;
            PriceCode priceCode = null;

            try
            {
                if (funcParams != null && funcParams.Count == 4)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("priceCodeId") && funcParams.ContainsKey("countryCode") && funcParams.ContainsKey("currencyCode"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        int? priceCodeId = funcParams["priceCodeId"] as int?;
                        string countryCode = funcParams["countryCode"].ToString();
                        string currencyCode = funcParams["currencyCode"].ToString();
                        if (groupId.HasValue && priceCodeId.HasValue && !string.IsNullOrEmpty(countryCode) && !string.IsNullOrEmpty(currencyCode))
                        {
                            priceCode = new TvinciPricing(groupId.Value).GetPriceCodeDataByCountyAndCurrency(priceCodeId.Value, countryCode, currencyCode);
                            if (priceCode != null)
                            {
                                res = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetPriceCodeByCountryAndCurrency failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<PriceCode, bool>(priceCode, res);
        }

        internal static Tuple<DiscountModule, bool> GetDiscountModuleByCountryAndCurrency(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DiscountModule discountModule = null;

            try
            {
                if (funcParams != null && funcParams.Count == 4)
                {
                    if (funcParams.ContainsKey("groupId") && funcParams.ContainsKey("discountCodeId") && funcParams.ContainsKey("countryCode") && funcParams.ContainsKey("currencyCode"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        int? discountCodeId = funcParams["discountCodeId"] as int?;
                        string countryCode = funcParams["countryCode"].ToString();
                        string currencyCode = funcParams["currencyCode"].ToString();
                        if (groupId.HasValue && discountCodeId.HasValue && !string.IsNullOrEmpty(countryCode) && !string.IsNullOrEmpty(currencyCode))
                        {
                            discountModule = GetDiscountModuleByCountryAndCurrency(discountCodeId.Value, countryCode, currencyCode);
                            if (discountModule != null)
                            {
                                res = true;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetDiscountModuleByCountryAndCurrency failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<DiscountModule, bool>(discountModule, res);
        }

        private static DiscountModule GetDiscountModuleByCountryAndCurrency(int discountCodeId, string countryCode, string currencyCode)
        {
            DiscountModule discountModule = null;
            try
            {
                DataSet ds = DAL.PricingDAL.GetDiscountModuleLocale(discountCodeId, countryCode, currencyCode);
                DataRow dr = null;
                if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables.Count == 2)
                    {
                        dr = ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count == 1 ? ds.Tables[1].Rows[0] : null;
                    }
                    else
                    {
                        dr = ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count == 1 ? ds.Tables[0].Rows[0] : null;
                    }
                }
                if (dr != null)
                {
                    int currencyId = ODBCWrapper.Utils.GetIntSafeVal(dr, "CURRENCY_CD");
                    string discountCodeName = ODBCWrapper.Utils.GetSafeStr(dr, "CODE");
                    double price = ODBCWrapper.Utils.GetDoubleSafeVal(dr, "PRICE");
                    double discountPercent = ODBCWrapper.Utils.GetDoubleSafeVal(dr, "DISCOUNT_PERCENT");
                    RelationTypes theRelationType = (RelationTypes)ODBCWrapper.Utils.GetIntSafeVal(dr, "RELATION_TYPE");
                    Price localPrice = new Price();
                    localPrice.InitializeByCodeID(currencyId, price);
                    DateTime? startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "START_DATE");
                    startDate = startDate.HasValue ? startDate : new DateTime(2000, 1, 1);
                    DateTime? endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "END_DATE");
                    endDate = endDate.HasValue ? endDate : new DateTime(2099, 1, 1);
                    WhenAlgoType oWhenAlgoType = (WhenAlgoType)ODBCWrapper.Utils.GetIntSafeVal(dr, "WHENALGO_TYPE");
                    int nWhenAlgoTimes = ODBCWrapper.Utils.GetIntSafeVal(dr, "WHENALGO_TIMES");
                    WhenAlgo wa = new WhenAlgo();
                    wa.Initialize(oWhenAlgoType, nWhenAlgoTimes);
                    discountModule = new DiscountModule();
                    discountModule.Initialize(discountCodeName, localPrice, DiscountModule.GetDiscountCodeDescription(discountCodeId), discountCodeId,
                                              discountPercent, theRelationType, startDate.Value, endDate.Value, wa);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetDiscountModuleByCountryAndCurrency, discountCodeId: {0}, countryCode: {1}, currencyCode: {2}",
                            discountCodeId, countryCode, !string.IsNullOrEmpty(currencyCode) ? currencyCode : string.Empty), ex);
            }

            return discountModule;
        }

        internal static Tuple<APILogic.ConditionalAccess.AdsControlData, bool> GetGetGroupAdsControl(Dictionary<string, object> funcParams)
        {
            bool res = false;
            APILogic.ConditionalAccess.AdsControlData adsData = null;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        DataTable dt = DAL.PricingDAL.GetGroupAdsControlParams(groupId.Value);
                        if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                        {
                            adsData = new APILogic.ConditionalAccess.AdsControlData();
                            int adsPolicy = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ADS_POLICY"]);
                            if (adsPolicy > 0)
                            {
                                adsData.AdsPolicy = (ApiObjects.AdsPolicy)adsPolicy;
                                adsData.AdsParam = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["ADS_PARAM"]);
                            }
                            res = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGetGroupAdsControl failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            return new Tuple<APILogic.ConditionalAccess.AdsControlData, bool>(adsData, res);
        }

        internal static void BuildCouponXML(XmlNode rootNode, XmlDocument xmlDoc, string couponCode, int groupId, long couponGroupId)
        {
            XmlNode rowNode;
            XmlNode codeNode;
            XmlNode groupIdNode;
            XmlNode couponGroupIdNode;

            rowNode = xmlDoc.CreateElement("row");

            codeNode = xmlDoc.CreateElement("code");
            codeNode.InnerText = couponCode;
            rowNode.AppendChild(codeNode);

            groupIdNode = xmlDoc.CreateElement("group_id");
            groupIdNode.InnerText = groupId.ToString();
            rowNode.AppendChild(groupIdNode);

            couponGroupIdNode = xmlDoc.CreateElement("coupon_group_id");
            couponGroupIdNode.InnerText = couponGroupId.ToString();
            rowNode.AppendChild(couponGroupIdNode);

            rootNode.AppendChild(rowNode);
        }

        public static int GetIntValFromConfig(string sKey, int deafultVal)
        {
            int result = 0;
            result = TVinciShared.WS_Utils.GetTcmIntValue(sKey);
            if (result == 0)
            {
                result = deafultVal;
            }
            return result;
        }

        public static List<SubscriptionCouponGroup> GetSubscriptionCouponsGroup(long subscriptionId, int groupId, bool withExpiry = true)
        {
            List<SubscriptionCouponGroup> sgList = new List<SubscriptionCouponGroup>();
            DataTable dt;
            if (withExpiry)
            {
                dt = PricingDAL.Get_SubscriptionsCouponGroup(groupId, new List<long>(1) { subscriptionId });
            }
            else
            {
                dt = PricingDAL.Get_SubscriptionsCouponGroupWithExpiry(groupId, new List<long>(1) { subscriptionId });
            }

            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                SubscriptionCouponGroup scg = null;
                foreach (DataRow dr in dt.Rows)
                {
                    scg = new SubscriptionCouponGroup();
                    long subID = ODBCWrapper.Utils.GetLongSafeVal(dr, "subscription_id");
                    long couponGroupID = ODBCWrapper.Utils.GetLongSafeVal(dr, "COUPON_GROUP_ID");
                    DateTime? startDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "START_DATE");
                    DateTime? endDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "END_DATE");

                    CouponsGroup couponGroupData = null;
                    if (couponGroupID > 0)
                    {
                        BaseCoupons c = null;
                        GetBaseImpl(ref c, groupId);
                        if (c != null)
                        {
                            couponGroupData = c.GetCouponGroupData(couponGroupID.ToString());
                        }
                    }
                    scg.Initialize(startDate, endDate, couponGroupData);
                    sgList.Add(scg);
                }
            }
            return sgList;
        }      
    }
}
