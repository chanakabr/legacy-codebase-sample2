using System;
using TVinciShared;
using WebAPI.Models.Upload;
using WebAPI.ModelsValidators;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class BulkUploadFilterMapper
    {
        public static DateTime GetCreateDate(this KalturaBulkUploadFilter model)
        {
            DateTime createDate;
            if (model.CreateDateGreaterThanOrEqual.HasValue)
            {
                createDate = DateUtils.UtcUnixTimestampSecondsToDateTime(model.CreateDateGreaterThanOrEqual.Value);
            }
            else
            {
                createDate = DateTime.UtcNow.AddDays(-BulkUploadFilterValidator.MIN_RECORD_DAYS_TO_WATCH);
            }

            return createDate;
        }
    }
}
