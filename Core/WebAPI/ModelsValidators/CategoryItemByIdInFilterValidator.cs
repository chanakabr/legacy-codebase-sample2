using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class CategoryItemByIdInFilterValidator
    {
        public static void Validate(this KalturaCategoryItemByIdInFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var orderBy = model.GetDefaultOrderByValue();
            if (model.OrderBy != orderBy)
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "orderBy", orderBy);
            }
        }
    }
}