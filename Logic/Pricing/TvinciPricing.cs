using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Services;
using KLogMonitor;
using ApiObjects.Response;
using ApiObjects;
using System.Data;
using ApiObjects.Pricing;
using System.Xml;
using ApiObjects.IngestBusinessModules;

namespace Core.Pricing
{
    public class TvinciPricing : BasePricing
    {
        private static string PRICING_CONNECTION = "PRICING_CONNECTION";
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string INGEST_ERROR_NOT_EXISTS_FORMAT = "{0} = '{1}' does not exist";
        private const string INGEST_ERROR_NOT_EXIST_FORMAT = "the following {0} do not exist: '{1}'";
        private const string INGEST_ERROR_ALREADY_EXISTS_FORMAT = "{0} = '{1}' already exists";
        private const string INGEST_ERROR_MANDATORY_FORMAT = "{0} is mandatory";
        private const string INGEST_FAILED_ERROR_FORMAT = "failed to {0}";
        private const string INGEST_UNEXPECTED_ERROR = "unexpected error";

        public const int NUM_OF_TRY = 10; // add this val to TCM

        public TvinciPricing(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        static protected LanguageContainer[] GetPriceCodeDescription(Int32 nPriceCodeID)
        {
            LanguageContainer[] theContainer = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(PRICING_CONNECTION);
                selectQuery += "select * from price_code_descriptions with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("price_code_id", "=", nPriceCodeID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        theContainer = new LanguageContainer[nCount];
                    }
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sLang = selectQuery.Table("query").DefaultView[i].Row["language_code3"].ToString();
                        string sVal = selectQuery.Table("query").DefaultView[i].Row["description"].ToString();
                        LanguageContainer t = new LanguageContainer();
                        t.Initialize(sLang, sVal);
                        theContainer[nIndex] = t;
                        nIndex++;
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
            return theContainer;
        }

        protected PriceCode GetPriceCodeLocale(Int32 nPriceCodeID, string sCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            if (String.IsNullOrEmpty(sCountryCd))
                sCountryCd = string.Empty;
            if (String.IsNullOrEmpty(sLANGUAGE_CODE))
                sLANGUAGE_CODE = string.Empty;
            if (String.IsNullOrEmpty(sDEVICE_NAME))
                sDEVICE_NAME = string.Empty;
            PriceCode t = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(PRICING_CONNECTION);
                selectQuery += "select * from price_codes_locales with (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PRICE_CODE_ID", "=", nPriceCodeID);
                selectQuery += " and (COUNTRY_CODE is null or COUNTRY_CODE='' or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("COUNTRY_CODE", "=", sCountryCd);
                selectQuery += ") and (LANGUAGE_CODE is null or LANGUAGE_CODE='' or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("LANGUAGE_CODE", "=", sLANGUAGE_CODE);
                selectQuery += ") and (DEVICE_NAME is null or DEVICE_NAME='' or ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("DEVICE_NAME", "=", sDEVICE_NAME);
                selectQuery += ")";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    Int32 nTopScore = 0;
                    if (nCount > 0)
                    {
                        double dPrice = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                        Int32 nCurrency = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CD"].ToString());

                        Int32 nScore = 0;
                        string sCountry = selectQuery.Table("query").DefaultView[0].Row["COUNTRY_CODE"].ToString();
                        if (sCountry == sCountryCd)
                            nScore += 2;
                        string sLang = selectQuery.Table("query").DefaultView[0].Row["LANGUAGE_CODE"].ToString();
                        if (sLang == sLANGUAGE_CODE)
                            nScore += 1;
                        string sDevice = selectQuery.Table("query").DefaultView[0].Row["DEVICE_NAME"].ToString();
                        if (sDevice == sDEVICE_NAME)
                            nScore += 3;
                        if (nScore > nTopScore)
                        {
                            Price oPrise = new Price();
                            oPrise.InitializeByCodeID(nCurrency, dPrice);
                            if (t == null)
                                t = new PriceCode();
                            t.Initialize(sCode, oPrise, GetPriceCodeDescription(nPriceCodeID), nPriceCodeID);
                        }
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
            return t;
        }

        protected PriceCode[] GetTvinciPriceCodesList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PriceCode[] tmp = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(PRICING_CONNECTION);
                selectQuery += "select * from price_codes with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        tmp = new PriceCode[nCount];
                    Int32 nIndex = 0;
                    for (int i = 0; i < nCount; i++)
                    {
                        string sCode = selectQuery.Table("query").DefaultView[i].Row["CODE"].ToString();
                        Int32 nPriceCodeID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString()); ;
                        double dPrice = double.Parse(selectQuery.Table("query").DefaultView[i].Row["PRICE"].ToString());
                        Int32 nCurrency = int.Parse(selectQuery.Table("query").DefaultView[i].Row["CURRENCY_CD"].ToString());
                        PriceCode t = GetPriceCodeLocale(nPriceCodeID, sCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (t == null)
                        {
                            t = new PriceCode();
                            Price oPrise = new Price();
                            oPrise.InitializeByCodeID(nCurrency, dPrice);
                            t.Initialize(sCode, oPrise, GetPriceCodeDescription(nPriceCodeID), nPriceCodeID);
                        }
                        tmp[nIndex] = t;
                        nIndex++;
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
            return tmp;
        }

        public override PriceCode[] GetPriceCodeList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            try
            {
                return GetTvinciPriceCodesList(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPriceCodeList. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
        }

        public override PriceCode GetPriceCodeData(string sPC, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            Int32 nPriceCodeID = 0;

            if (!Int32.TryParse(sPC, out nPriceCodeID) || nPriceCodeID < 1)
            {
                return null;
            }
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            PriceCode tmp = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey(PRICING_CONNECTION);
                selectQuery += "select * from price_codes with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nPriceCodeID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        tmp = null;
                        string sCode = selectQuery.Table("query").DefaultView[0].Row["CODE"].ToString(); ;
                        double dPrice = double.Parse(selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString());
                        Int32 nCurrency = int.Parse(selectQuery.Table("query").DefaultView[0].Row["CURRENCY_CD"].ToString());
                        tmp = GetPriceCodeLocale(nPriceCodeID, sCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                        if (tmp == null)
                        {
                            tmp = new PriceCode();
                            Price oPrise = new Price();
                            oPrise.InitializeByCodeID(nCurrency, dPrice);
                            tmp.Initialize(sCode, oPrise, GetPriceCodeDescription(nPriceCodeID), nPriceCodeID);
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPriceCodeData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" PC: ", sPC));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
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
            }
            return tmp;
        }

