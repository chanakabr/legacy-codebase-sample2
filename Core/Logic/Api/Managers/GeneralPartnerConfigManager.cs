using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Repositories;
using TVinciShared;

namespace ApiLogic.Api.Managers
{
    public interface IGeneralPartnerConfigManager
    {
        bool IsValidCurrencyCode(int groupId, string currencyCode3);
        bool GetGroupDefaultCurrency(int groupId, ref string currencyCode);
        GeneralPartnerConfig GetGeneralPartnerConfig(int groupId);
        List<Currency> GetCurrencyList(int groupId);
        Dictionary<string, Currency> GetCurrencyMapByCode3(int groupId);
    }

    public class GeneralPartnerConfigManager : IGeneralPartnerConfigManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<GeneralPartnerConfigManager> lazy = new Lazy<GeneralPartnerConfigManager>(
            () => new GeneralPartnerConfigManager(ApiDAL.Instance, DeviceFamilyRepository.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IGeneralPartnerConfigRepository _repository;
        private readonly IDeviceFamilyRepository _deviceFamilyRepository;

        public static GeneralPartnerConfigManager Instance { get { return lazy.Value; } }

        public GeneralPartnerConfigManager(
            IGeneralPartnerConfigRepository repository,
            IDeviceFamilyRepository deviceFamilyRepository)
        {
            _repository = repository;
            _deviceFamilyRepository = deviceFamilyRepository;
        }

        public bool IsValidCurrencyCode(int groupId, string currencyCode3)
        {
            bool res = false;
            if (string.IsNullOrEmpty(currencyCode3))
            {
                return res;
            }

            res = GetCurrencyMapByCode3(groupId).ContainsKey(currencyCode3.ToLower());
            return res;
        }

        public bool GetGroupDefaultCurrency(int groupId, ref string currencyCode)
        {
            bool res = false;
            try
            {
                int defaultGroupCurrencyId = 0;
                if (LayeredCache.Instance.Get<int>(LayeredCacheKeys.GetGroupDefaultCurrencyKey(groupId), ref defaultGroupCurrencyId, GetGroupDefaultCurrency, new Dictionary<string, object>() { { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GET_DEFAULT_GROUP_CURRENCY_LAYERED_CACHE_CONFIG_NAME) && defaultGroupCurrencyId > 0)
                {
                    DataTable dt = null;
                    if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY, ref dt, GetAllCurrencies, new Dictionary<string, object>(), groupId,
                                                            LayeredCacheConfigNames.GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME) && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        currencyCode = (from row in dt.AsEnumerable()
                                        where (Int64)row["ID"] == defaultGroupCurrencyId
                                        select row.Field<string>("CODE3")).FirstOrDefault();
                        res = !string.IsNullOrEmpty(currencyCode);
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupDefaultCurrency, groupId: {0}", groupId), ex);
            }

            return res;

        }

        public List<Currency> GetCurrencyList(int groupId)
        {
            List<Currency> currencies = null;
            try
            {
                DataTable dt = null;
                if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY,
                    ref dt,
                    GetAllCurrencies,
                    new Dictionary<string, object>(),
                    groupId,
                    LayeredCacheConfigNames.GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME) && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    currencies = new List<Currency>();
                    Currency currency;
                    foreach (DataRow dr in dt.Rows)
                    {
                        currency = new Currency()
                        {
                            m_nCurrencyID = ODBCWrapper.Utils.GetIntSafeVal(dr, "id"),
                            m_sCurrencyName = ODBCWrapper.Utils.GetSafeStr(dr, "name"),
                            m_sCurrencyCD2 = ODBCWrapper.Utils.GetSafeStr(dr, "code2"),
                            m_sCurrencyCD3 = ODBCWrapper.Utils.GetSafeStr(dr, "code3"),
                            m_sCurrencySign = ODBCWrapper.Utils.GetSafeStr(dr, "currency_sign")
                        };

                        currencies.Add(currency);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetCurrencyList, groupId: {0}, ex: {1}", groupId, ex);
            }

            return currencies;
        }

        internal Status UpdateGeneralPartnerConfig(int groupId, GeneralPartnerConfig partnerConfigToUpdate)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            try
            {
                bool shouldInvalidateRegions = false;

                // check for MainLanguage valid
                if (partnerConfigToUpdate.MainLanguage.HasValue)
                {
                    if (!IsValidLanguageId(groupId, partnerConfigToUpdate.MainLanguage.Value))
                    {
                        log.ErrorFormat("Error while update generalPartnerConfig. MainLanguage {0}, groupId: {1}", partnerConfigToUpdate.MainLanguage.Value, groupId);
                        response.Set((int)eResponseStatus.InvalidLanguage, eResponseStatus.InvalidLanguage.ToString());
                        return response;
                    }
                }

                // check for SecondaryLanguages valid
                if (partnerConfigToUpdate.SecondaryLanguages != null && partnerConfigToUpdate.SecondaryLanguages.Count > 0)
                {
                    foreach (var secondaryLanguageId in partnerConfigToUpdate.SecondaryLanguages)
                    {
                        if (!IsValidLanguageId(groupId, secondaryLanguageId))
                        {
                            log.ErrorFormat("Error while update generalPartnerConfig. SecondaryLanguageId {0}, groupId: {1}", secondaryLanguageId, groupId);
                            response.Set((int)eResponseStatus.InvalidLanguage, eResponseStatus.InvalidLanguage.ToString());
                            return response;
                        }
                    }
                }

                // check for MainCurrency valid
                if (partnerConfigToUpdate.MainCurrency.HasValue)
                {
                    if (!IsValidCurrencyId(groupId, partnerConfigToUpdate.MainCurrency.Value))
                    {
                        log.ErrorFormat("Error while update generalPartnerConfig. MainCurrencyId {0}, groupId: {1}", partnerConfigToUpdate.MainCurrency.Value, groupId);
                        response.Set((int)eResponseStatus.InvalidCurrency, eResponseStatus.InvalidCurrency.ToString());
                        return response;
                    }
                }

                // check for SecondaryCurrencys valid
                if (partnerConfigToUpdate.SecondaryCurrencies != null && partnerConfigToUpdate.SecondaryCurrencies.Count > 0)
                {
                    foreach (var secondaryCurrencyId in partnerConfigToUpdate.SecondaryCurrencies)
                    {
                        if (!IsValidCurrencyId(groupId, secondaryCurrencyId))
                        {
                            log.ErrorFormat("Error while update generalPartnerConfig. SecondaryCurrency {0}, groupId: {1}", secondaryCurrencyId, groupId);
                            response.Set((int)eResponseStatus.InvalidCurrency, eResponseStatus.InvalidCurrency.ToString());
                            return response;
                        }
                    }
                }

                if (partnerConfigToUpdate.HouseholdLimitationModule.HasValue)
                {
                    var limitationsManagerResponse = Core.Domains.Module.GetDLMList(groupId);
                    if (limitationsManagerResponse != null && limitationsManagerResponse.HasObjects() &&
                        limitationsManagerResponse.Objects.Count(x => x.domianLimitID == partnerConfigToUpdate.HouseholdLimitationModule.Value) == 0)
                    {
                        log.ErrorFormat("Error while update generalPartnerConfig. HouseholdLimitationModule {0} not exist in groupId: {1}", partnerConfigToUpdate.HouseholdLimitationModule.Value, groupId);
                        response.Set((int)eResponseStatus.DlmNotExist, eResponseStatus.DlmNotExist.ToString());
                        return response;
                    }
                }

                if (partnerConfigToUpdate.DefaultRegion.HasValue)
                {
                    var defaultRegion = RegionManager.GetRegion(groupId, partnerConfigToUpdate.DefaultRegion.Value);
                    if (defaultRegion == null)
                    {
                        log.ErrorFormat("Error while update generalPartnerConfig. DefaultRegion {0} not exist in groupId: {1}", partnerConfigToUpdate.DefaultRegion.Value, groupId);
                        response.Set((int)eResponseStatus.RegionDoesNotExist, eResponseStatus.RegionDoesNotExist.ToString());
                        return response;
                    }

                    CatalogGroupCache catalogGroupCache;
                    shouldInvalidateRegions = (CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache)
                                               && defaultRegion.id != catalogGroupCache.DefaultRegion);
                }

                if (partnerConfigToUpdate?.RollingDeviceRemovalData?.RollingDeviceRemovalFamilyIds?.Count > 0 || partnerConfigToUpdate.DowngradePriorityFamilyIds.Count > 0)
                {
                    // validate deviceFamilyIds
                    var listResponse = _deviceFamilyRepository.List(groupId);
                    if (!listResponse.IsOkStatusCode())
                    {
                        response.Message = "No DeviceFamilies";
                        return response;
                    }

                    List<int> allFamilyIds = new List<int>();
                    if (partnerConfigToUpdate?.RollingDeviceRemovalData?.RollingDeviceRemovalFamilyIds?.Count > 0)
                    {
                        var rollingDeviceRemovalFamilyIds = partnerConfigToUpdate.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds;

                        partnerConfigToUpdate.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds =
                            rollingDeviceRemovalFamilyIds.Distinct().ToList();
                        allFamilyIds.AddRange((rollingDeviceRemovalFamilyIds));
                        
                    }

                    if (partnerConfigToUpdate.DowngradePriorityFamilyIds.Count > 0)
                    {
                        allFamilyIds.AddRange((partnerConfigToUpdate.DowngradePriorityFamilyIds));
                    }
                    
                    var existingFamilies = allFamilyIds.Intersect(listResponse.Objects.Select(x => x.Id));
                    var notDeviceFamilies = allFamilyIds.Except(existingFamilies).ToList();

                    if (notDeviceFamilies.Count > 0)
                    {
                        response.Set((int)eResponseStatus.NonExistingDeviceFamilyIds,
                            $"The ids: {string.Join(", ", notDeviceFamilies)} are non-existing DeviceFamilyIds");
                        return response;
                    }

                }

                var generalPartnerConfig = GetGeneralPartnerConfig(groupId);
                if (generalPartnerConfig != null)
                {
                    partnerConfigToUpdate.SetUnchangedProperties(generalPartnerConfig);
                }

                // upsert GeneralPartnerConfig -           
                if (!ApiDAL.UpdateGeneralPartnerConfig(groupId, partnerConfigToUpdate))
                {
                    log.ErrorFormat("Error while update generalPartnerConfig. groupId: {0}", groupId);
                    return response;
                }

                string invalidationKey = LayeredCacheKeys.GetGeneralPartnerConfigInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for generalPartnerConfig with invalidationKey: {0}", invalidationKey);
                }

                invalidationKey = LayeredCacheKeys.GetCatalogGroupCacheInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for catalogGroupCache with invalidationKey: {0}", invalidationKey);
                }

                if (shouldInvalidateRegions)
                {
                    ApiLogic.Api.Managers.RegionManager.InvalidateRegions(groupId);
                }

                response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateDeviceConcurrencyPriority failed ex={0}, groupId={1}", ex, groupId);
            }

            return response;
        }

