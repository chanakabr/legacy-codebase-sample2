using ApiLogic.Api.Managers;
using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;

namespace Core.Catalog.Handlers
{
    public class CategoryItemHandler : ICrudHandler<CategoryItem, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CategoryItemHandler> lazy = new Lazy<CategoryItemHandler>(() => new CategoryItemHandler());

        public static CategoryItemHandler Instance { get { return lazy.Value; } }

        private CategoryItemHandler() { }

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
                    var assetStructId = PartnerConfigurationManager.GetAssetStructByObjectVirtualAssetInfo(contextData.GroupId, ObjectVirtualAssetInfoType.Category, objectToAdd.Type);
                    if (assetStructId == 0)
                    {
                        response.SetStatus(eResponseStatus.CategoryTypeNotExist, $"Category type '{objectToAdd.Type}' does not exist.");
                        return response;
                    }
                }

                if (objectToAdd.ChildrenIds?.Count > 0)
                {
                    var groupCategories = CategoriesManager.GetGroupCategoriesIds(contextData.GroupId, objectToAdd.ChildrenIds);

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
                    }
                }

                if (objectToAdd.UnifiedChannels?.Count > 0)
                {
                    if (!IsUnifiedChannelsValid(contextData.GroupId, contextData.UserId.HasValue ? contextData.UserId.Value : 0, objectToAdd.UnifiedChannels))
                    {
                        response.SetStatus(eResponseStatus.ChannelDoesNotExist, "Channel does not exist");
                        return response;
                    }
                }

                if (!objectToAdd.IsActive.HasValue)
                {
                    objectToAdd.IsActive = true;
                }

                if (!CategoriesManager.Add(contextData.GroupId, contextData.UserId.Value, objectToAdd))
                {
                    response.SetStatus(eResponseStatus.Error, "Failed to add item");
                    return response;
                }

                // set child category's order
                if (objectToAdd.ChildrenIds?.Count > 0)
                {
                    if (!CatalogDAL.UpdateCategoryOrderNum(contextData.GroupId, contextData.UserId, objectToAdd.Id, objectToAdd.ChildrenIds, null))
                    {
                        log.Error($"Error while re-order child categories. contextData: {contextData.ToString()}. new categoryId: {objectToAdd.Id}");
                        response.SetStatus(eResponseStatus.Error);
                        return response;
                    }
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

        public GenericResponse<CategoryItem> Update(ContextData contextData, CategoryItem objectToUpdate)
        {
            var response = new GenericResponse<CategoryItem>();
            VirtualAssetInfo virtualAssetInfo = null;

            try
            {
                // Get the current category
                var currentCategory = CategoriesManager.GetCategoryItem(contextData.GroupId, objectToUpdate.Id);
                if (currentCategory == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
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
                        UserId = contextData.UserId.Value
                    };
                }

                bool updateChildCategories = false;
                List<long> categoriesToRemove = new List<long>();
                var status = CategoriesManager.HandleCategoryChildUpdate(contextData.GroupId, objectToUpdate.Id, objectToUpdate.ChildrenIds,
                                                                        currentCategory.ChildrenIds, ref categoriesToRemove, out updateChildCategories);
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
                        if (!IsUnifiedChannelsValid(contextData.GroupId, contextData.UserId.HasValue ? contextData.UserId.Value : 0, objectToUpdate.UnifiedChannels))
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
                        status = CategoriesManager.HandleNamesInOtherLanguages(contextData.GroupId, objectToUpdate.NamesInOtherLanguages, out languageCodeToName);
                        if (!status.IsOkStatusCode())
                        {
                            response.SetStatus(status);
                            return response;
                        }
                    }
                }

                if (!CatalogDAL.UpdateCategory(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.Name,
                    languageCodeToName, objectToUpdate.UnifiedChannels, objectToUpdate.DynamicData, objectToUpdate.IsActive, objectToUpdate.TimeSlot))
                {
                    log.Error($"Error while updateCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                // set child category's order
                if (categoriesToRemove?.Count > 0 || updateChildCategories)
                {
                    if (!CatalogDAL.UpdateCategoryOrderNum(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.ChildrenIds, categoriesToRemove))
                    {
                        log.Error($"Error while re-order child categories. contextData: {contextData.ToString()}. new categoryId: {objectToUpdate.Id}");
                        response.SetStatus(eResponseStatus.Error);
                        return response;
                    }
                }

                if (virtualAssetInfo != null)
                {
                    api.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                }

                if (updateChildCategories || categoriesToRemove.Count > 0)
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));
                    foreach (var item in categoriesToRemove)
                    {
                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(item));
                    }

                    if (objectToUpdate.ChildrenIds?.Count > 0)
                    {
                        foreach (var item in objectToUpdate.ChildrenIds)
                        {
                            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(item));
                        }
                    }
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(objectToUpdate.Id));

                response.Object = CategoriesManager.GetCategoryItem(contextData.GroupId, objectToUpdate.Id);
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

            try
            {
                //Check if category exist
                var item = CategoriesManager.GetCategoryItem(contextData.GroupId, id);
                if (item == null)
                {
                    response.Set(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                if (!DeleteCategoryItem(contextData.GroupId, contextData.UserId.Value, id))
                {
                    return response;
                }

                // need to check if category is a root category. In case it is, all the sub categories should removed as well
                var successors = CategoriesManager.GetCategoryItemSuccessors(contextData.GroupId, id);
                foreach (long successor in successors)
                {
                    DeleteCategoryItem(contextData.GroupId, contextData.UserId.Value, successor);

                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));
                if (item.ParentId > 0)
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(item.ParentId.Value));
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem delete. contextData:{contextData.ToString()}, id:{id}", ex);
            }

            return response;
        }

        public GenericResponse<CategoryItem> Get(ContextData contextData, long id)
        {
            var response = new GenericResponse<CategoryItem>();

            try
            {
                // Get the current category
                var currentCategory = CategoriesManager.GetCategoryItem(contextData.GroupId, id);
                if (currentCategory == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                response.Object = currentCategory;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem get. contextData:{contextData.ToString()}, id:{id}", ex);
            }

            return response;
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemFilter filter, CorePager pager)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemAncestorsFilter filter, CorePager pager)
        {
            GenericListResponse<CategoryItem> response = new GenericListResponse<CategoryItem>();

            if (pager.PageIndex != 0)
            {
                response.Status.Set(eResponseStatus.InvalidValue, "Page index value must be 1.");
                return response;
            }

            CategoryItem ci = CategoriesManager.GetCategoryItem(contextData.GroupId, filter.Id);

            if (ci == null)
            {
                response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                return response;
            }

            var ancestors = CategoriesManager.GetCategoryItemAncestors(contextData.GroupId, filter.Id);
            if (ancestors?.Count > 0)
            {
                response.Objects = ancestors.Select(x => CategoriesManager.GetCategoryItem(contextData.GroupId, x)).ToList();
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
                    categoryItem = CategoriesManager.GetCategoryItem(contextData.GroupId, categoryId);
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
                assetStructId = PartnerConfigurationManager.GetAssetStructByObjectVirtualAssetInfo(contextData.GroupId, ObjectVirtualAssetInfoType.Category, filter.TypeEqual);
                if (assetStructId == 0)
                {
                    response.SetStatus(eResponseStatus.CategoryTypeNotExist, $"Category type '{filter.TypeEqual}' does not exist.");
                    return response;
                }
            }

            if (filter.RootOnly)
            {
                var groupCategories = CategoriesManager.GetGroupCategoriesIds(contextData.GroupId, null, true);
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
                result = api.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Category,
                                                           categories, 0, 0, filter.OrderBy);
            }
            else
            {
                result = api.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Category,
                                                           categories, pager.PageIndex, pager.PageSize, filter.OrderBy);
            }

            if (result.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
            {
                response.SetStatus(result.Status);
                return response;
            }

            if (result.ObjectIds?.Count > 0)
            {
                response.Objects = result.ObjectIds.Select(x => CategoriesManager.GetCategoryItem(contextData.GroupId, x)).ToList();

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

        public GenericResponse<CategoryTree> Duplicate(int groupId, long userId, long id, string name)
        {
            var response = new GenericResponse<CategoryTree>();

            try
            {
                CategoryItem root = CategoriesManager.GetCategoryItem(groupId, id);

                if (root == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                CategoryItem copiedRoot = (CategoryItem)root.Clone();
                copiedRoot.Name = name;

                Dictionary<long, long> newTreeMap = new Dictionary<long, long>();

                DuplicateChildren(groupId, userId, copiedRoot, newTreeMap);

                // in case the duplicate category have parent, it should bw updated with his new child :)
                if (root.ParentId.HasValue && root.ParentId.Value > 0)
                {
                    // Get parent
                    CategoryItem parent = CategoriesManager.GetCategoryItem(groupId, root.ParentId.Value);

                    if (parent == null)
                    {
                        response.SetStatus(eResponseStatus.CategoryNotExist, "Parent Category does not exist");
                        return response;
                    }

                    // update parent with new child
                    parent.ChildrenIds.Add(newTreeMap[id]);
                    if (!CatalogDAL.UpdateCategoryOrderNum(groupId, userId, root.ParentId.Value, parent.ChildrenIds, null))
                    {
                        log.Error($"Error while re-order child categories. groupId: {groupId}. after duplicate categoryId: {id}");
                        response.SetStatus(eResponseStatus.Error);
                        return response;
                    }
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(root.ParentId.Value));
                }

                response = GetCategoryTree(groupId, newTreeMap[id]);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem duplicate.  groupId: {groupId} id: {id}.", ex);
            }

            return response;
        }

        private void DuplicateChildren(int groupId, long userId, CategoryItem parent, Dictionary<long, long> newTreeMap)
        {
            List<long> children = new List<long>();
            if (parent.ChildrenIds?.Count > 0)
            {
                CategoryItem ci;
                foreach (var item in parent.ChildrenIds)
                {
                    ci = CategoriesManager.GetCategoryItem(groupId, item);
                    DuplicateChildren(groupId, userId, ci, newTreeMap);
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
                Type = parent.Type
            };

            if (CategoriesManager.Add(groupId, userId, newICategory))
            {
                newTreeMap.Add(parent.Id, newICategory.Id);

                DuplicateCategoryImages(groupId, userId, parent.Id, newICategory.Id);
            }
        }

        public GenericResponse<CategoryTree> GetCategoryTree(int groupId, long id, bool filter = false, bool onlyActive = false)
        {
            GenericResponse<CategoryTree> response = new GenericResponse<CategoryTree>();

            CategoryTree categoryTree = null;

            // Get the current category
            var categoryItem = CategoriesManager.GetCategoryItem(groupId, id, filter, onlyActive);
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
                List<CategoryItem> childern = categoryItem.ChildrenIds.Select(x => CategoriesManager.GetCategoryItem(groupId, x, filter, onlyActive)).ToList();

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

        private List<CategoryTree> FindTreeChildren(int groupId, List<CategoryItem> children, bool filter, bool onlyActive)
        {
            List<CategoryTree> response = new List<CategoryTree>();
            CategoryTree ct;
            children.RemoveAll(item => item == null);

            foreach (var c in children)
            {
                ct = BuildCategoryTree(groupId, c);
                var ch = c.ChildrenIds.Select(x => CategoriesManager.GetCategoryItem(groupId, x, onlyActive)).ToList();
                ch.RemoveAll(item => item == null);
                if (ch.Any(i => i != null))
                {
                    ct.Children = FindTreeChildren(groupId, ch, filter, onlyActive);
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
            var images = Catalog.CatalogManagement.ImageManager.GetImagesByObject(groupId, categoryItem.Id, eAssetImageType.Category);
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

            foreach (var unifiedChannel in unifiedChannels)
            {
                var unifiedChannelInfo = unifiedChannel as UnifiedChannelInfo;
                UnifiedChannelInfo uci = new UnifiedChannelInfo() { Id = unifiedChannel.Id };

                //check external channel exist
                if (unifiedChannel.Type == UnifiedChannelType.External)
                {
                    var ec = ExternalChannelManager.GetChannelById(groupId, (int)unifiedChannel.Id, true, userId);
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
                    var channel = ChannelManager.GetChannelById(groupId, (int)unifiedChannel.Id, true, userId);
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
            var images = Catalog.CatalogManagement.ImageManager.GetImagesByObject(groupId, categoryFromId, eAssetImageType.Category);
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

                    var newImageresponse = Catalog.CatalogManagement.ImageManager.AddImage(groupId, newImage, userId);
                    if (newImageresponse.HasObject())
                    {
                        imageTypeIds.Add(image.ImageTypeId);

                        string imageOriginalUrl = $"{image.Url}/width/0/height/0";

                        var status = Catalog.CatalogManagement.ImageManager.SetContent(groupId, userId, newImageresponse.Object.Id, imageOriginalUrl);
                        if (!status.IsOkStatusCode())
                        {
                            log.Error($"Failed to set image for category id:{categoryToId}, url:{imageOriginalUrl}");
                        }
                    }
                }
            }
        }

        private bool DeleteCategoryItem(int groupId, long userId, long id)
        {
            if (!CatalogDAL.DeleteCategory(groupId, userId, id))
            {
                log.Error($"Error while DeleteCategory. id: {id}");
                return false;
            }

            // Delete the virtual asset
            var vai = new VirtualAssetInfo()
            {
                Type = ObjectVirtualAssetInfoType.Category,
                Id = id,
                UserId = userId
            };

            api.DeleteVirtualAsset(groupId, vai);

            //delete images
            var images = Catalog.CatalogManagement.ImageManager.GetImagesByObject(groupId, id, eAssetImageType.Category);
            if (images.HasObjects())
            {
                foreach (var image in images.Objects)
                {
                    Catalog.CatalogManagement.ImageManager.DeleteImage(groupId, image.Id, userId);
                }
            }

            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(groupId));
            LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(id));

            return true;
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

        internal static Status RemoveChannelFromCategories(int groupId, int channelId, UnifiedChannelType unifiedChannelType, long userId)
        {
            Status status = new Status();
            DataTable categories = CatalogDAL.GetCategoriesIdsByChannelId(groupId, channelId, unifiedChannelType);

            if (categories == null)
            {
                log.Error($"failed to get GetCategoriesIdsByChannelId for groupId: {groupId} when calling RemoveChannelFromCategories");
                status.Set(eResponseStatus.Error);
                return status;
            }
            if (categories.Rows.Count > 0)
            {
                List<long> categoriesIds = categories.Rows.OfType<DataRow>().Select(dr => dr.Field<long>("CATEGORY_ID")).ToList();

                foreach (var categoryId in categoriesIds)
                {
                    //Check if category exist
                    var category = CategoriesManager.GetCategoryItem(groupId, categoryId);
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
                            status = CategoriesManager.HandleNamesInOtherLanguages(groupId, category.NamesInOtherLanguages, out languageCodeToName);
                            if (!status.IsOkStatusCode())
                            {
                                return status;
                            }
                        }

                        if (!CatalogDAL.UpdateCategory(groupId, userId, categoryId, category.Name, languageCodeToName, category.UnifiedChannels, category.DynamicData,
                                            category.IsActive, category.TimeSlot))
                        {
                            log.Error($"Error while updateCategory at RemoveChannelFromCategories . categoryId: {categoryId}.");
                            return status;
                        }

                        LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(categoryId));

                        status.Set(eResponseStatus.OK);
                    }
                }
            }

            return status;
        }
    }
}