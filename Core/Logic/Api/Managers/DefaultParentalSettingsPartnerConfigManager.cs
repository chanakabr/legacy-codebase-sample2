using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public interface IDefaultParentalSettingsPartnerConfigManager
    {
        Status UpsertParentalDefaultConfig(int groupId, long userId, DefaultParentalSettingsPartnerConfig catalogPartnerConfig);
        GenericResponse<DefaultParentalSettingsPartnerConfig> GetParentalDefaultConfig(int groupId);
    }

    public class DefaultParentalSettingsPartnerConfigManager : IDefaultParentalSettingsPartnerConfigManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<DefaultParentalSettingsPartnerConfigManager> lazy = new Lazy<DefaultParentalSettingsPartnerConfigManager>(() =>
            new DefaultParentalSettingsPartnerConfigManager(ApiDAL.Instance,
                                                            LayeredCache.Instance,
                                                            api.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static DefaultParentalSettingsPartnerConfigManager Instance { get { return lazy.Value; } }

        private readonly IDefaultParentalSettingsPartnerRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IParentalRuleManager _parentalRuleManager;

        public DefaultParentalSettingsPartnerConfigManager(IDefaultParentalSettingsPartnerRepository repository,
                                           ILayeredCache layeredCache,
                                           IParentalRuleManager parentalRuleManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _parentalRuleManager = parentalRuleManager;
        }

        public Status UpsertParentalDefaultConfig(int groupId, long userId, DefaultParentalSettingsPartnerConfig parentalPartnerConfig)
        {
            Status response = new Status(eResponseStatus.OK);
            var oldParentalConfig = GetParentalDefaultConfig(groupId);
            response.Set(oldParentalConfig.Status);

            if (oldParentalConfig == null || !oldParentalConfig.HasObject() || response.Code == (int)eResponseStatus.PartnerConfigurationDoesNotExist)
            {
                response.Set(eResponseStatus.OK);
                if (!_repository.InsertDefaultParentalSettingsPartnerConfig(groupId, userId, parentalPartnerConfig))
                {
                    var error = $"Error while Inserting DefaultParentalSettingsPartnerConfig. groupId: {groupId}.";
                    log.Error(error);
                    response.Set(eResponseStatus.Error, error);
                }
            }
            else if(response.IsOkStatusCode())
            {
                response = UpdateParentalDefaultConfig(groupId, userId, parentalPartnerConfig, oldParentalConfig);
            }

            return response;
        }

        private Status UpdateParentalDefaultConfig(int groupId, long userId, DefaultParentalSettingsPartnerConfig parentalPartnerConfig, GenericResponse<DefaultParentalSettingsPartnerConfig> oldParentalConfig)
        {
            Status response = Validate(groupId, parentalPartnerConfig);
            if (!response.IsOkStatusCode())
            {
                log.Error($"Error while Updating DefaultParentalSettingsPartnerConfig, parental rules are not validated. groupId: {groupId}.");
            }
            else if (parentalPartnerConfig.SetUnchangedProperties(oldParentalConfig.Object))
            {
                if (!_repository.UpdateDefaultParentalSettingsPartnerConfig(groupId, userId, parentalPartnerConfig))
                {
                    log.Error($"Error while updating DefaultParentalSettingsPartnerConfig. groupId: {groupId}.");
                    response.Set(eResponseStatus.Error);
                }

                string invalidationKey = LayeredCacheKeys.GetDefaultParentalSettingsPartnerConfigInvalidationKey(groupId);
                if (!_layeredCache.SetInvalidationKey(invalidationKey))
                {
                    log.Error($"Failed to set invalidation key for DefaultParentalSettingsPartnerConfig with invalidationKey: {invalidationKey}.");
                }
            }

            return response;
        }

        private Status Validate(int groupId, DefaultParentalSettingsPartnerConfig parentalPartnerConfig)
        {
            Status response = new Status(eResponseStatus.OK);

            if (!parentalPartnerConfig.DefaultMoviesParentalRuleId.HasValue && !parentalPartnerConfig.DefaultTvSeriesParentalRuleId.HasValue)
            {
                return response;
            }

            var parentalRules = _parentalRuleManager.GetParentalRules(groupId);
            if (!parentalRules.status.IsOkStatusCode())
            {
                log.Error($"Error while validating ParentalRules, cant validate parental rules. groupId: {groupId}, error: {parentalRules.status.Message}.");
                return parentalRules.status;
            }

            if (!(ValidateParentalRuleId(groupId, parentalPartnerConfig.DefaultMoviesParentalRuleId, parentalRules.rules) &&
                  ValidateParentalRuleId(groupId, parentalPartnerConfig.DefaultTvSeriesParentalRuleId, parentalRules.rules)))
            {
                response.Set(eResponseStatus.ParentalRuleDoesNotExist);
                return response;
            }

            return response;
        }

        private bool ValidateParentalRuleId(int groupId, long? parentalPartnerConfig, List<ParentalRule> rules)
        {
            if (parentalPartnerConfig.HasValue) 
            {
                // parentalPartnerConfig != 0 is because of ASW service
                // when this api is needed for something else than ASW then need to think of a broad solution
                if (!rules.Any(p => p.id == parentalPartnerConfig.Value) && parentalPartnerConfig != 0)
                {
                    log.Error($"Error while updating DefaultParentalSettingsPartnerConfig, parentalRule {parentalPartnerConfig.Value} doesnt exist. groupId: {groupId}.");
                    return false;
                }
            }
            
            return true;
        }

        public GenericResponse<DefaultParentalSettingsPartnerConfig> GetParentalDefaultConfig(int groupId)
        {
            var response = new GenericResponse<DefaultParentalSettingsPartnerConfig>();
            DefaultParentalSettingsPartnerConfig partnerConfig = null;
            string key = LayeredCacheKeys.GetDefaultParentalSettingsPartnerConfigKey(groupId);
            var invalidationKey = new List<string>() { LayeredCacheKeys.GetDefaultParentalSettingsPartnerConfigInvalidationKey(groupId) };

            if (!_layeredCache.Get(key,
                                    ref partnerConfig,
                                    arg => Tuple.Create(_repository.GetDefaultParentalSettingsPartnerConfig(groupId), true),
                                    new Dictionary<string, object>() { { "groupId", groupId } },
                                    groupId,
                                    LayeredCacheConfigNames.GET_DEFAULT_PARENTAL_SETTINGS_PARTNER_CONFIG,
                                    invalidationKey))
            {
                log.Error($"Failed getting GetParentalDefaultConfig from LayeredCache, groupId: {groupId}, key: {key}");
            }
            else
            {
                if (partnerConfig == null)
                {
                    response.SetStatus(eResponseStatus.PartnerConfigurationDoesNotExist, "DefaultParentalSettings partner configuration does not exist.");
                }
                else
                {
                    response.Object = partnerConfig;
                    response.SetStatus(eResponseStatus.OK);
                }
            }

            return response;
        }

        public GenericListResponse<DefaultParentalSettingsPartnerConfig> GetDefaultParentalSettingsConfigList(int groupId)
        {
            var response = new GenericListResponse<DefaultParentalSettingsPartnerConfig>();
            var parentalPartnerConfig = GetParentalDefaultConfig(groupId);
            if (parentalPartnerConfig != null && parentalPartnerConfig.HasObject())
            {
                response.Objects.Add(parentalPartnerConfig.Object);
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }
    }
}
