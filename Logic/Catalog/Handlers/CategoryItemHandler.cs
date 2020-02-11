using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Catalog.Request;
using Core.Catalog.Response;
using DAL;
using KLogMonitor;
using System;
using System.Linq;
using System.Reflection;

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
                    var groupCategories =  CatalogManagement.CatalogManager.GetGroupCategories(contextData.GroupId);
                    if (groupCategories == null || !groupCategories.Values.Any(x => x.Id == objectToAdd.ParentCategoryId.Value))
                    {
                        response.SetStatus(eResponseStatus.CategoryNotExist, "Category doe not exist");
                        return response;
                    }
                }

                long id = ApiDAL.InsertCategory(contextData.GroupId, contextData.UserId, objectToAdd.Name, objectToAdd.ParentCategoryId);

                if(id == 0 )
                {
                    log.Error($"Error while InsertCategory. contextData: {contextData.ToString()}.");
                    return response;
                }

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
            throw new NotImplementedException();
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<CategoryItem> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<CategoryItem> List(ContextData contextData, CategoryItemFilter filter)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<CategoryTree> Duplicate(int groupId, long id)
        {
            throw new NotImplementedException();
        }
    }
}