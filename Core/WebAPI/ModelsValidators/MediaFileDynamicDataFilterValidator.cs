using WebAPI.Exceptions;
using WebAPI.Models.API;

namespace WebAPI.ModelsValidators
{
    public static class MediaFileDynamicDataFilterValidator
    {
        internal static void Validate(this KalturaMediaFileDynamicDataFilter model, string argumentName)
        {
            if (!string.IsNullOrEmpty(model.IdIn) && !model.MediaFileTypeId.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    $"{nameof(KalturaMediaFileDynamicDataFilter)}.{nameof(KalturaMediaFileDynamicDataFilter.IdIn)}",
                    $"{nameof(KalturaMediaFileDynamicDataFilter)}.{nameof(KalturaMediaFileDynamicDataFilter.MediaFileTypeId)}");
            }

            if (!string.IsNullOrEmpty(model.ValueEqual) && !string.IsNullOrEmpty(model.ValueStartsWith))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER,
                    $"{nameof(KalturaMediaFileDynamicDataFilter)}.{nameof(KalturaMediaFileDynamicDataFilter.ValueEqual)}",
                    $"{nameof(KalturaMediaFileDynamicDataFilter)}.{nameof(KalturaMediaFileDynamicDataFilter.ValueStartsWith)}");
            }
        }
    }
}
