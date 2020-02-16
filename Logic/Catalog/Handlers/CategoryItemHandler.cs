using ApiLogic.Base;
using ApiLogic.Catalog;
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
                if (objectToAdd.ParentCategoryId.HasValue)
                {
                    //Check if ParentCategoryId Exist
                    var category = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, objectToAdd.ParentCategoryId.Value);
                    if (category == null)
                    {
                        response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                        return response;
                    }
                }

                List<long> channelsIds = null;
                if (objectToAdd.UnifiedChannels?.Count > 0)
                {
                    // TODO anat: Validate channels?  
                    foreach (var unifiedChannel in objectToAdd.UnifiedChannels)
                    {
                        // TODO anat: Validate channels?  ( is it inner and external?)
                        var channel = ChannelManager.GetChannelById(contextData.GroupId, int.Parse(unifiedChannel.Id.ToString()), false, long.Parse(contextData.UserId.ToString()));
                        if (!channel.HasObject())
                        {
                            log.Error($"Channel {unifiedChannel.Id} at groupId  {contextData.GroupId}, doesn't exist");
                            response.SetStatus(eResponseStatus.ChannelDoesNotExist, "Channel does not exist");
                            return response;
                        }
                    }

                    channelsIds = objectToAdd.UnifiedChannels.Select(x => x.Id).ToList();
                }

                long id = CatalogDAL.InsertCategory(contextData.GroupId, contextData.UserId, objectToAdd.Name,
                    objectToAdd.ParentCategoryId, channelsIds, objectToAdd.DynamicData);

                if (id == 0)
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                // Add VirtualAssetInfo for new category
                var virtualAssetInfo = new VirtualAssetInfo()
                {
                    Type = ObjectVirtualAssetInfoType.Category,
                    Id = id,
                    Name = objectToAdd.Name,
                    UserId = long.Parse(contextData.UserId.ToString())
                };

                api.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesDictionaryInvalidationKey(contextData.GroupId));

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

            try
            {
                // Get the current category
                var currentCategory = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, objectToUpdate.Id);
                if (currentCategory == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                if (currentCategory.ParentCategoryId != objectToUpdate.ParentCategoryId)
                {
                    if (objectToUpdate.ParentCategoryId.HasValue)
                    {
                        if (objectToUpdate.ParentCategoryId.Value > 0)
                        {
                            //Check if ParentCategoryId Exist
                            var category = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, objectToUpdate.ParentCategoryId.Value);
                            if (category == null)
                            {
                                response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                                return response;
                            }
                        }
                    }
                    else
                    {
                        objectToUpdate.ParentCategoryId = currentCategory.ParentCategoryId;
                    }
                }

                if (objectToUpdate.DynamicData == null)
                {
                    objectToUpdate.DynamicData = currentCategory.DynamicData;

                }

                if (!CatalogDAL.UpdateCategory(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.Name, objectToUpdate.ParentCategoryId, objectToUpdate.DynamicData))
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                response.Object = objectToUpdate;
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
                // Get the current category
                var currentCategory = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, id);
                if (currentCategory == null)
                {
                    response.Set(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                if (!CatalogDAL.DeleteCategory(contextData.GroupId, contextData.UserId, id))
                {
                    log.Error($"Error while DeleteCategory. contextData: {contextData.ToString()} id: {id}.");
                    return response;
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
                var currentCategory = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, id);
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

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemFilter filter)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<CategoryTree> Duplicate(int groupId, long userId, long id)
        {
            var response = new GenericResponse<CategoryTree>();

            try
            {
                // Get category to duplicate
                var categoryToDuplicate = CatalogManagement.CatalogManager.GetGroupCategory(groupId, id);
                if (categoryToDuplicate == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

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
    }
}