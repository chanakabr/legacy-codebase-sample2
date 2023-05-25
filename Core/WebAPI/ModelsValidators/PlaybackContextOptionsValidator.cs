using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class PlaybackContextOptionsValidator
    {
        public static void Validate(this KalturaPlaybackContextOptions model)
        {
            if (!model.Context.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPlaybackContextOptions.context");
            }
        }

        public static void Validate(this KalturaPlaybackContextOptions model, KalturaAssetType assetType, string assetId)
        {
            Validate(model);

            if (model.Context.HasValue)
            {
                if (((model.Context.Value == KalturaPlaybackContextType.CATCHUP || model.Context.Value == KalturaPlaybackContextType.START_OVER) && assetType != KalturaAssetType.epg) ||
                    (model.Context.Value == KalturaPlaybackContextType.TRAILER && assetType != KalturaAssetType.media) ||
                    (model.Context.Value == KalturaPlaybackContextType.PLAYBACK && assetType == KalturaAssetType.epg) ||
                    (model.Context.Value == KalturaPlaybackContextType.DOWNLOAD && assetType == KalturaAssetType.epg))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "KalturaPlaybackContextOptions.context", "assetType");
                }
            }

            if (!assetType.Equals(KalturaAssetType.recording))
            {
                int validAssetId = 0;
                if (!int.TryParse(assetId, out validAssetId) || validAssetId == 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "assetId");
                }
            }
        }
    }
}
