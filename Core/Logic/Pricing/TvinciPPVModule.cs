using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using Phx.Lib.Log;
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

        protected PPVModule[] GetTvinciPPVModuleList(bool shouldShrink)
        {
            PPVModule[] result = null;

            string key = LayeredCacheKeys.GetGroupPPVModuleIdsKey(GroupID);
            Dictionary<string, object> layeredCacheParameters = new Dictionary<string, object>()
                {
                    { "groupId", GroupID }
                };

            List<long> ppvIds = new List<long>();

            if (LayeredCache.Instance.Get<List<long>>(key, ref ppvIds, GetAllGroupPPVModulesIds, layeredCacheParameters,
                GroupID, LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(GroupID) }))
            {
                List<PPVModule> ppvModules = new List<PPVModule>();

                foreach (var ppvId in ppvIds)
                {
                    ppvModules.Add(GetPPVModuleData(ppvId.ToString(), shouldShrink));
                }

                result = ppvModules.ToArray();
            }
            else
            {
                log.ErrorFormat("Error when getting ppv modules data from layered cache");

                var ppvDtoList = PricingDAL.Instance.Get_PPVModuleData(m_nGroupID, null);

                if (ppvDtoList != null && ppvDtoList.Any())
                {
                    result = new PPVModule[ppvDtoList.Count];
                    Int32 nIndex = 0;
                    for (int i = 0; i < ppvDtoList.Count; i++)
                    {
                        PPVModule t = PpvManager.Instance.BuildPPVModuleFromDTO(m_nGroupID, ppvDtoList[i], shouldShrink);
                        if (t.m_oPriceCode != null)
                        {
                            result[nIndex] = t;
                            nIndex++;
                        }
                    }
                    if (nIndex < ppvDtoList.Count)
                    {
                        PPVModule[] tmp1 = new PPVModule[nIndex];
                        Array.Copy(result, tmp1, nIndex);
                        result = tmp1;
                    }
                }
            }

            return result;
        }

        private Tuple<List<long>, bool> GetAllGroupPPVModulesIds(Dictionary<string, object> funcParams)
        {
            bool success = false;
            List<long> result = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int groupId = Convert.ToInt32(funcParams["groupId"]);
                    var ppvDtoList = PricingDAL.Instance.Get_PPVModuleData(m_nGroupID, null);

                    if (ppvDtoList != null && ppvDtoList.Any())
                    {
                        success = true;
                        result = new List<long>();

                        foreach (var ppvDto in ppvDtoList)
                        {
                            result.Add(ppvDto.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.Error(string.Format("BuildPPVModule failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<List<long>, bool>(result, success);
        }

        public override PPVModule[] GetPPVModuleList()
        {
            return GetTvinciPPVModuleList(false);
        }

        public override PPVModule[] GetPPVModuleShrinkList()
        {
            return GetTvinciPPVModuleList(true);
        }

        public override PPVModule GetPPVModuleData(string stringPPVModuleCode, bool shouldShrink = false)
        {
            PPVModule result = null;

            try
            {
                int ppvModuleCode = 0;
                if (!int.TryParse(stringPPVModuleCode, out ppvModuleCode) || ppvModuleCode == 0)
                    return result;

                string key = LayeredCacheKeys.GetPPVModuleKey(ppvModuleCode, shouldShrink);
                Dictionary<string, object> layeredCacheParameters = new Dictionary<string, object>()
                {
                    { "ppvModuleCode", ppvModuleCode },
                    { "groupId", GroupID },
                    { "shouldShrink", shouldShrink }
                };

                if (!LayeredCache.Instance.Get<PPVModule>(key, ref result, BuildPPVModule, layeredCacheParameters,
                    GroupID, LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(GroupID) }))
                {
                    log.ErrorFormat("Error when getting ppv modules data from layered cache");

                    var ppvDtoList = PricingDAL.Instance.Get_PPVModuleData(m_nGroupID, ppvModuleCode);
                    if (ppvDtoList != null && ppvDtoList.Any())
                    {
                        result = PpvManager.Instance.BuildPPVModuleFromDTO(m_nGroupID, ppvDtoList[0], shouldShrink);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleData. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" PPVMC: ", stringPPVModuleCode));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }

            return result;
        }


        public override PPVModuleDataResponse GetPPVModuleDataResponse(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, long? shopUserId)
        {
            PPVModuleDataResponse result = new PPVModuleDataResponse();
            try
            {
                result.PPVModule = GetPPVModuleData(sPPVModuleCode);
                if (result.PPVModule == null)
                {
                    result.Status = new Status(eResponseStatus.ModuleNotExists, eResponseStatus.ModuleNotExists.ToString());
                    return result;
                }

                result.Status = ValidatePpvInUserShop(result.PPVModule, shopUserId, GroupID);
            }
            catch (Exception ex)
            {
                result.Status = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
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

                                ppvModule.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, GetPPVDescription(ppvModuleID), m_nGroupID, ppvModuleID.ToString(), bSubOnly,
                                               sName, string.Empty, string.Empty, string.Empty, GetPPVFileTypes(m_nGroupID, ppvModuleID), bIsFirstDeviceLimitation, productCode);

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
                                    GetPPVDescription(ppvModuleID),
                                    groupId, ppvModuleID.ToString(), bSubOnly,
                                    sName, string.Empty, string.Empty, string.Empty, GetPPVFileTypes(groupId, ppvModuleID),
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

        private static Tuple<PPVModule, bool> BuildPPVModule(Dictionary<string, object> funcParams)
        {
            bool success = false;
            PPVModule result = null;

            try
            {
                if (funcParams != null &&
                    funcParams.ContainsKey("ppvModuleCode") &&
                    funcParams.ContainsKey("groupId")
                    )
                {
                    int groupId = Convert.ToInt32(funcParams["groupId"]);
                    int ppvModuleCode = Convert.ToInt32(funcParams["ppvModuleCode"]);

                    bool shouldShrink = false;

                    if (funcParams.ContainsKey("shouldShrink"))
                    {
                        shouldShrink = Convert.ToBoolean(funcParams["shouldShrink"]);
                    }

                    if (ppvModuleCode > 0)
                    {
                        var ppvDtoList = PricingDAL.Instance.Get_PPVModuleData(groupId, ppvModuleCode);
                        if (ppvDtoList != null && ppvDtoList.Any())
                        {
                            result = PpvManager.Instance.BuildPPVModuleFromDTO(groupId, ppvDtoList[0], shouldShrink);
                            success = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                success = false;
                log.Error(string.Format("BuildPPVModule failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<PPVModule, bool>(result, success);
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
                        ppvModules[i] = GetPPVModuleData(ppvModulesCodes[i]);
                    }
                }
            }

            return ppvModules;
        }

    }
}
