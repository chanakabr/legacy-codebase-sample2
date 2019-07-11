using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace ApiLogic.Base
{
    // TODO SHIR - CRUD changes
    public interface ICrudHandler<CoreT> where CoreT : ICrudHandeledObject
    {
        GenericResponse<CoreT> Add(int groupId, CoreT objectToAdd, Dictionary<string, object> extraParams = null);
        GenericResponse<CoreT> Update(int groupId, CoreT objectToUpdate, Dictionary<string, object> extraParams = null);
        Status Delete(int groupId, long id);
        GenericResponse<CoreT> Get(int groupId, long id);
    }

    public abstract class BaseCrudHandler<CoreT> where CoreT : ICrudHandeledObject
    {
        //protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public abstract GenericResponse<CoreT> Add(int groupId, CoreT objectToAdd, Dictionary<string, object> extraParams = null);
        public abstract GenericResponse<CoreT> Update(int groupId, CoreT objectToUpdate, Dictionary<string, object> extraParams = null);
        public abstract Status Delete(int groupId, long id);
        public abstract GenericResponse<CoreT> Get(int groupId, long id);

        private BaseCrudHandler() { }

        // TODO SHIR - FINISH GENERIC LIST METHOD in ICrudHandler
        //GenericListResponse<CoreT> List(U filter, KalturaFilter);
    }
}