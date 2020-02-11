using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
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
            throw new NotImplementedException();
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