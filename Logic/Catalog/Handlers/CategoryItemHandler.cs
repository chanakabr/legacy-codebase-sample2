using ApiLogic.Base;
using ApiLogic.Catalog;
using APILogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tvinci.Core.DAL;

namespace Core.Catalog.Handlers
{
    public class CategoryItemHandler : ICrudHandler<CategoryItem, long, CategoryItemFilter>
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

                if (objectToAdd.ChildCategoriesIds?.Count > 0)
                {
                    var groupCategories = CategoriesManager.GetGroupCategoriesIds(contextData.GroupId, objectToAdd.ChildCategoriesIds);

                    if (groupCategories == null || groupCategories.Count != objectToAdd.ChildCategoriesIds.Count)
                    {
                        response.SetStatus(eResponseStatus.ChildCategoryNotExist, "Child category does not exist.");
                        return response;
                    }

                    foreach (long childCategoryId in objectToAdd.ChildCategoriesIds)
                    {
                        if (groupCategories[childCategoryId].ParentId > 0)
                        {
                            response.SetStatus(eResponseStatus.ChildCategoryAlreadyBelongsToAnotherCategory, $"Child Category contains other parent. id = {childCategoryId}");
                            return response;
                        }
                    }
                }

                List<KeyValuePair<long, int>> channels = null;

                if (objectToAdd.UnifiedChannels?.Count > 0)
                {
                    if (!IsUnifiedChannelsValid(contextData.GroupId, contextData.UserId.HasValue ? contextData.UserId.Value : 0, objectToAdd.UnifiedChannels))
                    {
                        response.SetStatus(eResponseStatus.ChannelDoesNotExist, "Channel does not exist");
                        return response;
                    }

                    channels = objectToAdd.UnifiedChannels.Select(x => new KeyValuePair<long, int>(x.Id, (int)x.Type)).ToList();
                }

                long id = CatalogDAL.InsertCategory(contextData.GroupId, contextData.UserId, objectToAdd.Name, channels, objectToAdd.DynamicData);

                if (id == 0)
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                // set child category's order
                if (objectToAdd.ChildCategoriesIds?.Count > 0 && !CatalogDAL.UpdateCategoryOrderNum(contextData.GroupId, contextData.UserId, id, objectToAdd.ChildCategoriesIds))
                {
                    log.Error($"Error while order child categories. contextData: {contextData.ToString()}. new categoryId: {id}");
                    response.SetStatus(eResponseStatus.Error);
                    return response;
                }

