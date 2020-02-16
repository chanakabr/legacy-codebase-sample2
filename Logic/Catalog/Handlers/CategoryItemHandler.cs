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
                    if (!CatalogManager.IsCategoryExist(contextData.GroupId, objectToAdd.ParentCategoryId.Value))
                    {
                        response.SetStatus(eResponseStatus.CategoryNotExist, "Parent Category does not exist");
                        return response;
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

                long id = CatalogDAL.InsertCategory(contextData.GroupId, contextData.UserId, objectToAdd.Name,
                    objectToAdd.ParentCategoryId, channels, objectToAdd.DynamicData);

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
                    UserId = contextData.UserId.Value
                };

                api.AddVirtualAsset(contextData.GroupId, virtualAssetInfo);

                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(contextData.GroupId));

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
                var currentCategory = CatalogManager.GetCategoryItem(contextData.GroupId, objectToUpdate.Id);
                if (currentCategory == null)
                {
                    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                    return response;
                }

                bool needToInvalidate = false;
                if (currentCategory.ParentCategoryId != objectToUpdate.ParentCategoryId)
                {
                    if (objectToUpdate.ParentCategoryId.HasValue)
                    {
                        if (objectToUpdate.ParentCategoryId.Value > 0)
                        {
                            //Check if ParentCategoryId Exist
                            if (!CatalogManager.IsCategoryExist(contextData.GroupId, objectToUpdate.ParentCategoryId.Value))
                            {
                                response.SetStatus(eResponseStatus.CategoryNotExist, "Parent Category does not exist");
                                return response;
                            }
                            
                            needToInvalidate = true;
                        }
                    }
                    else
                    {
                        objectToUpdate.ParentCategoryId = currentCategory.ParentCategoryId;
                    }
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

                if (!CatalogDAL.UpdateCategory(contextData.GroupId, contextData.UserId, objectToUpdate.Id, objectToUpdate.Name, objectToUpdate.ParentCategoryId,
                    channels, objectToUpdate.DynamicData))
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

                // if name change need to update virtualAsset
                if (objectToUpdate.Name != currentCategory.Name)
                {
                    var virtualAssetInfo = new VirtualAssetInfo()
                    {
                        Type = ObjectVirtualAssetInfoType.Category,
                        Id = currentCategory.Id,
                        Name = objectToUpdate.Name,
                        UserId = contextData.UserId.Value
                    };

                    api.UpdateVirtualAsset(contextData.GroupId, virtualAssetInfo);
                }

                if (needToInvalidate)
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesInvalidationKey(contextData.GroupId));
                }
                
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetCategoryIdInvalidationKey(contextData.GroupId));

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
                //var currentCategory = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, id);
                //if (currentCategory == null)
                //{
                //    response.Set(eResponseStatus.CategoryNotExist, "Category does not exist");
                //    return response;
                //}

                //if (!CatalogDAL.DeleteCategory(contextData.GroupId, contextData.UserId, id))
                //{
                //    log.Error($"Error while DeleteCategory. contextData: {contextData.ToString()} id: {id}.");
                //    return response;
                //}

                //// Delete the virtual asset
                //var virtualAssetInfo = new VirtualAssetInfo()
                //{
                //    Type = ObjectVirtualAssetInfoType.Category,
                //    Id = id,
                //    UserId = long.Parse(contextData.UserId.ToString())
                //};

                //api.DeleteVirtualAsset(contextData.GroupId, virtualAssetInfo);

                //LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupCategoriesDictionaryInvalidationKey(contextData.GroupId));

                //response.Set(eResponseStatus.OK);
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
                //// Get the current category
                //var currentCategory = CatalogManagement.CatalogManager.GetGroupCategory(contextData.GroupId, id);
                //if (currentCategory == null)
                //{
                //    response.SetStatus(eResponseStatus.CategoryNotExist, "Category does not exist");
                //    return response;
                //}

                //response.Object = currentCategory;
                //response.Status.Set(eResponseStatus.OK);
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
                //var categoryToDuplicate = CatalogManagement.CatalogManager.GetGroupCategory(groupId, id);
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