using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class CategoryVersionValidator
    {
        public static void ValidateForAdd(this KalturaCategoryVersion model)
        {
            if (string.IsNullOrEmpty(model.Name))
            {
                throw new Exceptions.BadRequestException(Exceptions.BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }
        }
    }
}