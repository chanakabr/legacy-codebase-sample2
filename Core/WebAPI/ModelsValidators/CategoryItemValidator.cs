using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class CategoryItemValidator
    {
        public static void ValidateForAdd(this KalturaCategoryItem model)
        {
            if (model.Name == null)
            {
                throw new Exceptions.BadRequestException(Exceptions.BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "multilingualName");
            }

            model.Name.Validate("multilingualName");

            if (model.DynamicData?.Count > 0)
            {
                var isEmptyOrNullKeyExist = model.DynamicData.Any(x => string.IsNullOrEmpty(x.Key));
                if (isEmptyOrNullKeyExist)
                {
                    throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, "dynamicData");
                }
            }

            if (model.StartDateInSeconds.HasValue && model.EndDateInSeconds.HasValue && model.StartDateInSeconds >= model.EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }

            if (model.UnifiedChannels?.Count > 0)
            {
                foreach (var unifiedChannels in model.UnifiedChannels)
                {
                    unifiedChannels.Validate();
                }
            }
        }

        public static void ValidateForUpdate(this KalturaCategoryItem model)
        {
            if (model.Name != null)
            {
                model.Name.Validate("multilingualName");
            }

            if (model.DynamicData?.Count > 0)
            {
                var isEmptyOrNullKeyExist = model.DynamicData.Any(x => string.IsNullOrEmpty(x.Key));
                if (isEmptyOrNullKeyExist)
                {
                    throw new BadRequestException(BadRequestException.KEY_CANNOT_BE_EMPTY_OR_NULL, "dynamicData");
                }
            }

            if (model.StartDateInSeconds.HasValue && model.EndDateInSeconds.HasValue && model.StartDateInSeconds >= model.EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }

            // fill empty feilds
            if (model.NullableProperties != null && model.NullableProperties.Contains("unifiedchannels"))
            {
                model.UnifiedChannels = new List<KalturaUnifiedChannel>();
            }
        }
    }
}