        private Tuple<int, bool> GetGroupDefaultCurrency(Dictionary<string, object> funcParams)
        {
            bool res = false;
            int groupDefaultCurrencyId = 0;
            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue && groupId.Value > 0)
                    {
                        groupDefaultCurrencyId = ConditionalAccessDAL.GetGroupDefaultCurrency(groupId.Value);
                        res = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupDefaultCurrency failed, function parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<int, bool>(groupDefaultCurrencyId, res);
        }

        private Tuple<DataTable, bool> GetAllCurrencies(Dictionary<string, object> funcParams)
        {
            bool res = false;
            DataTable dt = null;
            try
            {
                dt = ConditionalAccessDAL.GetAllCurrencies();
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetAllCurrencies failed, function parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            res = dt != null;
            return new Tuple<DataTable, bool>(dt, res);
        }

        private bool IsValidCurrencyId(int groupId, int currencyId)
        {
            bool res = false;
            if (currencyId <= 0)
            {
                return res;
            }

            try
            {
                DataTable dt = null;
                if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY, ref dt, GetAllCurrencies, new Dictionary<string, object>(), groupId,
                                                        LayeredCacheConfigNames.GET_CURRENCIES_LAYERED_CACHE_CONFIG_NAME) && dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = (from row in dt.AsEnumerable()
                           where ((long)row["id"]) == (long)currencyId
                           select row).Count() > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsValidCurrencyId, groupId: {0}, currencyCode: {1}", groupId, currencyId), ex);
            }

            return res;
        }

