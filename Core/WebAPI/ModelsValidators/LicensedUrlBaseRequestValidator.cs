using System;
using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ModelsValidators
{
    public static class LicensedUrlBaseRequestValidator
    {
        public static void Validate(this KalturaLicensedUrlBaseRequest model)
        {
            if (string.IsNullOrEmpty(model.AssetId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaLicensedUrlBaseRequest.assetId");
            }
            
            switch (model)
            {
                case KalturaLicensedUrlMediaRequest c: c.Validate(); break;
                case KalturaLicensedUrlRecordingRequest c: c.Validate(); break;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }
        
        private static void Validate(this KalturaLicensedUrlMediaRequest model)
        {
            if (model.ContentId == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaLicensedUrlMediaRequest.contentId");
            }
        }

        private static void Validate(this KalturaLicensedUrlRecordingRequest model)
        {
            if (string.IsNullOrEmpty(model.FileType))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaLicensedUrlRecordingRequest.fileType");
            }
            int parsed = 0;
            if (!int.TryParse(model.AssetId, out parsed))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "KalturaLicensedUrlRecordingRequest.assetId");
            }
        }
    }
}