using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.ModelsValidators
{
    public static class HouseholdLimitationsValidator
    {
        public static void ValidateAssociatedDevices(this KalturaHouseholdLimitations model)
        {
            if (model.AssociatedDeviceFamiliesIdsIn != null && model.DeviceFamiliesLimitations != null)
            {
                var associatedIds = model.AssociatedDeviceFamiliesIdsIn.GetItemsIn<long>(out var failed, true).ThrowIfFailed(failed, () => new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "householdLimitations.AssociatedDeviceFamiliesIdsIn, id"));

                if (model.DeviceFamiliesLimitations.Exists(dfl => !dfl.Id.HasValue))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "DeviceFamiliesLimitations.Id");
                }

                if (model.DeviceFamiliesLimitations.Exists(dfl => !associatedIds.Contains(dfl.Id.Value)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "DeviceFamiliesLimitations.id", "AssociatedDeviceFamiliesIdsIn");
                }
            }
        }

        public static void ValidateUpdate(this KalturaHouseholdLimitations model)
        {
            if (model.AssociatedDeviceFamiliesIdsIn == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CANNOT_BE_EMPTY, "AssociatedDeviceFamiliesIdsIn");
            }

            model.ValidateAssociatedDevices();
        }
    }
}
