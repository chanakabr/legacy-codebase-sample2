using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Api;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    // work with objects\categoeyitem - validate and logic  
    public interface ICategoryItemManager
    {
        GenericResponse<CategoryTree> Duplicate(int groupId, long userId, long id, string name = null);
        Status Delete(ContextData contextData, long id);
    }

    public class CategoryItemHandler : ICategoryItemManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CategoryItemHandler> lazy = new Lazy<CategoryItemHandler>(() => 
            new CategoryItemHandler(VirtualAssetPartnerConfigManager.Instance, 
                                    api.Instance, 
                                    ImageManager.Instance,
                                    CatalogManager.Instance,
                                    ChannelManager.Instance,
                                    ExternalChannelManager.Instance,
                                    CategoryCache.Instance,
                                    CatalogPartnerConfigManager.Instance), 
            LazyThreadSafetyMode.PublicationOnly);

        public static CategoryItemHandler Instance { get { return lazy.Value; } }

        private readonly IVirtualAssetPartnerConfigManager _virtualAssetPartnerManager;
        private readonly IVirtualAssetManager _virtualAssetManager;
        private readonly IImageManager _imageManager;
        private readonly ICatalogManager _catalogManager;
        private readonly IChannelManager _channelManager;
        private readonly IExternalChannelManager _externalChannelManager;
        private readonly ICategoryCache _categoryCache;
        private readonly ICatalogPartnerConfigManager _catalogPartnerConfig;

        public CategoryItemHandler(IVirtualAssetPartnerConfigManager virtualAssetPartnerManager,
                                   IVirtualAssetManager virtualAssetManager, 
                                   IImageManager imageManager, 
                                   ICatalogManager catalogManager, 
                                   IChannelManager channelManager, 
                                   IExternalChannelManager externalChannelManager,
                                   ICategoryCache categoryCache,
                                   ICatalogPartnerConfigManager catalogPartnerConfig)
        {
            _virtualAssetPartnerManager = virtualAssetPartnerManager;
            _virtualAssetManager = virtualAssetManager;
            _imageManager = imageManager;
            _catalogManager = catalogManager;
            _channelManager = channelManager;
            _externalChannelManager = externalChannelManager;
            _categoryCache = categoryCache;
            _catalogPartnerConfig = catalogPartnerConfig;
        }

        #region Public methods

        public GenericResponse<CategoryItem> Add(ContextData contextData, CategoryItem objectToAdd)
        {
            var response = new GenericResponse<CategoryItem>();

            try
            {
                if (string.IsNullOrEmpty(objectToAdd.Name))
                {
                    response.SetStatus(eResponseStatus.NameRequired, "Name Required");
                    return response;
                }

                if (!string.IsNullOrEmpty(objectToAdd.Type))
                {
                    // check that type exist.
                    var assetStructId = _virtualAssetPartnerManager.GetAssetStructByObjectVirtualAssetInfo(contextData.GroupId, ObjectVirtualAssetInfoType.Category, objectToAdd.Type);
                    if (assetStructId == 0)
                    {
                        response.SetStatus(eResponseStatus.CategoryTypeNotExist, $"Category type '{objectToAdd.Type}' does not exist.");
                        return response;
                    }
                }

                if (objectToAdd.ChildrenIds?.Count > 0)
                {
                    var groupCategories = _categoryCache.GetGroupCategoriesIds(contextData.GroupId, objectToAdd.ChildrenIds);

                    if (groupCategories == null || groupCategories.Count != objectToAdd.ChildrenIds.Count)
                    {
                        response.SetStatus(eResponseStatus.ChildCategoryNotExist, "Child category does not exist.");
                        return response;
                    }

                    foreach (long childCategoryId in objectToAdd.ChildrenIds)
                    {
                        if (groupCategories[childCategoryId].ParentId > 0)
                        {
                            response.SetStatus(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory, $"Child Category contains other parent. id = {childCategoryId}");
                            return response;
                        }

                        if (groupCategories[childCategoryId].VersionId.HasValue)
                        {
                            response.SetStatus(eResponseStatus.CategoryIsAlreadyAssociatedToVersion,
                                $"Child Category {childCategoryId} is already associated to version {groupCategories[childCategoryId].VersionId}.");
                            return response;
                        }
                    }
                }

                if (objectToAdd.UnifiedChannels?.Count > 0)
                {
                    if (!IsUnifiedChannelsValid(contextData.GroupId, contextData.UserId ?? 0, objectToAdd.UnifiedChannels))
                    {
                        response.SetStatus(eResponseStatus.ChannelDoesNotExist, "Channel does not exist");
                        return response;
                    }
                }

                if (!objectToAdd.IsActive.HasValue)
                {
                    objectToAdd.IsActive = true;
                }

                if (!AddCategoryItem(contextData.GroupId, contextData.UserId.Value, objectToAdd))
                {
                    response.SetStatus(eResponseStatus.Error, "Failed to add item");
                    return response;
                }

                response.Object = objectToAdd;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem add. contextData:{contextData.ToString()}, Name:{objectToAdd.Name}", ex);
            }

            return response;
        }

        public GenericResponse<CategoryTree> GetTreeByVersion(ContextData contextData, long? versionId, int? deviceFamilyId)
        {
            var response = new GenericResponse<CategoryTree>();
            try
            {
                CategoryVersion categoryVersion = null;
                if (!versionId.HasValue)
                {
                    // get default version 
                    long treeId = _catalogPartnerConfig.GetCategoryVersionTreeIdByDeviceFamilyId(contextData, deviceFamilyId);
                    var defaultList = _categoryCache.ListCategoryVersionDefaults(contextData);
                    if (defaultList.HasObjects())
                    {
                        categoryVersion = defaultList.Objects.FirstOrDefault(x => x.TreeId == treeId);
                    }
                }
                else
                {
                    var categoryVersionResponse = _categoryCache.GetCategoryVersion(contextData.GroupId, versionId.Value);
                    if (categoryVersionResponse.HasObject())
                    {
                        categoryVersion = categoryVersionResponse.Object;
                    }
                }

                if (categoryVersion == null)
                {
                    response.SetStatus(eResponseStatus.CategoryVersionDoesNotExist, $"Category version {versionId} does not exist");
                    return response;
                }

                response = GetCategoryTree(contextData.GroupId, categoryVersion.CategoryItemRootId);

            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in GetTreeByVersion. contextData:{contextData}, versionId:{versionId ?? 0}", ex);
            }

            return response;
        }

        public GenericResponse<CategoryItem> Update(ContextData contextData, CategoryItem objectToUpdate)
        {
            var response = new GenericResponse<CategoryItem>();
            VirtualAssetInfo virtualAssetInfo = null;

            try
            {
                // Get the current category
                var currentCategory = GetCategoryItem(contextData.GroupId, objectToUpdate.Id);
                if (currentCategory == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                // validate delete if have version
                if (currentCategory.VersionId.HasValue)
                {
                    var versionResponse = _categoryCache.GetCategoryVersion(contextData.GroupId, currentCategory.VersionId.Value);
                    if (versionResponse.HasObject() && versionResponse.Object.State != CategoryVersionState.Draft)
                    {
                        response.SetStatus(eResponseStatus.CategoryVersionIsNotDraft, $"Cannot update categoryItem in categoryVersion state {versionResponse.Object.State}");
                        return response;
                    }
                }

                // if name change need to update virtualAsset                               
                if (objectToUpdate.Name != null && objectToUpdate.Name.Trim() == "")
                {
                    response.SetStatus(eResponseStatus.NameRequired, "Name Required");
                    return response;
                }
                else if (objectToUpdate.Name != currentCategory.Name)
                {
                    virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Category,
                        Id = currentCategory.Id,
                        Name = objectToUpdate.Name,
                        UserId = contextData.UserId.Value,
                        withExtendedTypes = true,
                        DuplicateAssetId = currentCategory.VirtualAssetId,
                        IsActive = currentCategory.IsActive
                    };
                }

                bool updateChildCategories = false;
                List<long> categoriesToRemove = new List<long>();
                var status = HandleCategoryChildUpdate(contextData.GroupId, objectToUpdate.Id, objectToUpdate.ChildrenIds, currentCategory.ChildrenIds, 
                                                       currentCategory.VersionId, ref categoriesToRemove, out updateChildCategories);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }

                if (objectToUpdate.UnifiedChannels == null)
                {
                    if (currentCategory.UnifiedChannels != null)
                    {
                        objectToUpdate.UnifiedChannels = currentCategory.UnifiedChannels;
                    }
                }
                else
                {
                    if (objectToUpdate.UnifiedChannels?.Count > 0)
                    {
                        if (!IsUnifiedChannelsValid(contextData.GroupId, contextData.UserId ?? 0, objectToUpdate.UnifiedChannels))
                        {
                            response.SetStatus(eResponseStatus.ChannelDoesNotExist, "Channel does not exist");
                            return response;
                        }

                        objectToUpdate.UnifiedChannels = HandleUnifiedChannelsTimeSlotToUpdate(objectToUpdate.UnifiedChannels, currentCategory.UnifiedChannels);
                    }
                }

                if (objectToUpdate.DynamicData == null)
                {
                    objectToUpdate.DynamicData = currentCategory.DynamicData;
                }

                objectToUpdate.TimeSlot = HandleTimeSlotToUpdate(objectToUpdate.TimeSlot, currentCategory.TimeSlot);

                //set NamesInOtherLanguages
                List<KeyValuePair<long, string>> languageCodeToName = null;
                if (objectToUpdate.NamesInOtherLanguages == null)
                {
                    if (currentCategory.NamesInOtherLanguages != null)
                    {
                        objectToUpdate.NamesInOtherLanguages = currentCategory.NamesInOtherLanguages;
                    }
                }
                else
                {
                    languageCodeToName = new List<KeyValuePair<long, string>>();
                    if (objectToUpdate.NamesInOtherLanguages?.Count > 0)
                    {
                        status = HandleNamesInOtherLanguages(contextData.GroupId, objectToUpdate.NamesInOtherLanguages, out languageCodeToName);
                        if (!status.IsOkStatusCode())
                        {
                            response.SetStatus(status);
                            return response;
                        }
                    }
                }

                // Due to atomic action update virtual asset before category update
                if (virtualAssetInfo != null)
                {
                    var virtualAssetInfoResponse = _virtualAssetManager.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                    if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                    {
                        log.Error($"Error while update category's virtualAsset. groupId: {contextData.GroupId}, CategoryId: {currentCategory.Id}, CategoryName: {currentCategory.Name} ");
                        if (virtualAssetInfoResponse.ResponseStatus != null)
                        {
                            response.SetStatus(virtualAssetInfoResponse.ResponseStatus);
                        }
                        else
                        {
                            response.SetStatus(eResponseStatus.Error, "Error while updating category.");
                        }
                        
                        return response;
                    }
                }

                if (!_categoryCache.UpdateCategory(contextData.GroupId, contextData.UserId, languageCodeToName, objectToUpdate))
                {
                    return response;
                }

                // set child category's order
                if (categoriesToRemove?.Count > 0 || updateChildCategories)
                {
                    if (!_categoryCache.UpdateCategoryOrderNum(contextData.GroupId, contextData.UserId, objectToUpdate.Id, currentCategory.VersionId,
                                                               objectToUpdate.ChildrenIds, categoriesToRemove))
                    {
                        response.SetStatus(eResponseStatus.Error);
                        return response;
                    }

                    _categoryCache.InvalidateGroupCategory(contextData.GroupId);
                    foreach (var item in categoriesToRemove)
                    {
                        _categoryCache.InvalidateCategoryItem(contextData.GroupId, item);
                    }

                    if (objectToUpdate.ChildrenIds?.Count > 0)
                    {
                        foreach (var item in objectToUpdate.ChildrenIds)
                        {
                            _categoryCache.InvalidateCategoryItem(contextData.GroupId, item);
                        }
                    }
                }

                response.Object = GetCategoryItem(contextData.GroupId, objectToUpdate.Id);
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem update. contextData:{contextData.ToString()}, Name:{objectToUpdate.Name}", ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status response = new Status();

            // Check if category exist
            var item = GetCategoryItem(contextData.GroupId, id);
            if (item == null)
            {
                response.Set(eResponseStatus.CategoryNotExist, "Category does not exist");
                return response;
            }

            // validate delete if have version
            if (item.VersionId.HasValue)
            {
                var versionResponse = _categoryCache.GetCategoryVersion(contextData.GroupId, item.VersionId.Value);
                if (versionResponse.HasObject())
                {
                    if (versionResponse.Object.State != CategoryVersionState.Draft)
                    {
                        response.Set(eResponseStatus.CategoryVersionIsNotDraft, $"Cannot delete categoryItem in category version state {versionResponse.Object.State}");
                        return response;
                    }

                    if (!item.ParentId.HasValue || item.ParentId.Value == 0)
                    {
                        response.Set(eResponseStatus.CategoryItemIsRoot, $"Cannot delete categoryItem root of category version {versionResponse.Object.Id}");
                        return response;
                    }
                }
            }

            if (!DeleteCategoryItem(contextData.GroupId, contextData.UserId.Value, item.Id))
            {
                response.Set(eResponseStatus.Error, $"Failed to delete categoryItem {item.Id}");
                return response;
            }

            // need to check if category is a root category. In case it is, all the sub categories should removed as well
            var successors = _categoryCache.GetCategoryItemSuccessors(contextData.GroupId, item.Id);
            foreach (long successor in successors)
            {
                DeleteCategoryItem(contextData.GroupId, contextData.UserId.Value, successor);
            }

            if (item.ParentId > 0)
            {
                _categoryCache.InvalidateCategoryItem(contextData.GroupId, item.ParentId.Value);
            }

            response.Set(eResponseStatus.OK);

            return response;
        }

        public GenericResponse<CategoryItem> Get(ContextData contextData, long id)
        {
            var response = new GenericResponse<CategoryItem>();
            var categoryItem = GetCategoryItem(contextData.GroupId, id);
            if (categoryItem != null)
            {
                response.Object = categoryItem;
                response.SetStatus(eResponseStatus.OK);
            }
            else
            {
                response.SetStatus(eResponseStatus.CategoryNotExist);
            }

            return response;
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemAncestorsFilter filter, CorePager pager)
        {
            GenericListResponse<CategoryItem> response = new GenericListResponse<CategoryItem>();

            if (pager.PageIndex != 0)
            {
                response.Status.Set(eResponseStatus.InvalidValue, "Page index value must be 1.");
                return response;
            }

            CategoryItem ci = GetCategoryItem(contextData.GroupId, filter.Id);

            if (ci == null)
            {
                response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                return response;
            }

            var ancestors = GetCategoryItemAncestors(contextData.GroupId, filter.Id);
            if (ancestors?.Count > 0)
            {
                response.Objects = ancestors.Select(x => GetCategoryItem(contextData.GroupId, x)).ToList();
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemByIdInFilter filter, CorePager pager)
        {
            GenericListResponse<CategoryItem> response = new GenericListResponse<CategoryItem>();

            List<long> categoriesIds = null;
            CategoryItem categoryItem = null;

            if (filter?.IdIn?.Count > 0)
            {
                if (pager.PageIndex != 0)
                {
                    response.Status.Set(eResponseStatus.InvalidValue, "Page index value must be 1.");
                    return response;
                }

                if (pager.PageSize < filter.IdIn.Count)
                {
                    response.Status.Set(eResponseStatus.InvalidValue, "Page size must to be greater or equal to the size of CategoryItemIds");
                    return response;
                }

                categoriesIds = filter.IdIn;
                foreach (var categoryId in categoriesIds)
                {
                    categoryItem = GetCategoryItem(contextData.GroupId, categoryId);
                    if (categoryItem != null)
                    {
                        response.Objects.Add(categoryItem);
                    }
                }

                response.TotalItems = response.Objects.Count;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemSearchFilter filter, CorePager pager)
        {
            GenericListResponse<CategoryItem> response = new GenericListResponse<CategoryItem>();

            List<long> categoriesIds = new List<long>();
            long assetStructId = 0;

            if (!string.IsNullOrEmpty(filter.TypeEqual))
            {
                assetStructId = _virtualAssetPartnerManager.GetAssetStructByObjectVirtualAssetInfo(contextData.GroupId, ObjectVirtualAssetInfoType.Category, filter.TypeEqual);
                if (assetStructId == 0)
                {
                    response.SetStatus(eResponseStatus.CategoryTypeNotExist, $"Category type '{filter.TypeEqual}' does not exist.");
                    return response;
                }
            }

            if (filter.RootOnly)
            {
                var groupCategories = _categoryCache.GetGroupCategoriesIds(contextData.GroupId, null, true);
                if (groupCategories?.Count > 0)
                {
                    categoriesIds = groupCategories.Keys.ToList();
                }
                else
                {
                    response.SetStatus(eResponseStatus.OK);
                    return response;
                }
            }

            AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition()
            {
                Filter = filter.Ksql,
                UserId = contextData.UserId.Value,
                IsAllowedToViewInactiveAssets = true,
                NoSegmentsFilter = true,
                AssetStructId = assetStructId
            };

            var categories = new HashSet<long>(categoriesIds);
            ObjectVirtualAssetFilter result;
            // if order by update date need. Get all categories            
            if (filter.IsOrderByUpdateDate)
            {
                result = _virtualAssetManager.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Category,
                                                           categories, 0, 0, filter.OrderBy);
            }
            else
            {
                result = _virtualAssetManager.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Category,
                                                           categories, pager.PageIndex, pager.PageSize, filter.OrderBy);
            }

            if (result.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
            {
                response.SetStatus(result.Status);
                return response;
            }

            if (result.ObjectIds?.Count > 0)
            {
                response.Objects = result.ObjectIds.Select(x => GetCategoryItem(contextData.GroupId, x)).Where(x => x != null).ToList();
                
                if (filter.IsOrderByUpdateDate)
                {
                    if (filter.OrderBy.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.ASC)
                    {
                        response.Objects = pager.PageSize > 0 ? response.Objects.OrderBy(x => x.UpdateDate).Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
                    }
                    else
                    {
                        response.Objects = pager.PageSize > 0 ? response.Objects.OrderByDescending(x => x.UpdateDate).Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList() : response.Objects;
                    }
                }

                response.TotalItems = result.TotalItems;
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericResponse<CategoryTree> Duplicate(int groupId, long userId, long id, string name = null)
        {
            var response = new GenericResponse<CategoryTree>();

            try
            {
                CategoryItem root = GetCategoryItem(groupId, id);

                if (root == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                CategoryItem copiedRoot = (CategoryItem)root.Clone();
                if (!string.IsNullOrEmpty(name))
                {
                    copiedRoot.Name = name;
                }
                
                Dictionary<long, long> newTreeMap = new Dictionary<long, long>();

                bool result = DuplicateChildren(groupId, userId, copiedRoot, newTreeMap);

                if (!result)
                {
                    foreach (var newId in newTreeMap.Values)
                    {
                        DeleteCategoryItem(groupId, userId, newId);
                    }

                    return response;
                }

                // in case the duplicate category have parent, it should bw updated with his new child :)
                if (root.ParentId.HasValue && root.ParentId.Value > 0)
                {
                    // Get parent
                    CategoryItem parent = GetCategoryItem(groupId, root.ParentId.Value);

                    if (parent == null)
                    {
                        response.SetStatus(eResponseStatus.CategoryNotExist, "Parent Category does not exist");
                        return response;
                    }

                    // update parent with new child
                    parent.ChildrenIds.Add(newTreeMap[id]);
                    if (!_categoryCache.UpdateCategoryOrderNum(groupId, userId, root.ParentId.Value, null, parent.ChildrenIds, null))
                    {
                        response.SetStatus(eResponseStatus.Error);
                        return response;
                    }
                    _categoryCache.InvalidateCategoryItem(groupId, root.ParentId.Value);
                }

                response = GetCategoryTree(groupId, newTreeMap[id]);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem duplicate.  groupId: {groupId} id: {id}.", ex);
            }

            return response;
        }

        public GenericResponse<CategoryTree> GetCategoryTree(int groupId, long id, bool filter = false, bool onlyActive = false)
        {
            GenericResponse<CategoryTree> response = new GenericResponse<CategoryTree>();

            CategoryTree categoryTree = null;

            // Get the current category
            var categoryItem = GetCategoryItem(groupId, id, filter, onlyActive);
            if (categoryItem == null)
            {
                response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                return response;
            }

            if (categoryItem.ParentId > 0)
            {
                log.DebugFormat("Tree for not root item");
            }

            categoryTree = BuildCategoryTree(groupId, categoryItem);

            if (categoryItem?.ChildrenIds?.Count > 0)
            {
                List<CategoryItem> childern = categoryItem.ChildrenIds.Select(x => GetCategoryItem(groupId, x, filter, onlyActive)).ToList();

                childern.RemoveAll(item => item == null);

                if (childern.Any(i => i != null))
                {
                    categoryTree.Children = FindTreeChildren(groupId, childern, filter, onlyActive);
                }
            }

            response.Object = categoryTree;
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public Status RemoveChannelFromCategories(int groupId, int channelId, UnifiedChannelType unifiedChannelType, long userId)
        {
            Status status = new Status();
            
            var categoriesIds = _categoryCache.GetCategoriesIdsByChannelId(groupId, channelId, unifiedChannelType);
            if (categoriesIds == null)
            {
                log.Error($"failed to get GetCategoriesIdsByChannelId for groupId: {groupId} when calling RemoveChannelFromCategories");
                status.Set(eResponseStatus.Error);
                return status;
            }

            if (categoriesIds.Count > 0)
            {
                foreach (var categoryId in categoriesIds)
                {
                    //Check if category exist
                    var category = GetCategoryItem(groupId, categoryId);
                    if (category == null)
                    {
                        status.Set(eResponseStatus.CategoryNotExist, $"Category does not exist {categoryId} ");
                        return status;
                    }

                    if (category.UnifiedChannels != null && category.UnifiedChannels.Count > 0)
                    {
                        category.UnifiedChannels.Remove((category.UnifiedChannels.Where(x => x.Id == channelId && x.Type == unifiedChannelType).First()));

                        var languageCodeToName = new List<KeyValuePair<long, string>>();
                        if (category.NamesInOtherLanguages?.Count > 0)
                        {
                            status = HandleNamesInOtherLanguages(groupId, category.NamesInOtherLanguages, out languageCodeToName);
                            if (!status.IsOkStatusCode())
                            {
                                return status;
                            }
                        }

                        if (!_categoryCache.UpdateCategory(groupId, userId, languageCodeToName, category))
                        {
                            return status;
                        }
                        
                        status.Set(eResponseStatus.OK);
                    }
                }
            }

            return status;
        }

        #endregion

        #region Private methods

        private bool DuplicateChildren(int groupId, long userId, CategoryItem parent, Dictionary<long, long> newTreeMap)
        {
            bool result = false;
            List<long> children = new List<long>();
            if (parent.ChildrenIds?.Count > 0)
            {
                CategoryItem ci;
                foreach (var item in parent.ChildrenIds)
                {
                    ci = GetCategoryItem(groupId, item);
                    result = DuplicateChildren(groupId, userId, ci, newTreeMap);

                    if (!result)
                    {
                        return result;
                    }

                    if (newTreeMap.ContainsKey(item))
                    {
                        children.Add(newTreeMap[item]);
                    }
                }
            }

            CategoryItem newICategory = new CategoryItem()
            {
                ChildrenIds = children,
                DynamicData = parent.DynamicData,
                Name = parent.Name,
                UnifiedChannels = parent.UnifiedChannels,
                IsActive = parent.IsActive,
                TimeSlot = parent.TimeSlot,
                Type = parent.Type,
                VersionId = parent.VersionId,
                ReferenceId = parent.ReferenceId,
                NamesInOtherLanguages = parent.NamesInOtherLanguages
            };

            if (AddCategoryItem(groupId, userId, newICategory, parent.VirtualAssetId))
            {
                newTreeMap.Add(parent.Id, newICategory.Id);
                DuplicateCategoryImages(groupId, userId, parent.Id, newICategory.Id);
                result = true;
            }

            return result;
        }

        private List<CategoryTree> FindTreeChildren(int groupId, List<CategoryItem> children, bool filter, bool onlyActive)
        {
            List<CategoryTree> response = new List<CategoryTree>();
            CategoryTree ct;
            children.RemoveAll(item => item == null);

            foreach (var c in children)
            {
                ct = BuildCategoryTree(groupId, c);
                if (c.ChildrenIds != null)
                {
                    var ch = c.ChildrenIds.Select(x => GetCategoryItem(groupId, x, filter, onlyActive)).ToList();
                    ch.RemoveAll(item => item == null);
                    if (ch.Any(i => i != null))
                    {
                        ct.Children = FindTreeChildren(groupId, ch, filter, onlyActive);
                    }
                }
                
                response.Add(ct);
            }

            return response;
        }

        private CategoryTree BuildCategoryTree(int groupId, CategoryItem categoryItem)
        {
            CategoryTree categoryTree = new CategoryTree(categoryItem);

            //1. channels
            if (categoryItem.UnifiedChannels?.Count > 0)
            {
                categoryTree.UnifiedChannels = new List<UnifiedChannelInfo>();
                var uci = GetUnifiedChannelsInfo(groupId, 0, categoryItem.UnifiedChannels);

                foreach (var item in categoryItem.UnifiedChannels)
                {
                    if (uci.ContainsKey($"{item.Id}_{item.Type}"))
                    {
                        categoryTree.UnifiedChannels.Add(uci[$"{item.Id}_{item.Type}"]);
                    }
                }
            }

            //2. images
            var images = _imageManager.GetImagesByObject(groupId, categoryItem.Id, eAssetImageType.Category);
            if (images.HasObjects())
            {
                categoryTree.Images = images.Objects;
            }

            return categoryTree;
        }

        private bool IsUnifiedChannelsValid(int groupId, long userId, List<UnifiedChannel> unifiedChannels)
        {
            var unifiedChannelInfo = GetUnifiedChannelsInfo(groupId, userId, unifiedChannels);
            return unifiedChannelInfo != null && unifiedChannels.Count == unifiedChannelInfo.Count;
        }

        private Dictionary<string, UnifiedChannelInfo> GetUnifiedChannelsInfo(int groupId, long userId, List<UnifiedChannel> unifiedChannels)
        {
            Dictionary<string, UnifiedChannelInfo> channelsInfo = new Dictionary<string, UnifiedChannelInfo>();
            var contextData = new ContextData(groupId) { UserId = userId };

            foreach (var unifiedChannel in unifiedChannels)
            {
                var unifiedChannelInfo = unifiedChannel as UnifiedChannelInfo;
                UnifiedChannelInfo uci = new UnifiedChannelInfo() { Id = unifiedChannel.Id };

                //check external channel exist
                if (unifiedChannel.Type == UnifiedChannelType.External)
                {
                    var ec = _externalChannelManager.GetChannelById(contextData, (int)unifiedChannel.Id, true);
                    if (ec != null && ec.IsOkStatusCode() && ec.Object != null)
                    {
                        uci.Type = unifiedChannel.Type;
                        uci.Name = ec.Object.Name;

                        if (unifiedChannelInfo != null && unifiedChannelInfo.TimeSlot != null)
                        {
                            uci.TimeSlot = unifiedChannelInfo.TimeSlot;
                        }

                        channelsInfo.Add($"{unifiedChannel.Id}_{UnifiedChannelType.External}", uci);
                    }
                }
                else
                {
                    //check internal channel exist
                    var channel = _channelManager.GetChannelById(contextData, (int)unifiedChannel.Id, true);
                    if (channel.IsOkStatusCode() && channel.Object != null)
                    {
                        uci.Type = UnifiedChannelType.Internal;
                        uci.Name = channel.Object.m_sName;

                        if (unifiedChannelInfo != null && unifiedChannelInfo.TimeSlot != null)
                        {
                            uci.TimeSlot = unifiedChannelInfo.TimeSlot;
                        }

                        channelsInfo.Add($"{unifiedChannel.Id}_{UnifiedChannelType.Internal}", uci);
                    }
                }
            }

            return channelsInfo; ;
        }

        private void DuplicateCategoryImages(int groupId, long userId, long categoryFromId, long categoryToId)
        {
            var images = _imageManager.GetImagesByObject(groupId, categoryFromId, eAssetImageType.Category);
            if (images.HasObjects())
            {
                HashSet<long> imageTypeIds = new HashSet<long>();

                foreach (var image in images.Objects)
                {
                    if (imageTypeIds.Contains(image.ImageTypeId))
                    {
                        continue;
                    }

                    Image newImage = new Image()
                    {
                        ImageTypeId = image.ImageTypeId,
                        ImageObjectId = categoryToId,
                        ImageObjectType = image.ImageObjectType
                    };

                    var newImageresponse = _imageManager.AddImage(groupId, newImage, userId);
                    if (newImageresponse.HasObject())
                    {
                        imageTypeIds.Add(image.ImageTypeId);

                        string imageOriginalUrl = $"{image.Url}/width/0/height/0";

                        var status = _imageManager.SetContent(groupId, userId, newImageresponse.Object.Id, imageOriginalUrl);
                        if (!status.IsOkStatusCode())
                        {
                            log.Error($"Failed to set image for category id:{categoryToId}, url:{imageOriginalUrl}");
                        }
                    }
                }
            }
        }

        private TimeSlot HandleTimeSlotToUpdate(TimeSlot timeSlotToUpdate, TimeSlot currentTimeSlot)
        {
            if (timeSlotToUpdate == null)
            {
                timeSlotToUpdate = currentTimeSlot;
            }
            else
            {
                if (currentTimeSlot != null)
                {
                    if (!timeSlotToUpdate.StartDateInSeconds.HasValue && currentTimeSlot.StartDateInSeconds.HasValue)
                    {
                        timeSlotToUpdate.StartDateInSeconds = currentTimeSlot.StartDateInSeconds;
                    }

                    if (!timeSlotToUpdate.EndDateInSeconds.HasValue && currentTimeSlot.EndDateInSeconds.HasValue)
                    {
                        timeSlotToUpdate.EndDateInSeconds = currentTimeSlot.EndDateInSeconds;
                    }
                }
            }

            return timeSlotToUpdate;
        }

        private List<UnifiedChannel> HandleUnifiedChannelsTimeSlotToUpdate(List<UnifiedChannel> unifiedChannelsToUpdate, List<UnifiedChannel> currentUnifiedChannels)
        {
            if (unifiedChannelsToUpdate != null && currentUnifiedChannels != null && currentUnifiedChannels.Count > 0)
            {
                UnifiedChannelInfo channelInfoToUpdate = null;
                UnifiedChannelInfo currentUnifiedChannelInfo = null;
                foreach (var unifiedChannelToUpdate in unifiedChannelsToUpdate)
                {
                    channelInfoToUpdate = unifiedChannelToUpdate as UnifiedChannelInfo;
                    if (channelInfoToUpdate != null)
                    {
                        var channel = currentUnifiedChannels.Where(x => x.Id == channelInfoToUpdate.Id && x.Type == channelInfoToUpdate.Type).FirstOrDefault();
                        if (channel != null)
                        {
                            currentUnifiedChannelInfo = channel as UnifiedChannelInfo;
                            if (currentUnifiedChannelInfo != null)
                            {
                                channelInfoToUpdate.TimeSlot = HandleTimeSlotToUpdate(channelInfoToUpdate.TimeSlot, currentUnifiedChannelInfo.TimeSlot);
                            }
                        }
                    }
                }
            }

            return unifiedChannelsToUpdate;
        }

        private bool IsCategoryExist(int groupId, long id)
        {
            var categories = _categoryCache.GetGroupCategoriesIds(groupId);
            if (categories == null || !categories.ContainsKey(id))
            {
                return false;
            }

            return true;
        }

        private CategoryItem GetCategoryItem(int groupId, long id, bool filter = false, bool onlyActive = false)
        {
            CategoryItem result = null;
            var categoryItemResponse = _categoryCache.GetCategoryItem(groupId, id);
            if (categoryItemResponse.HasObject())
            {
                result = categoryItemResponse.Object;
                if (onlyActive && !result.IsActive.Value)
                    return null;

                if (filter && result.TimeSlot != null && !result.TimeSlot.IsValid())
                    return null;
            }

            return result;
        }

        private List<long> GetCategoryItemAncestors(int groupId, long id)
        {
            List<long> ancestors = new List<long>();

            var categories = _categoryCache.GetGroupCategoriesIds(groupId);
            if (categories?.Count > 0 && categories.ContainsKey(id))
            {
                long parentId = categories[id].ParentId;
                while (parentId > 0)
                {
                    if (categories.ContainsKey(parentId))
                    {
                        if (ancestors.Contains(parentId))
                        {
                            ancestors.Clear();
                            break;
                        }

                        ancestors.Add(parentId);
                        parentId = categories[parentId].ParentId;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            return ancestors;
        }

        private Status HandleCategoryChildUpdate(int groupId, long id, List<long> newChildCategoriesIds, List<long> oldChildCategoriesIds, 
                                                 long? categoryVersionId, ref List<long> categoriesToRemove, out bool updateChildCategories)
        {
            updateChildCategories = false;
            categoriesToRemove = new List<long>();

            if (newChildCategoriesIds == null)
            {
                return new Status(eResponseStatus.OK);
            }

            if (newChildCategoriesIds.Count == 0)
            {
                categoriesToRemove = oldChildCategoriesIds;
                return new Status(eResponseStatus.OK);
            }
            else
            {
                if (newChildCategoriesIds.Contains(id))
                {
                    return new Status(eResponseStatus.ChildCategoryCannotBeTheCategoryItself, "A child category cannot be the category itself.");
                }

                //validate ChildCategoriesIds
                var groupChildCategories = _categoryCache.GetGroupCategoriesIds(groupId, newChildCategoriesIds);

                if (groupChildCategories == null || groupChildCategories.Count != newChildCategoriesIds.Count)
                {
                    return new Status(eResponseStatus.ChildCategoryNotExist, "Child Category does not exist.");
                }

                foreach (var childCategory in groupChildCategories)
                {
                    if (childCategory.Value.ParentId == 0)
                    {
                        var successors = _categoryCache.GetCategoryItemSuccessors(groupId, childCategory.Key);
                        if (successors.Contains(id))
                        {
                            return new Status(eResponseStatus.ParentIdShouldNotPointToItself, "Circle alert!!!!");
                        }
                    }
                    else if (childCategory.Value.ParentId != id)
                    {
                        return new Status(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory, 
                                          $"Child Category contains other parent. id = {childCategory.Key}");
                    }

                    if (childCategory.Value.VersionId.HasValue && childCategory.Value.VersionId != categoryVersionId)
                    {
                        return new Status(eResponseStatus.CategoryIsAlreadyAssociatedToVersion, 
                                          $"Child Category {childCategory.Key} is already associated to version {childCategory.Value.VersionId}.");
                    }
                }

                if (oldChildCategoriesIds.Count == 0)
                {
                    updateChildCategories = true;
                }
                else
                {
                    Dictionary<long, int> ccim = new Dictionary<long, int>();
                    for (int i = 0; i < newChildCategoriesIds.Count; i++)
                    {
                        long item = newChildCategoriesIds[i];
                        ccim.Add(item, i);
                    }

                    for (int i = 0; i < oldChildCategoriesIds.Count; i++)
                    {
                        long item = oldChildCategoriesIds[i];

                        if (!ccim.ContainsKey(item))
                        {
                            categoriesToRemove.Add(item);
                        }
                        else if (ccim[item] != i)
                        {
                            updateChildCategories = true;
                        }
                    }

                    if (oldChildCategoriesIds.Count < newChildCategoriesIds.Count)
                    {
                        updateChildCategories = true;
                    }
                }
            }

            return new Status(eResponseStatus.OK);
        }

        private bool AddCategoryItem(int groupId, long userId, CategoryItem objectToAdd, long? duplicateVirtualAssetId = null)
        {
            bool result = false;
            try
            {
                //set NamesInOtherLanguages
                var status = HandleNamesInOtherLanguages(groupId, objectToAdd.NamesInOtherLanguages, out List<KeyValuePair<long, string>> languageCodeToName);
                if (!status.IsOkStatusCode())
                {
                    log.Error($"Error while HandleNamesInOtherLanguages");
                    return result;
                }

                long id = _categoryCache.InsertCategory(groupId, userId, objectToAdd.Name, languageCodeToName, objectToAdd.UnifiedChannels, 
                                                        objectToAdd.DynamicData, objectToAdd.IsActive, objectToAdd.TimeSlot, objectToAdd.Type, 
                                                        objectToAdd.VersionId, objectToAdd.ReferenceId);
                if (id == 0)
                {
                    log.Error($"Error while InsertCategory");
                    return result;
                }

                // Add VirtualAssetInfo for new category 
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    Name = objectToAdd.Name,
                    UserId = userId,
                    DuplicateAssetId = duplicateVirtualAssetId,
                    IsActive = objectToAdd.IsActive
                };

                var virtualAssetInfoResponse = _virtualAssetManager.AddVirtualAsset(groupId, virtualAssetInfo, objectToAdd.Type);
                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while AddVirtualAsset - categoryId: {id} will delete ");
                    DeleteCategoryItem(groupId, userId, id, true);
                    return false;
                }

                if (virtualAssetInfoResponse.Status == VirtualAssetInfoStatus.OK && virtualAssetInfoResponse.AssetId > 0)
                {
                    objectToAdd.VirtualAssetId = virtualAssetInfoResponse.AssetId;
                    _categoryCache.UpdateCategoryVirtualAssetId(groupId, id, objectToAdd.VirtualAssetId.Value, DateUtils.GetUtcUnixTimestampNow(), userId);
                }

                // set child category's order
                if (objectToAdd.ChildrenIds?.Count > 0)
                {
                    // no version in this state (when add/duplicate)
                    if (!_categoryCache.UpdateCategoryOrderNum(groupId, userId, id, null, objectToAdd.ChildrenIds))
                    {
                        return false;
                    }

                    foreach (var item in objectToAdd.ChildrenIds)
                    {
                        _categoryCache.InvalidateCategoryItem(groupId, item);
                    }
                }

                _categoryCache.InvalidateGroupCategory(groupId);

                objectToAdd.UpdateDate = null;
                objectToAdd.Id = id;
                result = true;
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem add. Name:{objectToAdd.Name}", ex);
            }

            return result;
        }

        private Status HandleNamesInOtherLanguages(int groupId, List<LanguageContainer> namesInOtherLanguages, out List<KeyValuePair<long, string>> languageCodeToName)
        {
            languageCodeToName = null;
            if (namesInOtherLanguages != null && namesInOtherLanguages.Count > 0)
            {
                CatalogGroupCache catalogGroupCache = null;
                if (!_catalogManager.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    log.Error($"failed to get catalogGroupCache for groupId: {groupId} when calling HandleNamesInOtherLanguages");
                    return new Status(eResponseStatus.Error);
                }

                languageCodeToName = new List<KeyValuePair<long, string>>();
                foreach (LanguageContainer language in namesInOtherLanguages)
                {
                    languageCodeToName.Add(new KeyValuePair<long, string>(catalogGroupCache.LanguageMapByCode[language.m_sLanguageCode3].ID, language.m_sValue));
                }
            }

            return new Status(eResponseStatus.OK);
        }

        private bool DeleteCategoryItem(int groupId, long userId, long id, bool DBOnly = false)
        {
            if (!DBOnly)
            {
                // Due to atomic action delete virtual asset before category delete
                // Delete the virtual asset
                var vai = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    UserId = userId
                };

                var response = _virtualAssetManager.DeleteVirtualAsset(groupId, vai);
                if (response.Status == VirtualAssetInfoStatus.Error)
                {
                    log.Error($"Error while delete category virtual asset id {vai.ToString()}");
                    return false;
                }

                //delete images
                if (response.Status == VirtualAssetInfoStatus.OK)
                {
                    var images = _imageManager.GetImagesByObject(groupId, id, eAssetImageType.Category);
                    if (images.HasObjects())
                    {
                        foreach (var image in images.Objects)
                        {
                            _imageManager.DeleteImage(groupId, image.Id, userId);
                        }
                    }
                }
            }

            var isDeleted = _categoryCache.DeleteCategoryItem(groupId, userId, id, DBOnly);
            return isDeleted;
        }

        #endregion
    }
}