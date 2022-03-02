using System;
using WebAPI.Exceptions;
using WebAPI.Models.General;
using WebAPI.Models.Upload;

namespace WebAPI.ModelsValidators
{
    public static class BulkUploadJobDataValidator
    {
        /// <summary>
        /// Validate the specifics of the job data
        /// Will throw an exception if not valid
        /// </summary>
        public static void Validate(this KalturaBulkUploadJobData model, KalturaOTTFile fileData)
        {
            switch (model)
            {
                case KalturaBulkUploadExcelJobData c: c.Validate(fileData); break;
                case KalturaBulkUploadIngestJobData c: break;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }

        private static void Validate(this KalturaBulkUploadExcelJobData model, KalturaOTTFile fileData)
        {
            if (!fileData.name.EndsWith(ExcelFormatterConsts.EXCEL_EXTENTION))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "fileData.name");
            }
        }
    }
}
