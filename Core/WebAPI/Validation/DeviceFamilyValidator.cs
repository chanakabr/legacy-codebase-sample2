using System;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.Validation
{
    public class DeviceFamilyValidator : IDeviceFamilyValidator
    {
        private const int ID_RANGE_LENGTH = 10000;
        private const int FAMILY_COUNT = 50;
        private const int MAX_NAME_LENGTH = 50;

        private static readonly Lazy<DeviceFamilyValidator> Lazy = new Lazy<DeviceFamilyValidator>(() => new DeviceFamilyValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceFamilyValidator Instance => Lazy.Value;

        public void ValidateToAdd(long groupId, KalturaDeviceFamily deviceFamily)
        {
            ValidateId(groupId, deviceFamily.Id);
            ValidateName(deviceFamily.Name);
        }

        public void ValidateToUpdate(long groupId, KalturaDeviceFamily deviceFamily)
        {
            ValidateId(groupId, deviceFamily.Id);
            if (deviceFamily.Name != null)
            {
                ValidateName(deviceFamily.Name);
            }
        }

        private void ValidateId(long groupId, long? id)
        {
            var minId = groupId * ID_RANGE_LENGTH;
            var maxId = minId + FAMILY_COUNT - 1;

            if (!id.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(KalturaDeviceFamily.Id));
            }

            if (!Utils.Utils.IsBetween(id.Value, minId, maxId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE, nameof(KalturaDeviceFamily.Id), $"{minId},{maxId}");
            }
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(KalturaDeviceFamily.Name));
            }

            if (name.Length > MAX_NAME_LENGTH)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, nameof(KalturaDeviceFamily.Name), MAX_NAME_LENGTH);
            }
        }
    }
}