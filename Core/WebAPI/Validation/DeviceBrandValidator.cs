using System;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.Validation
{
    public class DeviceBrandValidator : IDeviceBrandValidator
    {
        private const int ID_RANGE_LENGTH = 10000;
        private const int FAMILY_COUNT = 50;
        private const int BRAND_COUNT = 1000;
        private const int MAX_NAME_LENGTH = 50;

        private static readonly Lazy<DeviceBrandValidator> Lazy = new Lazy<DeviceBrandValidator>(() => new DeviceBrandValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceBrandValidator Instance => Lazy.Value;

        public void ValidateToAdd(long groupId, KalturaDeviceBrand deviceBrand)
        {
            ValidateId(groupId, deviceBrand.Id);
            ValidateDeviceFamilyId(groupId, deviceBrand.DeviceFamilyId);
            ValidateName(deviceBrand.Name);
        }

        public void ValidateToUpdate(long groupId, KalturaDeviceBrand deviceBrand)
        {
            ValidateId(groupId, deviceBrand.Id);
            if (deviceBrand.DeviceFamilyId.HasValue)
            {
                ValidateDeviceFamilyId(groupId, deviceBrand.DeviceFamilyId);
            }

            if (deviceBrand.Name != null)
            {
                ValidateName(deviceBrand.Name);
            }
        }

        private void ValidateId(long groupId, long? id)
        {
            var minId = groupId * ID_RANGE_LENGTH;
            var maxId = minId + BRAND_COUNT - 1;

            if (!id.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(KalturaDeviceBrand.Id));
            }

            if (!Utils.Utils.IsBetween(id.Value, minId, maxId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE, nameof(KalturaDeviceBrand.Id), $"{minId},{maxId}");
            }
        }

        private void ValidateDeviceFamilyId(long groupId, long? deviceFamilyId)
        {
            const long minSystemId = 1;
            const long maxSystemId = ID_RANGE_LENGTH - 1;
            var minCustomId = groupId * ID_RANGE_LENGTH;
            var maxCustomId = minCustomId + FAMILY_COUNT - 1;

            if (!deviceFamilyId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(KalturaDeviceBrand.DeviceFamilyId));
            }

            if (!(Utils.Utils.IsBetween(deviceFamilyId.Value, minSystemId, maxSystemId) || Utils.Utils.IsBetween(deviceFamilyId.Value, minCustomId, maxCustomId)))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_NOT_IN_PREDEFINED_RANGE, nameof(KalturaDeviceBrand.DeviceFamilyId), $"{minSystemId},{maxSystemId}] or [{minCustomId},{maxCustomId}");
            }
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, nameof(KalturaDeviceBrand.Name));
            }

            if (name.Length > MAX_NAME_LENGTH)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_LENGTH_CROSSED, nameof(KalturaDeviceBrand.Name), MAX_NAME_LENGTH);
            }
        }
    }
}