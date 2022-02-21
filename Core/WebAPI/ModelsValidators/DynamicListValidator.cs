using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.ModelsValidators
{
    public static class DynamicListValidator
    {
        public static void ValidateForAdd(this KalturaDynamicList model)
        {
            if (string.IsNullOrEmpty(model.Name) || string.IsNullOrWhiteSpace(model.Name))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }
    }
}
