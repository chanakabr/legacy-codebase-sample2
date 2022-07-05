using WebAPI.Exceptions;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class LicensedUrlRecordingRequestMapper
    {
        public static int GetRecordingId(this KalturaLicensedUrlRecordingRequest model)
        {
            int parsed = 0;
            if (!int.TryParse(model.AssetId, out parsed))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "KalturaLicensedUrlRecordingRequest.assetId");
            }
            return parsed;
        }
    }
}