using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class RecordingFilterValidator
    {
        public static void Validate(this KalturaRecordingFilter filter)
        {
            if (!string.IsNullOrEmpty(filter.ExternalRecordingIdIn) && !string.IsNullOrEmpty(filter.StatusIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaRecordingFilter.externalRecordingIdIn", "KalturaRecordingFilter.statusIn");
            }
        }
    }
}