        public override PriceCode GetPriceCodeDataByCountyAndCurrency(int priceCodeId, string countryCode, string currencyCode)
        {
            PriceCode res = null;
            try
            {
                DataSet ds = DAL.PricingDAL.GetPriceCodeLocale(priceCodeId, countryCode, currencyCode);
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
                    string priceCodeName = ODBCWrapper.Utils.GetSafeStr(dr, "CODE");
                    if (currencyId > 0 && !string.IsNullOrEmpty(priceCodeName))
                    {
                        double price = ODBCWrapper.Utils.GetDoubleSafeVal(dr, "PRICE");
                        Price localePrice = new Price();
                        localePrice.InitializeByCodeID(currencyId, price);
                        res = new PriceCode();
                        res.Initialize(priceCodeName, localePrice, GetPriceCodeDescription(priceCodeId), priceCodeId);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetPriceCodeByCountyAndCurrency, priceCodeId: {0}, countryCode: {1}, currencyCode: {2}",
                            priceCodeId, countryCode, !string.IsNullOrEmpty(currencyCode) ? currencyCode : string.Empty), ex);
            }

            return res;
        }

        public override ApiObjects.Response.Status InsertPriceCode(int groupID, string code, Price price)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                if (string.IsNullOrEmpty(code))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no code send");
                }
                if (price == null || price.m_dPrice < 0.0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no price send");
                }
                if (price.m_oCurrency == null || price.m_oCurrency.m_nCurrencyID == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no currency send");
                }

                int res = 0;

                // insert new price code to DB 
                res = DAL.PricingDAL.Insert_PriceCode(groupID, code, price.m_dPrice, price.m_oCurrency.m_nCurrencyID);

                switch (res)
                {
                    case 1:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "price code insert successful");
                        break;
                    case 2:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "price code already exsits");
                        break;
                    case 3:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "CurrencyID not exsits");
                        break;
                    case 4:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "alias is not  unique");
                        break;
                    default:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                        break;
                }
            }
            catch (Exception ex)
            {
                log.Error("InsertPriceCode - " + string.Format("failed to InsertPriceCode groupID = {0}, priceCode = {1}, Price = {2}, CurrencyID = {3}, ex = {4}",
                    groupID, code, price != null ? price.m_dPrice : 0, price != null && price.m_oCurrency != null ? price.m_oCurrency.m_nCurrencyID : 0, ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        public override ApiObjects.Response.Status InsertDiscountCode(int groupID, DiscountModule discount)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                if (discount == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "discount object is null");
                }
                if (string.IsNullOrEmpty(discount.m_sCode))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no code send");
                }
                if (discount.m_oPrise == null || discount.m_oPrise.m_dPrice < 0.0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no price send");
                }
                if (discount.m_oPrise.m_oCurrency == null || discount.m_oPrise.m_oCurrency.m_nCurrencyID == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no currency send");
                }
                if (discount.m_oWhenAlgo == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "validity of discount is empty");
                }

                // insert new discount code to DB 

                string alias = discount.alias;
                bool isUniqe = false;
                // check that alias is unique               
                isUniqe = CheckAliasUniqe(groupID, "discount_codes", alias);

                int res = 0;
                if (isUniqe)
                {
                    // insert new price code to DB 
                    res = DAL.PricingDAL.Insert_NewDiscountCode(groupID, discount.m_sCode, discount.m_oPrise.m_dPrice, discount.m_oPrise.m_oCurrency.m_nCurrencyID, discount.m_dPercent, (int)discount.m_eTheRelationType, discount.m_dStartDate, discount.m_dEndDate,
                    (int)discount.m_oWhenAlgo.m_eAlgoType, discount.m_oWhenAlgo.m_nNTimes, alias);
                }
                else
                {
                    res = 4;
                }

                switch (res)
                {
                    case 1:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "discount code insert successful");
                        break;
                    case 2:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "discount code already exsits");
                        break;
                    case 3:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "CurrencyID not exsits");
                        break;
                    case 4:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "alias is not  unique");
                        break;
                    default:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                        break;
                }

            }
            catch (Exception ex)
            {
                log.Error("InsertDiscountCode - " + string.Format("failed to InsertDiscountCode groupID = {0}, discountCode = {1}, Price = {2}, CurrencyID = {3}, ex = {4}",
                    groupID, discount != null ? discount.m_sCode : "no code", discount.m_oPrise != null ? discount.m_oPrise.m_dPrice : 0, discount.m_oPrise != null && discount.m_oPrise.m_oCurrency != null ? discount.m_oPrise.m_oCurrency.m_nCurrencyID : 0, ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        public override ApiObjects.Response.Status InsertCouponGroup(int groupID, CouponsGroup coupon)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                if (coupon == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "couponsGroup object is null");
                }
                if (string.IsNullOrEmpty(coupon.m_sGroupName))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no group coupon name send");
                }
                if (coupon.m_oDiscountCode == null || coupon.m_oDiscountCode.m_nObjectID == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no discount code send");
                }
                if (coupon.m_nFinancialEntityID == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "must send financial entity");
                }

                string alias = coupon.alias;
                bool isUniqe = false;
                // check that alias is unique               
                isUniqe = CheckAliasUniqe(groupID, "discount_codes", alias);

                int res = 0;
                if (isUniqe)
                {
                    // insert new CouponGroup to DB                

                    res = DAL.PricingDAL.InsertCouponGroup(groupID, coupon.m_sGroupName, coupon.m_oDiscountCode.m_nObjectID, coupon.m_dStartDate, coupon.m_dEndDate, coupon.m_nMaxUseCountForCoupon, coupon.m_nMaxRecurringUsesCountForCoupon,
                        coupon.m_nFinancialEntityID, alias);
                }
                else
                {
                    res = 4;
                }
                switch (res)
                {
                    case 1:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "coupon group insert successful");
                        break;
                    case 2:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "discount code not exsits");
                        break;
                    case 3:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "coupon group exsits");
                        break;
                    case 4:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "alias is not  unique");
                        break;
                    default:
                        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                        break;
                }

            }
            catch (Exception ex)
            {
                log.Error("InsertCouponGroup - " + string.Format("failed to InsertCouponGroup groupID = {0}, coupon group name = {1}, discountCode = {2}, ex = {3}",
                    groupID, coupon != null ? coupon.m_sGroupName : "empty coupon group name", coupon != null && coupon.m_oDiscountCode != null ? coupon.m_oDiscountCode.m_nObjectID : 0,
                    ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        public override ApiObjects.Response.Status InsertUsageModule(int groupID, UsageModule usageModule)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                if (usageModule == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "usageModule object is null");
                }
                if (string.IsNullOrEmpty(usageModule.m_sVirtualName))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no usage module name send");
                }
                if (usageModule.m_tsViewLifeCycle == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no view life cycle send");
                }
                if (usageModule.m_tsMaxUsageModuleLifeCycle == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no full life cycle send ");
                }

                // insert new UsageModule to DB                
                int res = 0;

                res = DAL.PricingDAL.InsertUsageModule(groupID, usageModule.m_sVirtualName, usageModule.m_tsViewLifeCycle, usageModule.m_tsMaxUsageModuleLifeCycle, usageModule.m_nMaxNumberOfViews,
                usageModule.m_bWaiver, usageModule.m_nWaiverPeriod, usageModule.m_bIsOfflinePlayBack,
               usageModule.m_ext_discount_id, usageModule.m_internal_discount_id, usageModule.m_pricing_id, usageModule.m_coupon_id, usageModule.m_type, usageModule.m_subscription_only, usageModule.m_is_renew, usageModule.m_num_of_rec_periods,
               usageModule.m_device_limit_id);

                switch (res)
                {
                    case 1:
                        status = new Status((int)eResponseStatus.OK, "usage moodule insert successful");
                        break;
                    case 2:
                        status = new Status((int)eResponseStatus.Error, "usage moodule name already exsits");
                        break;
                    case 3:
                        status = new Status((int)eResponseStatus.Error, "one of the period life cycle not exsits");
                        break;
                    case 4:
                        status = new Status((int)eResponseStatus.Error, "price code not exsits");
                        break;
                    case 5:
                        status = new Status((int)eResponseStatus.Error, "discount code not exsits");
                        break;
                    case 6:
                        status = new Status((int)eResponseStatus.Error, "group coupon code not exsits");
                        break;
                    default:
                        status = new Status((int)eResponseStatus.Error, "Error");
                        break;
                }

            }
            catch (Exception ex)
            {
                log.Error("InsertUsageModule - " + string.Format("failed to InsertUsageModule groupID = {0}, usage module group name = {1}, ex = {2}",
                    groupID, usageModule != null ? usageModule.m_sVirtualName : "empty usage module name", ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        public override ApiObjects.Response.Status UpdatetUsageModule(int groupID, UsageModule usageModule)
        {
            ApiObjects.Response.Status status = null;
            try
            {
                if (usageModule == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "usageModule object is null");
                }
                if (string.IsNullOrEmpty(usageModule.m_sVirtualName))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no usage module name send");
                }
                if (usageModule.m_tsViewLifeCycle == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no view life cycle send");
                }
                if (usageModule.m_tsMaxUsageModuleLifeCycle == 0)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no full life cycle send ");
                }

                // Update UsageModule to DB
                int res = 0;

                res = DAL.PricingDAL.UpdatetUsageModule(groupID, usageModule.m_sVirtualName, usageModule.m_tsViewLifeCycle, usageModule.m_tsMaxUsageModuleLifeCycle, usageModule.m_nMaxNumberOfViews,
                usageModule.m_bWaiver, usageModule.m_nWaiverPeriod, usageModule.m_bIsOfflinePlayBack,
               usageModule.m_ext_discount_id, usageModule.m_internal_discount_id, usageModule.m_pricing_id, usageModule.m_coupon_id, usageModule.m_type, usageModule.m_subscription_only, usageModule.m_is_renew, usageModule.m_num_of_rec_periods,
               usageModule.m_device_limit_id);

                switch (res)
                {
                    case 1:
                        status = new Status((int)eResponseStatus.OK, "usage moodule updated successful");
                        break;
                    case 3:
                        status = new Status((int)eResponseStatus.Error, "one of the period life cycle not exsits");
                        break;
                    case 4:
                        status = new Status((int)eResponseStatus.Error, "price code not exsits");
                        break;
                    case 5:
                        status = new Status((int)eResponseStatus.Error, "discount code not exsits");
                        break;
                    case 6:
                        status = new Status((int)eResponseStatus.Error, "group coupon code not exsits");
                        break;
                    default:
                        status = new Status((int)eResponseStatus.Error, "Error");
                        break;
                }

            }
            catch (Exception ex)
            {
                log.Error("UpdatetUsageModule - " + string.Format("failed to UpdatetUsageModule groupID = {0}, usage module group name = {1}, ex = {2}",
                    groupID, usageModule != null ? usageModule.m_sVirtualName : "empty usage module name", ex.Message), ex);
                status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
                return status;
            }

            return status;
        }

        //public override ApiObjects.Response.Status InsertPreviewModule(int groupID, PreviewModule previewModule)
        //{
        //    ApiObjects.Response.Status status = null;
        //    try
        //    {
        //        if (previewModule == null)
        //        {
        //            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "preview module object is null");
        //        }
        //        if (string.IsNullOrEmpty(previewModule.m_sName))
        //        {
        //            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no preview module name send");
        //        }
        //        if (previewModule.m_tsFullLifeCycle == 0)
        //        {
        //            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no full life cycle send");
        //        }
        //        if (previewModule.m_tsNonRenewPeriod == 0)
        //        {
        //            return new ApiObjects.Response.Status((int)eResponseStatus.Error, "no Non Renewing Period send ");
        //        }

        //        // insert new UsageModule to DB                
        //        string alias = previewModule.alias;
        //        bool isUniqe = false;
        //        // check that alias is unique               
        //        isUniqe = CheckAliasUniqe(groupID, "preview_modules", alias);

        //        int res = 0;
        //        if (isUniqe)
        //        {
        //            res = DAL.PricingDAL.InsertPreviewModule(groupID, previewModule.m_sName, previewModule.m_tsFullLifeCycle, previewModule.m_tsNonRenewPeriod, alias);
        //        }
        //        else
        //        {
        //            res = 4;
        //        }

        //        switch (res)
        //        {
        //            case 1:
        //                status = new Status((int)eResponseStatus.OK, "preview module insert successful");
        //                break;
        //            case 2:
        //                status = new Status((int)eResponseStatus.Error, "preview module name already exsits");
        //                break;
        //            case 3:
        //                status = new Status((int)eResponseStatus.Error, "one of the period life cycle not exsits");
        //                break;
        //            case 4:
        //                status = new Status((int)eResponseStatus.Error, "alias is not  unique");
        //                break;
        //            default:
        //                status = new Status((int)eResponseStatus.Error, "Error");
        //                break;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        log.Error("InsertPreviewModule - " + string.Format("failed to InsertPreviewModule groupID = {0}, preview module name = {1}, ex = {2}",
        //            groupID, previewModule != null ? previewModule.m_sName : "empty preview module name", ex.Message), ex);
        //        status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Internal Error");
        //        return status;
        //    }

        //    return status;
        //}


        public override BusinessModuleResponse InsertPPV(IngestPPV ppv)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                int priceCodeID = 0;
                int currencyID = 0;
                int usageModuleID = 0;
                int discountID = 0;
                int couponGroupID = 0;
                List<long> fileTypes = new List<long>();

                Status status = ValidatePPV(ppv, eIngestAction.Insert, ref priceCodeID, ref currencyID, ref usageModuleID, ref discountID, ref couponGroupID, ref fileTypes);
                int Id = 0;
                string Message = string.Empty;
                if ((int)status.Code == (int)eResponseStatus.OK && priceCodeID == 0) // create new price code in DB
                {
                    string code = string.Format("{0}-{1}", ppv.PriceCode.Price, ppv.PriceCode.Currency);
                    priceCodeID = DAL.PricingDAL.InsertPriceCode(m_nGroupID, currencyID, ppv.PriceCode.Price, code);
                    if (priceCodeID == 0)
                    {
                        string errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "price_code", ppv.PriceCode.Price);
                        status = new Status((int)eResponseStatus.InvalidPriceCode, errorMessage);
                    }
                }

                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.InsertPPV(m_nGroupID, ppv, priceCodeID, usageModuleID, discountID, couponGroupID, fileTypes);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "insert"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }

                response = new BusinessModuleResponse(Id, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to insert PPV code = {0} , ex = {1}", ppv == null ? string.Empty : ppv.Code, ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }

            return response;
        }

        public override BusinessModuleResponse UpdatePPV(IngestPPV ppv)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                int priceCodeID = -1;
                int currencyID = -1;
                int usageModuleID = -1;
                int discountID = -1;// If a field for the entry is NOT provided – it should NOT be updated
                int couponGroupID = -1;// If a field for the entry is NOT provided – it should NOT be updated
                List<long> fileTypes = new List<long>();
                Status status = ValidatePPV(ppv, eIngestAction.Update, ref priceCodeID, ref currencyID, ref usageModuleID, ref discountID, ref couponGroupID, ref fileTypes);

                int Id = 0;
                string Message = string.Empty;
                
                if ((int)status.Code == (int)eResponseStatus.OK && priceCodeID == 0) // create new price code in DB
                {
                    string code = string.Format("{0}-{1}", ppv.PriceCode.Price, ppv.PriceCode.Currency);                    
                    priceCodeID = DAL.PricingDAL.InsertPriceCode(m_nGroupID, currencyID, ppv.PriceCode.Price, code);
                    if (priceCodeID == 0)
                    {
                        string errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "price_code", ppv.PriceCode.Price);
                        status = new Status((int)eResponseStatus.InvalidPriceCode, errorMessage);
                    }
                }

                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.UpdatePPV(m_nGroupID, ppv, priceCodeID, usageModuleID, discountID, couponGroupID, fileTypes);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "update"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                response = new BusinessModuleResponse(Id, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to update PPV ppv code = {0} ,ex = {1}", ppv == null ? string.Empty : ppv.Code, ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }

            return response;
        }

        public override BusinessModuleResponse DeletePPV(string ppv)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                Status status;
                string Message = string.Empty;

                int Id = DAL.PricingDAL.DeletePPV(m_nGroupID, ppv);
                if (Id == 0)
                {
                    status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "delete"));
                }
                else if (Id == -1)
                {
                    status = new Status((int)eResponseStatus.CodeNotExist, string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "code", ppv));
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                response = new BusinessModuleResponse(Id, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to delete PPV code = {0} ,ex = {1}", ppv, ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }

            return response;
        }

        public override BusinessModuleResponse InsertMPP(IngestMultiPricePlan mpp)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                List<KeyValuePair<long, int>> pricePlansCodes = new List<KeyValuePair<long, int>>();
                List<long> channels = new List<long>();
                List<long> fileTypes = new List<long>();
                int previewModuleID = 0;
                int internalDiscountID = 0;
                string Message = string.Empty;

                Status status = ValidateMPP(mpp, eIngestAction.Insert, ref pricePlansCodes, ref channels, ref fileTypes, ref previewModuleID, ref internalDiscountID);
                int Id = 0;
                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.InsertMPP(m_nGroupID, mpp, pricePlansCodes, channels, fileTypes, previewModuleID, internalDiscountID);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "insert"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }
                response = new BusinessModuleResponse(Id, status);
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Failed to insert MPP ex = {0}", ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        public override BusinessModuleResponse UpdateMPP(IngestMultiPricePlan mpp)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                List<KeyValuePair<long, int>> pricePlansCodes = new List<KeyValuePair<long, int>>();
                List<long> channels = new List<long>();
                List<long> fileTypes = new List<long>();
                int previewModuleID = -1;
                int internalDiscountID = -1;
                int Id = 0;
                string Message = string.Empty;

                Status status = ValidateMPP(mpp, eIngestAction.Update, ref pricePlansCodes, ref channels, ref fileTypes, ref previewModuleID, ref internalDiscountID);
                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.UpdateMPP(m_nGroupID, mpp, pricePlansCodes, channels, fileTypes, previewModuleID, internalDiscountID);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "update"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }

                response = new BusinessModuleResponse(Id, status);
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Failed to update MPP ex = {0}", ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        public override BusinessModuleResponse DeleteMPP(string multiPricePlan)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                Status status;
                string Message = string.Empty;
                int Id = DAL.PricingDAL.DeleteMPP(m_nGroupID, multiPricePlan);
                if (Id == 0)
                {
                    status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "delete"));
                }
                else if (Id == -1)
                {
                    status = new Status((int)eResponseStatus.CodeNotExist, string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "code", multiPricePlan));
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                response = new BusinessModuleResponse(Id, status);
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed to delete MPP ex = {0}", ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        public override BusinessModuleResponse InsertPricePlan(IngestPricePlan pricePlan)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                Status status;

                int fullLifeCycleID = 0;
                int viewLifeCycleID = 0;
                int priceCodeID = 0;
                int currencyID = 0;
                int discountID = 0;
                status = ValidatePricePlan(pricePlan, eIngestAction.Insert, ref priceCodeID, ref currencyID, ref fullLifeCycleID, ref viewLifeCycleID, ref discountID);
                if ((int)status.Code == (int)eResponseStatus.OK && priceCodeID == 0)
                {
                    string code = string.Format("{0}-{1}", pricePlan.PriceCode.Price, pricePlan.PriceCode.Currency);
                    priceCodeID = DAL.PricingDAL.InsertPriceCode(m_nGroupID, currencyID, pricePlan.PriceCode.Price, code);
                    if (priceCodeID == 0)
                    {
                        string errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "price_code", pricePlan.PriceCode.Price);
                        status = new Status((int)eResponseStatus.InvalidPriceCode, errorMessage);
                    }
                }
                int Id = 0;
                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.InsertPricePlan(m_nGroupID, pricePlan, priceCodeID, fullLifeCycleID, viewLifeCycleID, discountID);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "insert"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }

                response = new BusinessModuleResponse(Id, status);
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Failed to insert Pric Plan ex = {0}", ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        public override BusinessModuleResponse UpdatePricePlan(IngestPricePlan pricePlan)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            try
            {
                Status status;

                int fullLifeCycleID = -1;
                int viewLifeCycleID = -1;
                int priceCodeID = -1;// If a field for the entry is NOT provided – it should NOT be updated
                int currencyID = -1;// If a field for the entry is NOT provided – it should NOT be updated
                int discountID = -1; // If a field for the entry is NOT provided – it should NOT be updated


                status = ValidatePricePlan(pricePlan, eIngestAction.Update, ref priceCodeID, ref currencyID, ref fullLifeCycleID, ref viewLifeCycleID, ref discountID);
                if ((int)status.Code == (int)eResponseStatus.OK && priceCodeID == 0)
                {
                    string code = string.Format("{0}-{1}", pricePlan.PriceCode.Price, pricePlan.PriceCode.Currency);
                    priceCodeID = DAL.PricingDAL.InsertPriceCode(m_nGroupID, currencyID, pricePlan.PriceCode.Price, code);
                    if (priceCodeID == 0)
                    {
                        string errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "price_code", pricePlan.PriceCode.Price);
                        status = new Status((int)eResponseStatus.InvalidPriceCode, errorMessage);
                    }
                }
                int Id = 0;
                if ((int)status.Code == (int)eResponseStatus.OK)
                {
                    Id = DAL.PricingDAL.UpdatePricePlan(m_nGroupID, pricePlan, priceCodeID, fullLifeCycleID, viewLifeCycleID, discountID);
                    if (Id == 0)
                    {
                        status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "update"));
                    }
                    else
                    {
                        status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                }

                response = new BusinessModuleResponse(Id, status);
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Failed to insert PricePlan ex = {0}", ex.Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        public override BusinessModuleResponse DeletePricePlan(string pricePlan)
        {
            BusinessModuleResponse response = new BusinessModuleResponse();
            string Message;
            try
            {
                Status status;
                int Id = DAL.PricingDAL.DeletePricePlan(m_nGroupID, pricePlan);

                if (Id == 0)
                {
                    status = new Status((int)eResponseStatus.Error, string.Format(INGEST_FAILED_ERROR_FORMAT, "delete"));
                }
                else if (Id == -1)
                {
                    status = new Status((int)eResponseStatus.CodeNotExist, string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "code", pricePlan));
                }
                else
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                response = new BusinessModuleResponse(Id, status);
            }
            catch (Exception ex)
            {
                Message = string.Format("Failed to delete PricePlan ex = {0}", ex.Message);
                log.Error(Message);
                response = new BusinessModuleResponse(0, new Status((int)eResponseStatus.Error, INGEST_UNEXPECTED_ERROR));
            }
            return response;
        }

        private Status ValidatePricePlan(IngestPricePlan pricePlan, eIngestAction action, ref int priceCodeID, ref int currencyID, ref int fullLifeCycleID, ref int viewLifeCycleID, ref int discountID)
        {
            Status status = new Status((int)eResponseStatus.Error, "unexpected error");
            try 
            {
                string errorMessage = string.Empty;

                status = ValidateMandatoryPricePlan(pricePlan);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    return status;
                }

                status = new Status((int)eResponseStatus.Error, "unexpected error");
                                
                DataTable result = DAL.PricingDAL.ValidatePricePlan(m_nGroupID, pricePlan.Code, pricePlan.FullLifeCycle, pricePlan.ViewLifeCycle,
                    pricePlan.PriceCode != null ? pricePlan.PriceCode.Currency: null, pricePlan.PriceCode != null ? pricePlan.PriceCode.Price: null, pricePlan.Discount);

                string tableName = string.Empty;
                int id = 0;
                if (result != null && result.Rows != null && result.Rows.Count > 0)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    foreach (DataRow dr in result.Rows)
                    {                                             
                        errorMessage = string.Empty;
                        tableName = ODBCWrapper.Utils.GetSafeStr(dr, "tableName");
                        id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                            switch (tableName)
                            {
                                case "Code":
                                    if (id == 0 && action != eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "code", pricePlan.Code);
                                    status = new Status((int)eResponseStatus.CodeNotExist, errorMessage);
                                }
                                else if (id != 0 && action == eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_ALREADY_EXISTS_FORMAT, "code", pricePlan.Code);
                                    status = new Status((int)eResponseStatus.CodeMustBeUnique, errorMessage);
                                }
                                    break;
                                case "PriceCode":
                                    if (pricePlan.PriceCode != null && pricePlan.PriceCode.Price != null)
                                    {
                                        priceCodeID = id;
                                    }
                                    break;
                                case "Currency":
                                    if (id == 0 && pricePlan.PriceCode != null && !string.IsNullOrEmpty(pricePlan.PriceCode.Currency))
                                    {
                                        errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "currency", pricePlan.PriceCode.Currency);
                                        status = new Status((int)eResponseStatus.InvalidCurrency, errorMessage);
                                    }
                                    if (pricePlan.PriceCode != null && pricePlan.PriceCode.Currency != null) // if Currency is null no need to insert value for currencyID
                                    {
                                        currencyID = id;
                                    }
                                    break;
                                case "FullLifeCycle":
                                    if (id == 0 && !string.IsNullOrEmpty(pricePlan.FullLifeCycle))
                                    {
                                        errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "full_life_cycle", pricePlan.FullLifeCycle);
                                        status = new Status((int)eResponseStatus.InvalidValue, errorMessage);
                                    }
                                    if (pricePlan.FullLifeCycle != null) // if FullLifeCycle is null no need to insert value for fullLifeCycleID
                                    {
                                        fullLifeCycleID = id;
                                    }
                                    break;
                                case "ViewLifeCycle":
                                    if (id == 0 && !string.IsNullOrEmpty(pricePlan.ViewLifeCycle))
                                    {
                                        errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "view_life_cycle", pricePlan.ViewLifeCycle);
                                        status = new Status((int)eResponseStatus.InvalidValue, errorMessage);
                                    }
                                    if (pricePlan.ViewLifeCycle != null) // if ViewLifeCycle is null no need to insert value for viewLifeCycleID
                                    {
                                        viewLifeCycleID = id;
                                    }
                                    break;
                                case "Discount":
                                    if (id == 0 && !string.IsNullOrEmpty(pricePlan.Discount))
                                    {
                                        errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "discount", pricePlan.Discount);
                                        status = new Status((int)eResponseStatus.InvalidDiscountCode, errorMessage);
                                    }
                                    if (pricePlan.Discount != null) // if discount is null no need to insert value for discountID
                                    {
                                        discountID = id;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            if ((int)status.Code != (int)eResponseStatus.OK)
                            {
                                return status;
                            }
                        }
                    }
                }
            catch (Exception ex)
            {
                log.ErrorFormat("failed ex = {0}", ex.Message);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return status;
        }

        private static Status ValidateMandatoryPricePlan(IngestPricePlan pricePlan)
        {
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            if (pricePlan == null)
            {
                log.ErrorFormat("Price plae object is null");
                return new Status((int)eResponseStatus.MandatoryField, "PricePlan object is null");
            }
            if (string.IsNullOrEmpty(pricePlan.Code))
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "code"));
            }
            if (pricePlan.Action == eIngestAction.Insert)
            {
                if (string.IsNullOrEmpty(pricePlan.FullLifeCycle))
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "full_life_cycle"));
                }
                if (string.IsNullOrEmpty(pricePlan.ViewLifeCycle))
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "view_life_cycle"));
                }
                if (pricePlan.PriceCode == null || string.IsNullOrEmpty(pricePlan.PriceCode.Currency))
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code currency"));
                }
                if (pricePlan.PriceCode.Price == null || pricePlan.PriceCode.Price <= 0.0)
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code price must be greater than 0"));
                }
            }
            if (pricePlan.PriceCode != null && pricePlan.PriceCode.Currency!= null && pricePlan.PriceCode.Currency.Count() != 3) //currency must have 3 characters 
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code currency must have 3 characters"));
            }
            if (pricePlan.PriceCode != null && pricePlan.PriceCode.Price!= null &&  pricePlan.PriceCode.Price <= 0.0)
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code price must be greater than 0"));
            }
            if (pricePlan.PriceCode != null && (pricePlan.PriceCode.Price == null || string.IsNullOrEmpty(pricePlan.PriceCode.Currency)))
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code must have all members"));
            }

            return status;
        }


        private Status ValidateMPP(IngestMultiPricePlan mpp, eIngestAction action, ref List<KeyValuePair<long, int>> pricePlansCodes, ref List<long> channels, ref List<long> fileTypes,
           ref int previewModuleID, ref int internalDiscountID)
        {
            Status status = new Status((int)eResponseStatus.Error, "unexpected error");
            try
            {
                status = ValidateMandatoryMPP(mpp);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    return status;
                }

                status = new Status((int)eResponseStatus.Error, "unexpected error");

                // validate by action
                List<string> couponGroupCodes = new List<string>();
                if (mpp.couponGroups != null && mpp.couponGroups.Count > 0)
                {
                    couponGroupCodes = mpp.couponGroups.Select(x => x.Code).ToList();
                }

                DataTable result = DAL.PricingDAL.ValidateMPP(m_nGroupID, mpp.Code, mpp.InternalDiscount, mpp.PricePlansCodes, mpp.Channels, mpp.FileTypes, mpp.PreviewModule, couponGroupCodes);
                
                if (result != null && result.Rows != null)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    string errorMessage = string.Empty;
                    string tableName = string.Empty;
                    int id = 0;

                    int orderNum = 0;
                    DataRow[] drBasic = result.Select("tableName in ('Code','Discount','PreviewModule')");

                    foreach (DataRow dr in drBasic)
                    {
                        errorMessage = string.Empty;
                        tableName = ODBCWrapper.Utils.GetSafeStr(dr, "tableName");
                        id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                        switch (tableName)
                        {
                            case "Code":
                                if (id == 0 && action != eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "code", mpp.Code);
                                    status = new Status((int)eResponseStatus.CodeNotExist, errorMessage);
                                }
                                else if (id != 0 && action == eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_ALREADY_EXISTS_FORMAT, "code", mpp.Code);
                                    status = new Status((int)eResponseStatus.CodeMustBeUnique, errorMessage);
                                }
                                break;
                            case "Discount":
                                if (id == 0 && !string.IsNullOrEmpty(mpp.InternalDiscount))
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "internal_discount", mpp.Code, mpp.InternalDiscount);
                                    status = new Status((int)eResponseStatus.InvalidDiscountCode, errorMessage);
                                }
                                if (!string.IsNullOrEmpty(mpp.InternalDiscount))
                                {
                                    internalDiscountID = id;
                                }
                                break;
                            case "PreviewModule":
                                if (id == 0 && !string.IsNullOrEmpty(mpp.PreviewModule))
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "preview_module", mpp.Code, mpp.PreviewModule);
                                    status = new Status((int)eResponseStatus.InvalidPreviewModule, errorMessage);
                                }
                                if (mpp.PreviewModule != null) // if PreviewModule is null no need to insert value for previewModuleID
                                {
                                    previewModuleID = id;
                                }
                                break;
                            default:
                                break;
                        }
                        if ((int)status.Code != (int)eResponseStatus.OK)
                        {
                            return status;
                        }
                    }
                                       
                    List<string> missingPricePlans = result.AsEnumerable()
                        .Where<DataRow>(r => r.Field<string>("tableName") == "PricePlan" && r.Field<long>("id") == 0)
                         .Select(r => r.Field<string>("code")).ToList<string>();                    
                    if (missingPricePlans.Count() > 0)
                    { 
                        errorMessage = string.Format(INGEST_ERROR_NOT_EXIST_FORMAT, "price_plans", string.Join("', '", missingPricePlans));
                        return new Status((int)eResponseStatus.InvalidPricePlan, errorMessage);
                    }
                    
                    DataRow[] drPricePlan = result.Select("tableName = 'PricePlan'");
                    foreach (DataRow dr in drPricePlan)
                    {
                        id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                        pricePlansCodes.Add(new KeyValuePair<long, int>(id, orderNum));
                        orderNum++;
                    }

                    List<string> missingChannels = result.AsEnumerable()
                        .Where<DataRow>(r => r.Field<string>("tableName") == "Channels" && r.Field<long>("id") == 0)
                        .Select(r => r.Field<string>("code")).ToList<string>();
                    if (missingChannels.Count() > 0)
                    {
                        errorMessage = string.Format(INGEST_ERROR_NOT_EXIST_FORMAT, "channels", string.Join("', '", missingChannels));
                        return new Status((int)eResponseStatus.InvalidChannels, errorMessage);
                    }

                    channels = result.AsEnumerable()
                       .Where<DataRow>(r => r.Field<string>("tableName") == "Channels")
                        .Select(r => r.Field<long>("id")).ToList();

                   List<string> missingFileTypes = result.AsEnumerable()
                      .Where<DataRow>(r => r.Field<string>("tableName") == "FileTypes" && r.Field<long>("id") == 0)
                       .Select(r => r.Field<string>("code")).ToList<string>();
                    if (missingFileTypes.Count() > 0)
                    {
                        errorMessage = string.Format(INGEST_ERROR_NOT_EXIST_FORMAT, "file_types", string.Join("', '", missingFileTypes));
                        return new Status((int)eResponseStatus.InvalidFileTypes, errorMessage);
                    }
                    fileTypes = result.AsEnumerable()
                       .Where<DataRow>(r => r.Field<string>("tableName") == "FileTypes")
                        .Select(r => r.Field<long>("id")).ToList();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed ex = {0}", ex.Message);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return status;
        }

        private static Status ValidateMandatoryMPP(IngestMultiPricePlan mpp)
        {
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            if (mpp == null)
            {
                log.ErrorFormat("MPP object is null");
                return new Status((int)eResponseStatus.Error, "MPP object is null");
            }
            if (string.IsNullOrEmpty(mpp.Code))
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "code"));
            }
            if (mpp.Action == eIngestAction.Insert)
            {
                if (string.IsNullOrEmpty(mpp.InternalDiscount))
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "internal_discount"));
                }
                if (mpp.PricePlansCodes == null || mpp.PricePlansCodes.Count == 0)
                {
                    return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_plan_codes"));
                }
            }
            // go over the list remove empty values
            if (mpp.Channels != null && mpp.Channels.Count > 0)
            {
                mpp.Channels.RemoveAll(item => string.IsNullOrEmpty(item));
            }
            if (mpp.FileTypes != null && mpp.FileTypes.Count > 0)
            {
                mpp.FileTypes.RemoveAll(item => string.IsNullOrEmpty(item));
            }
            if (mpp.Descriptions != null && mpp.Descriptions.Count > 0)
            {
                mpp.Descriptions.RemoveAll(item => string.IsNullOrEmpty(item.key) || string.IsNullOrEmpty(item.value));
            }
            if (mpp.Titles != null && mpp.Titles.Count > 0)
            {
                mpp.Titles.RemoveAll(item => string.IsNullOrEmpty(item.key) || string.IsNullOrEmpty(item.value));
            }
            if (mpp.PricePlansCodes != null && mpp.PricePlansCodes.Count > 0)
            {
                mpp.PricePlansCodes.RemoveAll(item => string.IsNullOrEmpty(item));
            }
            return status;
        }

        private Status ValidatePPV(IngestPPV ppv, eIngestAction action, ref int priceCodeID, ref int currencyID, ref int usageModuleID, ref int discountID, ref int couponGroupID, ref List<long> fileTypes)
        {
            Status status = new Status((int)eResponseStatus.Error, "unexpected error");

            try
            {
                status = ValidateMandatoryPPV(ppv);
                if (status.Code != (int)eResponseStatus.OK)
                {
                    return status;
                }

                status = new Status((int)eResponseStatus.Error, "unexpected error");

                // validate by action
                DataTable result = DAL.PricingDAL.ValidatePPV(m_nGroupID, ppv.Code, ppv.PriceCode != null ? ppv.PriceCode.Currency : null, ppv.PriceCode != null ? ppv.PriceCode.Price : null
                    , ppv.UsageModule, ppv.Discount, ppv.CouponGroup, ppv.FileTypes);

                if (result != null && result.Rows != null && result.Rows.Count > 0)
                {
                    status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

                    string errorMessage = string.Empty;
                    string tableName;
                    int id = 0;

                    DataRow[] drBasic = result.Select("tableName <> 'FileTypes'");

                    foreach (DataRow dr in drBasic)
                    {
                        tableName = ODBCWrapper.Utils.GetSafeStr(dr, "tableName");
                        id = ODBCWrapper.Utils.GetIntSafeVal(dr, "id");
                        switch (tableName)
                        {
                            case "Code":
                                if (id == 0 && action != eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_MANDATORY_FORMAT, "code");
                                    status = new Status((int)eResponseStatus.CodeNotExist, errorMessage);
                                }
                                else if (id != 0 && action == eIngestAction.Insert)
                                {
                                    errorMessage = string.Format(INGEST_ERROR_ALREADY_EXISTS_FORMAT, "code", ppv.Code);
                                    status = new Status((int)eResponseStatus.CodeMustBeUnique, errorMessage);
                                }
                                break;
                            case "Discount":
                                if (id == 0 && !string.IsNullOrEmpty(ppv.Discount)) //Discount
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "discount", ppv.Discount);
                                    status = new Status((int)eResponseStatus.InvalidDiscountCode, errorMessage);
                                }
                                if (ppv.Discount != null) // if discount is null no need to insert value for discountID
                                {
                                    discountID = id;
                                }
                                break;
                            case "UsageModule":
                                if (id == 0 && !string.IsNullOrEmpty(ppv.UsageModule)) //UsageModule
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "usage_module", ppv.UsageModule);
                                    status = new Status((int)eResponseStatus.InvalidUsageModule, errorMessage);
                                }
                                if (!string.IsNullOrEmpty(ppv.UsageModule))
                                {
                                    usageModuleID = id;
                                }
                                break;
                            case "PriceCode":
                                if (ppv.PriceCode != null && ppv.PriceCode.Price != null)
                                {
                                    priceCodeID = id;
                                }
                                break;
                            case "Currency":
                                if (id == 0 && ppv.PriceCode!= null && !string.IsNullOrEmpty(ppv.PriceCode.Currency))
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "currency", ppv.PriceCode.Currency);
                                    status = new Status((int)eResponseStatus.InvalidCurrency, errorMessage);
                                }
                                if (ppv.PriceCode != null && !string.IsNullOrEmpty(ppv.PriceCode.Currency))
                                {
                                    currencyID = id;
                                }
                                break;
                            case "CouponGroup":
                                if (id == 0 && !string.IsNullOrEmpty(ppv.CouponGroup))//CouponGroup is not mandatory
                                {
                                    errorMessage = string.Format(INGEST_ERROR_NOT_EXISTS_FORMAT, "coupon_group", ppv.CouponGroup);
                                    status = new Status((int)eResponseStatus.InvalidCouponGroup, errorMessage);
                                }
                                if (!string.IsNullOrEmpty(ppv.CouponGroup)) // if CouponGroup is null no need to insert value for couponGroupID
                                {
                                    couponGroupID = id;
                                }
                                break;
                            default:
                                break;
                        }
                        if ((int)status.Code != (int)eResponseStatus.OK)
                        {
                            return status;
                        }
                    }


                    List<string> missingFileTypes = new List<string>();
                    missingFileTypes = result.AsEnumerable()
                        .Where<DataRow>(r => r.Field<string>("tableName") == "FileTypes" && r.Field<long>("id") == 0)
                        .Select(r => r.Field<string>("code")).ToList();
                    if (missingFileTypes.Count > 0)
                    {
                        errorMessage = string.Format(INGEST_ERROR_NOT_EXIST_FORMAT, "file_types" , string.Join("', '", missingFileTypes));
                        return new Status((int)eResponseStatus.InvalidFileTypes, errorMessage);
                    }

                    fileTypes = result.AsEnumerable()
                        .Where<DataRow>(r => r.Field<string>("tableName") == "FileTypes")
                        .Select(r => r.Field<long>("id")).ToList();
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("failed ex = {0}", ex.Message);
                status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }

            return status;
        }

        private static Status ValidateMandatoryPPV(IngestPPV ppv)
        {
            Status status = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            if (ppv == null)
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "PPV object is null");
            }
            if (string.IsNullOrEmpty(ppv.Code))
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "code"));
            }
            if (ppv.Action == eIngestAction.Insert)
            {
                if (ppv.PriceCode == null || string.IsNullOrEmpty(ppv.PriceCode.Currency) || ppv.PriceCode.Price == null)
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code currency"));
                }
                if (string.IsNullOrEmpty(ppv.UsageModule))
                {
                    return new ApiObjects.Response.Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "usage_module"));
                }
            }
            if (ppv.PriceCode != null && ppv.PriceCode.Currency != null && ppv.PriceCode.Currency.Count() != 3) //currency must have 3 characters 
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code currency must have 3 characters"));
            }

            if (ppv.PriceCode != null &&  ppv.PriceCode.Price!= null && ppv.PriceCode.Price <= 0.0)
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code price must be greater than 0"));
            }

            if (ppv.PriceCode != null && (string.IsNullOrEmpty(ppv.PriceCode.Currency) || ppv.PriceCode.Price == null))
            {
                return new Status((int)eResponseStatus.MandatoryField, string.Format(INGEST_ERROR_MANDATORY_FORMAT, "price_code must have all members"));
            }
           
            // go over the list remove empty values
            if (ppv.FileTypes != null && ppv.FileTypes.Count > 0)
            {
                ppv.FileTypes.RemoveAll(item => string.IsNullOrEmpty(item));
            }
            if (ppv.Descriptions != null && ppv.Descriptions.Count > 0)
            {
                ppv.Descriptions.RemoveAll(item => string.IsNullOrEmpty(item.key) || string.IsNullOrEmpty(item.value));
            }          
            return status;
        }

        private bool GenerateUniqueAlias(int groupID, string tableName, ref string alias)
        {
            try
            {
                alias = Guid.NewGuid().ToString();
                bool isUniqe = CheckAliasUniqe(groupID, tableName, alias);
                int numOfTry = 0;
                while (!isUniqe && numOfTry < NUM_OF_TRY)
                {
                    isUniqe = DAL.PricingDAL.CheckAliasIsUniqe(groupID, alias, tableName);
                    if (!isUniqe)
                    {
                        alias = Guid.NewGuid().ToString();
                        numOfTry++;
                    }
                }
                return isUniqe;
            }
            catch 
            {
                return false;
            }
        }

        private bool CheckAliasUniqe(int groupID, string tableName, string alias)
        {
            if (!string.IsNullOrEmpty(alias))
            {
                return DAL.PricingDAL.CheckAliasIsUniqe(groupID, alias, tableName);
            }
            else
            {
                // generate unique alias
                return GenerateUniqueAlias(groupID, "price_codes", ref alias);
            }
        }
    }
}
