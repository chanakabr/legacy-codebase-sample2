using System;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.ModelsValidators
{
    public static class DynamicListFilterValidator
    {
        public static void Validate(this KalturaDynamicListFilter model)
        {
            switch (model)
            {
                case KalturaDynamicListIdInFilter c: c.Validate(); break;
                case KalturaDynamicListSearchFilter c: c.Validate(); break;
                default: throw new NotImplementedException($"ValidateForAdd for {model.objectType} is not implemented");
            }
        }

        private static void Validate(this KalturaDynamicListIdInFilter model)
        {
            if (string.IsNullOrEmpty(model.IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var items = WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(model.IdIn, "idIn", true);
            if (items.Count > 500)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "KalturaDynamicListIdInFilter.idIn", 500);
            }
        }

        private static void Validate(this KalturaDynamicListSearchFilter model)
        {
            if (model.IdEqual.HasValue && string.IsNullOrEmpty(model.ValueEqual))
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "valueEqual", "idEqual");
            }

            if (!string.IsNullOrEmpty(model.ValueEqual) && !model.IdEqual.HasValue)
            {
                throw new BadRequestException(BadRequestException.BOTH_ARGUMENTS_MUST_HAVE_VALUE, "idEqual", "valueEqual");
            }
        }
    }
}
