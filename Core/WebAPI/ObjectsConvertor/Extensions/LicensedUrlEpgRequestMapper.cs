using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Models.API;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class LicensedUrlEpgRequestMapper
    {
        public static int getEpgId(this KalturaLicensedUrlEpgRequest model)
        {
            int parsed = 0;
            if (!int.TryParse(model.AssetId, out parsed)) 
            { 
                throw new BadRequestException(BadRequestException.ARGUMENT_MUST_BE_NUMERIC, "KalturaLicensedUrlEpgRequest.assetId");
            }

            return parsed;
        }
    }
}