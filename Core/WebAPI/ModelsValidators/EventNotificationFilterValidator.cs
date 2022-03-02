using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class EventNotificationFilterValidator
    {
        public static void Validate(this KalturaEventNotificationFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.IdEqual) && filter.ObjectIdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "idEqual", "objectIdEqual");
            }

            if (filter.ObjectIdEqual.HasValue && string.IsNullOrEmpty(filter.EventObjectTypeEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "eventObjectTypeEqual");
            }

            if (filter.ObjectIdEqual <= 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "ObjectIdEqual");
            }
        }
    }
}
