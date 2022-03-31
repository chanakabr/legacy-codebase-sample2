using System;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.Validation
{
    public class DeviceFamilyFilterValidator : IDeviceFamilyFilterValidator
    {
        private static readonly Lazy<DeviceFamilyFilterValidator> Lazy = new Lazy<DeviceFamilyFilterValidator>(() => new DeviceFamilyFilterValidator(), LazyThreadSafetyMode.PublicationOnly);

        public static IDeviceFamilyFilterValidator Instance => Lazy.Value;

        public void Validate(KalturaDeviceFamilyFilter filter, string argumentName)
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