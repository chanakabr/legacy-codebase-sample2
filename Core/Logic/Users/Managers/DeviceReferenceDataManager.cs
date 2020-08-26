using ApiLogic.Base;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Reflection;
using CachingProvider.LayeredCache;
using System.Collections.Generic;
using System.Linq;
using DAL;
using Newtonsoft.Json;

namespace ApiLogic.Users.Managers
{
    public class DeviceReferenceDataManager : ICrudHandler<DeviceReferenceData, long>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<DeviceReferenceDataManager> lazy = new Lazy<DeviceReferenceDataManager>(() => new DeviceReferenceDataManager());
        public static DeviceReferenceDataManager Instance { get { return lazy.Value; } }

        private DeviceReferenceDataManager() { }

        public Status Delete(ContextData contextData, long id)
        {
            var response = new Status();
            var all = GetReferenceData(contextData.GroupId);
            var _object = all?.Where(d => d.Id == id).FirstOrDefault();

            if (_object == null)
            {
                response.Set(eResponseStatus.Error, $"No Device Reference Data exists with id: {id}");
            }
            else
            {
                var delete = UsersDal.DeleteDeviceInformation(contextData.GroupId, contextData.UserId, id);
                if (delete == null || !delete.Object)
                    response.Set(eResponseStatus.Error, $"Failed to delete Device Reference Data, id: {id}");
                else
                    response.Set(eResponseStatus.OK);
            }

            return response;
        }

        public GenericResponse<DeviceReferenceData> Get(ContextData contextData, long id)
        {
            var response = new GenericResponse<DeviceReferenceData>();
            var all = GetReferenceData(contextData.GroupId);
            response.Object = all?.Where(d => d.Id == id).FirstOrDefault();
            if (response.Object != null)
            {
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        public GenericResponse<DeviceReferenceData> Add<T>(ContextData contextData, T coreObject) where T : DeviceReferenceData
        {
            var response = new GenericResponse<DeviceReferenceData>();
            var groupId = contextData.GroupId;
            var updaterId = contextData.UserId;
            try
            {
                response = UsersDal.InsertDeviceReferenceData(groupId, updaterId, coreObject);

                if (response.IsOkStatusCode())
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(groupId));
                else
                {
                    log.Info($"Cannot Add Device Reference Data from type: {(DeviceInformationType)coreObject.GetType()}");
                    response.SetStatus(eResponseStatus.Error, "Cannot add Device Reference Data");
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Add. contextData: {contextData}, " +
                    $"Reference Data: {JsonConvert.SerializeObject(coreObject)}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public GenericResponse<DeviceReferenceData> Update<T>(ContextData contextData, T coreObject) where T : DeviceReferenceData
        {
            var response = new GenericResponse<DeviceReferenceData>();
            var groupId = contextData.GroupId;
            var updaterId = contextData.UserId;
            try
            {
                response = UsersDal.UpdateDeviceReferenceData
                        (groupId, updaterId, coreObject);

                if (response.IsOkStatusCode())
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(groupId));
                else
                {
                    log.Info($"Cannot update device information from type: {(DeviceInformationType)coreObject.GetType()}");
                    response.SetStatus(eResponseStatus.Error, "Cannot update device information");
                }
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in Update. contextData: {contextData}, " +
                    $"DeviceInformation: {JsonConvert.SerializeObject(coreObject)}", ex);
                response.SetStatus(eResponseStatus.Error);
            }

            return response;
        }

        public List<DeviceReferenceData> GetReferenceData(int groupId)
        {
            var response = new List<DeviceReferenceData>();
            try
            {
                IEnumerable<DeviceReferenceData> _response = null;
                var key = LayeredCacheKeys.GetDeviceReferenceDataByGroupKey(groupId);
                var cacheResult = LayeredCache.Instance.Get(
                    key,
                    ref _response,
                    GetReferenceDataByGroupId,
                    new Dictionary<string, object>() { { "groupId", groupId } },
                    groupId,
                    LayeredCacheConfigNames.GET_DEVICE_REFERENCE_DATA,
                    new List<string>() { LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(groupId) });


                response = _response?.ToList();
            }
            catch (Exception ex)
            {
                log.Error($"Failed groupID={groupId}, ex:{ex}");
            }

            return response;
        }

        private static Tuple<IEnumerable<DeviceReferenceData>, bool> GetReferenceDataByGroupId(Dictionary<string, object> arg)
        {
            try
            {
                var groupId = (int)arg["groupId"];
                var models = DAL.UsersDal.GetDeviceReferenceData(groupId);
                return new Tuple<IEnumerable<DeviceReferenceData>, bool>(models, true);
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get ReferenceData from DB group:[{arg["groupId"]}], ex: {ex}");
                return new Tuple<IEnumerable<DeviceReferenceData>, bool>(Enumerable.Empty<DeviceReferenceData>(), false);
            }
        }

        public GenericListResponse<DeviceReferenceData> List(ContextData contextData, DeviceReferenceDataFilter filter, CorePager pager)
        {
            throw new NotImplementedException();
        }

        public GenericListResponse<DeviceReferenceData> List(ContextData contextData, DeviceModelReferenceDataFilter filter, CorePager pager)
        {
            var response = new GenericListResponse<DeviceReferenceData>();

            if (filter == null)
            {
                filter = new DeviceModelReferenceDataFilter();
            }

            var dbContent = GetReferenceData(contextData.GroupId);

            var filtered = dbContent?.Where(rd => rd.GetType() == (int)DeviceInformationType.Model).ToList();

            if (filter.DeviceReferenceDataIdsIn != null && filter.DeviceReferenceDataIdsIn.Count > 0)
            {
                filtered = filtered?.Where(rd => filter.DeviceReferenceDataIdsIn.Contains((int)rd.Id)).ToList();
            }

            response.Objects = filtered;
            response.TotalItems = filtered.Count;
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public GenericListResponse<DeviceReferenceData> List(ContextData contextData, DeviceManufacturersReferenceDataFilter filter, CorePager pager)
        {
            var response = new GenericListResponse<DeviceReferenceData>();

            if (filter == null)
            {
                filter = new DeviceManufacturersReferenceDataFilter();
            }

            var dbContent = GetReferenceData(contextData.GroupId);

            var filtered = dbContent?.Where(rd => rd.GetType() == (int)DeviceInformationType.Manufacturer).ToList();

            if (filter.DeviceReferenceDataIdsIn != null && filter.DeviceReferenceDataIdsIn.Count > 0)
            {
                filtered = filtered?.Where(rd => filter.DeviceReferenceDataIdsIn.Contains((int)rd.Id)).ToList();
            }

            response.Objects = filtered;
            response.TotalItems = filtered.Count;
            response.SetStatus(eResponseStatus.OK);

            return response;
        }
    }
}
