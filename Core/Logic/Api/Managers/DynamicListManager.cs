using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using ApiObjects;
using DAL;
using TVinciShared;
using ApiObjects.BulkUpload;

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
            var response = Status.Error;

            try
            {
                var dynamicListResponse = Get(contextData, id);
                if (!dynamicListResponse.HasObject())
                {
                    response.Set(dynamicListResponse.Status);
                    return response;
                }

                if (!ApiDAL.DeleteDynamicList(contextData.GroupId, id))
                {
                    response.Set(eResponseStatus.Error, "Error while deleting DynamicList");
                    return response;
                }

                SetInvalidationKeys(contextData.GroupId, dynamicListResponse.Object.Type);
                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in delete DynamicList. contextData:{contextData}", ex);
            }

            return response;
        }

        public GenericResponse<DynamicList> Get(ContextData contextData, long id)
        {
            // TODO SHIR - get ID
            throw new NotImplementedException();
        }

        public GenericListResponse<DynamicList> GetDynamicListsByIds(ContextData contextData, DynamicListnIdInFilter filter)
        {
            // TODO SHIR - GET BY IDS
            throw new NotImplementedException();
        }

        public GenericListResponse<DynamicList> SearchDynamicLists(ContextData contextData, DynamicListSearchFilter filter, CorePager pager = null)
        {
            // TODO SHIR - SEARCH 
            throw new NotImplementedException();
        }

        public GenericResponse<DynamicList> AddDynamicList(ContextData contextData, DynamicList dynamicList)
        {
            var response = new GenericResponse<DynamicList>();

            try
            {
                var couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
                dynamicList.Id = (long)couchbaseManager.Increment("dynamic_list_sequence", 1);
                if (dynamicList.Id == 0)
                {
                    log.ErrorFormat("Error setting DynamicList id");
                    return response;
                }

                dynamicList.CreateDate = DateUtils.GetUtcUnixTimestampNow();
                dynamicList.UpdateDate = dynamicList.CreateDate;
                dynamicList.UpdaterId = contextData.UserId.Value;
                
                if (!ApiDAL.SaveDynamicList(dynamicList))
                {
                    log.ErrorFormat($"Error while saving DynamicList");
                    return response;
                }

                SetInvalidationKeys(contextData.GroupId, dynamicList.Type);

                response.Object = dynamicList;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Add DynamicList. contextData:{contextData}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<DynamicList> UpdateDynamicList(ContextData contextData, DynamicList dynamicList)
        {
            var response = new GenericResponse<DynamicList>();
            try
            {
                var oldDynamicListResponse = Get(contextData, dynamicList.Id);
                if (!oldDynamicListResponse.HasObject())
                {
                    response.SetStatus(oldDynamicListResponse.Status);
                    return response;
                }

                dynamicList.FillEmpty(oldDynamicListResponse.Object);
                dynamicList.UpdateDate = DateUtils.GetUtcUnixTimestampNow();
                dynamicList.UpdaterId = contextData.UserId.Value;

                if (!ApiDAL.SaveDynamicList(dynamicList))
                {
                    log.ErrorFormat($"Error while saving DynamicList");
                    response.SetStatus(eResponseStatus.Error, $"Error Updating DynamicList, ID:[{dynamicList.Id}].");
                    return response;
                }

                SetInvalidationKeys(contextData.GroupId, dynamicList.Type);
                response.Object = dynamicList;
                response.SetStatus(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Update DynamicList. contextData:{contextData}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        private void SetInvalidationKeys(int groupId, DynamicListType type)
        {
            throw new NotImplementedException();
        }
    }
}
