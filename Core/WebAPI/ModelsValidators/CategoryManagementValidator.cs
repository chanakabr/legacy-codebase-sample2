using WebAPI.Exceptions;
using WebAPI.Models.Partner;

namespace WebAPI.ModelsValidators
{
    public static class CategoryManagementValidator
    {
        public static void ValidateForUpdate(this KalturaCategoryManagement model)
        {
            if (model.DeviceFamilyToCategoryTree != null && !model.DefaultCategoryTreeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "defaultTreeId");
            }
        }
    }
}