        private bool IsValidLanguageId(int groupId, int languageId)
        {
            bool res = false;
            if (languageId <= 0)
            {
                return res;
            }

            try
            {
                List<LanguageObj> languageList = GetAllLanguages(groupId);
                if (languageList == null && languageList.Count == 0)
                {
                    return res;
                }

                res = languageList.Count(x => x.ID == languageId) > 0;
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed IsValidLanguageId, groupId: {0}, languageId: {1}, ex: {2}", groupId, languageId, ex);
            }

            return res;
        }

        public List<LanguageObj> GetAllLanguages(int groupId)
        {
            List<LanguageObj> languages = null;
            try
            {
                string key = LayeredCacheKeys.GetAllLanguageListKey();

                if (!LayeredCache.Instance.Get<List<LanguageObj>>(key,
                                                              ref languages,
                                                              APILogic.Utils.GetAllLanguagesList,
                                                              new Dictionary<string, object>(),
                                                              groupId,
                                                              LayeredCacheConfigNames.GET_ALL_LANGUAGE_LIST_LAYERED_CACHE_CONFIG_NAME))
                {
                    log.ErrorFormat("Failed getting language list by Ids from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetAllLanguagesList for groupId: {0}, ex: {1}", groupId, ex);
            }

            return languages;
        }

        public GeneralPartnerConfig GetGeneralPartnerConfig(int groupId)
        {
            GeneralPartnerConfig generalPartnerConfig = null;

            try
            {
                string key = LayeredCacheKeys.GetGeneralPartnerConfig(groupId);
                List<string> configInvalidationKey = new List<string>() { LayeredCacheKeys.GetGeneralPartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<GeneralPartnerConfig>(key,
                                                          ref generalPartnerConfig,
                                                          GetGeneralPartnerConfigDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_GENERAL_PARTNER_CONFIG,
                                                          configInvalidationKey))
                {
                    log.ErrorFormat("Failed getting GetGeneralPartnerConfig from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGeneralPartnerConfig for groupId: {0}", groupId), ex);
            }

            return generalPartnerConfig;
        }

        private Tuple<GeneralPartnerConfig, bool> GetGeneralPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            GeneralPartnerConfig generalPartnerConfig = null;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    DataSet ds = _repository.GetGeneralPartnerConfig(groupId.Value);
                    if (ds != null && ds.Tables != null && ds.Tables.Count == 3)
                    {
                        DataTable dt = ds.Tables[0];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig = new GeneralPartnerConfig()
                            {
                                DateFormat = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "date_email_format"),
                                HouseholdLimitationModule = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "max_device_limit"),
                                MailSettings = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "mail_settings"),
                                MainCurrency = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "CURRENCY_ID"),
                                PartnerName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "GROUP_NAME"),
                                MainLanguage = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "LANGUAGE_ID"),
                                RollingDeviceRemovalData = GetRollingDeviceRemovalData(dt.Rows[0]),
                                LinearWatchHistoryThreshold = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "LINEAR_WATCH_HISTORY_THRESHOLD", 0),
                                FinishedPercentThreshold = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "FINISHED_PERCENT_THRESHOLD"),
                                AllowDeviceMobility = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "ALLOW_DEVICE_MOBILITY") == 1,
                                EnableMultiLcns = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "ENABLE_MULTI_LCNS") == 1
                            };

                            if (!generalPartnerConfig.FinishedPercentThreshold.HasValue || generalPartnerConfig.FinishedPercentThreshold.Value == 0)
                            {
                                generalPartnerConfig.FinishedPercentThreshold = CatalogLogic.FINISHED_PERCENT_THRESHOLD;
                            }

                            int? deleteMediaPolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DELETE_MEDIA_POLICY");
                            int? downgradePolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DOWNGRADE_POLICY");
                            int? defaultRegion = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DEFAULT_REGION");
                            int? enableRegionFiltering = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "IS_REGIONALIZATION_ENABLED");
                            int? suspensionProfileInheritanceType = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "SUSPENSION_PROFILE_INHERITANCE_TYPE");
                            string downgradePriority = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "DOWNGRADE_DEVICE_PRIORITY_FAMILY_IDS");
                            if (deleteMediaPolicy.HasValue)
                            {
                                generalPartnerConfig.DeleteMediaPolicy = (DeleteMediaPolicy)deleteMediaPolicy.Value;
                            }

                            if (downgradePolicy.HasValue)
                            {
                                generalPartnerConfig.DowngradePolicy = (DowngradePolicy)downgradePolicy.Value;
                            }
                            string rollingDeviceRemovalPolicyIds =
                                ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "ROLLING_DEVICE_REMOVAL_FAMILY_IDS");


                            if (!downgradePriority.IsNullOrEmptyOrWhiteSpace())
                            {
                                //gets the family ids
                                generalPartnerConfig.DowngradePriorityFamilyIds = downgradePriority.Split(',').Select(int.Parse).ToList();
                            }

                            if (enableRegionFiltering.HasValue)
                            {
                                generalPartnerConfig.EnableRegionFiltering = enableRegionFiltering.Value == 1;
                            }

                            if (defaultRegion.HasValue && defaultRegion.Value > 0)
                            {
                                generalPartnerConfig.DefaultRegion = defaultRegion.Value;
                            }

                            if (suspensionProfileInheritanceType.HasValue)
                            {
                                generalPartnerConfig.SuspensionProfileInheritanceType = (SuspensionProfileInheritanceType)suspensionProfileInheritanceType;
                            }
                            else
                            {
                                generalPartnerConfig.SuspensionProfileInheritanceType = SuspensionProfileInheritanceType.Default;
                            }
                        }

                        dt = ds.Tables[1];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig.SecondaryLanguages = new List<int>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                generalPartnerConfig.SecondaryLanguages.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "LANGUAGE_ID"));
                            }
                        }

                        dt = ds.Tables[2];
                        if (dt.Rows.Count > 0)
                        {
                            generalPartnerConfig.SecondaryCurrencies = new List<int>();

                            foreach (DataRow dr in dt.Rows)
                            {
                                generalPartnerConfig.SecondaryCurrencies.Add(ODBCWrapper.Utils.GetIntSafeVal(dr, "CURRENCY_ID"));
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGeneralPartnerConfig failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<GeneralPartnerConfig, bool>(generalPartnerConfig, generalPartnerConfig != null);
        }

        private RollingDeviceRemovalData GetRollingDeviceRemovalData(DataRow dataRow)
        {
            //get rolling device data
            //default data
            var rollingDeviceRemovalData = new RollingDeviceRemovalData
            {
                RollingDeviceRemovalPolicy = RollingDevicePolicy.NONE,
                RollingDeviceRemovalFamilyIds = new List<int>()
            };

            string rollingDeviceRemovalPolicyIds =
                ODBCWrapper.Utils.GetSafeStr(dataRow, "ROLLING_DEVICE_REMOVAL_FAMILY_IDS");


            if (!rollingDeviceRemovalPolicyIds.IsNullOrEmptyOrWhiteSpace())
            {
                //gets the family ids
                var familyIds = rollingDeviceRemovalPolicyIds.Split(',').Select(int.Parse);

                if (familyIds.Any())
                {
                    rollingDeviceRemovalData.RollingDeviceRemovalFamilyIds.AddRange(familyIds);

                    //gets policy
                    rollingDeviceRemovalData.RollingDeviceRemovalPolicy =
                        (RollingDevicePolicy)ODBCWrapper.Utils.GetNullableInt(dataRow, "ROLLING_DEVICE_REMOVAL_POLICY")
                            .Value;
                }
            }

            return rollingDeviceRemovalData;
        }

        public GenericListResponse<GeneralPartnerConfig> GetGeneralPartnerConfiguration(int groupId)
        {
            GenericListResponse<GeneralPartnerConfig> response = new GenericListResponse<GeneralPartnerConfig>();
            var generalPartnerConfig = GetGeneralPartnerConfig(groupId);
            if (generalPartnerConfig != null)
            {
                response.Objects.Add(generalPartnerConfig);
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }
        
        public Dictionary<string, Currency> GetCurrencyMapByCode3(int groupId)
        {
            Dictionary<string, Currency> currencyMap = new Dictionary<string, Currency>();
            var allCurrencies = this.GetCurrencyList(groupId);
            if (allCurrencies == null)
            {
                log.Warn($"could not get Currency list for groupId:{groupId}");
                return currencyMap;
            }
            foreach (var currency in allCurrencies)
            {
                var lowerCode3 = currency.m_sCurrencyCD3.ToLower();
                if (!currencyMap.ContainsKey(lowerCode3))
                {
                    currencyMap.Add(lowerCode3, currency);
                }
            }
            return currencyMap;
        }
    }
}
