using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using ApiObjects.Response;
using ApiObjects.Pricing;
using ApiObjects;

namespace Core.Pricing
{
    public class TvinciPPVModule : BasePPVModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciPPVModule(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        static protected LanguageContainer[] GetPPVDescription(Int32 nPPVModuleID)
        {
            LanguageContainer[] theContainer = null;
            DataTable dtPPVDescription = PricingDAL.Get_PPVDescription(nPPVModuleID);
            if (dtPPVDescription != null)
            {
                int nCount = dtPPVDescription.Rows.Count;
                if (nCount > 0)
                {
                    theContainer = new LanguageContainer[nCount];
                }
                Int32 nIndex = 0;
                for (int i = 0; i < nCount; i++)
                {
                    DataRow ppvDescriptionRow = dtPPVDescription.Rows[i];
                    string sLang = ODBCWrapper.Utils.GetSafeStr(ppvDescriptionRow["language_code3"]);
                    string sVal = ODBCWrapper.Utils.GetSafeStr(ppvDescriptionRow["description"]);
                    LanguageContainer t = new LanguageContainer();
                    t.Initialize(sLang, sVal);
                    theContainer[nIndex] = t;
                    nIndex++;
                }

            }
            return theContainer;
        }

        static protected List<int> GetPPVFileTypes(int nGroupID, int nPPVModuleID)
        {
            List<int> retVal = null;

            DataTable dtFileTypes = PricingDAL.Get_PPVFileTypes(nGroupID, nPPVModuleID);
            if (dtFileTypes != null)
            {
                int nCount = dtFileTypes.Rows.Count;
                if (nCount > 0)
                    retVal = new List<int>();
                for (int i = 0; i < nCount; i++)
                {
                    DataRow fileTypesRow = dtFileTypes.Rows[i];
                    int nFileTypeID = ODBCWrapper.Utils.GetIntSafeVal(fileTypesRow["file_type_id"]);
                    retVal.Add(nFileTypeID);
                }
            }
            return retVal;
        }

        protected PPVModule[] GetTvinciPPVModuleList(bool bShrink, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {

            PPVModule[] tmp = null;

            DataTable dtPPVModuleData = PricingDAL.Get_PPVModuleData(m_nGroupID, null);

            if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
            {
                tmp = new PPVModule[dtPPVModuleData.Rows.Count];
                Int32 nIndex = 0;
                for (int i = 0; i < dtPPVModuleData.Rows.Count; i++)
                {
                    DataRow ppvModuleDataRow = dtPPVModuleData.Rows[i];
                    int nPPVModuleID = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["ID"]);
                    string sPriceCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["PRICE_CODE"]);
                    string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["USAGE_MODULE_CODE"]);
                    string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["DISCOUNT_MODULE_CODE"]);
                    string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["COUPON_GROUP_CODE"]);
                    string sName = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["NAME"]);
                    bool bSubOnly = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["SUBSCRIPTION_ONLY"]));
                    bool bIsFirstDeviceLimitation = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["FIRSTDEVICELIMITATION"]));
                    string productCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["Product_Code"]);
                    string adsParam = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["ADS_PARAM"]);

                    int adsPolicyInt = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["ADS_POLICY"]);
                    AdsPolicy? adsPolicy = null;
                    if (adsPolicyInt > 0)
                    {
                        adsPolicy = (AdsPolicy)adsPolicyInt;
                    }

                    PPVModule t = new PPVModule();
                    if (!bShrink)
                        t.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, GetPPVDescription(nPPVModuleID), m_nGroupID, nPPVModuleID.ToString(), bSubOnly, sName, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, GetPPVFileTypes(nPPVModuleID, m_nGroupID), bIsFirstDeviceLimitation, productCode, 0, adsPolicy, adsParam);
                    else
                        t.Initialize(sPriceCode, string.Empty, string.Empty, string.Empty, GetPPVDescription(nPPVModuleID), m_nGroupID, nPPVModuleID.ToString(), bSubOnly, sName, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, GetPPVFileTypes(m_nGroupID, nPPVModuleID), bIsFirstDeviceLimitation, productCode, 0, adsPolicy, adsParam);
                    if (t.m_oPriceCode != null)
                    {
                        tmp[nIndex] = t;
                        nIndex++;
                    }
                }
                if (nIndex < dtPPVModuleData.Rows.Count)
                {
                    PPVModule[] tmp1 = new PPVModule[nIndex];
                    Array.Copy(tmp, tmp1, nIndex);
                    tmp = tmp1;
                }
            }
            return tmp;
        }

        public override PPVModule[] GetPPVModuleList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetTvinciPPVModuleList(false, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override PPVModule[] GetPPVModuleShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            return GetTvinciPPVModuleList(true, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
        }

        public override PPVModule GetPPVModuleData(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            try
            {
                Int32 nPPVModuleCode = 0;
                PPVModule tmp = new PPVModule();

                if (!Int32.TryParse(sPPVModuleCode, out nPPVModuleCode) || nPPVModuleCode == 0)
                    return null;

                DataTable dtPPVModuleData = PricingDAL.Get_PPVModuleData(m_nGroupID, nPPVModuleCode);

                if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
                {
                    DataRow ppvModuleDataRow = dtPPVModuleData.Rows[0];
                    string sPriceCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["PRICE_CODE"]);
                    string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["USAGE_MODULE_CODE"]);
                    string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["DISCOUNT_MODULE_CODE"]);
                    string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["COUPON_GROUP_CODE"]);
                    string sName = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["NAME"]);
                    bool bSubOnly = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["SUBSCRIPTION_ONLY"]));
                    bool bIsFirstDeviceLimitation = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["FIRSTDEVICELIMITATION"]));
                    string productCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["Product_Code"]);
                    string adsParam = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow, "ADS_PARAM");

                    int adsPolicyInt = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow, "ADS_POLICY");
                    AdsPolicy? adsPolicy = null;
                    if (adsPolicyInt > 0)
                    {
                        adsPolicy = (AdsPolicy)adsPolicyInt;
                    }

                    tmp.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, GetPPVDescription(nPPVModuleCode), m_nGroupID, sPPVModuleCode, bSubOnly,
                                   sName, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, GetPPVFileTypes(m_nGroupID, nPPVModuleCode), bIsFirstDeviceLimitation, productCode, 0, adsPolicy, adsParam);

                    return tmp;
                }

            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" PPVMC: ", sPPVModuleCode));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }

            return null;
        }

        public override PPVModuleDataResponse GetPPVModuleDataResponse(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModuleDataResponse result = new PPVModuleDataResponse();
            try
            {
                result.PPVModule = GetPPVModuleData(sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                if (result.PPVModule != null)
                {                    
                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    result.Status = new ApiObjects.Response.Status((int)eResponseStatus.ModuleNotExists, eResponseStatus.ModuleNotExists.ToString());
                }

            }
            catch (Exception ex)
            {
                result.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleDataResponse. ");
                sb.Append(String.Concat(" GroupID: ", m_nGroupID));
                sb.Append(String.Concat(" PPVModuleCode: ", sPPVModuleCode));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion                
            }

            return result;
        }

        public override PPVModule[] GetPPVModulesData(string[] sPPVModuleCodes, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModule[] ppvModules = null;
            if (sPPVModuleCodes != null && sPPVModuleCodes.Length > 0)
            {
                List<long> ppvModuleCodes = new List<long>();
                long ppvModuleCode;
                foreach (string code in sPPVModuleCodes)
                {
                    if (long.TryParse(code, out ppvModuleCode) && ppvModuleCode > 0)
                    {
                        ppvModuleCodes.Add(ppvModuleCode);
                    }
                    else
                    {
                        log.Error("Error - " + string.Format("Failed to parse: {0} into long", code));
                    }
                }

                if (ppvModuleCodes.Count > 0)
                {
                    DataTable dt = PricingDAL.Get_PPVModulesData(m_nGroupID, ppvModuleCodes);
                    if (dt != null & dt.Rows != null && dt.Rows.Count > 0)
                    {
                        ppvModules = new PPVModule[dt.Rows.Count];
                        int index = 0;
                        foreach (DataRow ppvModuleDataRow in dt.Rows)
                        {
                            PPVModule ppvModule = new PPVModule();
                            int ppvModuleID = ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["ID"]);
                            string sPriceCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["PRICE_CODE"]);
                            string sUsageModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["USAGE_MODULE_CODE"]);
                            string sDiscountModuleCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["DISCOUNT_MODULE_CODE"]);
                            string sCouponGroupCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["COUPON_GROUP_CODE"]);
                            string sName = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["NAME"]);
                            bool bSubOnly = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["SUBSCRIPTION_ONLY"]));
                            bool bIsFirstDeviceLimitation = Convert.ToBoolean(ODBCWrapper.Utils.GetIntSafeVal(ppvModuleDataRow["FIRSTDEVICELIMITATION"]));
                            string productCode = ODBCWrapper.Utils.GetSafeStr(ppvModuleDataRow["Product_Code"]);
                            ppvModule.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, GetPPVDescription(ppvModuleID), m_nGroupID, ppvModuleID.ToString(), bSubOnly,
                                           sName, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, GetPPVFileTypes(m_nGroupID, ppvModuleID), bIsFirstDeviceLimitation, productCode);

                            if (ppvModule != null)
                            {
                                ppvModules[index] = ppvModule;
                            }

                            index++;
                        }
                    }
                    else
                    {
                        log.Error("Error - " + string.Format("Get_PPVModulesData returned invalid DataTable, ppvModuleCodes: {0}", sPPVModuleCodes));
                    }
                }
                else
                {
                    log.Error("Error - " + string.Format("ppvModuleCodes is empty: {0}", ppvModuleCodes));
                }
            }

            return ppvModules;

        }
        
        public override PPVModule[] GetPPVModulesDataByProductCodes(List<string> productCodes)
        {
            PPVModule[] ppvModules = null;
            if (productCodes != null && productCodes.Count > 0)
            {
                string[] ppvModulesCodes = DAL.PricingDAL.Get_PPVsFromProductCodes(productCodes.Distinct().ToList(), m_nGroupID).Keys.ToArray();
                if (ppvModulesCodes != null && ppvModulesCodes.Length > 0)
                {
                    ppvModules = new PPVModule[ppvModulesCodes.Length];
                    for (int i = 0; i < ppvModules.Length; i++)
                    {
                        ppvModules[i] = GetPPVModuleData(ppvModulesCodes[i], string.Empty, string.Empty, string.Empty);
                    }
                }
            }

            return ppvModules;
        }
    }
}
