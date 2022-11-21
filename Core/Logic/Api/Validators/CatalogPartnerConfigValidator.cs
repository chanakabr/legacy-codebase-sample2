using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;

namespace ApiLogic.Api.Validators
{
    public class CatalogPartnerConfigValidator : ICatalogPartnerConfigValidator
    {
        private readonly ICategoryCache _categoryCache;
        private readonly IDeviceFamilyRepository _deviceFamilyRepository;
        private readonly ITopicManager _topicManager;

        private static readonly Lazy<CatalogPartnerConfigValidator> Lazy = new Lazy<CatalogPartnerConfigValidator>(
            () => new CatalogPartnerConfigValidator(CategoryCache.Instance, DeviceFamilyRepository.Instance, TopicManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static ICatalogPartnerConfigValidator Instance => Lazy.Value;

        public CatalogPartnerConfigValidator(ICategoryCache categoryCache, IDeviceFamilyRepository deviceFamilyRepository, ITopicManager topicManager)
        {
            _categoryCache = categoryCache;
            _deviceFamilyRepository = deviceFamilyRepository;
            _topicManager = topicManager;
        }

        public Status Validate(long groupId, CatalogPartnerConfig catalogPartnerConfig)
        {
            if (catalogPartnerConfig.CategoryManagement != null)
            {
                var categoryManagementStatus = ValidateCategoryManagement(groupId, catalogPartnerConfig.CategoryManagement);
                if (!categoryManagementStatus.IsOkStatusCode())
                {
                    return categoryManagementStatus;
                }
            }

            if (catalogPartnerConfig.ShopMarkerMetaId.HasValue)
            {
                var shopMarkerMetaIdStatus = ValidateShopMarkerMetaId(groupId, catalogPartnerConfig.ShopMarkerMetaId.Value);
                if (!shopMarkerMetaIdStatus.IsOkStatusCode())
                {
                    return shopMarkerMetaIdStatus;
                }
            }

            return Status.Ok;
        }

        private Status ValidateCategoryManagement(long groupId, CategoryManagement categoryManagement)
        {
            if (categoryManagement.DefaultCategoryTree.HasValue)
            {
                var categoryTreeValidationStatus = ValidateCategoryTree(groupId, categoryManagement.DefaultCategoryTree.Value);
                if (!categoryTreeValidationStatus.IsOkStatusCode())
                {
                    return categoryTreeValidationStatus;
                }
            }

            if (categoryManagement.DeviceFamilyToCategoryTree != null)
            {
                // validate deviceFamilyIds
                var deviceFamilyListResponse = _deviceFamilyRepository.List(groupId);
                if (!deviceFamilyListResponse.IsOkStatusCode())
                {
                    return new Status(eResponseStatus.NonExistingDeviceFamilyIds, $"DeviceFamilyIds {string.Join(", ", categoryManagement.DeviceFamilyToCategoryTree.Keys)} do not exist");
                }

                HashSet<int> deviceFamilies = deviceFamilyListResponse.Objects.Select(x => x.Id).ToHashSet();
                var notExistDeviceFamilies = categoryManagement.DeviceFamilyToCategoryTree.Keys.Where(x => !deviceFamilies.Contains(x)).ToList();
                if (notExistDeviceFamilies.Count > 0)
                {
                    return new Status(eResponseStatus.NonExistingDeviceFamilyIds, $"DeviceFamilyIds {string.Join(", ", notExistDeviceFamilies)} do not exist");
                }

                foreach (var categoryTreeId in categoryManagement.DeviceFamilyToCategoryTree.Values)
                {
                    var categoryTreeValidationStatus = ValidateCategoryTree(groupId, categoryTreeId);
                    if (!categoryTreeValidationStatus.IsOkStatusCode())
                    {
                        return categoryTreeValidationStatus;
                    }
                }
            }

            return Status.Ok;
        }

        private Status ValidateCategoryTree(long groupId, long categoryTreeId)
        {
            var contextDate = new ContextData((int)groupId);
            var filter = new CategoryVersionFilterByTree { TreeId = categoryTreeId };
            var categoryVersionResponse = _categoryCache.ListCategoryVersionByTree(contextDate, filter);

            return categoryVersionResponse.HasObjects()
                ? Status.Ok
                : new Status(eResponseStatus.CategoryTreeDoesNotExist, $"Category tree {categoryTreeId} does not exist");
        }

        private Status ValidateShopMarkerMetaId(long groupId, long shopMarkerMetaId)
        {
            var topicsResponse = _topicManager.GetTopicsByIds((int)groupId, new List<long> { shopMarkerMetaId }, MetaType.All);
            if (!topicsResponse.Status.IsOkStatusCode())
            {
                return topicsResponse.Status;
            }

            var meta = topicsResponse.Objects.FirstOrDefault();
            if (meta == null)
            {
                return new Status(eResponseStatus.MetaNotFound);
            }

            return Status.Ok;
        }
    }
}