                // Add VirtualAssetInfo for new category 
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    Name = objectToAdd.Name,
                    UserId = contextData.UserId.Value
                };

                api.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));

                objectToAdd.Id = id;
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
                var status = CategoriesManager.HandleCategoryChildUpdate(contextData.GroupId, objectToUpdate.Id, objectToUpdate.ChildCategoriesIds,
                                                                        currentCategory.ChildCategoriesIds, ref categoriesToRemove, out updateChildCategories);
                if (!status.IsOkStatusCode())
                {
                    response.SetStatus(status);
                    return response;
                }

                List<KeyValuePair<long, int>> channels = null;

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

                        channels = objectToUpdate.UnifiedChannels.Select(x => new KeyValuePair<long, int>(x.Id, (int)x.Type)).ToList();
                    }
                }

                if (objectToUpdate.DynamicData == null)
                {
                    objectToUpdate.DynamicData = currentCategory.DynamicData;
                }

                if (!CatalogDAL.UpdateCategory(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.Name, channels, objectToUpdate.DynamicData))
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                // set child category's order
                if (categoriesToRemove?.Count > 0 || updateChildCategories)
                {
                    if (!CatalogDAL.UpdateCategoryOrderNum(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.ChildCategoriesIds, categoriesToRemove))
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
                }

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(contextData.GroupId));

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
                if (!CategoriesManager.IsCategoryExist(contextData.GroupId, id))
                {
                    response.Set(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                // need to check if category is a root category. In case it is, all the sub categories should removed as well
                var successors = CategoriesManager.GetCategoryItemSuccessors(contextData.GroupId, id);
                foreach (long successor in successors)
                {
                    if (!CatalogDAL.DeleteCategory(contextData.GroupId, contextData.UserId, id))
                    {
                        log.Error($"Error while DeleteCategory. contextData: {contextData.ToString()} id: {successor}.");
                    }

                    // Delete the virtual asset
                    var vai = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Category,
                        Id = successor,
                        UserId = long.Parse(contextData.UserId.ToString())
                    };

                    api.DeleteVirtualAsset(contextData.GroupId, vai);
                }

                if (!CatalogDAL.DeleteCategory(contextData.GroupId, contextData.UserId, id))
                {
                    log.Error($"Error while DeleteCategory. contextData: {contextData.ToString()} id: {id}.");
                    return response;
                }

                // Delete the virtual asset
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    UserId = long.Parse(contextData.UserId.ToString())
                };

                api.DeleteVirtualAsset(contextData.GroupId, virtualAssetInfo);

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));

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
        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemByIdInFilter filter, CorePager pager)
        {
            GenericListResponse<CategoryItem> response = new GenericListResponse<CategoryItem>();

            List<long> categoriesIds = null;
            CategoryItem categoryItem = null;

            if (filter?.IdIn?.Count > 0)
            {
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

            if (string.IsNullOrEmpty(filter.Ksql) && !filter.RootOnly)
            {
                return List(contextData, new CategoryItemFilter(), pager);
            }

            if (!filter.RootOnly)
            {
                var categories = CategoriesManager.GetGroupCategoriesIds(contextData.GroupId, null, true);
                if (categories?.Count > 0)
                {
                    categoriesIds = categories.Keys.ToList();
                }
            }

            if (!string.IsNullOrEmpty(filter.Ksql) || categoriesIds?.Count > 0)
            {
                AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition()
                {
                    Filter = filter.Ksql,
                    UserId = contextData.UserId.Value,
                    IsAllowedToViewInactiveAssets = RolesPermissionsManager.IsAllowedToViewInactiveAssets(contextData.GroupId, contextData.UserId.Value.ToString(), true) 
                };

                HashSet<long> categories = null;
                if (categoriesIds.Count > 0)
                {
                    categories = new HashSet<long>();
                    foreach (long item in categoriesIds)
                    {
                        categories.Add(item);
                    }
                }

                var result = api.GetObjectVirtualAssetObjectIds(contextData.GroupId, assetSearchDefinition, ObjectVirtualAssetInfoType.Category,
                                                                categories, pager.PageIndex, pager.PageSize, filter.OrderBy);

                if (result.ResultStatus == ObjectVirtualAssetFilterStatus.Error)
                {
                    response.SetStatus(result.Status);
                    return response;
                }

                if (result.ResultStatus == ObjectVirtualAssetFilterStatus.None)
                {
                    response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                }

                categoriesIds = result.ObjectIds;
            }

            if (categoriesIds?.Count > 0)
            {
                List<CategoryItem> categories = new List<CategoryItem>();
                foreach (var item in categoriesIds)
                {
                    var categoryItem = CategoriesManager.GetCategoryItem(contextData.GroupId, item);
                    categories.Add(categoryItem);
                }
            }



            

            //int totalCount;
            //result.Objects = SegmentationType.List(groupId, filter.ObjectIds?.ToList(), pageIndex, pageSize, out totalCount);
            //result.TotalItems = totalCount;
            //result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());


            //var categoriesMap = CategoriesManager.GetGroupCategoriesIds(contextData.GroupId);
            //if (categoriesMap?.Count > 0)
            //{
            //    List<long> categoriesIds = categoriesMap.Where(x => x.Value.ParentId == 0).Select(x => x.Key).ToList();
            //    foreach (var categoryId in categoriesIds)
            //    {
            //        categoryItem = CategoriesManager.GetCategoryItem(contextData.GroupId, categoryId);
            //        if (categoryItem != null)
            //        {
            //            response.Objects.Add(categoryItem);
            //        }
            //    }

            //    response.TotalItems = response.Objects.Count;
            //    response.SetStatus(eResponseStatus.OK);
            //}

            return response;
        }

        public GenericResponse<CategoryTree> Duplicate(int groupId, long userId, long id)
        {
            var response = new GenericResponse<CategoryTree>();

            try
            {
                // Get category to duplicate
                //var categoryToDuplicate = CatalogManagement.CategoriesManagerGetGroupCategory(groupId, id);
                //if (categoryToDuplicate == null)
                //{
                //    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                //    return response;
                //}

                //long newCategoryId = CatalogDAL.InsertCategory(groupId, userId, categoryToDuplicate.Name, categoryToDuplicate.ParentCategoryId, categoryToDuplicate.DynamicData);

                //if (newCategoryId == 0)
                //{
                //    log.Error($"Error while DuplicateCategory. groupId: {groupId} id: {id}.");
                //    return response;
                //}

                //categoryToDuplicate.Id = newCategoryId;
                //response.Object = new CategoryTree() { Id = newCategoryId, Name = categoryToDuplicate.Name };
                //response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in CategoryItem duplicate.  groupId: {groupId} id: {id}.", ex);
            }

            return response;
        }

        public GenericResponse<CategoryTree> GetCategoryTree(int groupId, long userId, long categoryItemId)
        {
            GenericResponse<CategoryTree> response = new GenericResponse<CategoryTree>();

            CategoryTree categoryTree = null;

            // Get the current category
            var categoryItem = CategoriesManager.GetCategoryItem(groupId, categoryItemId);
            if (categoryItem == null)
            {
                response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                return response;
            }

            categoryTree = new CategoryTree()
            {
                Id = categoryItem.Id,
                DynamicData = categoryItem.DynamicData,
                Name = categoryItem.Name
                //UnifiedChannels = categoryItem.UnifiedChannels                
            };

            if (categoryItem.ChildCategoriesIds?.Count > 0)
            {
                categoryTree.Children = new List<CategoryTree>();
                //TODO anat: images

                foreach (var categoryId in categoryItem.ChildCategoriesIds)
                {

                }
            }

            //TODO anat: images

            response.Object = categoryTree;
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private bool IsUnifiedChannelsValid(int groupId, long userId, List<UnifiedChannel> unifiedChannels)
        {
            List<long> externalChannels = unifiedChannels.Where(x => x.Type == UnifiedChannelType.External).Select(y => y.Id).ToList();
            List<long> intenalChannels = unifiedChannels.Where(x => x.Type == UnifiedChannelType.Internal).Select(y => y.Id).ToList();

            //check external channel exist
            foreach (var channelId in externalChannels)
            {
                if (CatalogDAL.GetExternalChannelById(groupId, (int)channelId) == null)
                {
                    return false;
                }
            }

            //check internal channel exist
            foreach (var channelId in intenalChannels)
            {
                var channel = ChannelManager.GetChannelById(groupId, (int)channelId, true, userId);
                if (!channel.HasObject())
                {
                    return false;
                }
            }
            return true;
        }
    }
}