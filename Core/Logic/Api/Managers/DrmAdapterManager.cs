using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using CachingHelpers;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ApiLogic.Api.Managers
{
    public class DrmAdapterManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const string ADAPTER_NOT_EXIST = "Adapter does not exist";
        private const string NO_ADAPTER_TO_INSERT = "No adapter to insert";
        private const string NAME_REQUIRED = "Name must have a value";
        private const string ADAPTER_URL_REQUIRED = "Adapter url must have a value";
        private const string EXTERNAL_IDENTIFIER_REQUIRED = "External identifier must have a value";
        private const string ERROR_EXT_ID_ALREADY_IN_USE = "External identifier must be unique";

        private readonly IDrmAdapterRepository _repository;

        private static readonly Lazy<DrmAdapterManager> lazy = new Lazy<DrmAdapterManager>(() => new DrmAdapterManager(
            ApiDAL.Instance), LazyThreadSafetyMode.PublicationOnly);
        
        public static DrmAdapterManager Instance { get { return lazy.Value; } }

        public DrmAdapterManager(IDrmAdapterRepository repository)
        {
            _repository = repository;
        }

        public GenericListResponse<DrmAdapter> GetDrmAdapters(int groupID)
        {
            GenericListResponse<DrmAdapter> response = new GenericListResponse<DrmAdapter>();
            try
            {
                response.Objects = _repository.GetDrmAdapters(groupID);

                if (response.Objects == null || response.Objects.Count == 0)
                {
                    response.Status.Set(new ApiObjects.Response.Status((int)eResponseStatus.OK, ADAPTER_NOT_EXIST));
                }
                else
                {
                    response.Status.Set(new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()));
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public GenericResponse<DrmAdapter> Add(ContextData contextData, DrmAdapter drmAdapter)
        {
            GenericResponse<DrmAdapter> response = new GenericResponse<DrmAdapter>();

            // Create Shared secret
            drmAdapter.SharedSecret = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16);

            long id = _repository.InsertDrmAdapter(drmAdapter, contextData.GroupId, contextData.UserId.Value);

            if (id == 0)
            {
                log.Error($"Error while ADD DrmAdapter. contextData: {contextData.ToString()}.");
                return response;
            }

            drmAdapter.ID = (int)id;
            response.Object = drmAdapter;
            response.Status.Set(eResponseStatus.OK);
            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());

            try
            {
                if (!_repository.IsDrmAdapterExists(contextData.GroupId, id))
                {
                    result.Set(eResponseStatus.DrmAdapterNotExist, $"Drm Adapter {id} does not exist");
                    return result;
                }

                if (!_repository.DeleteDrmAdapter(contextData.GroupId, id, contextData.UserId.Value))
                {
                    log.Error($"Error while DrmAdapter. contextData: {contextData.ToString()}.");
                    result.Set(eResponseStatus.Error);
                    return result;
                }

                result.Set(eResponseStatus.OK);
            }

            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in  delete DrmAdapter. contextData:{contextData.ToString()}, id:{id}.", ex);
            }

            return result;
        }
    }
}

