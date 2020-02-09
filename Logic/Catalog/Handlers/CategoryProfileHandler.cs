using ApiLogic.Base;
using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;

namespace Core.Catalog.Handlers
{
    public class CategoryProfileHandler : ICrudHandler<CategoryProfile, long, CategoryProfileFilter>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<CategoryProfileHandler> lazy = new Lazy<CategoryProfileHandler>(() => new CategoryProfileHandler());

        public static CategoryProfileHandler Instance { get { return lazy.Value; } }

        private CategoryProfileHandler() { }

        public GenericResponse<CategoryProfile> Add(ContextData contextData, CategoryProfile objectToAdd)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<CategoryProfile> Update(ContextData contextData, CategoryProfile objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<CategoryProfile> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<CategoryProfile> List(ContextData contextData, CategoryProfileFilter filter)
        {
            throw new NotImplementedException();
        }
    }
}