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
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(contextData.GroupId));
                    response.Set(eResponseStatus.OK);
                }
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

            try
            {
                response = UsersDal.InsertDeviceReferenceData(contextData, coreObject, TVinciShared.DateUtils.GetUtcUnixTimestampNow());

                if (response.IsOkStatusCode())
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(contextData.GroupId));
                else
                {
                    log.Info($"Cannot Add Device Reference Data from type: {(DeviceInformationType)coreObject.Type}");
                    response.SetStatus(response.Status);
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

            try
            {
                var _list = List(contextData, new DeviceReferenceDataFilter() { IdsIn = new List<int> { (int)coreObject.Id } }, null);
                if (_list == null || !_list.IsOkStatusCode() || _list.TotalItems == 0)
                {
                    response.SetStatus(eResponseStatus.Error, $"DeviceReferenceData Id: {coreObject.Id} not found");
                    return response;
                }

                var _response = UsersDal.UpdateDeviceReferenceData(contextData, coreObject);

                if (_response.IsOkStatusCode())
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(contextData.GroupId));
                    response.Object = List(contextData, new DeviceReferenceDataFilter()
                    { IdsIn = new List<int> { (int)coreObject.Id } }, null)?.Objects?.FirstOrDefault();
                    response.SetStatus(eResponseStatus.OK);
                }
                else
                {
                    log.Info($"Cannot update device information from type: {(DeviceInformationType)coreObject.Type}");
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
            var response = new GenericListResponse<DeviceReferenceData>();

            response.Objects = GetReferenceData(contextData.GroupId);

            if (filter is DeviceManufacturersReferenceDataFilter)
            {
                response.Objects = response.Objects?.Where(rd => rd.Type == (int)DeviceInformationType.Manufacturer).ToList();
                var _filter = (DeviceManufacturersReferenceDataFilter)filter;
                if (!string.IsNullOrEmpty(_filter.NameEqual))
                {
                    response.Objects = response.Objects?.Where(rd => rd.Name.ToUpper() == _filter.NameEqual.Trim().ToUpper()).ToList();
                }
            }

            if (filter.IdsIn != null && filter.IdsIn.Count > 0)
            {
                response.Objects = response.Objects?.Where(rd => filter.IdsIn.Contains((int)rd.Id)).ToList();
            }

            response.TotalItems = response.Objects == null ? 0 : response.Objects.Count;

            if (pager != null && pager.PageSize > 0)
            {
                response.Objects = response.Objects?.Skip(pager.PageIndex * pager.PageSize)?.Take(pager.PageSize).ToList();
            }

            response.SetStatus(eResponseStatus.OK);

            return response;
        }
    }
}
