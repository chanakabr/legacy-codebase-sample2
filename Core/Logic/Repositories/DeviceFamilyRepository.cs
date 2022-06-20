using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using Microsoft.Extensions.Logging;
using ODBCWrapper;
using Phx.Lib.Log;

namespace ApiLogic.Repositories
{
    public class DeviceFamilyRepository : IDeviceFamilyRepository
    {
        private static readonly Lazy<DeviceFamilyRepository> LazyInstance = new Lazy<DeviceFamilyRepository>(
            () => new DeviceFamilyRepository(DeviceFamilyDal.Instance, LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IDeviceFamilyDal _deviceFamilyDal;
        private readonly ILayeredCache _cache;
        private readonly ILogger _logger;

        public static IDeviceFamilyRepository Instance => LazyInstance.Value;
        
        public DeviceFamilyRepository(IDeviceFamilyDal deviceFamilyDal, ILayeredCache cache)
            : this(deviceFamilyDal, cache, new KLogger(nameof(DeviceFamilyRepository)))
        {
        }

        public DeviceFamilyRepository(IDeviceFamilyDal deviceFamilyDal, ILayeredCache cache, ILogger logger)
        {
            _deviceFamilyDal = deviceFamilyDal;
            _cache = cache;
            _logger = logger;
        }

        public GenericResponse<DeviceFamily> Add(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            GenericResponse<DeviceFamily> response;
            try
            {
                var dataSet = _deviceFamilyDal.Add(groupId, deviceFamily, updaterId);
                response = CreateDeviceFamily(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Add)}: {e.Message}.");
                response = new GenericResponse<DeviceFamily>();
            }

            return response;
        }

        public GenericResponse<DeviceFamily> Update(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            GenericResponse<DeviceFamily> response;
            try
            {
                var dataSet = _deviceFamilyDal.Update(groupId, deviceFamily, updaterId);
                response = CreateDeviceFamily(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Update)}: {e.Message}.");
                response = new GenericResponse<DeviceFamily>();
            }

            return response;
        }

        public GenericResponse<DeviceFamily> GetByDeviceBrandId(long groupId, long deviceBrandId)
        {
            GenericResponse<DeviceFamily> response;
            try
            {
                var dataSet = _deviceFamilyDal.GetByDeviceBrandId(groupId, deviceBrandId);
                response = CreateDeviceFamily(dataSet);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(GetByDeviceBrandId)}: {e.Message}.");
                response = new GenericResponse<DeviceFamily>();
            }

            return response;
        }

        public GenericListResponse<DeviceFamily> List(long groupId)
        {
            GenericListResponse<DeviceFamily> response;
            try
            {
                List<DeviceFamily> deviceFamilies = null;
                var deviceFamiliesKey = LayeredCacheKeys.GetDeviceFamiliesKey(groupId);
                var invalidationKeys = new List<string> { LayeredCacheKeys.GetDeviceFamiliesInvalidationKey(groupId) };
                var cacheResult = _cache.Get(
                    deviceFamiliesKey,
                    ref deviceFamilies,
                    GetDeviceFamilies,
                    new Dictionary<string, object> { { "groupId", groupId } },
                    (int)groupId,
                    LayeredCacheConfigNames.GET_DEVICE_FAMILIES_CACHE_CONFIG_NAME,
                    invalidationKeys);

                if (cacheResult)
                {
                    response = new GenericListResponse<DeviceFamily>(Status.Ok, deviceFamilies);
                }
                else
                {
                    _logger.LogError($"{nameof(List)} - Failed to get device families: {nameof(groupId)}={groupId}.");

                    response = new GenericListResponse<DeviceFamily>();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(List)}: {e.Message}.");

                response = new GenericListResponse<DeviceFamily>();
            }

            return response;
        }

        private Tuple<List<DeviceFamily>, bool> GetDeviceFamilies(Dictionary<string, object> funcParams)
        {
            List<DeviceFamily> deviceFamilies = null;
            var result = false;
            try
            {
                var groupId = (long)funcParams["groupId"];
                var dataSet = _deviceFamilyDal.List(groupId);
                deviceFamilies = CreateDeviceFamilies(dataSet);
                result = deviceFamilies != null;
            }
            catch (Exception e)
            {
                var parameters = funcParams != null
                    ? string.Join(";", funcParams.Select(x => $"{{key: {x.Key}, value:{x.Value}}}"))
                    : string.Empty;
                _logger.LogError(e, $"Error while executing {nameof(GetDeviceFamilies)}({parameters}): {e.Message}.");
            }

            return new Tuple<List<DeviceFamily>, bool>(deviceFamilies, result);
        }

        private List<DeviceFamily> CreateDeviceFamilies(DataSet dataSet)
        {
            List<DeviceFamily> deviceFamilies = null;
            if (dataSet != null && dataSet.Tables.Count == 1)
            {
                deviceFamilies = new List<DeviceFamily>(dataSet.Tables[0].Rows.Count);
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var deviceFamily = CreateDeviceFamily(row);
                    deviceFamilies.Add(deviceFamily);
                }
            }

            return deviceFamilies;
        }

        private GenericResponse<DeviceFamily> CreateDeviceFamily(DataSet dataSet)
        {
            GenericResponse<DeviceFamily> response;
            if (dataSet == null || dataSet.Tables.Count != 1 || dataSet.Tables[0].Rows.Count != 1)
            {
                response = new GenericResponse<DeviceFamily>();
            }
            else
            {
                var deviceFamily = CreateDeviceFamily(dataSet.Tables[0].Rows[0]);
                response = new GenericResponse<DeviceFamily>(Status.Ok, deviceFamily);
            }

            return response;
        }

        private static DeviceFamily CreateDeviceFamily(DataRow row)
        {
            var id = Utils.GetIntSafeVal(row, "ID");
            var name = Utils.GetSafeStr(row, "NAME");
            var deviceFamily = new DeviceFamily(id, name);

            return deviceFamily;
        }

        private void InvalidateCache(long groupId)
        {
            var invalidationKey = LayeredCacheKeys.GetDeviceFamiliesInvalidationKey(groupId);
            var result = _cache.SetInvalidationKey(invalidationKey);
            if (!result)
            {
                _logger.LogError("Failed to set invalidation key for device families. key = {0}.", invalidationKey);
            }
        }
    }
}