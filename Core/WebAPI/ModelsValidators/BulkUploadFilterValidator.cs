using System;
using TVinciShared;
using WebAPI.Exceptions;
using WebAPI.Models.Upload;

namespace WebAPI.ModelsValidators
{
    public static class BulkUploadFilterValidator
    {
        public const double MIN_RECORD_DAYS_TO_WATCH = 60;

        public static void Validate(this KalturaBulkUploadFilter model)
        {
            if (string.IsNullOrEmpty(model.BulkObjectTypeEqual))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkObjectTypeEqual");
            }

            if (model.CreateDateGreaterThanOrEqual.HasValue)
            {
                var createDate = DateUtils.UtcUnixTimestampSecondsToDateTime(model.CreateDateGreaterThanOrEqual.Value);
                if (createDate.AddDays(MIN_RECORD_DAYS_TO_WATCH) < DateTime.UtcNow)
                {
                    var minCreateDate = DateTime.UtcNow.AddDays(-MIN_RECORD_DAYS_TO_WATCH).ToUtcUnixTimestampSeconds();
                    throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "createDateGreaterThanOrEqual", minCreateDate);
                }
            }
        }
    }
}
