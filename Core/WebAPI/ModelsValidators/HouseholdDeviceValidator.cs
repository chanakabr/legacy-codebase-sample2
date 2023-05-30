using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using static WebAPI.Exceptions.BadRequestException;

namespace WebAPI.ModelsValidators
{

    public static class KalturaHouseholdDeviceValidator
    {
        public static void Validate(this KalturaHouseholdDevice device)
        {
            if (device.Udid.IsNullOrEmptyOrWhiteSpace())
            {
                throw new BadRequestException(ARGUMENT_CANNOT_BE_EMPTY, "udid");
            }

            device.DynamicData.Validate();
        }
    }
}
