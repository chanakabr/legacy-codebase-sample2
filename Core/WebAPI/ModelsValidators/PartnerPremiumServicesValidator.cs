using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class PartnerPremiumServicesValidator
    {
        public static void ValidateForUpdate(this KalturaPartnerPremiumServices model)
        {
            if (model.PremiumServices == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "premiumServices");
            }

            var duplicates = model.PremiumServices.GroupBy(x => x.Id).Count(t => t.Count() >= 2);

            if (duplicates >= 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "premiumServices");
            }
        }
    }
}