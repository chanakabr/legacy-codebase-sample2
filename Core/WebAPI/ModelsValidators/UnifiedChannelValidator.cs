using System;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class UnifiedChannelValidator
    {
        public static void Validate(this KalturaUnifiedChannel model)
        {
            switch (model)
            {
                case KalturaUnifiedChannelInfo c: c.Validate(); break;
            }
        }
        
        public static void Validate(this KalturaUnifiedChannelInfo model)
        {
            if (model.StartDateInSeconds.HasValue && model.EndDateInSeconds.HasValue && model.StartDateInSeconds >= model.EndDateInSeconds)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "startDateInSeconds", "endDateInSeconds");
            }
        }
    }
}