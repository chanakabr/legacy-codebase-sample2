using System.Collections.Generic;
using System.Linq;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class HouseholdLimitationsMapper
    {
        const int NO_LIMITATION_VALUE = -1;

        public static IEnumerable<KalturaHouseholdDeviceFamilyLimitations> AssociatedDeviceFamiliesToLimitations(this KalturaHouseholdLimitations model)
        {
            var associatedIds = model.AssociatedDeviceFamiliesIdsIn.GetItemsIn<long>(out var failed, true)
                .ThrowIfFailed(failed, () => new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "householdLimitations.AssociatedDeviceFamiliesIdsIn, id"));

            return associatedIds.Where(ai => !model.DeviceFamiliesLimitations.Any(dfl => dfl.Id == ai)).
                                                                                         Select(x =>
                                                                                         new KalturaHouseholdDeviceFamilyLimitations()
                                                                                         {
                                                                                             Id = x,
                                                                                             ConcurrentLimit = NO_LIMITATION_VALUE,
                                                                                             DeviceLimit = NO_LIMITATION_VALUE,
                                                                                             Frequency = NO_LIMITATION_VALUE
                                                                                         });
        }
    }
}
