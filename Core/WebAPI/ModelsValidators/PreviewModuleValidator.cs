using WebAPI.Exceptions;
using WebAPI.Models.Pricing;

namespace WebAPI.ModelsValidators
{
    public static class PreviewModuleValidator
    {
        public static void ValidateForAdd(this KalturaPreviewModule model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            if (!model.NonRenewablePeriod.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "nonRenewablePeriod");
            if (!model.LifeCycle.HasValue)
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "lifeCycle");
        }
    }
}