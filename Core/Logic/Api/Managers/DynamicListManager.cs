using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ApiObjects;

namespace ApiLogic.Api.Managers
{
    public class DynamicListManager : ICrudHandler<DynamicList, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<DynamicListManager> lazy = new Lazy<DynamicListManager>(() => new DynamicListManager());
        public static DynamicListManager Instance { get { return lazy.Value; } }

        private DynamicListManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<DynamicList> Get(ContextData contextData, long id)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<DynamicList> GetDynamicListsByIds(ContextData contextData, DynamicListnIdInFilter filter)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<DynamicList> SearchDynamicLists(ContextData contextData, DynamicListSearchFilter filter, CorePager pager = null)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<DynamicList> AddDynamicList(ContextData contextData, DynamicList dynamicList)
        {
            throw new NotImplementedException();
        }

        public GenericResponse<DynamicList> UpdateDynamicList(ContextData contextData, DynamicList dynamicList)
        {
            throw new NotImplementedException();
        }
    }
}
