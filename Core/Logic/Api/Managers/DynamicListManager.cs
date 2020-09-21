using ApiLogic.Base;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using ApiObjects;
using DAL;
using TVinciShared;
using ApiObjects.BulkUpload;
using System.Linq;
using CachingProvider.LayeredCache;
using System.Collections.Generic;
using Tvinci.Core.DAL;

namespace ApiLogic.Api.Managers
{
    public class DynamicListManager : ICrudHandler<DynamicList, long>
    {
        private const int MAX_DYNAMIC_LIST = 100;

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

                if (!ApiDAL.DeleteDynamicList(contextData.GroupId, dynamicListResponse.Object))
                {
                    response.Set(eResponseStatus.Error, "Error while deleting DynamicList");
                    return response;
                }

                SetInvalidationKeys(contextData, id, dynamicListResponse.Object.Type);
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
            var response = new GenericResponse<DynamicList>();

            DynamicList _dynamicList = null;

            try
            {
                var key = LayeredCacheKeys.GetDynamicListKey(contextData.GroupId, id);
                var cacheResult = LayeredCache.Instance.Get(key,
                                                            ref _dynamicList,
                                                            Get_DynamicListByIdDB,
                                                            new Dictionary<string, object>()
                                                            {
                                                                    { "groupId", contextData.GroupId },
                                                                    { "id", id }
                                                            },
                                                            contextData.GroupId,
                                                            LayeredCacheConfigNames.GET_DYNAMIC_LIST_BY_ID,
                                                            new List<string>() { LayeredCacheKeys.GetDynamicListInvalidationKey(contextData.GroupId, id) });
            }
            catch (Exception ex)
            {
                log.Error($"Failed to Get DynamicList contextData: {contextData}, id: {id} ex: {ex}", ex);
            }

            if (_dynamicList != null)
            {
                response.Object = _dynamicList;
                response.SetStatus(eResponseStatus.OK);
            }
            else
            {
                response.SetStatus(eResponseStatus.DynamicListDoesNotExist, $"DynamicList: {id} not found");
            }

            return response;
        }

        public GenericListResponse<DynamicList> GetDynamicListsByIds(ContextData contextData, DynamicListnIdInFilter filter)
        {
            var response = new GenericListResponse<DynamicList>();
            if (filter.IdIn?.Count > 0)
            {
                response.Objects = filter.IdIn.Select(id => Get(contextData, id)).Where(dynamicListResponse => dynamicListResponse.HasObject()).Select(x => x.Object).ToList();
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<DynamicList> SearchDynamicLists(ContextData contextData, DynamicListSearchFilter filter, CorePager pager = null) 
        {
            var response = new GenericListResponse<DynamicList>();

            if (filter.IdEqual.HasValue)
            {
                var dynamicListResponse = Get(contextData, filter.IdEqual.Value);
                if (dynamicListResponse.HasObject() && dynamicListResponse.Object.Type == filter.TypeEqual) 
                {
                    // currently we have only UDID type
                    List<string> udids = ApiDAL.GetUdidDynamicList(contextData.GroupId, filter.IdEqual.Value);
                    if (udids != null && udids.Contains(filter.ValueEqual))
                    {
                        response.Objects.Add(dynamicListResponse.Object);
                    }
                }
            }
            else
            {
                var groupDynamicLists = GetDynamicListIdsByGroup(contextData.GroupId, filter.TypeEqual);
                if (groupDynamicLists.Count > 0)
                {
                    response.Objects = groupDynamicLists.Select(id => Get(contextData, id)).Where(dynamicListResponse => dynamicListResponse.HasObject()).Select(x => x.Object).ToList();
                    if (pager != null)
                    {
                        response.Objects = response.Objects?.Skip(pager.PageIndex * pager.PageSize).Take(pager.PageSize).ToList();
                    }
                }
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericResponse<DynamicList> AddDynamicList(ContextData contextData, DynamicList dynamicList)
        {
            var response = new GenericResponse<DynamicList>();

            try
            {
                var groupDynamicLists = GetDynamicListIdsByGroup(contextData.GroupId, dynamicList.Type);
                if (groupDynamicLists?.Count > MAX_DYNAMIC_LIST)
                {
                    response.SetStatus(eResponseStatus.ExceededMaxCapacity, "dynamic lists Exceeded Max capacity");
                    return response;
                }

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

                if (!ApiDAL.SaveDynamicList(contextData.GroupId, dynamicList))
                {
                    log.ErrorFormat($"Error while saving DynamicList");
                    return response;
                }

                SetInvalidationKeys(contextData, dynamicList.Id, dynamicList.Type);

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

                if (!ApiDAL.SaveDynamicList(contextData.GroupId, dynamicList))
                {
                    log.ErrorFormat($"Error while saving DynamicList");
                    response.SetStatus(eResponseStatus.Error, $"Error Updating DynamicList, ID:[{dynamicList.Id}].");
                    return response;
                }

                SetInvalidationKeys(contextData, dynamicList.Id, dynamicList.Type);
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

        private void SetInvalidationKeys(ContextData contextData, long id, DynamicListType type)
        {
            var invalidationKey = LayeredCacheKeys.GetDynamicListInvalidationKey(contextData.GroupId, id);
            if (LayeredCache.Instance.SetInvalidationKey(invalidationKey))
            {
                var mapKey = LayeredCacheKeys.GetDynamicListGroupMappingInvalidationKey(contextData.GroupId, (int)type);
                LayeredCache.Instance.SetInvalidationKey(mapKey);
            }
        }

        private List<long> GetDynamicListIdsByGroup(int groupId, DynamicListType dynamicListType)
        {
            var key = LayeredCacheKeys.GetDynamicListGroupMappingKey(groupId, (int)dynamicListType);
            List<long> groupDynamicListIds = null;

            if (!LayeredCache.Instance.Get(key,
                                          ref groupDynamicListIds,
                                          Get_DynamicListMap,
                                          new Dictionary<string, object>()
                                          {
                                                { "groupId", groupId },
                                                { "type", dynamicListType }
                                          },
                                          groupId,
                                          LayeredCacheConfigNames.GET_DYNAMIC_LIST_MAP,
                                          new List<string>() { LayeredCacheKeys.GetDynamicListGroupMappingInvalidationKey(groupId, (int)dynamicListType) }))
            {
                log.Error($"Failed to Get DynamicListMap from LayeredCache. contextData: {groupId}");
            }

            return groupDynamicListIds;
        }

        private static Tuple<DynamicList, bool> Get_DynamicListByIdDB(Dictionary<string, object> arg)
        {
            DynamicList _dynamicList = null;

            try
            {
                var groupId = (int)arg["groupId"];
                var id = (long)arg["id"];
                _dynamicList = ApiDAL.GetDynamicList(id);
            }
            catch (Exception ex)
            {
                log.Error($"Failed GetDynamicList group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<DynamicList, bool>(_dynamicList, _dynamicList != null);
        }

        private static Tuple<List<long>, bool> Get_DynamicListMap(Dictionary<string, object> arg)
        {
            List<long> _dynamicListMap = null;

            try
            {
                int groupId = (int)arg["groupId"];
                DynamicListType dynamicListType = (DynamicListType)arg["type"];
                _dynamicListMap = ApiDAL.GetDynamicListGroupMapping(groupId, dynamicListType);
            }
            catch (Exception ex)
            {
                log.Error($"Failed Get_DynamicListMap group:[{arg["groupId"]}], ex: {ex}", ex);
            }

            return new Tuple<List<long>, bool>(_dynamicListMap, _dynamicListMap != null);
        }
    }
}
