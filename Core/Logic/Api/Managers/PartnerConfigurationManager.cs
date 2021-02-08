using ApiLogic.Users.Security;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using CouchbaseManager;
using DAL;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using TVinciShared;

namespace ApiLogic.Api.Managers
{
    public class PartnerConfigurationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region internal methods 
        internal static GenericListResponse<OpcPartnerConfig> GetOpcPartnerConfiguration(int groupId)
        {
            var response = new GenericListResponse<OpcPartnerConfig>();
            var opcPartnerConfig = GetOpcPartnerConfig(groupId);
            if (opcPartnerConfig != null)
            {
                response.Objects.Add(opcPartnerConfig);
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        internal static GenericListResponse<GeneralPartnerConfig> GetGeneralPartnerConfiguration(int groupId)
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

        internal static Status UpdateOpcPartnerConfig(int groupId, OpcPartnerConfig partnerConfigToUpdate)
        {
            Status response = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                if (!ApiDAL.UpdateOpcPartnerResetPassword(groupId, partnerConfigToUpdate.ResetPassword))
                {
                    log.Error($"Error while update OpcPartnerResetPassword [ResetPassword]. groupId: {groupId}");
                    return response;
                }

                var invalidationKey = LayeredCacheKeys.GetOpcPartnerConfigInvalidationKey(groupId);
                if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                {
                    log.ErrorFormat("Failed to set invalidation key for opcPartnerConfig with invalidationKey: {0}", invalidationKey);
                }

                response.Set((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UpdateOpcPartnerConfig failed ex={0}, groupId={1}", ex, groupId);
            }
            return response;
        }

        internal static Status UpdateGeneralPartnerConfig(int groupId, GeneralPartnerConfig partnerConfigToUpdate)
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
                    shouldInvalidateRegions = (Core.Catalog.CatalogManagement.CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache)
                                               && defaultRegion.id != catalogGroupCache.DefaultRegion);
                }

                if (partnerConfigToUpdate?.RollingDeviceRemovalData?.RollingDeviceRemovalFamilyIds?.Count > 0)
                {
                    var rollingDeviceRemovalFamilyIds = partnerConfigToUpdate.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds;

                    partnerConfigToUpdate.RollingDeviceRemovalData.RollingDeviceRemovalFamilyIds =
                        rollingDeviceRemovalFamilyIds.Distinct().ToList();

                    // validate deviceFamilyIds
                    var deviceFamilyList = Core.Api.Module.GetDeviceFamilyList(groupId);
                    List<DeviceFamily> deviceFamilies = deviceFamilyList.DeviceFamilies;
                    if (deviceFamilyList.Status.Code != (int)eResponseStatus.OK || deviceFamilies.Count == 0)
                    {
                        response.Message = "No DeviceFamilies";
                        return response;
                    }

                    var existingFamilies = rollingDeviceRemovalFamilyIds.Intersect(deviceFamilies.Select(x => x.Id));
                    var notDeviceFamilies = rollingDeviceRemovalFamilyIds.Except(existingFamilies).ToList();

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

                // upsert GeneralPartnerConfig            
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

        internal static List<LanguageObj> GetAllLanguages(int groupId)
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

        internal static List<Currency> GetCurrencyList(int groupId)
        {
            List<Currency> currencies = null;
            try
            {
                int defaultGroupCurrencyId = 0;
                if (LayeredCache.Instance.Get<int>(LayeredCacheKeys.GetGroupDefaultCurrencyKey(groupId), ref defaultGroupCurrencyId, GetGroupDefaultCurrency, new Dictionary<string, object>() { { "groupId", groupId } }, groupId, LayeredCacheConfigNames.GET_DEFAULT_GROUP_CURRENCY_LAYERED_CACHE_CONFIG_NAME) && defaultGroupCurrencyId > 0)
                {
                    DataTable dt = null;
                    if (LayeredCache.Instance.Get<DataTable>(LayeredCacheKeys.GET_CURRENCIES_KEY, ref dt, GetAllCurrencies, new Dictionary<string, object>(), groupId,
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
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed GetCurrencyList, groupId: {0}, ex: {1}", groupId, ex);
            }

            return currencies;
        }

        internal static bool GetGroupDefaultCurrency(int groupId, ref string currencyCode)
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

        internal static bool IsValidCurrencyCode(int groupId, string currencyCode3)
        {
            bool res = false;
            if (string.IsNullOrEmpty(currencyCode3))
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
                           where ((string)row["CODE3"]).ToUpper() == currencyCode3.ToUpper()
                           select row).Count() > 0;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed IsValidCurrencyCode, groupId: {0}, currencyCode: {1}", groupId, currencyCode3), ex);
            }

            return res;
        }

        public static GenericListResponse<CommercePartnerConfig> GetCommercePartnerConfigList(int groupId)
        {
            var response = new GenericListResponse<CommercePartnerConfig>();
            var commercePartnerConfig = GetCommercePartnerConfig(groupId);

            if (commercePartnerConfig.HasObject())
            {
                response.Objects.Add(commercePartnerConfig.Object);
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        internal static GenericResponse<CommercePartnerConfig> GetCommercePartnerConfig(int groupId)
        {
            var response = new GenericResponse<CommercePartnerConfig>();

            try
            {
                CommercePartnerConfig commercePartnerConfig = null;
                string key = LayeredCacheKeys.GetCommercePartnerConfigKey(groupId);
                var invalidationKey = new List<string>() { LayeredCacheKeys.GetCommercePartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get(key,
                                               ref commercePartnerConfig,
                                               GetCommercePartnerConfigDB,
                                               new Dictionary<string, object>() { { "groupId", groupId } },
                                               groupId,
                                               LayeredCacheConfigNames.GET_COMMERCE_PARTNER_CONFIG,
                                               invalidationKey))
                {
                    log.Error($"Failed getting GetCommercePartnerConfig from LayeredCache, groupId: {groupId}, key: {key}");
                }
                else
                {
                    if (commercePartnerConfig == null)
                    {
                        response.SetStatus(eResponseStatus.PartnerConfigurationDoesNotExist, "Commerce partner configuration does not exist.");
                    }
                    else
                    {
                        response.Object = commercePartnerConfig;
                        response.SetStatus(eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetCommercePartnerConfig for groupId: {groupId}", ex);
            }

            return response;
        }

        public static Status UpdateCommerceConfig(int groupId, CommercePartnerConfig commercePartnerConfig)
        {
            Status response = new Status(eResponseStatus.Error);

            try
            {
                var needToUpdate = false;
                var oldCommerceConfig = GetCommercePartnerConfig(groupId);

                if (!oldCommerceConfig.HasObject())
                {
                    needToUpdate = true;
                }
                else
                {
                    needToUpdate = commercePartnerConfig.SetUnchangedProperties(oldCommerceConfig.Object);
                }

                if (needToUpdate)
                {
                    if (!ApiDAL.SaveCommercePartnerConfig(groupId, commercePartnerConfig))
                    {
                        log.Error($"Error while save CommercePartnerConfig. groupId: {groupId}.");
                        return response;
                    }

                    string invalidationKey = LayeredCacheKeys.GetCommercePartnerConfigInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.Error($"Failed to set invalidation key for CommercePartnerConfig with invalidationKey: {invalidationKey}.");
                    }
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.Error($"An Exception was occurred in UpdateCommerceConfig. groupId:{groupId}.", ex);
            }

            return response;
        }

        public static Status UpdatePlaybackConfig(int groupId, PlaybackPartnerConfig playbackPartnerConfig)
        {
            Status response = new Status(eResponseStatus.Error);

            try
            {
                var needToUpdate = false;
                var oldPlayadapterConfig = GetPlaybackConfig(groupId);

                if (oldPlayadapterConfig == null || !oldPlayadapterConfig.HasObject())
                {
                    needToUpdate = true;
                }
                else
                {
                    needToUpdate = playbackPartnerConfig.SetUnchangedProperties(oldPlayadapterConfig.Object);
                }

                if (needToUpdate)
                {
                    response = ValidateDefaultAdapters(groupId, playbackPartnerConfig.DefaultAdapters);
                    if (!response.IsOkStatusCode())
                    {
                        return response;
                    }

                    if (!ApiDAL.SavePlaybackPartnerConfig(groupId, playbackPartnerConfig))
                    {
                        log.Error($"Error while save PlaybackPartnerConfig. groupId: {groupId}.");
                        return response;
                    }

                    string invalidationKey = LayeredCacheKeys.GetPlaybackPartnerConfigInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.Error($"Failed to set invalidation key for PlaybackPartnerConfig with invalidationKey: {invalidationKey}.");
                    }
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.Error($"An Exception was occurred in UpdatePlaybackConfig. groupId:{groupId}.", ex);
            }

            return response;
        }

        public static GenericResponse<PlaybackPartnerConfig> GetPlaybackConfig(int groupId)
        {
            var response = new GenericResponse<PlaybackPartnerConfig>();

            try
            {
                PlaybackPartnerConfig partnerConfig = null;
                string key = LayeredCacheKeys.GetPlaybackPartnerConfigKey(groupId);
                var invalidationKey = new List<string>() { LayeredCacheKeys.GetPlaybackPartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get(key,
                                               ref partnerConfig,
                                               GetPlaybackPartnerConfigDB,
                                               new Dictionary<string, object>() { { "groupId", groupId } },
                                               groupId,
                                               LayeredCacheConfigNames.GET_PLAYBACK_PARTNER_CONFIG,
                                               invalidationKey))
                {
                    log.Error($"Failed getting GetPlaybackConfig from LayeredCache, groupId: {groupId}, key: {key}");
                }
                else
                {
                    if (partnerConfig == null || partnerConfig.DefaultAdapters == null)
                    {
                        response.SetStatus(eResponseStatus.PartnerConfigurationDoesNotExist, "Playback partner configuration does not exist.");
                    }
                    else
                    {
                        response.Object = partnerConfig;
                        response.SetStatus(eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetPlaybackConfig for groupId: {groupId}", ex);
            }

            return response;
        }

        public static GenericListResponse<PlaybackPartnerConfig> GetPlaybackConfigList(int groupId)
        {
            var response = new GenericListResponse<PlaybackPartnerConfig>();
            var partnerConfig = GetPlaybackConfig(groupId);

            if (partnerConfig.HasObject())
            {
                response.Objects.Add(partnerConfig.Object);
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        #region Payment

        public static GenericResponse<PaymentPartnerConfig> GetPaymentConfig(int groupId)
        {
            var response = new GenericResponse<PaymentPartnerConfig>();

            try
            {
                PaymentPartnerConfig partnerConfig = null;
                string key = LayeredCacheKeys.GetPaymentPartnerConfigKey(groupId);
                var invalidationKey = new List<string>() { LayeredCacheKeys.GetPaymentPartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get(key,
                                               ref partnerConfig,
                                               GetPaymentPartnerConfigDB,
                                               new Dictionary<string, object>() { { "groupId", groupId } },
                                               groupId,
                                               LayeredCacheConfigNames.GET_PAYMENT_PARTNER_CONFIG,
                                               invalidationKey))
                {
                    log.Error($"Failed getting GetPaymentConfig from LayeredCache, groupId: {groupId}, key: {key}");
                }
                else
                {
                    if (partnerConfig == null)
                    {
                        response.SetStatus(eResponseStatus.PartnerConfigurationDoesNotExist, "Payment partner configuration does not exist.");
                    }
                    else
                    {
                        response.Object = partnerConfig;
                        response.SetStatus(eResponseStatus.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetPaymentConfig for groupId: {groupId}", ex);
            }

            return response;
        }

        public static GenericListResponse<PaymentPartnerConfig> GetPaymentConfigList(int groupId)
        {
            var response = new GenericListResponse<PaymentPartnerConfig>();
            var partnerConfig = GetPaymentConfig(groupId);

            if (partnerConfig.HasObject())
            {
                response.Objects.Add(partnerConfig.Object);
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public static Status UpdatePaymentConfig(int groupId, PaymentPartnerConfig paymentPartnerConfig)
        {
            Status response = new Status(eResponseStatus.Error);

            try
            {
                var needToUpdate = false;
                var oldPaymentConfig = GetPaymentConfig(groupId);

                if (oldPaymentConfig == null || !oldPaymentConfig.HasObject())
                {
                    needToUpdate = true;
                }
                else
                {
                    needToUpdate = paymentPartnerConfig.SetUnchangedProperties(oldPaymentConfig.Object);
                }

                if (needToUpdate)
                {
                    response = ValidatePaymentPartnerConfigForUpdate(groupId, paymentPartnerConfig);
                    if (!response.IsOkStatusCode())
                    {
                        return response;
                    }

                    if (!ApiDAL.SavePaymentPartnerConfig(groupId, paymentPartnerConfig))
                    {
                        log.Error($"Error while save PaymentPartnerConfig. groupId: {groupId}.");
                        return response;
                    }

                    string invalidationKey = LayeredCacheKeys.GetPaymentPartnerConfigInvalidationKey(groupId);
                    if (!LayeredCache.Instance.SetInvalidationKey(invalidationKey))
                    {
                        log.Error($"Failed to set invalidation key for PaymentPartnerConfig with invalidationKey: {invalidationKey}.");
                    }
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.Error($"An Exception was occurred in UpdatePaymentConfig. groupId:{groupId}.", ex);
            }

            return response;
        }

        #endregion

        #endregion

        #region private methods

        internal static GeneralPartnerConfig GetGeneralPartnerConfig(int groupId)
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

        internal static OpcPartnerConfig GetOpcPartnerConfig(int groupId)
        {
            OpcPartnerConfig opcPartnerConfig = null;

            try
            {
                var key = LayeredCacheKeys.GetOpcPartnerConfig(groupId);
                var configInvalidationKey = new List<string>() { LayeredCacheKeys.GetOpcPartnerConfigInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<OpcPartnerConfig>(key,
                                                          ref opcPartnerConfig,
                                                          GetOpcPartnerConfigDB,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_OPC_PARTNER_CONFIG,
                                                          configInvalidationKey))
                {
                    log.ErrorFormat("Failed getting OpcPartnerConfig from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGeneralPartnerConfig for groupId: {0}", groupId), ex);
            }

            return opcPartnerConfig;
        }


        private static Tuple<OpcPartnerConfig, bool> GetOpcPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            var generalPartnerConfig = new OpcPartnerConfig();
            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    generalPartnerConfig.ResetPassword = ApiDAL.GetResetPasswordPartnerConfig(groupId.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetOpcPartnerConfig failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<OpcPartnerConfig, bool>(generalPartnerConfig, generalPartnerConfig != null);
        }

        private static Tuple<GeneralPartnerConfig, bool> GetGeneralPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            GeneralPartnerConfig generalPartnerConfig = null;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    DataSet ds = ApiDAL.GetGeneralPartnerConfig(groupId.Value);
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
                                FinishedPercentThreshold = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "FINISHED_PERCENT_THRESHOLD"),
                            };

                            if (!generalPartnerConfig.FinishedPercentThreshold.HasValue || generalPartnerConfig.FinishedPercentThreshold.Value == 0)
                            {
                                generalPartnerConfig.FinishedPercentThreshold = CatalogLogic.FINISHED_PERCENT_THRESHOLD;
                            }

                            int? deleteMediaPolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DELETE_MEDIA_POLICY");
                            int? downgradePolicy = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DOWNGRADE_POLICY");
                            int? defaultRegion = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "DEFAULT_REGION");
                            int? enableRegionFiltering = ODBCWrapper.Utils.GetNullableInt(dt.Rows[0], "IS_REGIONALIZATION_ENABLED");

                            if (deleteMediaPolicy.HasValue)
                            {
                                generalPartnerConfig.DeleteMediaPolicy = (DeleteMediaPolicy)deleteMediaPolicy.Value;
                            }

                            if (downgradePolicy.HasValue)
                            {
                                generalPartnerConfig.DowngradePolicy = (DowngradePolicy)downgradePolicy.Value;
                            }

                            if (enableRegionFiltering.HasValue)
                            {
                                generalPartnerConfig.EnableRegionFiltering = enableRegionFiltering.Value == 1;
                            }

                            if (defaultRegion.HasValue && defaultRegion.Value > 0)
                            {
                                generalPartnerConfig.DefaultRegion = defaultRegion.Value;
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

        private static RollingDeviceRemovalData GetRollingDeviceRemovalData(DataRow dataRow)
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

        private static bool IsValidLanguageId(int groupId, int languageId)
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

        private static bool IsValidCurrencyId(int groupId, int currencyId)
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

        private static Tuple<DataTable, bool> GetAllCurrencies(Dictionary<string, object> funcParams)
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

        private static Tuple<int, bool> GetGroupDefaultCurrency(Dictionary<string, object> funcParams)
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

        private static Tuple<CommercePartnerConfig, bool> GetCommercePartnerConfigDB(Dictionary<string, object> funcParams)
        {
            CommercePartnerConfig commercePartnerConfig = null;
            bool result = false;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    commercePartnerConfig = ApiDAL.GetCommercePartnerConfig(groupId.Value, out eResultStatus resultStatus);
                    result = true;

                    if (resultStatus == eResultStatus.KEY_NOT_EXIST)
                    {
                        // save null document
                        if (!ApiDAL.SaveCommercePartnerConfig(groupId.Value, new CommercePartnerConfig()))
                        {
                            log.Error($"failed to save CommercePartnerConfig null document for groupId {groupId.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCommercePartnerConfigDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<CommercePartnerConfig, bool>(commercePartnerConfig, result);
        }

        private static Tuple<PlaybackPartnerConfig, bool> GetPlaybackPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            PlaybackPartnerConfig partnerConfig = null;
            bool result = false;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    partnerConfig = ApiDAL.GetPlaybackPartnerConfig(groupId.Value, out eResultStatus resultStatus);
                    result = true;

                    if (resultStatus == eResultStatus.KEY_NOT_EXIST)
                    {
                        // save null document
                        if (!ApiDAL.SavePlaybackPartnerConfig(groupId.Value, new PlaybackPartnerConfig()))
                        {
                            log.Error($"failed to save PlaybackPartner null document for groupId {groupId.Value}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetPlaybackPartnerConfigDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<PlaybackPartnerConfig, bool>(partnerConfig, result);
        }

        private static Status ValidateDefaultAdapters(int groupId, DefaultPlaybackAdapters defaultAdapters)
        {
            if (defaultAdapters != null)
            {
                GenericListResponse<PlaybackProfile> playbackProfile = null;
                if (defaultAdapters.EpgAdapterId > 0)
                {
                    playbackProfile = api.GetPlaybackProfile(groupId, defaultAdapters.EpgAdapterId, false);
                    if (playbackProfile == null || playbackProfile.HasObjects() == false)
                    {
                        log.Debug($"ValidateDefaultAdapters EpgAdapterId {defaultAdapters.EpgAdapterId} not exist");
                        return new Status(eResponseStatus.AdapterNotExists, $"Adapter ID = {defaultAdapters.EpgAdapterId} does not exist");
                    }
                }

                if (defaultAdapters.MediaAdapterId > 0)
                {
                    playbackProfile = api.GetPlaybackProfile(groupId, defaultAdapters.MediaAdapterId, false);
                    if (playbackProfile == null || playbackProfile.HasObjects() == false)
                    {
                        log.Debug($"ValidateDefaultAdapters MediaAdapterId {defaultAdapters.MediaAdapterId} not exist");
                        return new Status(eResponseStatus.AdapterNotExists, $"Adapter ID = {defaultAdapters.MediaAdapterId} does not exist");
                    }
                }

                if (defaultAdapters.RecordingAdapterId > 0)
                {
                    playbackProfile = api.GetPlaybackProfile(groupId, defaultAdapters.RecordingAdapterId, false);
                    if (playbackProfile == null || playbackProfile.HasObjects() == false)
                    {
                        log.Debug($"ValidateDefaultAdapters RecordingAdapterId {defaultAdapters.RecordingAdapterId} not exist");
                        return new Status(eResponseStatus.AdapterNotExists, $"Adapter ID = {defaultAdapters.RecordingAdapterId} does not exist");
                    }
                }
            }

            return new Status(eResponseStatus.OK);
        }

        private static Tuple<PaymentPartnerConfig, bool> GetPaymentPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            PaymentPartnerConfig partnerConfig = null;
            bool result = false;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    partnerConfig = ApiDAL.GetPaymentPartnerConfig(groupId.Value);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetPaymentPartnerConfigDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<PaymentPartnerConfig, bool>(partnerConfig, result);
        }

        private static Status ValidatePaymentPartnerConfigForUpdate(int groupId, PaymentPartnerConfig paymentPartnerConfig)
        {
            var response = Status.Ok;

            if (paymentPartnerConfig.UnifiedBillingCycles != null)
            {
                foreach (var unifiedBillingCycle in paymentPartnerConfig.UnifiedBillingCycles)
                {
                    if (unifiedBillingCycle.PaymentGatewayId.HasValue)
                    {
                        var paymentGatewayResponse = Core.Billing.Module.GetPaymentGatewayById(groupId, unifiedBillingCycle.PaymentGatewayId.Value);
                        if (!paymentGatewayResponse.HasObject())
                        {
                            response.Set(paymentGatewayResponse.Status);
                            return response;
                        }
                    }
                }
            }

            return response;
        }

        #endregion

        #region Security Config
        public static Status UpdateSecurityConfig(int groupId, SecurityPartnerConfig partnerConfig, long userId)
        {
            var currentConfig = GetSecurityConfig(groupId);
            var alreadyExist = currentConfig.Encryption?.Username != null;
            if (alreadyExist) return new Status(eResponseStatus.NotAllowed, "Can't alter account's security parameters");

            // save randomly-generated encryption key
            var keyStorage = UserEncryptionKeyStorage.Instance();
            var encryptionType = partnerConfig.Encryption.Username.EncryptionType;
            var newKey = new EncryptionKey(-1, groupId, keyStorage.GenerateRandomEncryptionKey(encryptionType), encryptionType);
            if (!keyStorage.AddEncryptionKey(newKey, userId)) return new Status(eResponseStatus.Error, "Encryption key is not added");

            // save security config

            // we need to be sure, that all machines start applying Encryption in the same time (or at least with small diff).
            // but we save security configuration to the cache, because of performance. this cache will be invalidated after X sec.
            // it means that all machines will have the same config after 2*X sec for sure,
            // that's why we have 2*X delay in applying and using this config            
            partnerConfig.Encryption.Username.ApplyAfter = DateTime.UtcNow.AddSeconds(2 * GetSecurityConfigInMemoryTTL());
            if (!ApiDAL.SaveSecurityPartnerConfig(groupId, partnerConfig)) return new Status(eResponseStatus.Error, "SecurityPartnerConfig not saved");

            var invalidationKey = LayeredCacheKeys.GetSecurityPartnerConfigInvalidationKey(groupId);
            LayeredCache.Instance.SetInvalidationKey(invalidationKey);

            return new Status(eResponseStatus.OK);
        }

        public static SecurityPartnerConfig GetSecurityConfig(int groupId)
        {
            var partnerConfig = ApiDAL.GetSecurityPartnerConfig(groupId, out var resultStatus);
            if (resultStatus == eResultStatus.KEY_NOT_EXIST)
            {
                var emptyConfig = new SecurityPartnerConfig();
                ApiDAL.SaveSecurityPartnerConfig(groupId, emptyConfig);
                return emptyConfig;
            }
            return partnerConfig;
        }

        public static GenericListResponse<SecurityPartnerConfig> GetSecurityConfigList(int groupId)
        {
            var response = new GenericListResponse<SecurityPartnerConfig>();
            response.Objects.Add(GetSecurityConfig(groupId));
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        // This method is used in performance critical places.
        // If we use this method to return response in PartnerConfigurationController,
        // we'll have different responses in different machines because of delay in cache invalidation => bad user experience
        internal static GenericResponse<SecurityPartnerConfig> GetSecurityPartnerConfigFromCache(int groupId)
        {
            SecurityPartnerConfig partnerConfig = null;
            var key = LayeredCacheKeys.GetSecurityPartnerConfigKey(groupId);
            var invalidationKey = new List<string>() { LayeredCacheKeys.GetSecurityPartnerConfigInvalidationKey(groupId) };

            var success = LayeredCache.Instance.Get(
                key,
                ref partnerConfig,
                GetSecurityConfig,
                new Dictionary<string, object>() { { "groupId", groupId } },
                groupId,
                LayeredCacheConfigNames.GET_SECURITY_PARTNER_CONFIG,
                invalidationKey);

            return success
                ? new GenericResponse<SecurityPartnerConfig>(Status.Ok, partnerConfig)
                : new GenericResponse<SecurityPartnerConfig>(Status.Error);
        }

        private static Tuple<SecurityPartnerConfig, bool> GetSecurityConfig(Dictionary<string, object> funcParams)
        {
            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue) return Tuple.Create(GetSecurityConfig(groupId.Value), true);
            }
            catch (Exception ex)
            {
                log.Error($"GetSecurityPartnerConfigDB failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return Tuple.Create<SecurityPartnerConfig, bool>(null, false);
        }

        private static uint GetSecurityConfigInMemoryTTL()
        {
            LayeredCache.Instance.TryGetInvalidationKeyLayeredCacheConfig(LayeredCacheConfigNames.GET_SECURITY_PARTNER_CONFIG, out var configs);
            return configs?.FirstOrDefault(_ => _.Type == LayeredCacheType.InMemoryCache)?.TTL ?? 0;
        }
        
        #endregion Security Config
    }
}