using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class CategoryItemAncestorsFilterValidator
    {
        public static void Validate(this KalturaCategoryItemAncestorsFilter model)
        {
            var orderBy = model.GetDefaultOrderByValue();
            if (model.OrderBy != orderBy)
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "orderBy", orderBy);
            }
        }
    }
}