using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Repositories;

namespace ApiLogic.Api.Managers
{
    public interface ICatalogPartnerConfigManager
    {
        Status UpdateCatalogConfig(int groupId, CatalogPartnerConfig catalogPartnerConfig);
        GenericResponse<CatalogPartnerConfig> GetCatalogConfig(int groupId);
        long GetCategoryVersionTreeIdByDeviceFamilyId(ContextData contextData, int? deviceFamilyId);
    }

    public class CatalogPartnerConfigManager : ICatalogPartnerConfigManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CatalogPartnerConfigManager> lazy = new Lazy<CatalogPartnerConfigManager>(() =>
            new CatalogPartnerConfigManager(ApiDAL.Instance,
                                            LayeredCache.Instance,
                                            CategoryCache.Instance,
                                            api.Instance,
                                            DeviceFamilyRepository.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static CatalogPartnerConfigManager Instance { get { return lazy.Value; } }

        private readonly ICatalogPartnerRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly ICategoryCache _categoryCache;
        private readonly IDomainDeviceManager _domainDeviceManager;
        private readonly IDeviceFamilyRepository _deviceFamilyRepository;

        public CatalogPartnerConfigManager(ICatalogPartnerRepository repository,
            ILayeredCache layeredCache,
            ICategoryCache categoryCache,
            IDomainDeviceManager domainDeviceManager,
            IDeviceFamilyRepository deviceFamilyRepository)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _categoryCache = categoryCache;
            _domainDeviceManager = domainDeviceManager;
            _deviceFamilyRepository = deviceFamilyRepository;
        }

        public Status UpdateCatalogConfig(int groupId, CatalogPartnerConfig catalogPartnerConfig)
        {
            Status response = new Status(eResponseStatus.Error);

            try
            {
                var needToUpdate = false;
                var oldPlayadapterConfig = GetCatalogConfig(groupId);

                if (oldPlayadapterConfig == null || !oldPlayadapterConfig.HasObject())
                {
                    needToUpdate = true;
                }
                else
                {
                    needToUpdate = catalogPartnerConfig.SetUnchangedProperties(oldPlayadapterConfig.Object);
                }

                if (needToUpdate)
                {
                    if (catalogPartnerConfig.CategoryManagement != null)
                    {
                        var validateStatus = ValidateCategoryManagment(groupId, catalogPartnerConfig.CategoryManagement);
                        if (!validateStatus.IsOkStatusCode())
                        {
                            response.Set(validateStatus);
                            return response;
                        }
                    }

                    if (!_repository.SaveCatalogPartnerConfig(groupId, catalogPartnerConfig))
                    {
                        log.Error($"Error while save PlaybackPartnerConfig. groupId: {groupId}.");
                        return response;
                    }

                    string invalidationKey = LayeredCacheKeys.GetCatalogPartnerConfigInvalidationKey(groupId);
                    if (!_layeredCache.SetInvalidationKey(invalidationKey))
                    {
                        log.Error($"Failed to set invalidation key for CatalogPartnerConfig with invalidationKey: {invalidationKey}.");
                    }
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                log.Error($"An Exception was occurred in UpdateCatalogConfig. groupId:{groupId}.", ex);
            }

            return response;
        }

        public GenericResponse<CatalogPartnerConfig> GetCatalogConfig(int groupId)
        {
            var response = new GenericResponse<CatalogPartnerConfig>();

            try
            {
                CatalogPartnerConfig partnerConfig = null;
                string key = LayeredCacheKeys.GetCatalogPartnerConfigKey(groupId);
                var invalidationKey = new List<string>() { LayeredCacheKeys.GetCatalogPartnerConfigInvalidationKey(groupId) };
                if (!_layeredCache.Get(key,
                                       ref partnerConfig,
                                       GetCatalogPartnerConfigDB,
                                       new Dictionary<string, object>() { { "groupId", groupId } },
                                       groupId,
                                       LayeredCacheConfigNames.GET_CATALOG_PARTNER_CONFIG,
                                       invalidationKey))
                {
                    log.Error($"Failed getting GetCatalogConfig from LayeredCache, groupId: {groupId}, key: {key}");
                }
                else
                {
                    if (partnerConfig == null)
                    {
                        response.SetStatus(eResponseStatus.PartnerConfigurationDoesNotExist, "Catalog partner configuration does not exist.");
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
                log.Error($"Failed GetCatalogConfig for groupId: {groupId}", ex);
            }

            return response;
        }

        public GenericListResponse<CatalogPartnerConfig> GetCatalogConfigList(int groupId)
        {
            var response = new GenericListResponse<CatalogPartnerConfig>();
            var generalPartnerConfig = GetCatalogConfig(groupId);
            if (generalPartnerConfig != null && generalPartnerConfig.HasObject())
            {
                response.Objects.Add(generalPartnerConfig.Object);
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public long GetCategoryVersionTreeIdByDeviceFamilyId(ContextData contextData, int? deviceFamilyId)
        {
            long treeId = 0;
            var catalogConfig = GetCatalogConfig(contextData.GroupId);
            if (!catalogConfig.HasObject() || catalogConfig.Object.CategoryManagement == null)
            {
                return treeId;
            }

            var categoryConfig = catalogConfig.Object.CategoryManagement;
            if (categoryConfig.DeviceFamilyToCategoryTree != null)
            {
                if (!deviceFamilyId.HasValue)
                {
                    deviceFamilyId = _domainDeviceManager.GetDeviceFamilyIdByUdid((int)(contextData.DomainId ?? 0), contextData.GroupId, contextData.Udid);
                }
                
                if (categoryConfig.DeviceFamilyToCategoryTree.ContainsKey(deviceFamilyId.Value))
                {
                    treeId = categoryConfig.DeviceFamilyToCategoryTree[deviceFamilyId.Value];
                    return treeId;
                }
            }
            
            treeId = categoryConfig.DefaultCategoryTree ?? 0;
            return treeId;
        }

        private Status ValidateCategoryManagment(int groupId, CategoryManagement categoryManagement)
        {
            Status validateStatus = Status.Ok;

            var contextDate = new ContextData(groupId);
            if (categoryManagement.DefaultCategoryTree.HasValue)
            {
                var filter = new CategoryVersionFilterByTree() { TreeId = categoryManagement.DefaultCategoryTree.Value };
                var defaultCategory = _categoryCache.ListCategoryVersionByTree(contextDate, filter);
                if (!defaultCategory.HasObjects())
                {
                    validateStatus.Set(eResponseStatus.CategoryTreeDoesNotExist, $"Category tree {categoryManagement.DefaultCategoryTree} does not exist");
                    return validateStatus;
                }
            }

            if (categoryManagement.DeviceFamilyToCategoryTree != null)
            {
                // validate deviceFamilyIds
                var deviceFamilyListResponse = _deviceFamilyRepository.List(groupId);
                if (!deviceFamilyListResponse.IsOkStatusCode())
                {
                    validateStatus.Set(eResponseStatus.NonExistingDeviceFamilyIds,
                        $"DeviceFamilyIds {string.Join(", ", categoryManagement.DeviceFamilyToCategoryTree.Keys)} do not exist");
                    return validateStatus;
                }

                HashSet<int> deviceFamilies = deviceFamilyListResponse.Objects.Select(x => x.Id).ToHashSet();
                var notExistDeviceFamilies = categoryManagement.DeviceFamilyToCategoryTree.Keys.Where(x => !deviceFamilies.Contains(x)).ToList();
                if (notExistDeviceFamilies.Count > 0)
                {
                    validateStatus.Set(eResponseStatus.NonExistingDeviceFamilyIds,
                        $"DeviceFamilyIds {string.Join(", ", notExistDeviceFamilies)} do not exist");
                    return validateStatus;
                }

                foreach (var categoryTree in categoryManagement.DeviceFamilyToCategoryTree.Values)
                {
                    var filter = new CategoryVersionFilterByTree() { TreeId = categoryTree };
                    var defaultCategory = _categoryCache.ListCategoryVersionByTree(contextDate, filter);
                    if (!defaultCategory.HasObjects())
                    {
                        validateStatus.Set(eResponseStatus.CategoryTreeDoesNotExist, $"Category tree {categoryTree} does not exist");
                        return validateStatus;
                    }
                }
            }

            return validateStatus;
        }

        private Tuple<CatalogPartnerConfig, bool> GetCatalogPartnerConfigDB(Dictionary<string, object> funcParams)
        {
            CatalogPartnerConfig partnerConfig = null;
            bool result = false;

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    partnerConfig = _repository.GetCatalogPartnerConfig(groupId.Value);
                    result = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetCatalogPartnerConfigDB failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<CatalogPartnerConfig, bool>(partnerConfig, result);
        }
    }
}
