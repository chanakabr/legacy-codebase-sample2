using WebAPI.Exceptions;
using WebAPI.Models.Domains;

namespace WebAPI.ModelsValidators
{
    public static class HouseholdFilterValidator
    {
        internal static void Validate(this KalturaHouseholdFilter model)
        {
            if (string.IsNullOrEmpty(model.ExternalIdEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "externalIdEqual");
            }
        }
    }
}
