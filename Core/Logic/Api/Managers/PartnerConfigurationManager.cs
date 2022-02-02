using ApiLogic.Users.Security;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using CouchbaseManager;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IPartnerConfigurationManager
    {        
    }

    public class PartnerConfigurationManager: IPartnerConfigurationManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PartnerConfigurationManager> lazy = new Lazy<PartnerConfigurationManager>(() => new PartnerConfigurationManager(LayeredCache.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static PartnerConfigurationManager Instance => lazy.Value;

        private readonly ILayeredCache _layeredCache;

        public PartnerConfigurationManager(ILayeredCache layeredCache)
        {
            _layeredCache = layeredCache;
        }

        #region internal methods 
        internal static GenericListResponse<OpcPartnerConfig> GetOpcPartnerConfiguration(int groupId)
        {
            var response = new GenericListResponse<OpcPartnerConfig>();
            var opcPartnerConfig = Instance.GetOpcPartnerConfig(groupId);
            if (opcPartnerConfig != null)
            {
                response.Objects.Add(opcPartnerConfig);
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        internal Status UpdateOpcPartnerConfig(int groupId, OpcPartnerConfig partnerConfigToUpdate)
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
                if (!_layeredCache.SetInvalidationKey(invalidationKey))
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

        internal OpcPartnerConfig GetOpcPartnerConfig(int groupId)
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