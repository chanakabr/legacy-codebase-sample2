using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using TVinciShared;

namespace Core.Catalog.CatalogManagement
{
    // validate to objects and logic
    public class CategoryVersionHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly ICategoryCache _categoryCache;
        private readonly ICategoryItemManager _categoryItemManager;

        private static readonly Lazy<CategoryVersionHandler> lazy = new Lazy<CategoryVersionHandler>(() => 
            new CategoryVersionHandler(CategoryCache.Instance, CategoryItemHandler.Instance), 
            LazyThreadSafetyMode.PublicationOnly);

        public static CategoryVersionHandler Instance { get { return lazy.Value; } }

        public CategoryVersionHandler(ICategoryCache categoryCacheManager, ICategoryItemManager categoryItemManager)
        {
            _categoryCache = categoryCacheManager;
            _categoryItemManager = categoryItemManager;
        }
        
        public GenericResponse<CategoryVersion> CreateTree(ContextData contextData, long categoryItemId, string name, string comment)
        {
            var response = new GenericResponse<CategoryVersion>();
            var categoryResponse = _categoryCache.GetCategoryItem(contextData.GroupId, categoryItemId);
            if (!categoryResponse.HasObject())
            {
                response.SetStatus(categoryResponse.Status);
                return response;
            }

            if (categoryResponse.Object.VersionId.HasValue)
            {
                var categoryVersionResponse = _categoryCache.GetCategoryVersion(contextData.GroupId, categoryResponse.Object.VersionId.Value);
                response.SetStatus(eResponseStatus.CategoryIsAlreadyAssociatedToVersionTree, 
                                   $"CategoryItem {categoryItemId} is already associated to a version {categoryResponse.Object.VersionId} from treeId {categoryVersionResponse.Object.TreeId}");
                return response;
            }

            if (categoryResponse.Object.ParentId.HasValue && categoryResponse.Object.ParentId != 0)
            {
                response.SetStatus(eResponseStatus.CategoryIsNotRoot, "Category is not a root");
                return response;
            }

            long treeId = DateUtils.GetUtcUnixTimestampNow();
            var categoryVersion = new CategoryVersion()
            {
                Name = name,
                TreeId = treeId,
                State = CategoryVersionState.Default,
                BaseVersionId = 0,
                CategoryItemRootId = categoryItemId,
                DefaultDate = treeId,
                UpdaterId = contextData.UserId.Value,
                Comment = comment,
                CreateDate = treeId,
                UpdateDate = treeId
            };

            response = _categoryCache.AddCategoryVersion(contextData.GroupId, categoryVersion);
            if (response.HasObject())
            {
                if (_categoryCache.SetDefault(contextData, response.Object.Id, 0))
                {
                    response = Get(contextData, response.Object.Id);
                }
                else
                {
                    response.Status.Set(eResponseStatus.Error, $"error while try to set tree {response.Object.TreeId} as default");
                }
            }
            return response;
        }

        public GenericResponse<CategoryVersion> Add(ContextData contextData, CategoryVersion objectToAdd)
        {
            var response = new GenericResponse<CategoryVersion>();

            var baseVersion = Get(contextData, objectToAdd.BaseVersionId);
            if (!baseVersion.HasObject())
            {
                response.SetStatus(baseVersion.Status);
                return response;
            }

            var categoryResponse = _categoryItemManager.Duplicate(contextData.GroupId, contextData.UserId.Value, baseVersion.Object.CategoryItemRootId);
            if (!categoryResponse.HasObject())
            {
                response.SetStatus(categoryResponse.Status);
                return response;
            }

            objectToAdd.Id = DateUtils.GetUtcUnixTimestampNow();
            objectToAdd.TreeId = baseVersion.Object.TreeId;
            objectToAdd.CategoryItemRootId = categoryResponse.Object.Id;
            objectToAdd.State = CategoryVersionState.Draft;
            objectToAdd.UpdaterId = contextData.UserId.Value;
            objectToAdd.CreateDate = DateUtils.GetUtcUnixTimestampNow();
            objectToAdd.UpdateDate = objectToAdd.CreateDate;

            response = _categoryCache.AddCategoryVersion(contextData.GroupId, objectToAdd);
            return response;
        }

        public GenericResponse<CategoryVersion> Update(ContextData contextData, CategoryVersion objectToUpdate)
        {
            var response = new GenericResponse<CategoryVersion>();

            var oldVersion = Get(contextData, objectToUpdate.Id);
            if (!oldVersion.HasObject())
            {
                response.SetStatus(oldVersion.Status);
                return response;
            }

            if (oldVersion.Object.State != CategoryVersionState.Draft)
            {
                response.SetStatus(eResponseStatus.CategoryVersionIsNotDraft,
                                   $"Cannot update CategoryVersion in state {oldVersion.Object.State}");
                return response;
            }

            bool needToUpdate = objectToUpdate.SetUnchangedProperties(oldVersion.Object);
            if (needToUpdate)
            {
                response = _categoryCache.UpdateCategoryVersion(contextData.GroupId, contextData.UserId.Value, objectToUpdate);
            }
            else
            {
                response.Object = objectToUpdate;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status response = new Status();
            
            var categoryVersionResponse = Get(contextData, id);
            if (!categoryVersionResponse.HasObject())
            {
                response.Set(categoryVersionResponse.Status);
                return response;
            }

            if (categoryVersionResponse.Object.State != CategoryVersionState.Draft)
            {
                response.Set(eResponseStatus.CategoryVersionIsNotDraft,
                             $"Cannot delete CategoryVersion in state {categoryVersionResponse.Object.State}");
                return response;
            }

            response = _categoryCache.DeleteCategoryVersion(contextData.GroupId, contextData.UserId.Value, categoryVersionResponse.Object);
            if (response.IsOkStatusCode())
            {
                response = _categoryItemManager.Delete(contextData, categoryVersionResponse.Object.CategoryItemRootId);
            }

            return response;
        }

        public GenericResponse<CategoryVersion> Get(ContextData contextData, long id)
        {
            var response = _categoryCache.GetCategoryVersion(contextData.GroupId, id);
            return response;
        }

        public Status SetDefault(ContextData contextData, long id, bool force = false)
        {
            var response = Status.Error;

            try
            {
                var categoryVersion = Get(contextData, id);
                if (!categoryVersion.HasObject())
                {
                    response.Set(categoryVersion.Status);
                    return response;
                }

                var listDefaults = _categoryCache.ListCategoryVersionDefaults(contextData);
                var currentDefault = listDefaults.Objects.FirstOrDefault(x => x.TreeId == categoryVersion.Object.TreeId);
                
                if (categoryVersion.Object.CreateDate < currentDefault.DefaultDate.Value && !force)
                {
                    response.Set(eResponseStatus.CategoryVersionIsOlderThanDefault, $"Cannot set old version as default");
                    return response;
                }

                if (categoryVersion.Object.State != CategoryVersionState.Draft)
                {
                    response.Set(eResponseStatus.CategoryVersionIsNotDraft, $"Cannot set {categoryVersion.Object.State} versions as default");
                    return response;
                }

                if (_categoryCache.SetDefault(contextData, id, currentDefault.Id))
                {
                    response.Set(eResponseStatus.OK);
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in SetDefault. contextData:{contextData}, versionId:{id}", ex);
            }

            return response;
        }

    }
}