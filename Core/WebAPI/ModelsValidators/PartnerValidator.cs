using WebAPI.Exceptions;
using WebAPI.Models.Users;

namespace WebAPI.ModelsValidators
{
    public static class PartnerValidator
    {
        internal static void ValidateForAdd(this KalturaPartner model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }
    }

    public static class PartnerSetupValidator
    {
        public static void ValidateForAdd(this KalturaPartnerSetup model)
        {
            if (string.IsNullOrEmpty(model.AdminUsername))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "AdminUsername");
            }
            if (string.IsNullOrEmpty(model.AdminPassword))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "AdminPassword");
            }
            if (model.BasePartnerConfiguration == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaBasePartnerConfiguration");
            }
        }
    }
}
