using System.Collections.Generic;
using ApiObjects;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class TagFilterValidator
    {
        public static void Validate(this KalturaTagFilter model)
        {
            if (!string.IsNullOrEmpty(model.IdIn))
            {
                if (!string.IsNullOrEmpty(model.TagEqual))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaTagFilter.idIn", "KalturaTagFilter.tagEqual");
                }

                if (!string.IsNullOrEmpty(model.TagStartsWith))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaTagFilter.idIn", "KalturaTagFilter.tagStartsWith");
                }
            }

            if (string.IsNullOrEmpty(model.IdIn) && (!model.TypeEqual.HasValue || model.TypeEqual <= 0))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTagFilter.typeEqual");
            }

            if (!string.IsNullOrEmpty(model.TagEqual) && !string.IsNullOrEmpty(model.TagStartsWith))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaTagFilter.tagEqual", "KalturaTagFilter.tagStartsWith");
            }

            if (!string.IsNullOrEmpty(model.LanguageEqual))
            {
                HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();
                if (!groupLanguageCodes.Contains(model.LanguageEqual))
                {
                    throw new BadRequestException(BadRequestException.GROUP_DOES_NOT_CONTAIN_LANGUAGE, model.LanguageEqual);
                }
            }
        }
    }
}