using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Tvinci.Core.DAL;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    // work with repo and cache
    public interface ICategoryCache
    {
        //CategoryItem
        GenericResponse<CategoryItem> GetCategoryItem(int groupId, long id);
        Dictionary<long, CategoryParentCache> GetGroupCategoriesIds(int groupId, List<long> ids = null, bool rootOnly = false);
        Dictionary<long, CategoryParentCache> GetGroupCategoriesIdsForVersion(int groupId, long? versionId);
        Dictionary<long, CategoryParentCache> GetGroupCategoriesByIds(int groupId, List<long> ids);
        Dictionary<long, CategoryParentCache> GetGroupRootCategoriesIds(int groupId);
        bool DeleteCategoryItem(int groupId, long userId, long id, bool DBOnly = false);
        List<long> GetCategoryItemSuccessors(int groupId, long id);
        void InvalidateCategoryItem(int groupId, long itemId);
        bool UpdateCategoryOrderNum(int groupId, long? userId, long id, long? versionId, List<long> childCategoriesIds, List<long> childCategoriesIdsToRemove = null);
        void InvalidateGroupCategory(int groupId);
        void InvalidateGroupCategoryForVersion(int groupId, long? versionId);
        void InvalidateGroupRootCategories(int groupId);
        bool UpdateCategory(int groupId, long? userId, List<KeyValuePair<long, string>> namesInOtherLanguages, CategoryItem categoryItemToUpdate);
        List<long> GetCategoriesIdsByChannelId(int groupId, int channelId, UnifiedChannelType channelType);
        long InsertCategory(int groupId, long? userId, string name, List<KeyValuePair<long, string>> namesInOtherLanguages,
            List<UnifiedChannel> channels, Dictionary<string, string> dynamicData, bool? isActive, TimeSlot timeSlot, string type, long? versionId, string referenceId);
        void UpdateCategoryVirtualAssetId(int groupId, long id, long? virtualAssetId, long updateDate, long userId);

        //CategoryVersion
        GenericResponse<CategoryVersion> GetCategoryVersion(int groupId, long id);
        GenericResponse<CategoryVersion> AddCategoryVersion(int groupId, CategoryVersion objectToAdd);
        GenericResponse<CategoryVersion> UpdateCategoryVersion(int groupId, long userId, CategoryVersion objectToUpdate);
        Status DeleteCategoryVersion(int groupId, long userId, CategoryVersion objectToDelete);
        GenericListResponse<CategoryVersion> ListCategoryVersionByTree(ContextData contextData, CategoryVersionFilterByTree filter, CorePager pager = null);
        GenericListResponse<CategoryVersion> ListCategoryVersionDefaults(ContextData contextData, CategoryVersionFilter filter = null, CorePager pager = null);
        bool SetDefault(ContextData contextData, long newDefaultVersionId, long currentDefaultVersionId);
    }

    public class CategoryCache : ICategoryCache
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CategoryCache> lazy = new Lazy<CategoryCache>(() =>
            new CategoryCache(LayeredCache.Instance, CatalogDAL.Instance, CatalogManager.Instance, api.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static CategoryCache Instance { get { return lazy.Value; } }

        private readonly ILayeredCache _layeredCache;
        private readonly ICategoryRepository _repository;
        private readonly ICatalogManager _catalogManager;
        private readonly IVirtualAssetManager _virtualAssetManager;

        public CategoryCache(ILayeredCache layeredCache, ICategoryRepository categoryRepository, ICatalogManager catalogManager, IVirtualAssetManager virtualAssetManager)
        {
            _layeredCache = layeredCache;
            _repository = categoryRepository;
            _catalogManager = catalogManager;
            _virtualAssetManager = virtualAssetManager;
        }

        #region CategoryItem

        public GenericResponse<CategoryItem> GetCategoryItem(int groupId, long id)
        {
            var response = new GenericResponse<CategoryItem>();

            try
            {
                // Get the current category
                CategoryItem result = null;
                string key = LayeredCacheKeys.GetCategoryItemKey(groupId, id);
                string invalidationKey = LayeredCacheKeys.GetCategoryIdInvalidationKey(groupId, id);
                bool cacheResult = _layeredCache.Get<CategoryItem>(key,
                                                        ref result,
                                                        BuildCategoryItem,
                                                        new Dictionary<string, object>()
                                                        {
                                                            { "groupId", groupId },
                                                            { "id", id }
                                                        },
                                                        groupId,
                                                        LayeredCacheConfigNames.GET_CATEGORY_ITEM,
                                                        new List<string>() { invalidationKey });
                if (!cacheResult)
                {
                    log.Error($"Failed getting BuildCategoryItem from LayeredCache, groupId: {groupId}, key: {key}");
                }

                if (result == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, $"Category item id {id} does not exist");
                    return response;
                }

                response.Object = result;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in GetCategoryItem. groupId:{groupId}, id:{id}", ex);
            }

            return response;
        }

        public Dictionary<long, CategoryParentCache> GetGroupCategoriesIds(int groupId, List<long> ids = null, bool rootOnly = false)
        {
            // save mapping between categoryItem and Parentcategory
            Dictionary<long, CategoryParentCache> result = new Dictionary<long, CategoryParentCache>();

            try
            {
                Dictionary<long, CategoryParentCache> groupCategoriesIds = new Dictionary<long, CategoryParentCache>();
                string key = LayeredCacheKeys.GetGroupCategoriesKey(groupId);
                string invalidationKey = LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId);
                if (!_layeredCache.Get<Dictionary<long, CategoryParentCache>>(key,
                                                                            ref groupCategoriesIds,
                                                                            BuildGroupCategories,
                                                                            new Dictionary<string, object>() { { "groupId", groupId } },
                                                                            groupId,
                                                                            LayeredCacheConfigNames.GET_GROUP_CATEGORIES,
                                                                            new List<string>() { invalidationKey }))
                {
                    log.Error($"Failed getting GetGroupCategories from LayeredCache, groupId: {groupId}, key: {key}");
                    return result;
                }

                if (groupCategoriesIds != null)
                {
                    if (ids?.Count > 0)
                    {
                        foreach (long item in ids)
                        {
                            if (groupCategoriesIds.ContainsKey(item))
                            {
                                result.Add(item, groupCategoriesIds[item]);
                            }
                        }
                    }
                    else
                    {
                        foreach (var item in groupCategoriesIds)
                        {
                            result.Add(item.Key, item.Value);
                        }
                    }

                    if (result?.Count > 0 && rootOnly)
                    {
                        result = result.Where(x => x.Value.ParentId == 0).ToDictionary(y => y.Key, z => z.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetGroupCategories, groupId: {groupId}", ex);
            }

            return result;
        }

        public Dictionary<long, CategoryParentCache> GetGroupCategoriesIdsForVersion(int groupId, long? versionId)
        {
            var result = new Dictionary<long, CategoryParentCache>();

            try
            {
                var cacheResult = new Dictionary<long, CategoryParentCache>();
                var key = LayeredCacheKeys.GetGroupCategoriesForVersionKey(groupId, versionId);
                var invalidationKey = LayeredCacheKeys.GetGroupCategoriesForVersionInvalidationKey(groupId, versionId);

                if (!_layeredCache.Get(key,
                        ref cacheResult,
                        BuildGroupCategoriesForVersion,
                        new Dictionary<string, object> {{"groupId", groupId}, {"versionId", versionId}},
                        groupId,
                        LayeredCacheConfigNames.GET_GROUP_CATEGORIES,
                        new List<string> {invalidationKey}))
                {
                    log.Error($"Failed getting GetGroupCategoriesIdsForVersion from LayeredCache, groupId: {groupId}, key: {key}");
                    return result;
                }

                if (cacheResult != null)
                {
                    result = cacheResult;
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed GetGroupCategoriesIdsForVersion, groupId: {groupId}, versionId: {versionId}", e);
            }

            return result;
        }

        public Dictionary<long, CategoryParentCache> GetGroupCategoriesByIds(int groupId, List<long> ids)
        {
            var result = new Dictionary<long, CategoryParentCache>();

            try
            {
                result = _repository.GetCategoriesByIds(groupId, ids);
            }
            catch (Exception e)
            {
                var idsString = ids == null ? "null" : $"[{string.Join(", ", ids)}]";
                log.Error($"Failed {nameof(GetGroupCategoriesByIds)}, groupId: {groupId}, ids: {idsString}", e);
            }

            return result;
        }

        public Dictionary<long, CategoryParentCache> GetGroupRootCategoriesIds(int groupId)
        {
            var result = new Dictionary<long, CategoryParentCache>();

            try
            {
                var cacheResult = new Dictionary<long, CategoryParentCache>();
                var key = LayeredCacheKeys.GetGroupRootCategoriesKey(groupId);
                var invalidationKey = LayeredCacheKeys.GetGroupRootCategoriesInvalidationKey(groupId);

                if (!_layeredCache.Get(key,
                        ref cacheResult,
                        BuildRootGroupCategories,
                        new Dictionary<string, object> {{"groupId", groupId}},
                        groupId,
                        LayeredCacheConfigNames.GET_GROUP_CATEGORIES,
                        new List<string> {invalidationKey}))
                {
                    log.Error($"Failed getting GetGroupRootCategoriesIds from LayeredCache, groupId: {groupId}, key: {key}");
                    return result;
                }

                if (cacheResult != null)
                {
                    result = cacheResult;
                }
            }
            catch (Exception e)
            {
                log.Error($"Failed GetGroupRootCategoriesIds, groupId: {groupId}", e);
            }

            return result;
        }

        public bool DeleteCategoryItem(int groupId, long userId, long id, bool DBOnly = false)
        {
            var response = Status.Error;

            try
            {
                var category = GetCategoryItem(groupId, id);
                if (!_repository.DeleteCategory(groupId, userId, id))
                {
                    log.Error($"Error while DeleteCategory. id: {id}");
                    return false;
                }

                if (DBOnly)
                {
                    return true;
                }

                if (category.HasObject())
                {
                    InvalidateGroupCategoryForVersion(groupId, category.Object.VersionId);
                    if (category.Object.ParentId == 0)
                    {
                        InvalidateGroupRootCategories(groupId);
                    }
                }

                InvalidateCategoryItem(groupId, id);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeleteCategoryItem. groupId:{groupId}, id:{id}", ex);
                return false;
            }

            return true;
        }

        // return all children of item (not including current)
        public List<long> GetCategoryItemSuccessors(int groupId, long id)
        {
            List<long> successors = new List<long>();

            var categoryItem = GetCategoryItem(groupId, id);

            if (!categoryItem.HasObject())
            {
                return successors;
            }

            var categories = GetGroupCategoriesIdsForVersion(groupId, categoryItem.Object.VersionId);
            GetCategoryItemSuccessors(categories, id, successors);

            return successors;
        }

        public void InvalidateCategoryItem(int groupId, long itemId)
        {
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(groupId, itemId));
        }

        public void InvalidateGroupCategory(int groupId)
        {
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId));
        }

        public void InvalidateGroupCategoryForVersion(int groupId, long? versionId)
        {
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesForVersionInvalidationKey(groupId, versionId));
        }

        public void InvalidateGroupRootCategories(int groupId)
        {
            _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupRootCategoriesInvalidationKey(groupId));
        }

        public bool UpdateCategoryOrderNum(int groupId, long? userId, long id, long? versionId, List<long> childCategoriesIds, List<long> childCategoriesIdsToRemove = null)
        {
            if (!_repository.UpdateCategoryOrderNum(groupId, userId, id, versionId, childCategoriesIds, childCategoriesIdsToRemove))
            {
                log.Error($"Error while re-order child categories. groupId: {groupId}. new categoryId: {id}");
                return false;
            }

            InvalidateGroupRootCategories(groupId);

            return true;
        }

        public bool UpdateCategory(int groupId, long? userId, List<KeyValuePair<long, string>> namesInOtherLanguages, CategoryItem categoryItemToUpdate)
        {
            if (!_repository.UpdateCategory(groupId, userId, categoryItemToUpdate.Id, categoryItemToUpdate.Name, namesInOtherLanguages, categoryItemToUpdate.UnifiedChannels,
                                            categoryItemToUpdate.DynamicData, categoryItemToUpdate.IsActive, categoryItemToUpdate.TimeSlot, categoryItemToUpdate.ReferenceId))
            {
                log.Error($"Error while updateCategory. categoryId: {categoryItemToUpdate.Id}.");
                return false;
            }

            InvalidateCategoryItem(groupId, categoryItemToUpdate.Id);
            return true;
        }

        public List<long> GetCategoriesIdsByChannelId(int groupId, int channelId, UnifiedChannelType channelType)
        {
            var categoriesIds = _repository.GetCategoriesIdsByChannelId(groupId, channelId, channelType);
            return categoriesIds;
        }

        public long InsertCategory(int groupId, long? userId, string name, List<KeyValuePair<long, string>> namesInOtherLanguages,
            List<UnifiedChannel> channels, Dictionary<string, string> dynamicData, bool? isActive, TimeSlot timeSlot, string type, long? versionId, string referenceId)
        {
            var id = _repository.InsertCategory(groupId, userId, name, namesInOtherLanguages, channels, dynamicData,
                                                isActive, timeSlot, type, versionId, referenceId);
            return id;
        }

        private void GetCategoryItemSuccessors(Dictionary<long, CategoryParentCache> groupCategories, long id, List<long> successors)
        {
            var childs = groupCategories.Where(x => x.Value.ParentId == id).Select(y => y.Key).ToList();

            foreach (long item in childs)
            {
                successors.Add(item);
                GetCategoryItemSuccessors(groupCategories, item, successors);
            }
        }

        private Tuple<CategoryItem, bool> BuildCategoryItem(Dictionary<string, object> funcParams)
        {
            CategoryItem result = null;
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("id"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? id = funcParams["id"] as long?;
                    if (groupId.HasValue)
                    {
                        result = GetCategory(groupId.Value, id.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"BuildCategoryItem failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<CategoryItem, bool>(result, success);
        }

        private CategoryItem GetCategory(int groupId, long id)
        {
            CategoryItem categoryItem = null;

            try
            {
                var categoryItemDTO = _repository.GetCategoryItemDTO(groupId, id);
                if (categoryItemDTO != null)
                {
                    categoryItem = new CategoryItem()
                    {
                        Id = categoryItemDTO.Id,
                        ParentId = categoryItemDTO.ParentId,
                        Name = categoryItemDTO.Name,
                        UpdateDate = categoryItemDTO.UpdateDate,
                        IsActive = categoryItemDTO.IsActive,
                        Type = categoryItemDTO.Type,
                        VersionId = categoryItemDTO.VersionId,
                        VirtualAssetId = categoryItemDTO.VirtualAssetId,
                        ReferenceId = categoryItemDTO.ReferenceId
                    };

                    // lazy update for virtual asset id
                    if (categoryItem.VirtualAssetId.HasValue && categoryItem.VirtualAssetId.Value == -1)
                    {
                        var virtualAssetInfo = new VirtualAssetInfo()
                        {
                            Type = ObjectVirtualAssetInfoType.Category,
                            Id = categoryItem.Id,
                            withExtendedTypes = true
                        };

                        var virtualAssetResponse = _virtualAssetManager.GetVirtualAsset(groupId, virtualAssetInfo);
                        if (virtualAssetResponse.Status == VirtualAssetInfoStatus.OK)
                        {
                            UpdateCategoryVirtualAssetId(groupId, categoryItem.Id, virtualAssetResponse.AssetId, DateUtils.GetUtcUnixTimestampNow(), 999);
                            categoryItem.VirtualAssetId = virtualAssetResponse.AssetId;
                        }
                        else
                        {
                            UpdateCategoryVirtualAssetId(groupId, categoryItem.Id, null, DateUtils.GetUtcUnixTimestampNow(), 999);
                            categoryItem.VirtualAssetId = null;
                        }
                    }

                    if (categoryItemDTO.HasDynamicData)
                    {
                        categoryItem.DynamicData = _repository.GetCategoryDynamicData(id);
                    }

                    if (categoryItemDTO.StartDate.HasValue || categoryItemDTO.EndDate.HasValue)
                    {
                        categoryItem.TimeSlot = new TimeSlot()
                        {
                            StartDateInSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(categoryItemDTO.StartDate),
                            EndDateInSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(categoryItemDTO.EndDate)
                        };
                    }

                    if (categoryItemDTO.UnifiedChannels != null && categoryItemDTO.UnifiedChannels.Count > 0)
                    {
                        categoryItem.UnifiedChannels = new List<UnifiedChannel>();

                        UnifiedChannelInfo unifiedChannelInfo = null;
                        foreach (var unifiedChannel in categoryItemDTO.UnifiedChannels)
                        {
                            unifiedChannelInfo = new UnifiedChannelInfo()
                            {
                                Id = unifiedChannel.Id,
                                Type = unifiedChannel.Type
                            };

                            if (unifiedChannel.StartDate.HasValue || unifiedChannel.EndDate.HasValue)
                            {
                                unifiedChannelInfo.TimeSlot = new TimeSlot()
                                {
                                    StartDateInSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(unifiedChannel.StartDate),
                                    EndDateInSeconds = DateUtils.DateTimeToUtcUnixTimestampSeconds(unifiedChannel.EndDate)
                                };
                            }

                            categoryItem.UnifiedChannels.Add(unifiedChannelInfo);
                            //TODO ANAT: check if channel exist
                        }
                    }

                    if (categoryItemDTO.NamesInOtherLanguages != null && categoryItemDTO.NamesInOtherLanguages.Count > 0)
                    {
                        categoryItem.NamesInOtherLanguages = new List<LanguageContainer>();

                        CatalogGroupCache catalogGroupCache = null;
                        if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                        {
                            log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling HandleNamesInOtherLanguages");
                            return null;
                        }

                        foreach (var nameInOtherLanguage in categoryItemDTO.NamesInOtherLanguages)
                        {
                            categoryItem.NamesInOtherLanguages.Add(new LanguageContainer()
                            {
                                m_sValue = nameInOtherLanguage.Value,
                                m_sLanguageCode3 = catalogGroupCache.LanguageMapById[nameInOtherLanguage.LanguageId].Code
                            });
                        }
                    }

                    // Set ChildCategoriesIds
                    var groupCategoriesIds = GetGroupCategoriesIdsForVersion(groupId, categoryItem.VersionId);
                    if (groupCategoriesIds != null)
                    {
                        categoryItem.ChildrenIds = groupCategoriesIds.Where(x => x.Value.ParentId == id).OrderBy(y => y.Value.Order).Select(z => z.Key).ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Error while getting GetCategoryItembyDb. group id = {groupId}", ex);
            }

            return categoryItem;
        }

        private Tuple<Dictionary<long, CategoryParentCache>, bool> BuildGroupCategories(Dictionary<string, object> funcParams)
        {
            Dictionary<long, CategoryParentCache> result = new Dictionary<long, CategoryParentCache>();
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    if (groupId.HasValue)
                    {
                        result = _repository.GetCategories(groupId.Value);
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"BuildGroupCategories failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, success);
        }

        private Tuple<Dictionary<long, CategoryParentCache>, bool> BuildGroupCategoriesForVersion(
            Dictionary<string, object> funcParams)
        {
            var result = new Dictionary<long, CategoryParentCache>();
            var success = false;
            try
            {
                if (funcParams == null || !funcParams.ContainsKey("groupId") || !funcParams.ContainsKey("versionId"))
                {
                    return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, false);
                }

                var groupId = funcParams["groupId"] as int?;
                var versionId = funcParams["versionId"] as long?;

                if (groupId.HasValue)
                {
                    result = _repository.GetCategoriesByVersion(groupId.Value, versionId);
                    success = true;
                }
            }
            catch (Exception e)
            {
                log.Error($"BuildGroupCategoriesForVersion failed, parameters : {string.Join(";", funcParams.Keys)}", e);
            }

            return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, success);
        }

        private Tuple<Dictionary<long, CategoryParentCache>, bool> BuildRootGroupCategories(Dictionary<string, object> funcParams)
        {
            var result = new Dictionary<long, CategoryParentCache>();
            var success = false;
            try
            {
                if (funcParams == null || !funcParams.ContainsKey("groupId"))
                {
                    return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, false);
                }

                var groupId = funcParams["groupId"] as int?;

                if (groupId.HasValue)
                {
                    result = _repository.GetRootCategories(groupId.Value);
                    success = true;
                }
            }
            catch (Exception e)
            {
                log.Error($"BuildRootGroupCategories failed, parameters : {string.Join(";", funcParams.Keys)}", e);
            }

            return new Tuple<Dictionary<long, CategoryParentCache>, bool>(result, success);
        }

        public void UpdateCategoryVirtualAssetId(int groupId, long id, long? virtualAssetId, long updateDate, long userId)
        {
            _repository.UpdateCategoryVirtualAssetId(groupId, id, virtualAssetId, updateDate, userId);
        }

        #endregion

        #region CategoryVersion

        public GenericResponse<CategoryVersion> GetCategoryVersion(int groupId, long id)
        {
            var response = new GenericResponse<CategoryVersion>();

            try
            {
                string key = LayeredCacheKeys.GetCategoryVersionKey(groupId, id);
                string invalidationKey = LayeredCacheKeys.GetCategoryVersionInvalidationKey(groupId, id);
                CategoryVersion categoryVersion = null;
                bool cacheResult = _layeredCache.Get<CategoryVersion>(key,
                                                        ref categoryVersion,
                                                        BuildCategoryVersion,
                                                        new Dictionary<string, object>()
                                                        {
                                                            { "groupId", groupId },
                                                            { "id", id }
                                                        },
                                                        groupId,
                                                        LayeredCacheConfigNames.BUILD_CATEGORY_VERSION,
                                                        new List<string>() { invalidationKey });
                if (!cacheResult)
                {
                    log.Error($"Failed BuildCategoryVersion from LayeredCache, groupId: {groupId}, key: {key}");
                    return response;
                }

                if (categoryVersion == null)
                {
                    response.SetStatus(eResponseStatus.CategoryVersionDoesNotExist, $"Category version {id} does not exist");
                    return response;
                }

                response.Object = categoryVersion;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Failed Get CategoryVersion, groupId: {groupId}", ex);
            }

            return response;
        }

        public GenericListResponse<CategoryVersion> ListCategoryVersionDefaults(ContextData contextData, CategoryVersionFilter filter = null, CorePager pager = null)
        {
            var response = new GenericListResponse<CategoryVersion>();

            try
            {
                string key = LayeredCacheKeys.GetCategoryVersionDefaultsKey(contextData.GroupId);
                string invalidationKey = LayeredCacheKeys.GetCategoryVersionDefaultsInvalidationKey(contextData.GroupId);
                List<long> versionsDefaults = null;
                bool cacheResult = _layeredCache.Get<List<long>>(key,
                                                                ref versionsDefaults,
                                                                GetCategoryVersionDefaults,
                                                                new Dictionary<string, object>()
                                                                {
                                                                    { "groupId", contextData.GroupId },
                                                                },
                                                                contextData.GroupId,
                                                                LayeredCacheConfigNames.GET_CATEGORY_VERSION_DEFAULTS,
                                                                new List<string>() { invalidationKey });
                if (!cacheResult)
                {
                    log.Error($"Failed GetCategoryVersionDefaults from LayeredCache, groupId: {contextData.GroupId}, key: {key}");
                    return response;
                }

                if (versionsDefaults != null)
                {
                    foreach (var versionId in versionsDefaults)
                    {
                        var versionResponse = GetCategoryVersion(contextData.GroupId, versionId);
                        if (versionResponse.HasObject())
                        {
                            response.Objects.Add(versionResponse.Object);
                        }
                    }

                    ManageOrderBy(filter, response);
                    ManagePagination(pager, response);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Failed List version defaults, groupId: {contextData.GroupId}", ex);
            }
            return response;
        }

        public GenericListResponse<CategoryVersion> ListCategoryVersionByTree(ContextData contextData, CategoryVersionFilterByTree filter, CorePager pager = null)
        {
            var response = new GenericListResponse<CategoryVersion>();

            try
            {
                string key = LayeredCacheKeys.GetCategoryVersionsOfTreeKey(contextData.GroupId, filter.TreeId);
                string invalidationKey = LayeredCacheKeys.GetCategoryVersionsOfTreeInvalidationKey(contextData.GroupId, filter.TreeId);
                List<long> versionsIds = null;
                bool cacheResult = _layeredCache.Get<List<long>>(key,
                                                                ref versionsIds,
                                                                GetCategoryVersionsOfTree,
                                                                new Dictionary<string, object>()
                                                                {
                                                                    { "groupId", contextData.GroupId },
                                                                    { "treeId", filter.TreeId},
                                                                },
                                                                contextData.GroupId,
                                                                LayeredCacheConfigNames.GET_CATEGORY_VERSIONS_OF_TREE,
                                                                new List<string>() { invalidationKey });
                if (!cacheResult)
                {
                    log.Error($"Failed GetCategoryVersionsOfTree from LayeredCache, groupId: {contextData.GroupId}, key: {key}");
                    return response;
                }

                if (versionsIds != null)
                {
                    foreach (var versionId in versionsIds)
                    {
                        var versionResponse = GetCategoryVersion(contextData.GroupId, versionId);
                        if (versionResponse.HasObject() && (!filter.State.HasValue || filter.State == versionResponse.Object.State))
                        {
                            response.Objects.Add(versionResponse.Object);
                        }
                    }

                    ManageOrderBy(filter, response);
                    ManagePagination(pager, response);
                }

                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"Failed List version by tree, groupId: {contextData.GroupId}, treeId: {filter.TreeId}", ex);
            }

            return response;
        }

        public GenericResponse<CategoryVersion> AddCategoryVersion(int groupId, CategoryVersion objectToAdd)
        {
            var response = new GenericResponse<CategoryVersion>();

            try
            {
                var insertedCategoryVersion = _repository.AddCategoryVersion(groupId, objectToAdd);
                if (insertedCategoryVersion == null)
                {
                    log.Error($"error while AddCategoryVersion");
                }
                else
                {
                    var setVersionStatus = SetVersionToCategoryItems(groupId, insertedCategoryVersion);
                    if (!setVersionStatus.IsOkStatusCode())
                    {
                        log.Error($"error while SetVersion {insertedCategoryVersion.Id} to tree {objectToAdd.TreeId} for category {objectToAdd.CategoryItemRootId}");
                        return response;
                    }

                    InvalidateCategoryVersion(groupId, false, null, objectToAdd.TreeId);
                    response.Object = insertedCategoryVersion;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in AddCategoryVersion. groupId:{groupId}, Name:{objectToAdd.Name}", ex);
            }

            return response;
        }

        public GenericResponse<CategoryVersion> UpdateCategoryVersion(int groupId, long userId, CategoryVersion objectToUpdate)
        {
            var response = new GenericResponse<CategoryVersion>();
            try
            {
                objectToUpdate.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                if (!_repository.UpdateCategoryVersion(groupId, userId, objectToUpdate))
                {
                    log.Error($"error while UpdateCategoryVersion");
                }
                else
                {
                    InvalidateCategoryVersion(groupId, false, objectToUpdate.Id);
                    response.Object = objectToUpdate;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UpdateCategoryVersion. groupId:{groupId}, Name:{objectToUpdate.Name}", ex);
            }

            return response;
        }

        public Status DeleteCategoryVersion(int groupId, long userId, CategoryVersion objectToDelete)
        {
            var response = Status.Error;
            try
            {
                objectToDelete.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                if (!_repository.DeleteCategoryVersion(groupId, userId, objectToDelete))
                {
                    log.Error($"error while DeleteCategoryVersion");
                }
                else
                {
                    InvalidateCategoryVersion(groupId, false, objectToDelete.Id, objectToDelete.TreeId);
                    response.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in DeleteCategoryVersion. groupId:{groupId}, Name:{objectToDelete.Name}", ex);
            }

            return response;
        }

        public bool SetDefault(ContextData contextData, long newDefaultVersionId, long currentDefaultVersionId)
        {
            if (!_repository.UpdateDefaultCategoryVersion
                (contextData.GroupId, contextData.UserId.Value, DateUtils.GetUtcUnixTimestampNow(), newDefaultVersionId, currentDefaultVersionId))
            {
                log.Error($"Error while tring to set {newDefaultVersionId} as default version");
                return false;
            }

            InvalidateCategoryVersion(contextData.GroupId, true, newDefaultVersionId);
            InvalidateCategoryVersion(contextData.GroupId, false, currentDefaultVersionId);

            return true;
        }

        private Tuple<CategoryVersion, bool> BuildCategoryVersion(Dictionary<string, object> funcParams)
        {
            CategoryVersion result = null;
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("id"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? id = funcParams["id"] as long?;
                    result = _repository.GetCategoryVersionById(groupId.Value, id.Value);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"BuildCategoryVersion failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<CategoryVersion, bool>(result, success);
        }

        private Tuple<List<long>, bool> GetCategoryVersionDefaults(Dictionary<string, object> funcParams)
        {
            List<long> result = null;
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    result = _repository.GetCategoryVersionDefaults(groupId.Value);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetCategoryVersionDefaults failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<List<long>, bool>(result, success);
        }

        private Tuple<List<long>, bool> GetCategoryVersionsOfTree(Dictionary<string, object> funcParams)
        {
            List<long> result = null;
            bool success = false;
            try
            {
                if (funcParams != null && funcParams.ContainsKey("groupId") && funcParams.ContainsKey("treeId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    long? treeId = funcParams["treeId"] as long?;
                    result = _repository.GetCategoryVersionsOfTree(groupId.Value, treeId.Value);
                    success = true;
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetCategoryVersionTreeVersions failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
            }

            return new Tuple<List<long>, bool>(result, success);
        }

        private bool InvalidateCategoryVersion(int groupId, bool setDefaults, long? categoryVersionId = null, long? treeId = null)
        {
            bool result = true;
            if (setDefaults)
            {
                var defaultsInvalidationKey = LayeredCacheKeys.GetCategoryVersionDefaultsInvalidationKey(groupId);
                if (!_layeredCache.SetInvalidationKey(defaultsInvalidationKey))
                {
                    log.Error($"Failed to invalidate {defaultsInvalidationKey}");
                    result = false;
                }
            }

            if (categoryVersionId.HasValue)
            {
                var categoryVersionInvalidationKey = LayeredCacheKeys.GetCategoryVersionInvalidationKey(groupId, categoryVersionId.Value);
                if (!_layeredCache.SetInvalidationKey(categoryVersionInvalidationKey))
                {
                    log.Error($"Failed to invalidate {categoryVersionInvalidationKey}");
                    result = false;
                }
            }

            if (treeId.HasValue)
            {
                var treeInvalidationKey = LayeredCacheKeys.GetCategoryVersionsOfTreeInvalidationKey(groupId, treeId.Value);
                if (!_layeredCache.SetInvalidationKey(treeInvalidationKey))
                {
                    log.Error($"Failed to invalidate {treeInvalidationKey}");
                    result = false;
                }
            }

            return result;
        }

        private Status SetVersionToCategoryItems(int groupId, CategoryVersion categoryVersion)
        {
            Status response = new Status();

            try
            {
                // get children
                var successors = GetCategoryItemSuccessors(groupId, categoryVersion.CategoryItemRootId);
                successors.Add(categoryVersion.CategoryItemRootId);
                if (!_repository.UpdateCategoriesVersionId(categoryVersion.Id, successors, categoryVersion.UpdateDate, categoryVersion.UpdaterId))
                {
                    log.Error($"error while SetVersionToCategoryItems {categoryVersion.Id} for category {categoryVersion.CategoryItemRootId}");
                    return response;
                }

                _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId));
                foreach (var item in successors)
                {
                    _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(groupId, item));
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in SetVersionToCategoryItems. groupId:{groupId}, id:{categoryVersion.Id}, CategoryRootId: {categoryVersion.CategoryItemRootId}", ex);
            }

            return response;
        }

        private void ManageOrderBy(CategoryVersionFilter filter, GenericListResponse<CategoryVersion> response)
        {
            if (filter != null)
            {
                if (filter.OrderBy != null)
                {
                    if (filter.OrderBy.Property == OrderProperty.UpdateDate)
                    {
                        if (filter.OrderBy.Direction == ApiObjects.SearchObjects.OrderDir.DESC)
                        {
                            response.Objects = response.Objects.OrderByDescending(x => x.UpdateDate).ToList();
                        }
                        else
                        {
                            response.Objects = response.Objects.OrderBy(x => x.UpdateDate).ToList();
                        }
                    }
                }
            }
        }

        private void ManagePagination(CorePager pager, GenericListResponse<CategoryVersion> response)
        {
            if (pager?.PageSize > 0)
            {
                response.TotalItems = response.Objects.Count;
                response.Objects = response.Objects.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
            }
        }

        #endregion
    }
}
