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
    public interface IDeviceReferenceDataManager
    {
        GenericListResponse<DeviceReferenceData> ListByManufacturer(ContextData contextData, DeviceManufacturersReferenceDataFilter filter, CorePager pager = null);
    }

    public class DeviceReferenceDataManager : ICrudHandler<DeviceReferenceData, long>, IDeviceReferenceDataManager
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
            response.Object = all.FirstOrDefault(d => d.Id == id);
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
                var filter = new DeviceReferenceDataFilter() { IdsIn = new List<int> { (int)coreObject.Id } };
                var _list = List(contextData, filter);
                if (_list.TotalItems == 0)
                {
                    response.SetStatus(eResponseStatus.Error, $"DeviceReferenceData Id: {coreObject.Id} not found");
                    return response;
                }

                var _response = UsersDal.UpdateDeviceReferenceData(contextData, coreObject);

                if (_response.IsOkStatusCode())
                {
                    LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(contextData.GroupId));
                    response.Object = List(contextData, filter).Objects.FirstOrDefault();
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
            var key = LayeredCacheKeys.GetDeviceReferenceDataByGroupKey(groupId);
            var cacheResult = LayeredCache.Instance.Get(
                key,
                ref response,
                arg => Tuple.Create(UsersDal.GetDeviceReferenceData(groupId), true),
                null,
                groupId,
                LayeredCacheConfigNames.GET_DEVICE_REFERENCE_DATA,
                new List<string>() { LayeredCacheKeys.GetDeviceReferenceDataInvalidationKey(groupId) });

            if (!cacheResult)
            {
                log.Warn($"could not get {key} from LayeredCache");
            }

            return response;
        }

        public DeviceReferenceData GetByManufacturerId(int groupId, long manufacturerId)
        {
            var allReferenceData = GetReferenceData(groupId);
            var response = allReferenceData.FirstOrDefault(x => x.Id == manufacturerId && x.Type == (int)DeviceInformationType.Manufacturer);
            return response;
        }

        public GenericListResponse<DeviceReferenceData> ListByManufacturer(ContextData contextData, DeviceManufacturersReferenceDataFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<DeviceReferenceData>(Status.Ok, GetReferenceData(contextData.GroupId));
            response.Objects = response.Objects.Where(rd => rd.Type == (int)DeviceInformationType.Manufacturer).ToList();
            if (filter != null)
            {
                if (!string.IsNullOrEmpty(filter.NameEqual))
                {
                    response.Objects = response.Objects.Where(rd => rd.Name.ToUpper() == filter.NameEqual.Trim().ToUpper()).ToList();
                }
                response.Objects = FilterByIds(response.Objects, filter);
            }
            response.TotalItems = response.Objects.Count;
            response.Objects = Page(response.Objects, pager);
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericListResponse<DeviceReferenceData> List(ContextData contextData, DeviceReferenceDataFilter filter, CorePager pager = null)
        {
            var response = new GenericListResponse<DeviceReferenceData>();
            response.Objects = GetReferenceData(contextData.GroupId);
            response.Objects = FilterByIds(response.Objects, filter);
            response.TotalItems = response.Objects.Count;
            response.Objects = Page(response.Objects, pager);
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        private List<DeviceReferenceData> FilterByIds(List<DeviceReferenceData> objects, DeviceReferenceDataFilter filter)
        {
            if (filter.IdsIn != null && filter.IdsIn.Count > 0)
            {
                objects = objects.Where(rd => filter.IdsIn.Contains((int)rd.Id)).ToList();
            }

            return objects;
        }

        private List<DeviceReferenceData> Page(List<DeviceReferenceData> objects, CorePager pager)
        {
            if (pager != null && pager.PageSize > 0)
            {
                objects = objects.Skip(pager.PageIndex * pager.PageSize)?.Take(pager.PageSize).ToList();
            }

            return objects;
        }
    }
}
