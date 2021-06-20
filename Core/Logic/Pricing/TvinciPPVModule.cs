using ApiLogic.Pricing.Handlers;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Core.Pricing
{
    public class TvinciPPVModule : BasePPVModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciPPVModule(Int32 nGroupID) : base(nGroupID) { }

        public override PPVModuleDataResponse GetPPVModuleDataResponse(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModuleDataResponse result = new PPVModuleDataResponse();
            try
            {
                result.PPVModule = PPVManager.Instance.GetPPVModuleData(m_nGroupID, sPPVModuleCode);
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

        public override PPVModule[] GetPPVModulesData(string[] sPPVModuleCodes)
        {
            PPVModule[] ppvModules = null;
            if (sPPVModuleCodes != null && sPPVModuleCodes.Length > 0)
            {
                List<long> ppvModuleCodes = new List<long>();
                foreach (string code in sPPVModuleCodes)
                {
                    long ppvModuleCode;

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
                    Dictionary<string, string> keysToOriginalValueMap = ppvModuleCodes.ToDictionary(x => LayeredCacheKeys.GetPPVModuleKey(x), x => x.ToString());
                    Dictionary<string, List<string>> keysToInvalidationKeysMap = keysToOriginalValueMap.Keys.ToDictionary(x => x, x => new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(GroupID) });
                    Dictionary<string, PPVModule> results = new Dictionary<string, PPVModule>();
                    Dictionary<string, object> layeredCacheParameters = new Dictionary<string, object>()
                        {
                            { "ppvModuleCodes", ppvModuleCodes },
                            { "groupId", GroupID }
                        };

                    if (!LayeredCache.Instance.GetValues<PPVModule>(keysToOriginalValueMap, ref results, BuildPPVModules, layeredCacheParameters, GroupID,
                                                                    LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME, keysToInvalidationKeysMap))
                    {
                        log.ErrorFormat("Error when getting ppv modules data from layered cache");

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

                                ppvModule.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, PPVManager.Instance.GetPPVDescription(ppvModuleID).ToArray(), m_nGroupID, ppvModuleID.ToString(), bSubOnly,
                                               sName, string.Empty, string.Empty, string.Empty, PPVManager.Instance.GetPPVFileTypes(m_nGroupID, ppvModuleID), bIsFirstDeviceLimitation, productCode);

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
                    else if (results?.Count > 0)
                    {
                        var count = results.Count();
                        ppvModules = new PPVModule[count];
                        int index = 0;
                        foreach (var result in results)
                        {
                            ppvModules[index] = result.Value;
                            index++;
                        }
                    }
                }
                else
                {
                    log.Error("Error - " + string.Format("ppvModuleCodes is empty: {0}", ppvModuleCodes));
                }
            }

            return ppvModules;

        }

        private static Tuple<Dictionary<string, PPVModule>, bool> BuildPPVModules(Dictionary<string, object> funcParams)
        {
            bool success = false;
            Dictionary<string, PPVModule> result = null;

            try
            {
                if (funcParams != null &&
                    funcParams.ContainsKey("ppvModuleCodes") &&
                    funcParams.ContainsKey("groupId")
                    )
                {
                    int groupId = Convert.ToInt32(funcParams["groupId"]);
                    List<long> ppvModuleCodes = funcParams["ppvModuleCodes"] != null ? funcParams["ppvModuleCodes"] as List<long> : null;

                    if (ppvModuleCodes != null && ppvModuleCodes.Count > 0)
                    {
                        DataTable dataTable = PricingDAL.Get_PPVModulesData(groupId, ppvModuleCodes);
                        if (dataTable != null & dataTable.Rows != null && dataTable.Rows.Count > 0)
                        {
                            result = new Dictionary<string, PPVModule>();

                            foreach (DataRow ppvModuleDataRow in dataTable.Rows)
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

                                ppvModule.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode,
                                    PPVManager.Instance.GetPPVDescription(ppvModuleID).ToArray(),
                                    groupId, ppvModuleID.ToString(), bSubOnly,
                                    sName, string.Empty, string.Empty, string.Empty, PPVManager.Instance.GetPPVFileTypes(groupId, ppvModuleID),
                                    bIsFirstDeviceLimitation, productCode);

                                if (ppvModule != null)
                                {
                                    result.Add(LayeredCacheKeys.GetPPVModuleKey(ppvModuleID), ppvModule);
                                }
                            }
                        }
                        else
                        {
                            log.Error("Error - " + string.Format("Get_PPVModulesData returned invalid DataTable, ppvModuleCodes: {0}", string.Join(",", ppvModuleCodes)));
                        }

                        success = result.Count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("BuildPPVModules failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, PPVModule>, bool>(result, success);
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
                        ppvModules[i] = PPVManager.Instance.GetPPVModuleData(m_nGroupID, ppvModulesCodes[i]);
                    }
                }
            }

            return ppvModules;
        }

    }
}