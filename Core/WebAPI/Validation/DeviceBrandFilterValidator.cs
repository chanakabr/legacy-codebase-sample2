using System;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public class DeviceBrandFilterValidator : IDeviceBrandFilterValidator
    {
        private static readonly Lazy<DeviceBrandFilterValidator> Lazy = new Lazy<DeviceBrandFilterValidator>(() => new DeviceBrandFilterValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceBrandFilterValidator Instance => Lazy.Value;

        public void Validate(KalturaDeviceBrandFilter filter, string argumentName)
        {
            var conditionCount = 0;
            if (filter.IdEqual.HasValue)
            {
                conditionCount++;
            }

            if (!string.IsNullOrEmpty(filter.NameEqual))
            {
                conditionCount++;
            }

            if (filter.DeviceFamilyIdEqual.HasValue)
            {
                conditionCount++;
            }

            if (filter.TypeEqual.HasValue)
            {
                conditionCount++;
            }

            if (conditionCount > 1)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, argumentName);
            }
        }
    }
}