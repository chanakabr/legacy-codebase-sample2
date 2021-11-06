using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class CategoryItemFilterValidator
    {
        public static void Validate(this KalturaCategoryItemFilter filter)
        {
            switch (filter)
            {
                case KalturaCategoryItemByIdInFilter c: c.Validate(); break;
                case KalturaCategoryItemAncestorsFilter c: c.Validate(); break;
            }
        }
    }
}