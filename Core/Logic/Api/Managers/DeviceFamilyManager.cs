using System;
using System.Linq;
using System.Threading;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Managers
{
    public class DeviceFamilyManager : IDeviceFamilyManager
    {
        private static readonly Lazy<DeviceFamilyManager> Lazy = new Lazy<DeviceFamilyManager>(
            () => new DeviceFamilyManager(DeviceFamilyRepository.Instance),
            LazyThreadSafetyMode.None);

        private readonly IDeviceFamilyRepository _deviceFamilyRepository;

        public static DeviceFamilyManager Instance => Lazy.Value;

        public DeviceFamilyManager(IDeviceFamilyRepository deviceFamilyRepository)
        {
            _deviceFamilyRepository = deviceFamilyRepository;
        }

        public GenericResponse<DeviceFamily> Add(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            GenericResponse<DeviceFamily> response;

            var listResponse = _deviceFamilyRepository.List(groupId);
            if (listResponse.IsOkStatusCode())
            {
                response = listResponse.Objects.Any(x => x.Id == deviceFamily.Id)
                    ? new GenericResponse<DeviceFamily>(eResponseStatus.DeviceFamilyIdAlreadyInUse)
                    : _deviceFamilyRepository.Add(groupId, deviceFamily, updaterId);
            }
            else
            {
                response = new GenericResponse<DeviceFamily>(listResponse.Status);
            }

            return response;
        }

        public GenericResponse<DeviceFamily> Update(long groupId, DeviceFamily deviceFamily, long updaterId)
        {
            GenericResponse<DeviceFamily> response;

            var listResponse = _deviceFamilyRepository.List(groupId);
            if (listResponse.IsOkStatusCode())
            {
                response = listResponse.Objects.All(x => x.Id != deviceFamily.Id)
                    ? new GenericResponse<DeviceFamily>(eResponseStatus.DeviceFamilyDoesNotExist)
                    : _deviceFamilyRepository.Update(groupId, deviceFamily, updaterId);
            }
            else
            {
                response = new GenericResponse<DeviceFamily>(listResponse.Status);
            }

            return response;
        }

        public GenericListResponse<DeviceFamily> List(long groupId, long? id, string name, bool? isSystem, bool orderByIdAsc, int pageIndex, int pageSize)
        {
            var response = _deviceFamilyRepository.List(groupId);
            if (response.IsOkStatusCode())
            {
                var predicate = GetFilterDeviceFamiliesPredicate(id, name, isSystem);
                var filteredResults = response.Objects
                    .Where(predicate)
                    .ToList();
                var pagedResults = filteredResults
                    .OrderBy(x => x.Id * (orderByIdAsc ? 1 : -1))
                    .Skip(pageIndex * pageSize)
                    .Take(pageSize);

                response = new GenericListResponse<DeviceFamily>(Status.Ok, pagedResults, filteredResults.Count);
            }

            return response;
        }

        private static Func<DeviceFamily, bool> GetFilterDeviceFamiliesPredicate(long? id, string name, bool? isSystem)
        {
            Func<DeviceFamily, bool> predicate;
            if (id.HasValue)
            {
                predicate = x => x.Id == id.Value;
            }
            else if (!string.IsNullOrEmpty(name))
            {
                predicate = x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase);
            }
            else if (isSystem.HasValue)
            {
                predicate = x => isSystem == (x.Id >= 0 && x.Id < 50);
            }
            else
            {
                predicate = x => true;
            }

            return predicate;
        }
    }
}