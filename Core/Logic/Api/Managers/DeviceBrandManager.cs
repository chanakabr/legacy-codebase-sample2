using System;
using System.Linq;
using System.Threading;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public class DeviceBrandManager : IDeviceBrandManager
    {
        private static readonly Lazy<DeviceBrandManager> Lazy = new Lazy<DeviceBrandManager>(
            () => new DeviceBrandManager(DeviceFamilyRepository.Instance, DeviceBrandRepository.Instance),
            LazyThreadSafetyMode.None);

        private readonly IDeviceFamilyRepository _deviceFamilyRepository;
        private readonly IDeviceBrandRepository _deviceBrandRepository;

        public static DeviceBrandManager Instance => Lazy.Value;

        public DeviceBrandManager(IDeviceFamilyRepository deviceFamilyRepository, IDeviceBrandRepository deviceBrandRepository)
        {
            _deviceFamilyRepository = deviceFamilyRepository;
            _deviceBrandRepository = deviceBrandRepository;
        }

        public GenericResponse<DeviceBrand> Add(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            GenericResponse<DeviceBrand> response;

            var familyListResponse = _deviceFamilyRepository.List(groupId);
            if (familyListResponse.IsOkStatusCode())
            {
                if (familyListResponse.Objects.Any(x => x.Id == deviceBrand.DeviceFamilyId))
                {
                    var brandListResponse = _deviceBrandRepository.List(groupId);
                    if (brandListResponse.IsOkStatusCode())
                    {
                        response = brandListResponse.Objects.Any(x => x.Id == deviceBrand.Id)
                            ? new GenericResponse<DeviceBrand>(eResponseStatus.DeviceBrandIdAlreadyInUse)
                            : _deviceBrandRepository.Add(groupId, deviceBrand, updaterId);
                    }
                    else
                    {
                        response = new GenericResponse<DeviceBrand>(brandListResponse.Status);
                    }
                }
                else
                {
                    response = new GenericResponse<DeviceBrand>(eResponseStatus.DeviceFamilyDoesNotExist);
                }
            }
            else
            {
                response = new GenericResponse<DeviceBrand>(familyListResponse.Status);
            }

            return response;
        }

        public GenericResponse<DeviceBrand> Update(long groupId, DeviceBrand deviceBrand, long updaterId)
        {
            GenericResponse<DeviceBrand> response;

            var familyListResponse = _deviceFamilyRepository.List(groupId);
            if (familyListResponse.IsOkStatusCode())
            {
                if (deviceBrand.DeviceFamilyId == 0
                    || familyListResponse.Objects.Any(x => x.Id == deviceBrand.DeviceFamilyId))
                {
                    var brandListResponse = _deviceBrandRepository.List(groupId);
                    if (brandListResponse.IsOkStatusCode())
                    {
                        response = brandListResponse.Objects.All(x => x.Id != deviceBrand.Id)
                            ? new GenericResponse<DeviceBrand>(eResponseStatus.DeviceBrandDoesNotExist)
                            : _deviceBrandRepository.Update(groupId, deviceBrand, updaterId);
                    }
                    else
                    {
                        response = new GenericResponse<DeviceBrand>(brandListResponse.Status);
                    }
                }
                else
                {
                    response = new GenericResponse<DeviceBrand>(eResponseStatus.DeviceFamilyDoesNotExist);
                }
            }
            else
            {
                response = new GenericResponse<DeviceBrand>(familyListResponse.Status);
            }

            return response;
        }

        public GenericListResponse<DeviceBrand> List(long groupId, long? id, long? deviceFamilyId, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize)
        {
            var response = _deviceBrandRepository.List(groupId);
            if (response.IsOkStatusCode())
            {
                var predicate = GetFilterDeviceBrandsPredicate(id, deviceFamilyId, name, isSystem);
                var filteredResults = response.Objects
                    .Where(predicate)
                    .ToList();
                var pagedResults = filteredResults
                    .OrderBy(x => x.Id * (orderByIdAsc ? 1 : -1))
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);

                response = new GenericListResponse<DeviceBrand>(Status.Ok, pagedResults, filteredResults.Count);
            }

            return response;
        }

        private static Func<DeviceBrand, bool> GetFilterDeviceBrandsPredicate(long? id, long? deviceFamilyId, string name, bool? isSystem)
        {
            Func<DeviceBrand, bool> predicate;
            if (id.HasValue)
            {
                predicate = x => x.Id == id.Value;
            }
            else if (deviceFamilyId.HasValue)
            {
                predicate = x => x.DeviceFamilyId == deviceFamilyId.Value;
            }
            else if (!string.IsNullOrEmpty(name))
            {
                predicate = x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            }
            else if (isSystem.HasValue)
            {
                predicate = x => isSystem == (x.Id >= 0 && x.Id < 1000);
            }
            else
            {
                predicate = x => true;
            }

            return predicate;
        }
    }
}