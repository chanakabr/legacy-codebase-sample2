using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class LanguageFilter
    {
        public static void Validate(this KalturaLanguageFilter model)
        {
            if (!string.IsNullOrEmpty(model.CodeIn) && model.ExcludePartner.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    "KalturaLanguageFilter.codeIn", "KalturaLanguageFilter.excludePartner");
            }
        }
    }
}