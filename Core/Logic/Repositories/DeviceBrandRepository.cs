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
using Phx.Lib.Log;
using Utils = ODBCWrapper.Utils;

namespace ApiLogic.Repositories
{
    public class DeviceBrandRepository : IDeviceBrandRepository
    {
        private static readonly Lazy<DeviceBrandRepository> LazyInstance = new Lazy<DeviceBrandRepository>(
            () => new DeviceBrandRepository(DeviceBrandDal.Instance, LayeredCache.Instance, new KLogger(nameof(DeviceBrandRepository))),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IDeviceBrandDal _deviceBrandDal;
        private readonly ILayeredCache _cache;
        private readonly ILogger _logger;

        public static IDeviceBrandRepository Instance => LazyInstance.Value;

        public DeviceBrandRepository(IDeviceBrandDal deviceBrandDal, ILayeredCache cache, ILogger logger)
        {
            _deviceBrandDal = deviceBrandDal;
            _cache = cache;
            _logger = logger;
        }

        public GenericResponse<DeviceBrand> Add(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            GenericResponse<DeviceBrand> response;
            try
            {
                var dataSet = _deviceBrandDal.Add(groupId, deviceBrand, updaterId);
                response = CreateDeviceBrand(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Add)}: {e.Message}.");
                response = new GenericResponse<DeviceBrand>();
            }

            return response;
        }

        public GenericResponse<DeviceBrand> Update(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            GenericResponse<DeviceBrand> response;
            try
            {
                var dataSet = _deviceBrandDal.Update(groupId, deviceBrand, updaterId);
                response = CreateDeviceBrand(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Update)}: {e.Message}.");
                response = new GenericResponse<DeviceBrand>();
            }

            return response;
        }

        public GenericListResponse<DeviceBrand> List(long groupId)
        {
            GenericListResponse<DeviceBrand> response;
            try
            {
                List<DeviceBrand> deviceBrands = null;
                var deviceBrandsKey = LayeredCacheKeys.GetDeviceBrandsKey(groupId);
                var invalidationKeys = new List<string> { LayeredCacheKeys.GetDeviceBrandsInvalidationKey(groupId) };
                var cacheResult = _cache.Get(
                    deviceBrandsKey,
                    ref deviceBrands,
                    GetDeviceBrands,
                    new Dictionary<string, object> { { "groupId", groupId } },
                    (int)groupId,
                    LayeredCacheConfigNames.GET_DEVICE_BRANDS_CACHE_CONFIG_NAME,
                    invalidationKeys);

                if (cacheResult)
                {
                    response = new GenericListResponse<DeviceBrand>(Status.Ok, deviceBrands);
                }
                else
                {
                    _logger.LogError($"{nameof(List)} - Failed to get device brands: {nameof(groupId)}={groupId}.");

                    response = new GenericListResponse<DeviceBrand>();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(List)}: {e.Message}.");

                response = new GenericListResponse<DeviceBrand>();
            }

            return response;
        }

        private Tuple<List<DeviceBrand>, bool> GetDeviceBrands(Dictionary<string, object> funcParams)
        {
            List<DeviceBrand> deviceBrands = null;
            var result = false;
            try
            {
                var groupId = (long)funcParams["groupId"];
                var dataSet = _deviceBrandDal.List(groupId);
                deviceBrands = CreateDeviceBrands(dataSet);
                result = deviceBrands != null;
            }
            catch (Exception e)
            {
                var parameters = funcParams != null
                    ? string.Join(";", funcParams.Select(x => $"{{key: {x.Key}, value:{x.Value}}}"))
                    : string.Empty;
                _logger.LogError(e, $"Error while executing {nameof(GetDeviceBrands)}({parameters}): {e.Message}.");
            }

            return new Tuple<List<DeviceBrand>, bool>(deviceBrands, result);
        }

        private List<DeviceBrand> CreateDeviceBrands(DataSet dataSet)
        {
            List<DeviceBrand> deviceBrands = null;
            if (dataSet != null && dataSet.Tables.Count == 1)
            {
                deviceBrands = new List<DeviceBrand>(dataSet.Tables[0].Rows.Count);
                foreach (DataRow row in dataSet.Tables[0].Rows)
                {
                    var deviceBrand = CreateDeviceBrand(row);
                    deviceBrands.Add(deviceBrand);
                }
            }

            return deviceBrands;
        }

        private GenericResponse<DeviceBrand> CreateDeviceBrand(DataSet dataSet)
        {
            GenericResponse<DeviceBrand> response;
            if (dataSet == null || dataSet.Tables.Count != 1 || dataSet.Tables[0].Rows.Count != 1)
            {
                response = new GenericResponse<DeviceBrand>();
            }
            else
            {
                var deviceBrand = CreateDeviceBrand(dataSet.Tables[0].Rows[0]);
                response = new GenericResponse<DeviceBrand>(Status.Ok, deviceBrand);
            }

            return response;
        }

        private static DeviceBrand CreateDeviceBrand(DataRow row)
        {
            var id = Utils.GetIntSafeVal(row, "ID");
            var name = Utils.GetSafeStr(row, "Name");
            var deviceFamilyId = Utils.GetIntSafeVal(row, "Device_Family_ID");
            var deviceBrand = new DeviceBrand(id, name, deviceFamilyId);

            return deviceBrand;
        }

        private void InvalidateCache(long groupId)
        {
            var invalidationKey = LayeredCacheKeys.GetDeviceBrandsInvalidationKey(groupId);
            var result = _cache.SetInvalidationKey(invalidationKey);
            if (!result)
            {
                _logger.LogError("Failed to set invalidation key for device brands. key = {0}.", invalidationKey);
            }
        }
    }
}