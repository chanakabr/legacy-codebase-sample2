using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class PPVManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PPVManager> lazy = new Lazy<PPVManager>(() =>
                                    new PPVManager(PricingDAL.Instance, LayeredCache.Instance, PricingCache.Instance, PricingDAL.Instance, PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static PPVManager Instance => lazy.Value;

        private readonly IPPVManagerRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IPricingCache _pricingCache;
        private readonly IPriceDetailsRepository _priceDetailsRepository;
        private readonly IModuleManagerRepository _moduleManagerRepository;

        public PPVManager(IPPVManagerRepository repository, ILayeredCache layeredCache, IPricingCache pricingCache,
            IPriceDetailsRepository priceDetailsRepository, IModuleManagerRepository moduleManagerRepository)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _pricingCache = pricingCache;
            _priceDetailsRepository = priceDetailsRepository;
            _moduleManagerRepository = moduleManagerRepository;
        }

        public GenericListResponse<PPVModule> GetPPVModulesData(int groupId, PpvByIdInFilter filter)
        {
            GenericListResponse<PPVModule> response = new GenericListResponse<PPVModule>();

            response.Objects = GetPPVModuleList(groupId);

            if (response.Objects?.Count > 0)
            {
                response.SetStatus(eResponseStatus.OK);

                if (filter != null)
                {
                    if (filter.CouponGroupIdEqual.HasValue)
                    {
                        response.Objects = response.Objects.Where(ppv => ppv.m_oCouponsGroup.m_sGroupCode == filter.CouponGroupIdEqual.Value.ToString()).ToList();
                    }
                    else if (filter.IdIn?.Count > 0)
                    {
                        response.Objects = response.Objects.Where(pi => filter.IdIn.Contains(pi.m_sObjectCode)).ToList();
                    }


                    switch (filter.OrderBy.m_eOrderDir)
                    {

                        case ApiObjects.SearchObjects.OrderDir.DESC:
                            response.Objects = response.Objects.OrderByDescending(r => r.m_sObjectVirtualName).ToList();
                            break;
                        case ApiObjects.SearchObjects.OrderDir.ASC:
                            response.Objects = response.Objects.OrderBy(r => r.m_sObjectVirtualName).ToList();
                            break;
                        default:
                            break;
                    }
                }

            }
            response.TotalItems = response.Objects?.Count == 0 ? 0 : response.Objects.Count;

            return response;
        }

        public List<PPVModule> GetPPVModuleList(int groupId, bool shouldShrink = false)
        {
            List<PPVModule> result = null;

            string key = LayeredCacheKeys.GetGroupPPVModuleIdsKey(groupId);
            Dictionary<string, object> layeredCacheParameters = new Dictionary<string, object>() { { "groupId", groupId } };
            List<long> ppvIds = new List<long>();

            if (_layeredCache.Get<List<long>>(key, ref ppvIds, GetAllGroupPPVModulesIds, layeredCacheParameters,
                groupId, LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId) }))
            {
                List<PPVModule> ppvModules = new List<PPVModule>();

                ppvIds.ForEach(ppvId =>
                {
                    ppvModules.Add(GetPPVModuleData(groupId, ppvId.ToString(), shouldShrink));
                });

                result = ppvModules;
            }
            else
            {
                log.ErrorFormat("Error when getting ppv modules data from layered cache");

                DataTable dtPPVModuleData = _repository.Get_PPVModuleData(groupId, null);

                if (dtPPVModuleData?.Rows?.Count > 0)
                {
                    PPVModule ppvModule;
                    foreach (DataRow row in dtPPVModuleData.Rows)
                    {
                        int nPPVModuleID = ODBCWrapper.Utils.GetIntSafeVal(row["ID"]);
                        ppvModule = BuildPPVModuleFromDataRow(groupId, nPPVModuleID, row, shouldShrink);
                        if (ppvModule.m_oPriceCode != null)
                        {
                            result.Add(ppvModule);
                        }
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

                    DataTable dtPPVModuleData = _repository.Get_PPVModuleData(groupId, null);

                    if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
                    {
                        success = true;
                        result = new List<long>();

                        foreach (DataRow row in dtPPVModuleData.Rows)
                        {
                            result.Add(ODBCWrapper.Utils.GetLongSafeVal(row, "ID"));
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

        public PPVModule GetPPVModuleData(int groupId, string stringPPVModuleCode, bool shouldShrink = false)
        {
            PPVModule result = null;

            try
            {
                if (!int.TryParse(stringPPVModuleCode, out int ppvModuleCode) || ppvModuleCode == 0)
                    return result;

                string key = LayeredCacheKeys.GetPPVModuleKey(ppvModuleCode, shouldShrink);
                Dictionary<string, object> layeredCacheParameters = new Dictionary<string, object>()
                    {
                        { "ppvModuleCode", ppvModuleCode },
                        { "groupId", groupId },
                        { "shouldShrink", shouldShrink }
                    };

                if (!_layeredCache.Get<PPVModule>(key, ref result, BuildPPVModule, layeredCacheParameters,
                    groupId, LayeredCacheConfigNames.PPV_MODULES_CACHE_CONFIG_NAME, new List<string>() { LayeredCacheKeys.GetPricingSettingsInvalidationKey(groupId) }))
                {
                    log.Error($"Error when getting ppv modules data from layered cache. groupId {groupId}");

                    DataTable dtPPVModuleData = _repository.Get_PPVModuleData(groupId, ppvModuleCode);

                    if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
                    {
                        result = BuildPPVModuleFromDataRow(groupId, ppvModuleCode, dtPPVModuleData.Rows[0], shouldShrink);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception at GetPPVModuleData. Ex Msg: {ex.Message} G ID:{groupId} PPVMC: {stringPPVModuleCode} Ex Type: {ex.GetType().Name}  ST: {ex.StackTrace}", ex);
                throw ex;
            }

            return result;
        }

        public List<LanguageContainer> GetPPVDescription(int ppvModuleId)
        {
            List<LanguageContainer> languageContainerList = null;
            DataTable dtPPVDescription = _repository.Get_PPVDescription(ppvModuleId);
            if (dtPPVDescription?.Rows.Count > 0)
            {
                languageContainerList = new List<LanguageContainer>();
                foreach (DataRow row in dtPPVDescription.Rows)
                {
                    languageContainerList.Add(new LanguageContainer()
                    {
                        m_sLanguageCode3 = ODBCWrapper.Utils.GetSafeStr(row["language_code3"]),
                        m_sValue = ODBCWrapper.Utils.GetSafeStr(row["description"])
                    });
                }
            }

            return languageContainerList;
        }

        public List<int> GetPPVFileTypes(int groupId, int ppvModuleId)
        {
            List<int> retVal = null;

            DataTable dtFileTypes = _repository.Get_PPVFileTypes(groupId, ppvModuleId);
            if (dtFileTypes?.Rows?.Count > 0)
            {
                retVal = new List<int>();

                foreach (DataRow row in dtFileTypes.Rows)
                {
                    retVal.Add(ODBCWrapper.Utils.GetIntSafeVal(row["file_type_id"]));
                }
            }

            return retVal;
        }

        private Tuple<PPVModule, bool> BuildPPVModule(Dictionary<string, object> funcParams)
        {
            bool success = false;
            PPVModule result = null;

            try
            {
                if (funcParams != null && funcParams.ContainsKey("ppvModuleCode") && funcParams.ContainsKey("groupId"))
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
                        DataTable dtPPVModuleData = _repository.Get_PPVModuleData(groupId, ppvModuleCode);

                        if (dtPPVModuleData != null && dtPPVModuleData.Rows != null && dtPPVModuleData.Rows.Count > 0)
                        {
                            result = BuildPPVModuleFromDataRow(groupId, ppvModuleCode, dtPPVModuleData.Rows[0], shouldShrink);
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

        public bool IsPPVModuleExist(int groupId, long ppvModuleId)
        {
            // check ppvModuleId  exist
            DataTable dt = _repository.Get_PPVModuleData(groupId, (int)ppvModuleId);
            return dt?.Rows?.Count > 0;
        }

        private PPVModule BuildPPVModuleFromDataRow(int groupId, int ppvModuleCode, DataRow ppvModuleDataRow, bool shouldShrink = false)
        {
            PPVModule result = new PPVModule();
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

            var ppvDescriptionList = GetPPVDescription(ppvModuleCode);
            var ppvDescriptions = ppvDescriptionList?.Count > 0 ? ppvDescriptionList.ToArray() : null;
            if (!shouldShrink)
            {              
                result.Initialize(sPriceCode, sUsageModuleCode, sDiscountModuleCode, sCouponGroupCode, ppvDescriptions , groupId, ppvModuleCode.ToString(), bSubOnly,
                               sName, string.Empty, string.Empty, string.Empty, GetPPVFileTypes(groupId, ppvModuleCode), bIsFirstDeviceLimitation, productCode, 0, adsPolicy, adsParam);
            }
            else
            {
                result.Initialize(sPriceCode, string.Empty, string.Empty, string.Empty, ppvDescriptions, groupId, ppvModuleCode.ToString(), bSubOnly,
                               sName, string.Empty, string.Empty, string.Empty, GetPPVFileTypes(groupId, ppvModuleCode), bIsFirstDeviceLimitation, productCode,
                               0, adsPolicy, adsParam);
            }

            return result;
        }


        public GenericResponse<PPVModule> Get(int groupId, long ppvCode)
        {
            GenericResponse<PPVModule> response = new GenericResponse<PPVModule>();

            var ppvModule = GetPPVModuleData(groupId, ppvCode.ToString());

            if (ppvModule == null)
            {
                response.SetStatus(eResponseStatus.ModuleNotExists, $"The ppv module {ppvCode} not exist");
            }
            else
            {
                response.SetStatus(eResponseStatus.OK);
                response.Object = ppvModule;
            }

            return response;
        }

        public GenericResponse<PPVModule> Add(ContextData contextData, PPVModule ppvToInsert)
        {
            var response = new GenericResponse<PPVModule>();

            try
            {
                if (string.IsNullOrEmpty(ppvToInsert.m_sObjectVirtualName))
                {
                    response.SetStatus(eResponseStatus.NameRequired, "Name required");
                    return response;
                }

                #region m_oPriceCode validation
                if (ppvToInsert.m_oPriceCode == null || ppvToInsert.m_oPriceCode.m_nObjectID < 0)
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "priceCode Id invalid value");
                    return response;
                }

                //check that priceCode is valid
                if (!_priceDetailsRepository.IsPriceCodeExistsById(contextData.GroupId, ppvToInsert.m_oPriceCode.m_nObjectID))
                {
                    response.SetStatus(eResponseStatus.PriceDetailsDoesNotExist, $"Price {ppvToInsert.m_oPriceCode.m_nObjectID} does not exist");
                    return response;
                }
                #endregion

                #region m_oUsageModule validation
                if (ppvToInsert.m_oUsageModule == null || ppvToInsert.m_oUsageModule.m_nObjectID < 0)
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "usageModule Id invalid value");
                    return response;
                }

                //check that UsageModule is valid
                if (!_moduleManagerRepository.IsUsageModuleExistsById(contextData.GroupId, ppvToInsert.m_oUsageModule.m_nObjectID))
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "usageModule Id invalid value");
                    return response;
                }
                #endregion

                #region m_oDiscountModule  validation
                if (ppvToInsert.m_oDiscountModule != null && ppvToInsert.m_oDiscountModule.m_nObjectID < 0)
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "discountModule Id invalid value");
                    return response;
                }

                //check that UsageModule is valid
                if (!_moduleManagerRepository.IsUsageModuleExistsById(contextData.GroupId, ppvToInsert.m_oUsageModule.m_nObjectID))
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "usageModule Id invalid value");
                    return response;
                }
                #endregion

                #region m_oCouponsGroup  validation
                if (ppvToInsert.m_oCouponsGroup == null || string.IsNullOrEmpty(ppvToInsert.m_oCouponsGroup.m_sGroupCode) || 
                    !int.TryParse(ppvToInsert.m_oCouponsGroup.m_sGroupCode, out int couponsGroupId) || couponsGroupId < 1)
                {
                    response.SetStatus(eResponseStatus.InvalidArgumentValue, "CouponGroup Id invalid value");
                    return response;
                }                             
                #endregion


                IngestPPV ingestPPV = new IngestPPV()
                {
                    Code = ppvToInsert.m_sObjectVirtualName,
                    IsActive = true,
                    SubscriptionOnly = ppvToInsert.m_bSubscriptionOnly,
                    FirstDeviceLimitation = ppvToInsert.m_bFirstDeviceLimitation,
                    ProductCode = ppvToInsert.m_Product_Code
                };

                int discountModuleId = ppvToInsert.m_oDiscountModule?.m_nObjectID > 0 ? ppvToInsert.m_oDiscountModule.m_nObjectID : 0;                

                int id = _repository.InsertPPV(contextData.GroupId, ingestPPV, ppvToInsert.m_oPriceCode.m_nObjectID, ppvToInsert.m_oUsageModule.m_nObjectID,
                    discountModuleId, couponsGroupId, ppvToInsert.m_relatedFileTypes.ConvertAll(i => (long)i));

                if (id == 0)
                {
                    log.Error($"Error while InsertPricePlan. contextData: {contextData.ToString()}.");
                    return response;
                }

                response.Object = GetPPVModuleData(contextData.GroupId, id.ToString(), true);
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in ppv. contextData:{contextData.ToString()}, name:{ppvToInsert.m_sObjectVirtualName}.", ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long ppvCode)
        {
            Status result = new Status();

            // get the MediaFilePPVModule 
            if (GetPPVModuleData(contextData.GroupId, ppvCode.ToString(), true) == null)
            {
                result.Set(eResponseStatus.ModuleNotExists, $"PPV {ppvCode} does not exist");
                return result;
            }

            int Id = _repository.DeletePPV(contextData.GroupId, ppvCode);
            if (Id == 0)
            {
                result.Set(eResponseStatus.Error);
            }
            else if (Id == -1)
            {
                result.Set(eResponseStatus.ModuleNotExists, $"The ppv module {ppvCode} not exist");
            }
            else
            {
                result.Set(eResponseStatus.OK);
            }

            return result;
        }
    }
}